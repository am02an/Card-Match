using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI matchCount;
    [SerializeField] private TextMeshProUGUI _turnCount;
    [SerializeField] private TextMeshProUGUI levelCount;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private Image[] starIcons;
    public CanvasGroup victoryPanel;

    [Header("FPS Setting")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f; // Update every half second
    #endregion

    #region Private Variables
    private int matches = 0;
    private Vector3 originalScale;

    private float accum = 0f;  // FPS accumulated over the interval
    private int frames = 0;    // Frames drawn over the interval
    private float timeLeft;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        if (matchCount != null)
            originalScale = matchCount.transform.localScale;
    }

    private void OnEnable()
    {
        BoardManager.OnMatchFound += HandleMatchFound;
        BoardManager.OnTurnChanged += HandleTurnChanged;
        BoardManager.OnResetScore += ResetScore;
        BoardManager.OnLevelUpdate += UpdateLevel;
    }

    private void OnDisable()
    {
        BoardManager.OnMatchFound -= HandleMatchFound;
        BoardManager.OnTurnChanged -= HandleTurnChanged;
        BoardManager.OnResetScore -= ResetScore;
        BoardManager.OnLevelUpdate -= UpdateLevel;
    }

    private void Update()
    {
        HandleFPSCounter();
    }
    #endregion

    #region UI Handlers
    private void HandleMatchFound()
    {
        matches++;
        matchCount.text = matches.ToString();
        StopAllCoroutines();
        StartCoroutine(PopEffect(matchCount.transform));
    }

    private void HandleTurnChanged(int turnCount)
    {
        _turnCount.text = turnCount.ToString();
        StartCoroutine(PopEffect(_turnCount.transform));
    }

    private void UpdateLevel(int _levelCount)
    {
        levelCount.text = "Level :" + " " + _levelCount.ToString();
    }

    private void ResetScore()
    {
        _turnCount.text = "0";
        matchCount.text = "0";
        matches = 0;
    }
    #endregion

    #region Combo System
    public void ShowCombo(int combo)
    {
        if (combo < 2) return; // show only for 2+ streak

        comboText.text = $" {combo}x Combo!";
        comboText.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ComboPopAnimation());
    }

    public void HideCombo()
    {
        comboText.gameObject.SetActive(false);
    }

    private IEnumerator ComboPopAnimation()
    {
        comboText.transform.localScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            comboText.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // fade out after delay
        float fadeTime = 0.5f;
        t = 0;
        Color startColor = comboText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (t < 1)
        {
            t += Time.deltaTime / fadeTime;
            comboText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        comboText.gameObject.SetActive(false);
        comboText.color = new Color(startColor.r, startColor.g, startColor.b, 1); // reset
    }
    #endregion

    #region Level Complete
    public void ShowLevelComplete(int stars, int turns)
    {
        Debug.Log("fadeout");
        StopAllCoroutines();
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].enabled = (i < stars);
            if (i < stars) StartCoroutine(StarPopAnimation(starIcons[i].transform));
        }
    }

    private IEnumerator StarPopAnimation(Transform star)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            star.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }
    #endregion

    #region Panel Controls
    public void ShowPanel()
    {
        StartCoroutine(UIFadeUtility.FadeIn(panel, 0.2f));
    }

    public void HidePanel()
    {
        StartCoroutine(UIFadeUtility.FadeOut(panel, 0.2f));
    }
    #endregion

    #region Effects
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
    #endregion

    #region FPS Counter
    private void HandleFPSCounter()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeLeft <= 0.0)
        {
            float fps = accum / frames;
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
    #endregion
}
