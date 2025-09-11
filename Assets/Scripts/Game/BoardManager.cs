using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{

    [Header("References")]
    public RectTransform boardContainer;
    public GridLayoutGroup gridLayout;
    public CardView cardPrefab;
    public GameConfig gameConfig;
    public List<CardData> availableCards;
    public static System.Action OnMatchFound;
    public static System.Action<int> OnTurnChanged;
    public static System.Action OnResetScore;
    public static System.Action<int> OnLevelUpdate;
    [Header("Grid Info ")]

    public int rows = 2;
    public int cols = 2;
    [Header("Game Rules")]
    public bool allowContinuousFlip = false;
    private Queue<(CardView, CardView)> checkQueue = new Queue<(CardView, CardView)>();
    private bool isProcessingQueue = false;

    private List<CardModel> activeCards = new List<CardModel>();
    private CardView firstSelected;
    private CardView secondSelected;
    private int matchesFound = 0;
    private int turnCount = 0;
    private int currentLevel = 1;
    private int currentCombo = 0;
    private int bestCombo = 0;

    private void Start()
    {
        StartLevel(1);
    }
    public void OnEnable()
    {
        CardEvents.CardFlipped += OnCardFlipped;
    }
    public void OnDisable()
    {
        CardEvents.CardFlipped -= OnCardFlipped;
        
    }
   
    public void StartLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, gameConfig.levels.Count);
        OnLevelUpdate?.Invoke(currentLevel);
        // fetch level config
        var config = gameConfig.levels[currentLevel - 1];
        rows = config.rows;
        cols = config.cols;

        GenerateBoard(rows, cols);
    }
    void GenerateBoard(int r,int c)
    {
        
        ClearBoard();

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        int totalCards = rows * cols;
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Board must have even number of cards");
            return;

        }
        if (availableCards.Count < totalCards / 2)
        {
            Debug.LogError($"Not enough unique cards! Need {totalCards / 2}, but only have {availableCards.Count}");
            return;
        }
        //Pick random pairs
        List<CardData> pool = new List<CardData>();
        while (pool.Count < totalCards / 2)
        {
            CardData random = availableCards[Random.Range(0, availableCards.Count)];
            if (!pool.Contains(random))
                pool.Add(random);
        }
        // duplicate to form pairs
        List<CardData> finalDeck = new List<CardData>();
        foreach (var card in pool)
        {
            finalDeck.Add(card);
            finalDeck.Add(card);
        }
        // shuffle
        for (int i = 0; i < finalDeck.Count; i++)
        {
            CardData temp = finalDeck[i];
            int randomNum = Random.Range(i, finalDeck.Count);
            finalDeck[i] = finalDeck[randomNum];
            finalDeck[randomNum] = temp;
        }
        // spawn
        for (int i = 0; i < finalDeck.Count; i++)
        {
            CardView card = Instantiate(cardPrefab, boardContainer);
            CardModel model = new CardModel(finalDeck[i].id, i);
            activeCards.Add(model);

            card.Bind(model, finalDeck[i].faceSprite);
        }
        AutoScaleCards();
    }
    private void ClearBoard()
    {
        foreach (Transform child in boardContainer)
        {
            Destroy(child.gameObject);
        }
        activeCards.Clear();
    }
    public void OnCardFlipped(CardModel model, CardView view)
    {
        if (model.isMatched) return; 
        if (view == firstSelected) return; 

       
        if (firstSelected == null)
        {
            firstSelected = view;
        }
        else
        {
           
            checkQueue.Enqueue((firstSelected, view));
            firstSelected = null;

            if (!isProcessingQueue)
                StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (checkQueue.Count > 0)
        {
            var (cardA, cardB) = checkQueue.Dequeue();

            yield return new WaitForSeconds(gameConfig.flipAnimationDuration);
            turnCount++;
            OnTurnChanged?.Invoke(turnCount);

            if (cardA.model.id == cardB.model.id)
            {
               
                cardA.model.isMatched = true;
                cardB.model.isMatched = true;
                matchesFound++;
                OnMatchFound?.Invoke();
                currentCombo++; // increase combo streak
                bestCombo = Mathf.Max(bestCombo, currentCombo);
                StartCoroutine(MatchAnimation(cardA.transform));
                StartCoroutine(MatchAnimation(cardB.transform));

                Debug.Log("Match found: " + cardA.model.id);
            }
            else
            {
                currentCombo = 0;
                UiManager.Instance.HideCombo();

                Debug.Log("Mismatch: " + cardA.model.id + " vs " + cardB.model.id);
                yield return StartCoroutine(MismatchAnimation(cardA.transform, cardB.transform));
                cardA.FlipDown();
                cardB.FlipDown();
            }

            if (matchesFound >= activeCards.Count / 2)
            {
                Debug.Log("Game Over! All matches found.");
                StopAllCoroutines();
                OnResetScore?.Invoke();
                EndLevel();
                ResetScore();
            }
        }

        isProcessingQueue = false;
    }
    private IEnumerator NextLevel()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(UIFadeUtility.FadeOut(UiManager.Instance. victoryPanel, 0.2f));
        StartLevel(currentLevel + 1);
    }
   void ResetScore()
    {
        turnCount = 0;
        matchesFound = 0;
    }
    private void EndLevel()
    {
        Debug.Log("Game Over! All matches found.");
        StartCoroutine(UIFadeUtility.FadeIn(UiManager.Instance.victoryPanel, 0.2f));
        int stars = CalculateStars(turnCount);
        Debug.Log("Level " + currentLevel + " completed with " + stars + " stars!");

        UiManager.Instance.ShowLevelComplete(stars, turnCount); // 🔔 notify UI

        StartCoroutine(NextLevel());
    }

    private int CalculateStars(int turns)
    {
        var config = gameConfig.levels[currentLevel - 1];

        int threeStar = Mathf.Min(config.threeStarTurns, config.twoStarTurns - 1);
        int twoStar = config.twoStarTurns;

        Debug.Log($"[StarCalc] Level {currentLevel} | Turns: {turns} | 3⭐ <= {threeStar} | 2⭐ <= {twoStar}");

        if (turns <= threeStar)
            return 3;
        else if (turns <= twoStar)
            return 2;
        else
            return 1;
    }


    private void AutoScaleCards()
    {
        Vector2 size = boardContainer.rect.size;
        float cellWidth = (size.x - (gridLayout.spacing.x * (cols - 1))) / cols;
        float cellHeight = (size.y - (gridLayout.spacing.y * (rows - 1))) / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }
    private IEnumerator MatchAnimation(Transform card)
    {
        Vector3 originalScale = card.localScale;

        // Pop effect
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            card.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            card.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t);
            yield return null;
        }

        // Idle pulse (small breathing effect for matched cards)
        while (true)
        {
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) / 2f; // 0 → 1
            float scale = Mathf.Lerp(1f, 0.8f, pulse);           // map into [1, 0.8]
            card.localScale = originalScale * scale;
            yield return null;
        }
    }

    // 🔹 Mismatch Animation (shake)
    private IEnumerator MismatchAnimation(Transform cardA, Transform cardB)
    {
        Vector3 posA = cardA.localPosition;
        Vector3 posB = cardB.localPosition;

        float shakeIntensity = 10f;
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(elapsed * 40f) * shakeIntensity;

            cardA.localPosition = posA + new Vector3(offset, 0, 0);
            cardB.localPosition = posB + new Vector3(-offset, 0, 0);

            yield return null;
        }

        cardA.localPosition = posA;
        cardB.localPosition = posB;
    }
}
