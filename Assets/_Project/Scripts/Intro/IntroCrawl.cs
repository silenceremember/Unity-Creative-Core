using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Star Wars-style text crawl.
/// Teleports Main Camera to crawlAnchor during crawl, then returns it back.
/// Holding mouse accelerates 3x, mouse icon blinks.
/// </summary>
public class IntroCrawl : MonoBehaviour
{

    [Header("References")]
    [Tooltip("RectTransform of the text block")]
    [SerializeField] private RectTransform textTransform;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform crawlAnchor;

    [Header("Config")]
    [SerializeField] private CrawlConfig config;

    [Header("Skip Icon")]
    [SerializeField] private Image skipIcon;

    [Header("Audio")]
    [SerializeField] private AudioSource crawlMusic;
    [SerializeField] private AudioSource sceneAudio;

    [Header("Canvas")]
    [SerializeField] private GameObject crawlRoot;

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
    private Vector3 _savedPos;
    private Quaternion _savedRot;
    private bool _isHolding;

    void Awake()
    {

        if (crawlRoot != null) crawlRoot.SetActive(false);
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

        if (crawlRoot != null) crawlRoot.SetActive(true);

        _cts = new CancellationTokenSource();
        DoCrawlAsync(_cts.Token).Forget();
    }

    private async UniTask DoCrawlAsync(CancellationToken ct)
    {
        try
        {
            gameStateChannel?.Raise(GameState.IntroCrawl);

            if (mainCamera != null && crawlAnchor != null)
            {
                _savedPos = mainCamera.transform.position;
                _savedRot = mainCamera.transform.rotation;
                mainCamera.transform.position = crawlAnchor.position;
                mainCamera.transform.rotation = crawlAnchor.rotation;
            }

            if (sceneAudio != null) sceneAudio.Stop();
            if (crawlMusic != null) crawlMusic.Play();

            if (skipIcon != null)
            {
                skipIcon.gameObject.SetActive(true);
                BlinkIconAsync(ct).Forget();
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
                    crawlMusic.pitch = _isHolding ? 2f : 1f;

                pos = textTransform.anchoredPosition;
                pos.y += speed * Time.deltaTime;
                textTransform.anchoredPosition = pos;

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (skipIcon != null) skipIcon.gameObject.SetActive(false);
            if (crawlMusic != null) { crawlMusic.pitch = 1f; crawlMusic.Stop(); }
            if (sceneAudio != null) sceneAudio.Play();
            if (crawlRoot != null) crawlRoot.SetActive(false);
            _cts?.Dispose();
            _cts = null;
        }

        gameStateChannel?.Raise(GameState.VisualNovel);
        novelStartChannel?.Raise();
    }

    private async UniTask BlinkIconAsync(CancellationToken ct)
    {
        Color baseColor = new Color(1f, 1f, 1f, 0.3f);
        Color holdColor = Color.white;

        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                if (_isHolding)
                {
                    skipIcon.color = holdColor;
                }
                else
                {
                    float t = Mathf.PingPong(Time.time * config.BlinkSpeed, 1f);
                    skipIcon.color = Color.Lerp(baseColor, holdColor, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (OperationCanceledException) { }
    }
}
