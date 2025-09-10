using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI matchCount;
    [SerializeField] private TextMeshProUGUI turnCount;
    [SerializeField] private CanvasGroup panel;

    private int matches = 0;
    private Vector3 originalScale;

    private void Awake()
    {
        if (matchCount != null)
            originalScale = matchCount.transform.localScale;
    }
    private void OnEnable()
    {
        BoardManager.OnMatchFound += HandleMatchFound;
    }

    private void OnDisable()
    {
        BoardManager.OnMatchFound -= HandleMatchFound;
    }

    private void HandleMatchFound()
    {
        matches++;
        matchCount.text = matches.ToString();
        StopAllCoroutines();
        StartCoroutine(PopEffect(matchCount.transform));
    }

    public void ShowPanel()
    {
        StartCoroutine(UIFadeUtility.FadeIn(panel, 0.2f)); 
    }

    public void HidePanel()
    {
        StartCoroutine(UIFadeUtility.FadeOut(panel, 0.2f));
    }
    private IEnumerator PopEffect(Transform target)
    {
        Vector3 bigScale = originalScale * 1.3f;

        // Scale up
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            target.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }

        // Scale back down
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            target.localScale = Vector3.Lerp(bigScale, originalScale, t);
            yield return null;
        }
    }
}
