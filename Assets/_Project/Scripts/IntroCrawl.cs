using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Star Wars-style текстовый кроул.
/// Телепортирует Main Camera к crawlAnchor на время кроула,
/// потом возвращает обратно.
/// Удержание мыши ускоряет в 3x, иконка мыши мигает.
/// </summary>
public class IntroCrawl : MonoBehaviour
{
    public static IntroCrawl Instance { get; private set; }

    [Header("References")]
    [Tooltip("RectTransform текстового блока (будет скроллиться вверх)")]
    public RectTransform textTransform;

    [Header("Camera")]
    [Tooltip("Основная камера — телепортируется к crawlAnchor")]
    public Camera mainCamera;

    [Tooltip("Якорь позиции для кроула (пустой Transform)")]
    public Transform crawlAnchor;

    [Header("Scroll")]
    [Tooltip("Скорость скролла (пикселей/сек)")]
    public float scrollSpeed = 60f;

    [Tooltip("Множитель при удержании мыши")]
    public float holdMultiplier = 3f;

    [Tooltip("Стартовая Y-позиция текста (anchoredPosition.y)")]
    public float startY = -800f;

    [Tooltip("Конечная Y-позиция текста (anchoredPosition.y)")]
    public float endY = 3000f;

    [Header("Skip Icon")]
    [Tooltip("Иконка мыши (Image) — мигает, белая при удержании")]
    public Image skipIcon;

    [Tooltip("Скорость мигания (циклов/сек)")]
    public float blinkSpeed = 1.5f;

    [Header("Audio")]
    [Tooltip("Музыка кроула (включается на время кроула)")]
    public AudioSource crawlMusic;

    [Tooltip("Аудио сцены (выключается на время кроула)")]
    public AudioSource sceneAudio;

    [Header("Canvas")]
    [Tooltip("Корневой Canvas (активируется при Play, деактивируется после)")]
    public GameObject crawlRoot;

    [Header("State")]
    [Tooltip("SO-канал состояний игры")]
    public GameStateChannel gameStateChannel;

    private Coroutine _crawl;
    private Coroutine _blink;
    private Vector3 _savedPos;
    private Quaternion _savedRot;
    private bool _isHolding;

    void Awake()
    {
        Instance = this;
        if (crawlRoot != null) crawlRoot.SetActive(false);
    }

    /// <summary>Запускает кроул</summary>
    public void Play()
    {
        if (_crawl != null) return;
        if (crawlRoot != null) crawlRoot.SetActive(true);
        _crawl = StartCoroutine(DoCrawl());
    }

    private IEnumerator DoCrawl()
    {
        // Объявляем состояние IntroCrawl — GameStateListener скроет меню автоматически
        gameStateChannel?.Raise(GameState.IntroCrawl);

        // Телепортируем камеру к якорю
        if (mainCamera != null && crawlAnchor != null)
        {
            _savedPos = mainCamera.transform.position;
            _savedRot = mainCamera.transform.rotation;
            mainCamera.transform.position = crawlAnchor.position;
            mainCamera.transform.rotation = crawlAnchor.rotation;
        }

        // Переключаем аудио
        if (sceneAudio != null) sceneAudio.Stop();
        if (crawlMusic != null) crawlMusic.Play();

        // Показываем иконку и запускаем мигание
        if (skipIcon != null)
        {
            skipIcon.gameObject.SetActive(true);
            _blink = StartCoroutine(BlinkIcon());
        }

        // Стартовая позиция
        Vector2 pos = textTransform.anchoredPosition;
        pos.y = startY;
        textTransform.anchoredPosition = pos;

        // Скроллим до конечной позиции
        while (textTransform.anchoredPosition.y < endY)
        {
            var mouse = Mouse.current;
            _isHolding = mouse != null && mouse.leftButton.isPressed;

            float speed = _isHolding ? scrollSpeed * holdMultiplier : scrollSpeed;

            // Ускоряем музыку при удержании
            if (crawlMusic != null)
                crawlMusic.pitch = _isHolding ? 2f : 1f;

            pos = textTransform.anchoredPosition;
            pos.y += speed * Time.deltaTime;
            textTransform.anchoredPosition = pos;
            yield return null;
        }

        // Останавливаем мигание
        if (_blink != null) { StopCoroutine(_blink); _blink = null; }
        if (skipIcon != null) skipIcon.gameObject.SetActive(false);

        // Останавливаем музыку кроула, восстанавливаем аудио сцены
        if (crawlMusic != null) crawlMusic.Stop();
        if (sceneAudio != null) sceneAudio.Play();

        // Кроул завершён
        if (crawlRoot != null) crawlRoot.SetActive(false);
        _crawl = null;

        Debug.Log("[IntroCrawl] Crawl complete. Handing off to VisualNovelManager.");

        // Переключаем состояние — VisualNovelManager запустится сам через StartNovel
        gameStateChannel?.Raise(GameState.VisualNovel);

        // На случай если VisualNovelManager не слушает канал — вызываем напрямую
        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.StartNovel();
        else
            Debug.LogWarning("[IntroCrawl] VisualNovelManager.Instance is null!");
    }

    private IEnumerator BlinkIcon()
    {
        Color baseColor = new Color(1f, 1f, 1f, 0.3f); // полупрозрачный белый
        Color holdColor = Color.white;                    // яркий белый

        while (true)
        {
            if (_isHolding)
            {
                // Удерживают — яркий белый
                skipIcon.color = holdColor;
            }
            else
            {
                // Мигание: плавно 0.3 → 1 → 0.3
                float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                skipIcon.color = Color.Lerp(baseColor, holdColor, t);
            }
            yield return null;
        }
    }
}
