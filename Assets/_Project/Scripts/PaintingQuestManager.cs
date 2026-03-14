using System.Collections;
using System.Collections.Generic;
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

    // ── state ─────────────────────────────────────────────────────────────
    // Фиксированный маппинг: нажатие N → индекс в pictureLabels
    // 1-е → [2] (Picture3), 2-е → [0] (Picture1), 3-е → [3] (Picture4), 4-е → [1] (Picture2)
    private static readonly int[] SlotOrder = { 2, 0, 3, 1 };

    private string         _enteredCode   = "";
    private int            _pressCount    = 0;
    private bool           _questActive   = false;
    private bool           _resolved      = false;

    private PaintingInteractable _nearPainting; // ближайшая (для E-prompt)

    // ── lifecycle ─────────────────────────────────────────────────────────

    void Awake() => Instance = this;

    void Start()
    {
        if (questCanvas  != null) questCanvas .SetActive(false);
        if (ePrompt      != null) ePrompt     .SetActive(false);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>[DEBUG] Мгновенно выполняет квест (O).</summary>
    public void DebugCompleteQuest()
    {
        if (!_questActive) StartQuest();

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
        StopAllCoroutines();
        StartCoroutine(Resolve());
        Debug.Log("[DEBUG] Quest force-completed.");
    }
#endif

    void Update()
    {
        if (!_questActive || _resolved) return;
        if (_nearPainting == null)      return;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
            InteractPainting(_nearPainting);
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>Вызывается из PaintingQuestManager.StartQuest — запускает квест.</summary>
    public void StartQuest()
    {
        // Захватываем ТЕКУЩИЙ поворот каждой картины (уже наклонённый PaintingShiftController-ом).
        // Именно в это положение будем возвращать их после reject.
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.CaptureQuestStart();

        _questActive  = true;
        _resolved     = false;
        _enteredCode  = "";
        _pressCount   = 0;

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

        painting.SnapToCorrect();   // картина встаёт на место

        // Назначаем слот ОДИН РАЗ навсегда (при первом нажатии)
        if (painting.AssignedSlotIndex == -1)
            painting.AssignedSlotIndex = SlotOrder[_pressCount];

        // Цифра кода = номер слота + 1 (слот 0 = Picture1 = цифра "1")
        // Первая попытка ВСЕГДА даёт "3142" (reveal), вторая при порядке 1→2→3→4 = accept
        _enteredCode += (painting.AssignedSlotIndex + 1).ToString();

        int slotIndex = painting.AssignedSlotIndex;

        if (slotIndex < pictureLabels.Length && pictureLabels[slotIndex] != null)
        {
            pictureLabels[slotIndex].color = colorDone;
            pictureLabels[slotIndex].fontStyle |= FontStyles.Strikethrough;
        }

        _pressCount++;

        // Скрываем E-prompt — картина уже использована
        if (ePrompt != null) ePrompt.SetActive(false);
        _nearPainting = null;

        Debug.Log($"[PaintingQuestManager] Press {_pressCount}: slot={painting.AssignedSlotIndex}, code so far='{_enteredCode}'");

        // После 4-го нажатия — проверяем
        if (_pressCount >= 4)
            StartCoroutine(Resolve());
    }

    // ── Resolve ───────────────────────────────────────────────────────────

    private IEnumerator Resolve()
    {
        _resolved   = true;
        _questActive = false;

        yield return new WaitForSeconds(0.3f);

        bool accepted = _enteredCode == correctCode;
        Debug.Log($"[PaintingQuestManager] Code '{_enteredCode}' vs '{correctCode}' → {(accepted ? "ACCEPT" : "REJECT")}");

        Color resultColor = accepted ? colorAccept : colorReject;

        if (accepted)
            XPLevelManager.Instance?.AddXP(1000);

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color = resultColor;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        if (accepted)
            StartCoroutine(PulseLabels());
        else
            StartCoroutine(RejectAndReset());
    }

    // ── Reset after Reject ────────────────────────────────────────────────

    private IEnumerator RejectAndReset()
    {
        yield return StartCoroutine(ShakeLabels());  // сначала shake
        yield return new WaitForSeconds(0.8f);        // чуть подождать

        // Возвращаем картины в начальное наклонённое положение
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.ResetPainting(0.5f);

        // Сбрасываем состояние квеста
        _enteredCode  = "";
        _pressCount   = 0;
        _resolved     = false;
        _questActive  = true;

        // Возвращаем жёлтый цвет на все строки задания
        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color     = colorDefault;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        Debug.Log("[PaintingQuestManager] Reset — player can try again.");
    }

    // ── Animations ────────────────────────────────────────────────────────

    private IEnumerator ShakeLabels()
    {
        // Трясём Group (родитель pictureLabels)
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) yield break;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        if (groupRT == null) yield break;

        Vector2 origin = groupRT.anchoredPosition;
        float elapsed  = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeMagnitude, shakeMagnitude) * (1f - elapsed / shakeDuration);
            groupRT.anchoredPosition = origin + new Vector2(x, 0f);
            yield return null;
        }
        groupRT.anchoredPosition = origin;
    }

    private IEnumerator PulseLabels()
    {
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) yield break;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        if (groupRT == null) yield break;

        Vector3 originScale = groupRT.localScale;
        float elapsed       = 0f;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Sin(elapsed / pulseDuration * Mathf.PI);   // 0→1→0
            groupRT.localScale = originScale * (1f + 0.12f * t);
            yield return null;
        }
        groupRT.localScale = originScale;
    }
}
