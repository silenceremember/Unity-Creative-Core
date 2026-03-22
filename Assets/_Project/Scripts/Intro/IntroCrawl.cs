using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Star Wars-style text crawl (Screen Space Camera).
/// Teleports Main Camera to John's anchor.
/// Ends with two-phase fade: text disappears → mesh revealed → background disappears.
/// </summary>
public class IntroCrawl : MonoBehaviour
{

    [Header("References")]
    [Tooltip("RectTransform of the text block")]
    [SerializeField] private RectTransform textTransform;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TransformRegistry cameraAnchors;
    [SerializeField] private CameraAnchor targetAnchor = CameraAnchor.JohnClose;

    [Header("Config")]
    [SerializeField] private CrawlConfig config;

    [Header("Skip Icon")]
    [SerializeField] private Image skipIcon;

    [Header("Audio")]
    [SerializeField] private AudioSource crawlMusic;
    [SerializeField] private AudioSource forestSound;

    [Header("Canvas")]
    [SerializeField] private GameObject crawlRoot;
    [SerializeField] private Canvas crawlCanvas;
    [SerializeField] private CanvasGroup backgroundCanvasGroup;
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Scene")]
    [SerializeField] private GameObject meshToHide;


    [Header("Visual Novel")]
    [SerializeField] private VoidChannel novelStartChannel;

    [Header("Channels")]
    [SerializeField] private VoidChannel startCrawlChannel;

    /// <summary>Hides the crawl root. Called by external systems instead of direct crawlRoot access.</summary>
    public void HideCrawl()
    {
        if (crawlRoot != null) crawlRoot.SetActive(false);
    }

    [Header("State")]
    [SerializeField] private GameStateChannel gameStateChannel;

    private CancellationTokenSource _cts;
    private bool _isHolding;
    private float _initialMusicVolume;
    private float _initialForestVolume;

    void Awake()
    {
        if (crawlRoot != null) crawlRoot.SetActive(false);
        if (crawlMusic != null) _initialMusicVolume = crawlMusic.volume;
        if (forestSound != null) _initialForestVolume = forestSound.volume;
    }

    void OnEnable()
    {
        if (startCrawlChannel != null)
            startCrawlChannel.OnRaised += Play;
    }

    void OnDisable()
    {
        if (startCrawlChannel != null)
            startCrawlChannel.OnRaised -= Play;
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    /// <summary>Starts the crawl.</summary>
    public void Play()
    {
        if (_cts != null) return;

        TeleportCameraToAnchor();
        SetupScreenSpaceCamera();

        if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 1f;
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
        if (meshToHide != null) meshToHide.SetActive(false);
        if (forestSound != null) forestSound.volume = 0f;
        if (crawlRoot != null) crawlRoot.SetActive(true);

        _cts = new CancellationTokenSource();
        DoCrawlAsync(_cts.Token).Forget();
    }

    private void TeleportCameraToAnchor()
    {
        if (mainCamera == null || cameraAnchors == null) return;
        if (!cameraAnchors.TryGet(targetAnchor, out var t)) return;

        mainCamera.transform.position = t.position;
        mainCamera.transform.rotation = t.rotation;
    }

    private void SetupScreenSpaceCamera()
    {
        if (crawlCanvas == null || mainCamera == null) return;
        crawlCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        crawlCanvas.worldCamera = mainCamera;
    }

    private async UniTask DoCrawlAsync(CancellationToken ct)
    {
        try
        {
            gameStateChannel?.Raise(GameState.IntroCrawl);

            if (crawlMusic != null)
            {
                crawlMusic.volume = _initialMusicVolume;
                crawlMusic.Play();
            }

            if (skipIcon != null)
            {
                skipIcon.gameObject.SetActive(true);
                BlinkIconAsync(ct).SuppressCancellationThrow().Forget();
            }

            Vector2 pos = textTransform.anchoredPosition;
            pos.y = config.StartY;
            textTransform.anchoredPosition = pos;

            while (textTransform.anchoredPosition.y < config.EndY)
            {
                ct.ThrowIfCancellationRequested();

                var mouse = Mouse.current;
                _isHolding = mouse != null && mouse.leftButton.isPressed;

                float speed = _isHolding ? config.ScrollSpeed * config.HoldMultiplier : config.ScrollSpeed;

                if (crawlMusic != null)
                    crawlMusic.pitch = _isHolding ? config.HoldMusicPitch : 1f;

                pos = textTransform.anchoredPosition;
                pos.y += speed * Time.deltaTime;
                textTransform.anchoredPosition = pos;

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            await FadeOutAsync(ct);
        }
        finally
        {
            if (skipIcon != null) skipIcon.gameObject.SetActive(false);
            if (crawlMusic != null) { crawlMusic.pitch = 1f; crawlMusic.Stop(); crawlMusic.volume = _initialMusicVolume; }
            if (forestSound != null) forestSound.volume = _initialForestVolume;
            if (crawlRoot != null) crawlRoot.SetActive(false);
            if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 1f;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
            if (meshToHide != null) meshToHide.SetActive(true);
            _cts?.Dispose();
            _cts = null;
        }

        gameStateChannel?.Raise(GameState.VisualNovel);
        novelStartChannel?.Raise();
    }

    private async UniTask FadeOutAsync(CancellationToken ct)
    {
        float totalDuration = config.TextFadeDuration + config.BackgroundFadeDuration;



        // Phase 1: fade text (text keeps scrolling)
        float elapsed = 0f;
        while (elapsed < config.TextFadeDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / config.TextFadeDuration);

            ScrollText();

            if (textCanvasGroup != null)
                textCanvasGroup.alpha = 1f - t;

            if (crawlMusic != null && totalDuration > 0f)
                crawlMusic.volume = Mathf.Lerp(_initialMusicVolume, 0f, elapsed / totalDuration);

            if (forestSound != null && totalDuration > 0f)
                forestSound.volume = Mathf.Lerp(0f, _initialForestVolume, elapsed / totalDuration);

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // Reveal mesh between phases
        if (meshToHide != null) meshToHide.SetActive(true);

        // Phase 2: fade background (text keeps scrolling)
        elapsed = 0f;
        while (elapsed < config.BackgroundFadeDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / config.BackgroundFadeDuration);

            ScrollText();

            if (backgroundCanvasGroup != null)
                backgroundCanvasGroup.alpha = 1f - t;

            if (crawlMusic != null && totalDuration > 0f)
            {
                float globalT = (config.TextFadeDuration + elapsed) / totalDuration;
                crawlMusic.volume = Mathf.Lerp(_initialMusicVolume, 0f, globalT);
            }

            if (forestSound != null && totalDuration > 0f)
            {
                float globalT = (config.TextFadeDuration + elapsed) / totalDuration;
                forestSound.volume = Mathf.Lerp(0f, _initialForestVolume, globalT);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    private void ScrollText()
    {
        Vector2 pos = textTransform.anchoredPosition;
        pos.y += config.ScrollSpeed * Time.deltaTime;
        textTransform.anchoredPosition = pos;
    }

    private async UniTask BlinkIconAsync(CancellationToken ct)
    {
        Color baseColor = new Color(1f, 1f, 1f, config.SkipIconBaseAlpha);
        Color holdColor = Color.white;

        while (true)
        {
            if (_isHolding)
            {
                skipIcon.color = holdColor;
            }
            else
            {
                float t = Mathf.PingPong(Time.time * config.BlinkSpeed, 1f);
                skipIcon.color = Color.Lerp(baseColor, holdColor, t);
            }

            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) break;
        }
    }
}
