using System.Collections.Generic;
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

    [Header("Scene Objects (activateObject)")]
    [Tooltip("Scene objects referenced by DialogueLine.activateObject. " +
             "Drag GameObjects here — works even if the object is initially inactive.")]
    [SerializeField] private List<SceneObjectEntry> sceneObjects = new();

    [Header("Audio")]
    [Tooltip("AudioMixerGroup for narrator voice")]
    [SerializeField] private AudioMixerGroup mixerGroup;

    [Header("Settings")]
    [Range(20, 200)]
    [SerializeField] private float charsPerSecond = 50f;
    [Range(50, 500)]
    [SerializeField] private float eraseSpeed = 1000f;
    [SerializeField] private float fadeSpeed = 4f;

    [Tooltip("Sound plays every N non-whitespace characters (Undertale-style).")]
    [Range(1, 10)]
    [SerializeField] private int blipEveryNChars = 4;

    private AudioSource _audioSource;
    private CancellationTokenSource _cts;
    private DialogueSequence _currentSequence;
    private Dictionary<string, GameObject> _sceneObjectMap;
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
        _audioSource.outputAudioMixerGroup = mixerGroup;

        _sceneObjectMap = new Dictionary<string, GameObject>(sceneObjects.Count);
        foreach (var entry in sceneObjects)
            if (!string.IsNullOrEmpty(entry.key) && entry.gameObject != null)
                _sceneObjectMap[entry.key] = entry.gameObject;
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
        PlaySequence(sequence, _cts.Token).Forget();
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
        try
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
        catch (System.OperationCanceledException) { }
    }

    private async UniTask ShowLine(DialogueLine line, CancellationToken ct)
    {
        _skipLine = false;

        if (!string.IsNullOrEmpty(line.ActivateObject))
        {
            if (_sceneObjectMap.TryGetValue(line.ActivateObject, out var go))
                go.SetActive(true);
        }

        if (subtitleRoot != null) subtitleRoot.SetActive(true);
        if (speakerText != null) speakerText.text = "";
        if (lineText != null)    lineText.text = "";
        _blipCounter = 0;

        if (lineText != null)
        {
            int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / charsPerSecond));
            foreach (char c in line.Text)
            {
                if (_skipLine)
                {
                    lineText.text = line.Text;
                    break;
                }
                lineText.text += c;
                if (!char.IsWhiteSpace(c))
                {
                    _blipCounter++;
                    if (_blipCounter >= blipEveryNChars)
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
            float elapsed   = line.Text.Length / charsPerSecond;
            float total     = line.GetDuration();
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
        int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / eraseSpeed));
        while (lineText.text.Length > 0)
        {
            lineText.text = lineText.text[..^1];
            await UniTask.Delay(delayMs, cancellationToken: ct);
        }
    }

    private void PlayVoiceBlip()
    {
        if (channel == null || _audioSource == null) return;
        var clip = channel.GetRandomBlip();
        if (clip == null) return;
        _audioSource.pitch = channel.GetRandomPitch();
        _audioSource.PlayOneShot(clip);
    }
}
