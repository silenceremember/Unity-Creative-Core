using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Plays DialogueSequence via NarratorChannel.
/// Attach to a GameObject in the scene. Assign channel and UI elements.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NarratorManager : MonoBehaviour
{
    [Header("SO Channel")]
    [SerializeField] private NarratorChannel channel;
    [SerializeField] private BoolVariable narratorPlayingVar;

    [Header("UI")]
    [SerializeField] private GameObject subtitleRoot;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI lineText;

    [Header("Scene Activation")]
    [Tooltip("Channel to raise when a DialogueLine.activateObject fires")]
    [SerializeField] private StringChannel activateChannel;

    [Header("Config")]
    [SerializeField] private NarratorConfig config;

    [Header("Localization")]
    [SerializeField] private LanguageVariable languageVar;

    private AudioSource _audioSource;
    private CancellationTokenSource _cts;
    private DialogueSequence _currentSequence;
    private int _blipCounter;

    private bool _skipLine;

    /// <summary>True while any dialogue is playing.</summary>
    public bool IsPlaying => _cts != null && !_cts.IsCancellationRequested;

    /// <summary>Current sequence (null if nothing is playing).</summary>
    public DialogueSequence CurrentSequence => _currentSequence;

    /// <summary>Saved sequence for restoration after restoreInterrupted=true.</summary>
    private DialogueSequence _savedSequence;

    void Update()
    {
#if UNITY_EDITOR
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.pKey.wasPressedThisFrame)
            _skipLine = true;
#endif
    }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (channel != null)
        {
            channel.OnSequenceRequested += Play;
            channel.OnStopRequested     += Stop;
        }
    }

    void OnDisable()
    {
        if (channel != null)
        {
            channel.OnSequenceRequested -= Play;
            channel.OnStopRequested     -= Stop;
        }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Play(DialogueSequence sequence)
    {
        if (sequence == null) return;

        if (_currentSequence != null)
        {
            if (sequence.Priority < _currentSequence.Priority)
                return;

            if (sequence.RestoreInterrupted)
                _savedSequence = _currentSequence;
        }

        Stop();
        _currentSequence = sequence;

        _cts = new CancellationTokenSource();
        if (narratorPlayingVar != null) narratorPlayingVar.Value = true;
        PlaySequence(sequence, _cts.Token).SuppressCancellationThrow().Forget();
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _currentSequence = null;

        if (narratorPlayingVar != null) narratorPlayingVar.Value = false;

        if (lineText != null) lineText.text = "";
        if (subtitleRoot != null) subtitleRoot.SetActive(false);
    }

    private async UniTask PlaySequence(DialogueSequence sequence, CancellationToken ct)
    {
        foreach (var line in sequence.Lines)
        {
            if (lineText != null && lineText.text.Length > 0)
                await EraseText(ct);

            await ShowLine(line, ct);

            float pauseLeft = line.PauseAfter;
            while (pauseLeft > 0f && !_skipLine)
            {
                await UniTask.Yield(ct);
                pauseLeft -= Time.deltaTime;
            }
            _skipLine = false;
        }

        if (lineText != null && lineText.text.Length > 0)
            await EraseText(ct);

        subtitleRoot?.SetActive(false);

        if (sequence.NextSequence != null)
        {
            channel?.NotifyCompleted(sequence);
            _currentSequence = sequence.NextSequence;
            await PlaySequence(sequence.NextSequence, ct);
        }
        else
        {
            _cts?.Dispose();
            _cts = null;
            _currentSequence = null;
            if (narratorPlayingVar != null) narratorPlayingVar.Value = false;
            channel?.NotifyCompleted(sequence);

            if (_savedSequence != null)
            {
                var toRestore = _savedSequence;
                _savedSequence = null;
                channel?.Raise(toRestore);
            }
        }
    }

    private async UniTask ShowLine(DialogueLine line, CancellationToken ct)
    {
        _skipLine = false;
        var lang = languageVar != null ? languageVar.Value : GameLanguage.Russian;
        string resolvedText = line.GetText(lang);

        if (!string.IsNullOrEmpty(line.ActivateObject) && activateChannel != null)
            activateChannel.Raise(line.ActivateObject);

        if (subtitleRoot != null) subtitleRoot.SetActive(true);
        if (speakerText != null) speakerText.text = "";
        if (lineText != null)    lineText.text = "";
        _blipCounter = 0;

        if (lineText != null)
        {
            int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / config.CharsPerSecond));
            foreach (char c in resolvedText)
            {
                if (_skipLine)
                {
                    lineText.text = resolvedText;
                    break;
                }
                lineText.text += c;
                if (!char.IsWhiteSpace(c))
                {
                    _blipCounter++;
                    if (_blipCounter >= config.BlipEveryNChars)
                    {
                        _blipCounter = 0;
                        PlayVoiceBlip();
                    }
                }
                await UniTask.Delay(delayMs, cancellationToken: ct);
            }
        }

        if (!_skipLine)
        {
            float elapsed   = resolvedText.Length / config.CharsPerSecond;
            float total     = Mathf.Max(1.5f, resolvedText.Length / 50f);
            float remaining = total - elapsed;
            if (remaining > 0f)
            {
                float endTime = Time.time + remaining;
                while (Time.time < endTime && !_skipLine)
                    await UniTask.Yield(ct);
            }
        }
    }

    private async UniTask EraseText(CancellationToken ct)
    {
        if (lineText == null) return;
        int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / config.EraseSpeed));
        while (lineText.text.Length > 0)
        {
            lineText.text = lineText.text[..^1];
            await UniTask.Delay(delayMs, cancellationToken: ct);
        }
    }

    private void PlayVoiceBlip()
    {
        if (config == null || _audioSource == null) return;
        var clip = config.GetRandomBlip();
        if (clip == null) return;
        _audioSource.pitch = config.GetRandomPitch();
        _audioSource.PlayOneShot(clip);
    }
}
