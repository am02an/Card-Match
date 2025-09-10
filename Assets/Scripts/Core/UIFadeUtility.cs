using System.Collections;
using UnityEngine;

public static class UIFadeUtility
{
    public static IEnumerator Fade(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        canvasGroup.interactable = targetAlpha > 0.95f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.95f;
    }

    public static IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        yield return Fade(canvasGroup, 1f, duration);
    }

    public static IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        yield return Fade(canvasGroup, 0f, duration);
    }
}
