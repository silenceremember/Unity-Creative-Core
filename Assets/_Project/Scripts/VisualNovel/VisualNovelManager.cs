using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual novel "The Smith Family".
///
/// Flow:
///  1. StartNovel() — camera snaps to anchor[line.CameraAnchor], show NovelCanvas, print line 0
///  2. User presses "Next"
///     a) If the NEXT line has narratorSequenceBefore — hide NovelCanvas, let narrator speak, wait for OnSequenceCompleted
///     b) After narrator (or immediately if none) — blend to new anchor if CameraAnchor changed, show NovelCanvas, print line
///     c) Repeat until end of list
///  3. On end — hide NovelCanvas, call NovelChannel.NotifyCompleted()
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VisualNovelManager : MonoBehaviour
{
    [Header("SO Channels")]
    [SerializeField] private NovelChannel novelChannel;
    [SerializeField] private GameStateChannel gameStateChannel;
    [SerializeField] private NarratorChannel narratorChannel;
    [SerializeField] private VoidChannel novelStartChannel;

    [Header("Novel Canvas")]
    [SerializeField] private GameObject novelCanvasRoot;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private Button nextButton;

    [Header("Sequence")]
    [SerializeField] private NovelSequence sequence;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TransformRegistry cameraAnchors;

    [Header("Config")]
    [SerializeField] private NovelConfig config;

    [Header("Localization")]
    [SerializeField] private LanguageVariable languageVar;

    private int _lineIndex = -1;
    private bool _waitingForNarrator = false;
    private bool _typewriterRunning = false;

    private CancellationTokenSource _cts;
    private bool _nextPressed = false;
    private bool _skipTypewriter = false;

    private CameraAnchor _currentAnchor;
    private bool _cameraInitialized;
    private AudioSource _audioSource;
    private int _blipCounter;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = config != null ? config.MixerGroup : null;
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
        if (novelChannel != null)
            novelChannel.OnNovelAbortRequested += AbortNovel;
        if (novelStartChannel != null)
            novelStartChannel.OnRaised += StartNovel;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (novelChannel != null)
            novelChannel.OnNovelAbortRequested -= AbortNovel;
        if (novelStartChannel != null)
            novelStartChannel.OnRaised -= StartNovel;
    }

    /// <summary>Force-hide the novel UI (for debug skip).</summary>
    public void ForceHideNovelCanvas() => HideNovelCanvas();

    /// <summary>
    /// Called from IntroCrawl when crawl finishes.
    /// Camera instantly teleports to anchor[0].
    /// </summary>
    public void StartNovel()
    {
        if (sequence == null || sequence.Lines == null || sequence.Lines.Length == 0)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _lineIndex = 0;
        _cameraInitialized = false;
        _waitingForNarrator = false;
        _nextPressed = false;
        _skipTypewriter = false;

        RunNovelAsync(_cts.Token).SuppressCancellationThrow().Forget();
    }

    /// <summary>Force-abort the novel (called from NovelChannel or directly).</summary>
    public void AbortNovel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        HideNovelCanvas();
    }

    /// <summary>"Next" button handler.</summary>
    public void OnNextButton()
    {
        if (_typewriterRunning)
        {
            _skipTypewriter = true;
            return;
        }

        _nextPressed = true;
    }

    private async UniTask RunNovelAsync(CancellationToken ct)
    {
        SnapCamera(sequence.Lines[0].CameraAnchor);

        if (sequence.Lines[0].NarratorSequenceBefore != null)
        {
            HideNovelCanvas();
            TriggerNarrator(sequence.Lines[0].NarratorSequenceBefore);
            _waitingForNarrator = true;
            await WaitForNarratorAsync(ct);
        }

        await ShowLineAndWaitAsync(ct);

        while (true)
        {
            _nextPressed = false;
            await UniTask.WaitUntil(() => _nextPressed, cancellationToken: ct);
            _nextPressed = false;

            _lineIndex++;

            if (sequence == null || _lineIndex >= sequence.Lines.Length)
            {
                EndNovel();
                return;
            }

            var line = sequence.Lines[_lineIndex];

            if (line.NarratorSequenceBefore != null)
            {
                HideNovelCanvas();
                _waitingForNarrator = true;

                if (line.CameraAnchor != _currentAnchor)
                    await BlendCameraAsync(line.CameraAnchor, ct);

                TriggerNarrator(line.NarratorSequenceBefore);
                await WaitForNarratorAsync(ct);
            }

            await ShowLineAndWaitAsync(ct);
        }
    }

    private async UniTask ShowLineAndWaitAsync(CancellationToken ct)
    {
        if (sequence == null || _lineIndex < 0 || _lineIndex >= sequence.Lines.Length) return;

        var line = sequence.Lines[_lineIndex];

        if (line.CameraAnchor != _currentAnchor)
        {
            if (!_cameraInitialized)
            {
                _currentAnchor = line.CameraAnchor;
                _cameraInitialized = true;
            }
            else
                await BlendCameraAsync(line.CameraAnchor, ct);
        }

        await DisplayLineAsync(line, ct);
    }

    private async UniTask DisplayLineAsync(NovelLine line, CancellationToken ct)
    {
        ShowNovelCanvas();
        var lang = languageVar != null ? languageVar.Value : GameLanguage.Russian;

        if (speakerText != null) speakerText.text = line.GetSpeakerDisplayName(lang);
        if (lineText != null) lineText.text = "";
        _blipCounter = 0;

        _typewriterRunning = true;
        _skipTypewriter = false;
        await TypewriterAsync(line.GetText(lang), line.Speaker, ct);
        _typewriterRunning = false;
    }

    private void TriggerNarrator(DialogueSequence seq)
    {
        narratorChannel?.Raise(seq);
    }

    private UniTaskCompletionSource _narratorTcs;

    private UniTask WaitForNarratorAsync(CancellationToken ct)
    {
        _narratorTcs = new UniTaskCompletionSource();
        return _narratorTcs.Task.AttachExternalCancellation(ct);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (!_waitingForNarrator) return;
        _waitingForNarrator = false;
        _narratorTcs?.TrySetResult();
        _narratorTcs = null;
    }

    private void EndNovel()
    {
        HideNovelCanvas();
        novelChannel?.NotifyCompleted();
        gameStateChannel?.Raise(GameState.Gameplay);
    }

    private void ShowNovelCanvas()
    {
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(true);
    }

    private void HideNovelCanvas()
    {
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(false);
    }

    /// <summary>Instant camera teleport.</summary>
    private void SnapCamera(CameraAnchor anchor)
    {
        if (mainCamera == null) return;
        if (!cameraAnchors.TryGet(anchor, out var t)) return;

        mainCamera.transform.position = t.position;
        mainCamera.transform.rotation = t.rotation;
        _currentAnchor = anchor;
    }

    /// <summary>Smooth camera transition (async UniTask).</summary>
    private async UniTask BlendCameraAsync(CameraAnchor anchor, CancellationToken ct)
    {
        if (mainCamera == null) return;
        if (!cameraAnchors.TryGet(anchor, out var t)) return;

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < config.BlendDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float p = config.BlendCurve.Evaluate(Mathf.Clamp01(elapsed / config.BlendDuration));
            mainCamera.transform.position = Vector3.Lerp(startPos, t.position, p);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, t.rotation, p);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        mainCamera.transform.position = t.position;
        mainCamera.transform.rotation = t.rotation;
        _currentAnchor = anchor;
    }

    private async UniTask TypewriterAsync(string text, Speaker speaker, CancellationToken ct)
    {
        if (lineText == null) return;

        var voice = config.GetVoice(speaker);
        int delay = Mathf.RoundToInt(1000f / config.CharsPerSecond);
        int blipInterval = voice != null ? voice.BlipInterval : 4;

        foreach (char c in text)
        {
            if (_skipTypewriter)
            {
                lineText.text = text;
                _skipTypewriter = false;
                _typewriterRunning = false;
                return;
            }

            ct.ThrowIfCancellationRequested();
            lineText.text += c;
            if (!char.IsWhiteSpace(c))
            {
                _blipCounter++;
                if (_blipCounter >= blipInterval)
                {
                    _blipCounter = 0;
                    PlayVoiceBlip(speaker);
                }
            }
            await UniTask.Delay(delay, cancellationToken: ct);
        }
    }

    private void PlayVoiceBlip(Speaker speaker)
    {
        if (_audioSource == null || config == null) return;
        var voice = config.GetVoice(speaker);
        if (voice == null) return;
        var clip = voice.GetRandomBlip();
        if (clip == null) return;
        _audioSource.pitch = voice.GetRandomPitch();
        _audioSource.PlayOneShot(clip);
    }
}
