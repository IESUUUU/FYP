using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ARTutorialPanel : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float slideDistance = 100f;
    
    [Header("Panel Settings")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine currentAnimation;

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Initialize panel state
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        // Stop any running animations
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        gameObject.SetActive(true);
        currentAnimation = StartCoroutine(ShowAnimation());
    }

    public void Hide()
    {
        // Stop any running animations
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(HideAnimation());
    }

    private IEnumerator ShowAnimation()
    {
        // Reset position
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 hiddenPos = new Vector2(startPos.x, startPos.y - slideDistance);
        panelRect.anchoredPosition = hiddenPos;

        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;

            // Smooth step interpolation for better animation feel
            float smoothT = t * t * (3f - 2f * t);

            // Animate position and fade
            canvasGroup.alpha = smoothT;
            panelRect.anchoredPosition = Vector2.Lerp(hiddenPos, startPos, smoothT);

            yield return null;
        }

        // Ensure we end up at exact values
        canvasGroup.alpha = 1f;
        panelRect.anchoredPosition = startPos;
    }

    private IEnumerator HideAnimation()
    {
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 hiddenPos = new Vector2(startPos.x, startPos.y - slideDistance);
        float startAlpha = canvasGroup.alpha;

        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;

            // Smooth step interpolation for better animation feel
            float smoothT = t * t * (3f - 2f * t);

            // Animate position and fade
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, smoothT);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, hiddenPos, smoothT);

            yield return null;
        }

        // Ensure we end up at exact values
        canvasGroup.alpha = 0f;
        panelRect.anchoredPosition = hiddenPos;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
    }
} 