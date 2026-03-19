using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Управляет финальной последовательностью игры (два триггера).
///
/// Триггер 0: плавно затемняет HDRI (skybox exposure → 0) + запускает диалог.
/// Триггер 1:
///   1. Замораживает игрока (PlayerController + CharacterController выключаются).
///   2. Камера плавно открепляется и движется к cameraEndAnchor, смотрит на игрока.
///   3. Запускает финальный диалог рассказчика.
///   4. После окончания диалога → Application.Quit().
/// </summary>
public class FinalSequenceManager : MonoBehaviour
{
    public static FinalSequenceManager Instance { get; private set; }

    // ─── References ──────────────────────────────────────────────────────────

    [Header("Player")]
    [Tooltip("Transform капсулы игрока")]
    public Transform playerTransform;

    [Tooltip("Main Camera (будет открепляться от игрока на триггере 2)")]
    public Camera mainCamera;

    [Header("GameState")]
    [Tooltip("Канал состояний — на триггере 2 переключаемся в GameState.Final")]
    public GameStateChannel gameStateChannel;

    // ─── HDRI (Trigger 1) ────────────────────────────────────────────────────

    [Header("HDRI Fade (Trigger 1)")]
    [Tooltip("Материал скайбокса (HDRI) — перетащи прямо из проекта")]
    public Material hdriMaterial;

    [Tooltip("Длительность затемнения HDRI в секундах")]
    public float hdriDarkDuration = 4f;

    [Tooltip("Имя свойства экспозиции в материале (обычно _Exposure)")]
    public string hdriExposureProperty = "_Exposure";

    // ─── Narrator (Trigger 0) ────────────────────────────────────────────────

    [Header("Narrator (Trigger 0)")]
    [Tooltip("Диалог рассказчика при касании триггера 0 (играет параллельно затемнению HDRI)")]
    public DialogueSequence seqTrigger0;

    // ─── Camera (Trigger 2) ──────────────────────────────────────────────────

    [Header("Camera Cinematic (Trigger 2)")]
    [Tooltip("Точка куда переместится камера при триггере 2 (пустой GameObject в сцене)")]
    public Transform cameraEndAnchor;

    [Tooltip("Длительность движения камеры к anchor (сек)")]
    public float cameraTravelDuration = 2.5f;

    [Tooltip("Кривая движения камеры")]
    public AnimationCurve cameraCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Дальняя точка камеры — отдаление в конце Part 2 (перед выходом). Оставь пустым — движения не будет.")]
    public Transform cameraFarAnchor;

    [Tooltip("Длительность плавного отдаления к дальней точке (сек)")]
    public float cameraFarDuration = 4f;

    // ─── Narrator (Trigger 2) ────────────────────────────────────────────────

    [Header("Narrator")]
    public NarratorChannel narratorChannel;

    [Tooltip("Часть 1 — после неё появляется подсказка ESC")]
    public DialogueSequence seqFinalPart1;

    [Tooltip("Часть 2 — играет автоматически после части 1; по окончании → выход из игры")]
    public DialogueSequence seqFinalPart2;

    [Tooltip("Задержка перед запуском финального диалога (сек) — камера должна доехать")]
    public float narratorDelayAfterCamera = 0.5f;

    // ─── ESC Hint UI ─────────────────────────────────────────────────────────

    [Header("ESC Hint")]
    [Tooltip("Объект с подсказкой ESC — активируется после части 1")]
    public GameObject escHintUI;

    // ─── Quit (after dialogue) ───────────────────────────────────────────────

    [Header("Quit")]
    [Tooltip("Задержка после окончания части 2 перед выходом (сек)")]
    public float quitDelay = 1.5f;

    // ─── Private ─────────────────────────────────────────────────────────────

    private bool     _trigger1Done  = false;
    private bool     _trigger2Done  = false;
    private bool     _escEnabled    = false;   // true после окончания части 1
    private Material _hdriClone;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;

        // Сразу создаём инстанс HDRI-материала, чтобы не трогать оригинальный ассет
        if (hdriMaterial != null)
        {
            _hdriClone = new Material(hdriMaterial);
            RenderSettings.skybox = _hdriClone;
        }
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

    void Update()
    {
        // ESC доступен после окончания части 1
        if (!_escEnabled) return;
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[FinalSequenceManager] ESC pressed — quitting.");
            QuitSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    // ── Public Entry Point ───────────────────────────────────────────────────

    public void OnFinalTrigger(int triggerId)
    {
        if (triggerId == 0 && !_trigger1Done)
        {
            _trigger1Done = true;
            Debug.Log("[FinalSequenceManager] Trigger 0 — locking WASD, HDRI fade + dialogue start.");

            // Блокируем горизонтальное движение — игрок падает ровно вниз к триггеру 1
            if (playerTransform != null)
            {
                var pc = playerTransform.GetComponent<PlayerController>();
                if (pc != null) pc.LockXZ();   // мгновенно обнуляет X/Z, только Y
            }

            FadeHDRI(this.GetCancellationTokenOnDestroy()).Forget();
            if (seqTrigger0 != null)
                narratorChannel?.Raise(seqTrigger0);
        }
        else if (triggerId == 1 && !_trigger2Done)
        {
            _trigger2Done = true;
            Debug.Log("[FinalSequenceManager] Trigger 2 — Final sequence start.");
            // Переключаем GameState — ExplorationManager и другие системы отреагируют
            gameStateChannel?.Raise(GameState.Final);
            StartFinalSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    // ── Trigger 1: HDRI Fade ─────────────────────────────────────────────────

    private async UniTask FadeHDRI(CancellationToken ct)
    {
        if (_hdriClone == null || !_hdriClone.HasProperty(hdriExposureProperty))
        {
            Debug.LogWarning("[FinalSequenceManager] hdriMaterial не назначен или свойство не найдено.");
            return;
        }

        float elapsed  = 0f;
        float startExp = _hdriClone.GetFloat(hdriExposureProperty);

        while (elapsed < hdriDarkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / hdriDarkDuration));
            _hdriClone.SetFloat(hdriExposureProperty, Mathf.Lerp(startExp, 0f, t));
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _hdriClone.SetFloat(hdriExposureProperty, 0f);
        Debug.Log("[FinalSequenceManager] HDRI fully darkened.");
    }

    // ── Trigger 2: Final Sequence ─────────────────────────────────────────────

    private async UniTask StartFinalSequence(CancellationToken ct)
    {
        // 1. Заморозка игрока
        FreezePlayer();

        // 2. Камера — открепляем и двигаем к anchor
        await MoveCameraToAnchor(ct);

        // 3. Небольшая пауза после камеры
        await UniTask.Delay(
            (int)(narratorDelayAfterCamera * 1000f),
            cancellationToken: ct);

        // 4. Финальный диалог — часть 1 (часть 2 запустится по OnNarratorCompleted)
        if (seqFinalPart1 != null)
        {
            Debug.Log("[FinalSequenceManager] Playing final dialogue part 1.");
            narratorChannel?.Raise(seqFinalPart1);
        }
        else
        {
            // Диалога нет — сразу выходим
            await QuitSequence(ct);
        }
    }

    // ── Freeze Player ─────────────────────────────────────────────────────────

    private void FreezePlayer()
    {
        // Выключаем PlayerController (движение + поворот мышью)
        if (playerTransform != null)
        {
            var pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.enabled = false;
                Debug.Log("[FinalSequenceManager] PlayerController disabled.");
            }

            // Выключаем CharacterController чтобы игрок не падал дальше
            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                Debug.Log("[FinalSequenceManager] CharacterController disabled.");
            }

            // Показываем капсулу игрока — GameplaySetup её скрывает, включаем для камеры
            var mr = playerTransform.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = true;
                mr.lightProbeUsage = LightProbeUsage.Off;  // отключаем blend probes
                Debug.Log("[FinalSequenceManager] Player capsule MeshRenderer enabled.");
            }
        }

        // Блокируем ввод — курсор освобождаем для финального диалога
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ── Move Camera ───────────────────────────────────────────────────────────

    private async UniTask MoveCameraToAnchor(CancellationToken ct)
    {
        if (mainCamera == null || cameraEndAnchor == null) return;

        // Открепляем камеру от игрока чтобы двигать независимо
        mainCamera.transform.SetParent(null, worldPositionStays: true);

        await MoveCameraToTarget(cameraEndAnchor, cameraTravelDuration, ct);
        Debug.Log("[FinalSequenceManager] Camera reached final anchor.");
    }

    // ── Narrator Completed ────────────────────────────────────────────────────

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        // Часть 1 завершилась → показываем ESC + запускаем отдаление камеры параллельно
        if (completed == seqFinalPart1)
        {
            Debug.Log("[FinalSequenceManager] Part 1 done — ESC hint + camera pull-back start.");
            _escEnabled = true;
            if (escHintUI != null) escHintUI.SetActive(true);
            // Отдаление начинается сразу, пока играет Part 2
            if (cameraFarAnchor != null)
                MoveCameraToTarget(cameraFarAnchor, cameraFarDuration, this.GetCancellationTokenOnDestroy()).Forget();
            return;
        }

        // Часть 2 завершилась → сразу выход (камера уже едет или доехала)
        if (completed == seqFinalPart2)
        {
            Debug.Log("[FinalSequenceManager] Part 2 done — auto quit.");
            QuitSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    // ── Pull-back then Quit ───────────────────────────────────────────────────

    // PullBackAndQuit удалён — отдаление запускается в OnNarratorCompleted (Part 1 done)

    // ── Move Camera to arbitrary target ──────────────────────────────────────

    /// <summary>Плавно перемещает камеру к <paramref name="target"/>, постоянно смотря на игрока.</summary>
    private async UniTask MoveCameraToTarget(Transform target, float duration, CancellationToken ct)
    {
        if (mainCamera == null || target == null) return;

        var    camT     = mainCamera.transform;
        Vector3    startPos = camT.position;
        Quaternion startRot = camT.rotation;
        float  elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = cameraCurve != null && cameraCurve.length > 0
                ? cameraCurve.Evaluate(Mathf.Clamp01(elapsed / duration))
                : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            camT.position = Vector3.Lerp(startPos, target.position, t);

            if (playerTransform != null)
            {
                Vector3 dir = (playerTransform.position - camT.position).normalized;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    camT.rotation = Quaternion.Lerp(startRot, targetRot, t);
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        camT.position = target.position;
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - camT.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
                camT.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    // ── Quit ──────────────────────────────────────────────────────────────────

    private async UniTask QuitSequence(CancellationToken ct)
    {
        await UniTask.Delay((int)(quitDelay * 1000f), cancellationToken: ct);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
