using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Затемнение экрана. Повесь на Canvas с полноэкранным чёрным Image.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Tooltip("Полноэкранный чёрный Image")]
    public Image fadeImage;

    [Range(0.2f, 5f)]
    public float fadeDuration = 1.5f;

    void Awake()
    {
        Instance = this;
        // Начинаем прозрачным
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    /// <summary>Плавно затемнить экран</summary>
    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(DoFade(0f, 1f, onComplete));
    }

    /// <summary>Плавно осветлить экран</summary>
    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(DoFade(1f, 0f, onComplete));
    }

    private IEnumerator DoFade(float from, float to, System.Action onComplete)
    {
        if (fadeImage == null) yield break;
        fadeImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        if (to <= 0f) fadeImage.gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}
