using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Главный менеджер квеста «поправь картины».
///
/// Логика кода:
///   - Фиксированный порядок слотов: [2, 0, 3, 1] (0-based Picture indices)
///     Значит: 1-е нажатие → Picture3, 2-е → Picture1, 3-е → Picture4, 4-е → Picture2
///   - У каждого PaintingInteractable есть codeDigit (1-4)
///   - Код = строка из codeDigit-ов в порядке нажатий
///   - Correct code = "1234"
///   - Accept зелёный; Reject красный + shake
/// </summary>
public class PaintingQuestManager : MonoBehaviour
{
    public static PaintingQuestManager Instance { get; private set; }

    [Header("Картины (все 4 интерактабла)")]
    [Tooltip("Назначи все 4 PaintingInteractable — нужны для сброса после reject")]
    public PaintingInteractable[] interactables = new PaintingInteractable[4];

    [Header("Quest Canvas")]
    [Tooltip("Корневой объект QuestCanvas")]
    public GameObject questCanvas;

    [Header("Quest Lines (Picture1..4 в Group)")]
    [Tooltip("Перетащи Picture1, Picture2, Picture3, Picture4 по порядку")]
    public TextMeshProUGUI[] pictureLabels = new TextMeshProUGUI[4];

    [Header("E-Prompt")]
    [Tooltip("UI-объект с подсказкой '[E] Поправить'")]
    public GameObject ePrompt;

    [Header("Код")]
    [Tooltip("Правильный код")]
    public string correctCode = "1234";

    [Header("Анимация")]
    public float shakeDuration   = 0.5f;
    public float shakeMagnitude  = 12f;    // pixels
    public float pulseDuration   = 0.4f;

    [Header("Цвета")]
    public Color colorDefault  = Color.yellow;
    public Color colorDone     = Color.gray;    // когда строка «вычеркнута»
    public Color colorAccept   = Color.green;
    public Color colorReject   = Color.red;

    [Header("Звуки квеста")]
    [Tooltip("Звук при нажатии E на картину (2D, не reject)")]
    public AudioClip interactSound;
    [Tooltip("Звук при успешном завершении квеста (Accept / зелёный)")]
    public AudioClip acceptSound;
    [Tooltip("Звук при провале / reject (красный)")]
    public AudioClip rejectSound;
    [Tooltip("AudioSource для воспроизведения звуков квеста (если пусто — найдём на этом объекте)")]
    public AudioSource questAudioSource;

    [Header("Нарратор")]
    [Tooltip("Канал рассказчика")]
    public NarratorChannel narratorChannel;

    [Tooltip("Реплики при нажатии на картину (4 штуки — перебираются по кругу)")]
    public DialogueSequence[] paintingClickDialogues = new DialogueSequence[4];

    [Tooltip("Реплики при reject-е (5 штук: reject 1..5)")]
    public DialogueSequence[] rejectDialogues = new DialogueSequence[5];

    [Tooltip("Диалог после успешного выполнения квеста (далее XPLevelManager добавит XP)")]
    public DialogueSequence seqPostQuest;

    // ── state ─────────────────────────────────────────────────────────────
    // Фиксированный маппинг: нажатие N → индекс в pictureLabels
    // 1-е → [2] (Picture3), 2-е → [0] (Picture1), 3-е → [3] (Picture4), 4-е → [1] (Picture2)
    private static readonly int[] SlotOrder = { 2, 0, 3, 1 };

    private string         _enteredCode   = "";
    private int            _pressCount    = 0;
    private bool           _questActive   = false;
    private bool           _resolved      = false;
    private int            _rejectCount   = 0;

    private PaintingInteractable _nearPainting; // ближайшая (для E-prompt)

    // Антиспам — задержка после диалога перед следующим E
    private float _eBlockedUntil  = 0f;
    private const float ESpamCooldown = 0.5f;

    [Header("Reject анимация E-подсказки")]
    [Tooltip("Амплитуда встряски E-промпта (пиксели)")]
    public float ePromptShakeMagnitude = 10f;
    [Tooltip("Длительность встряски (сек)")]
    public float ePromptShakeDuration  = 0.35f;
    private bool _ePromptShaking = false;

    // CancellationTokenSource для основного потока квеста
    private CancellationTokenSource _questCts;

    // ── lifecycle ─────────────────────────────────────────────────────────

    void Awake() => Instance = this;

    void Start()
    {
        if (questCanvas  != null) questCanvas .SetActive(false);
        if (ePrompt      != null) ePrompt     .SetActive(false);

        // Если AudioSource не назначен — пробуем найти на этом же объекте
        if (questAudioSource == null)
            questAudioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
    }

    void OnDestroy()
    {
        _questCts?.Cancel();
        _questCts?.Dispose();
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        // Когда пост-квест нарратив завершился — добавляем XP
        if (completed == seqPostQuest && XPLevelManager.Instance != null)
        {
            Debug.Log("[PaintingQuestManager] seqPostQuest finished — adding XP.");
            XPLevelManager.Instance.AddXP(XPLevelManager.Instance.questRewardXP);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>[DEBUG] Мгновенно выполняет квест (O).</summary>
    public void DebugCompleteQuest()
    {
        if (!_questActive) StartQuest();

        // Отменяем текущие задачи
        _questCts?.Cancel();
        _questCts?.Dispose();
        _questCts = null;

        // Назначаем слоты по умолчанию и снапаем все картины
        for (int i = 0; i < interactables.Length && i < 4; i++)
        {
            var p = interactables[i];
            if (p == null || p.IsUsed) continue;
            if (p.AssignedSlotIndex == -1)
                p.AssignedSlotIndex = SlotOrder[_pressCount];
            p.SnapToCorrect();
            _enteredCode += (p.AssignedSlotIndex + 1).ToString();

            int slotIndex = p.AssignedSlotIndex;
            if (slotIndex < pictureLabels.Length && pictureLabels[slotIndex] != null)
            {
                pictureLabels[slotIndex].color     = colorDone;
                pictureLabels[slotIndex].fontStyle |= FontStyles.Strikethrough;
            }
            _pressCount++;
        }

        // Подменяем код на правильный и форсируем resolve
        _enteredCode  = correctCode;
        _resolved     = true;
        _questActive  = false;

        _questCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        ResolveAsync(_questCts.Token).Forget();
        Debug.Log("[DEBUG] Quest force-completed.");
    }
#endif

    void Update()
    {
        if (!_questActive || _resolved) return;
        if (_nearPainting == null)      return;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            if (PauseMenuManager.IsPaused) return;
            bool triggerDialogue = ExplorationManager.Instance != null &&
                                   ExplorationManager.Instance.TriggerDialoguePlaying;
            if (triggerDialogue)
            {
                if (!_ePromptShaking && ePrompt != null)
                {
                    // Звук reject — нельзя взаимодействовать во время диалога
                    if (questAudioSource != null && rejectSound != null)
                        questAudioSource.PlayOneShot(rejectSound);
                    ShakeEPromptAsync(destroyCancellationToken).Forget();
                }
                return;
            }
            _eBlockedUntil = Time.unscaledTime + ESpamCooldown;
            InteractPainting(_nearPainting);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Вызывается из ExplorationManager — запускает квест.</summary>
    public void StartQuest()
    {
        // Захватываем ТЕКУЩИЙ поворот каждой картины (уже наклонённый PaintingShiftController-ом).
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.CaptureQuestStart();

        _questActive  = true;
        _resolved     = false;
        _enteredCode  = "";
        _pressCount   = 0;
        _rejectCount  = 0;

        if (questCanvas != null) questCanvas.SetActive(true);

        // Красим все строки в жёлтый
        foreach (var lbl in pictureLabels)
            if (lbl != null) lbl.color = colorDefault;

        Debug.Log("[PaintingQuestManager] Quest started.");
    }

    /// <summary>PaintingInteractable вызывает при входе игрока в зону.</summary>
    public void OnPaintingEnter(PaintingInteractable painting)
    {
        if (!_questActive || _resolved || painting.IsUsed) return;
        _nearPainting = painting;
        if (ePrompt != null) ePrompt.SetActive(true);
    }

    /// <summary>PaintingInteractable вызывает при выходе игрока из зоны.</summary>
    public void OnPaintingExit(PaintingInteractable painting)
    {
        if (_nearPainting != painting) return;
        _nearPainting = null;
        if (ePrompt != null) ePrompt.SetActive(false);
    }

    // ── Interaction ───────────────────────────────────────────────────────

    private void InteractPainting(PaintingInteractable painting)
    {
        if (painting.IsUsed) return;

        // Мгновенный 2D звук нажатия E (обратная связь)
        if (questAudioSource != null && interactSound != null)
            questAudioSource.PlayOneShot(interactSound);

        painting.SnapToCorrect();   // картина встаёт на место (PlaySlideSound 3D внутри)

        // Назначаем слот ОДИН РАЗ навсегда (при первом нажатии)
        if (painting.AssignedSlotIndex == -1)
            painting.AssignedSlotIndex = SlotOrder[_pressCount];

        // Цифра кода = номер слота + 1 (слот 0 = Picture1 = цифра "1")
        _enteredCode += (painting.AssignedSlotIndex + 1).ToString();

        int slotIndex = painting.AssignedSlotIndex;
        if (slotIndex < pictureLabels.Length && pictureLabels[slotIndex] != null)
        {
            pictureLabels[slotIndex].color = colorDone;
            pictureLabels[slotIndex].fontStyle |= FontStyles.Strikethrough;
        }

        _pressCount++;

        // Реплика рассказчика при нажатии (перебираем по кругу)
        if (paintingClickDialogues != null && paintingClickDialogues.Length > 0)
        {
            int idx = (_pressCount - 1) % paintingClickDialogues.Length;
            var seq = paintingClickDialogues[idx];
            if (seq != null) narratorChannel?.Raise(seq);
        }

        // Скрываем E-prompt — картина уже использована
        if (ePrompt != null) ePrompt.SetActive(false);
        _nearPainting = null;

        Debug.Log($"[PaintingQuestManager] Press {_pressCount}: slot={painting.AssignedSlotIndex}, code so far='{_enteredCode}'");

        // После 4-го нажатия — проверяем
        if (_pressCount >= 4)
        {
            _questCts?.Cancel();
            _questCts?.Dispose();
            _questCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            ResolveAsync(_questCts.Token).Forget();
        }
    }

    // ── Resolve ───────────────────────────────────────────────────────────

    private async UniTask ResolveAsync(CancellationToken ct)
    {
        _resolved    = true;
        _questActive = false;

        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f), cancellationToken: ct);

            bool accepted = _enteredCode == correctCode;
            Debug.Log($"[PaintingQuestManager] Code '{_enteredCode}' vs '{correctCode}' → {(accepted ? "ACCEPT" : "REJECT")}");

            Color resultColor = accepted ? colorAccept : colorReject;

            foreach (var lbl in pictureLabels)
                if (lbl != null)
                {
                    lbl.color = resultColor;
                    lbl.fontStyle &= ~FontStyles.Strikethrough;
                }

            if (accepted)
            {
                // Звук победы
                if (questAudioSource != null && acceptSound != null)
                    questAudioSource.PlayOneShot(acceptSound);

                await PulseLabelsAsync(ct);

                // Запускаем пост-квест нарратив. XPLevelManager добавит XP когда нарратор
                // дойдёт до нужной реплики (через activateObject или из XPLevelManager напрямую).
                if (seqPostQuest != null)
                    narratorChannel?.Raise(seqPostQuest);
                else
                    XPLevelManager.Instance?.AddXP(XPLevelManager.Instance != null ? XPLevelManager.Instance.questRewardXP : 1000);
            }
            else
            {
                // Звук провала / reject
                if (questAudioSource != null && rejectSound != null)
                    questAudioSource.PlayOneShot(rejectSound);

                await ShakeLabelsAsync(ct);

                _rejectCount++;
                Debug.Log($"[PaintingQuestManager] Reject #{_rejectCount}");

                // Реплика-подсказка рассказчика
                int rejectIdx = _rejectCount - 1;
                if (rejectDialogues != null && rejectIdx < rejectDialogues.Length && rejectDialogues[rejectIdx] != null)
                    narratorChannel?.Raise(rejectDialogues[rejectIdx]);

                if (_rejectCount >= 5)
                {
                    // 5-й reject: рассказчик решает квест за игрока после диалога
                    await AutoSolveAfterDialogueAsync(ct);
                }
                else
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(0.8f), cancellationToken: ct);
                    DoReset();
                }
            }
        }
        catch (System.OperationCanceledException) { }
    }

    // ── Auto-solve on 5th reject ──────────────────────────────────────────

    private async UniTask AutoSolveAfterDialogueAsync(CancellationToken ct)
    {
        // Ждём пока нарратор закончит реплику (5-й reject ~5 секунд)
        await UniTask.Delay(System.TimeSpan.FromSeconds(5f), cancellationToken: ct);

        // Возвращаем картины в наклонённое положение
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.ResetPainting(0.4f);

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.6f), cancellationToken: ct);

        // Сбрасываем состояние
        _resolved    = false;
        _questActive = true;
        _enteredCode = "";
        _pressCount  = 0;

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color     = colorDefault;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        // Рассказчик «делает это сам»
        DebugCompleteQuest();
    }

    // ── Reset after Reject ────────────────────────────────────────────────

    private void DoReset()
    {
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.ResetPainting(0.5f);

        _enteredCode  = "";
        _pressCount   = 0;
        _resolved     = false;
        _questActive  = true;

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color     = colorDefault;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        Debug.Log("[PaintingQuestManager] Reset — player can try again.");
    }

    // ── Animations ────────────────────────────────────────────────────────

    private async UniTask ShakeLabelsAsync(CancellationToken ct)
    {
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) return;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        if (groupRT == null) return;

        Vector2 origin = groupRT.anchoredPosition;
        float elapsed  = 0f;

        while (elapsed < shakeDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeMagnitude, shakeMagnitude) * (1f - elapsed / shakeDuration);
            groupRT.anchoredPosition = origin + new Vector2(x, 0f);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        groupRT.anchoredPosition = origin;
    }

    private async UniTask PulseLabelsAsync(CancellationToken ct)
    {
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) return;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        if (groupRT == null) return;

        Vector3 originScale = groupRT.localScale;
        float elapsed       = 0f;

        while (elapsed < pulseDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float t  = Mathf.Sin(elapsed / pulseDuration * Mathf.PI);   // 0→1→0
            groupRT.localScale = originScale * (1f + 0.12f * t);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        groupRT.localScale = originScale;
    }

    // ── Reject-встряска E-подсказки ───────────────────────────────────────

    private async UniTask ShakeEPromptAsync(CancellationToken ct)
    {
        if (ePrompt == null) return;
        _ePromptShaking = true;

        var rt = ePrompt.GetComponent<RectTransform>();
        if (rt == null) { _ePromptShaking = false; return; }

        // Красим текст временно красным
        var txt = ePrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Color originalColor = txt != null ? txt.color : Color.white;
        if (txt != null) txt.color = Color.red;

        Vector2 origin  = rt.anchoredPosition;
        float   elapsed = 0f;

        try
        {
            while (elapsed < ePromptShakeDuration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float x = Random.Range(-ePromptShakeMagnitude, ePromptShakeMagnitude) *
                          (1f - elapsed / ePromptShakeDuration);
                rt.anchoredPosition = origin + new Vector2(x, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        rt.anchoredPosition = origin;
        if (txt != null) txt.color = originalColor;
        _ePromptShaking = false;
    }
}
