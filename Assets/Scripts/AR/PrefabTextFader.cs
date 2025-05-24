 using UnityEngine;
using TMPro;
using System.Collections;

public class PrefabTextFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup targetGroup;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private float fadeDuration = 2f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (targetGroup == null)
        {
            targetGroup = GetComponent<CanvasGroup>();
            if (targetGroup == null)
            {
                Debug.LogWarning("[PrefabTextFader] CanvasGroup not assigned and not found on GameObject!");
            }
        }
        if (targetText == null)
        {
            targetText = GetComponentInChildren<TextMeshProUGUI>();
            if (targetText == null)
            {
                Debug.LogWarning("[PrefabTextFader] TextMeshProUGUI not assigned and not found in children!");
            }
        }
        if (targetGroup != null)
        {
            targetGroup.alpha = 0f;
        }
    }

    public void ShowPrefabText(string message)
    {
        Debug.Log($"[PrefabTextFader] ShowPrefabText called with message: {message}");
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInAndOut(message));
    }

    private IEnumerator FadeInAndOut(string message)
    {
        if (targetText == null)
        {
            Debug.LogWarning("[PrefabTextFader] targetText is null! Cannot show message.");
            yield break;
        }
        if (targetGroup == null)
        {
            Debug.LogWarning("[PrefabTextFader] targetGroup is null! Cannot fade panel.");
            yield break;
        }
        targetText.text = message;
        targetGroup.alpha = 1f; // Instantly show
        float startAlpha = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            targetGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        targetGroup.alpha = 0f;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (targetGroup == null) yield break;
        float startAlpha = targetGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            targetGroup.alpha = newAlpha;
            yield return null;
        }
        targetGroup.alpha = targetAlpha;
    }
}