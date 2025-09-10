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
    [Header("References")]

    public int rows = 2;
    public int cols = 2;

    private List<CardModel> activeCards = new List<CardModel>();
    private void Start()
    {
        GenerateBoard(rows,cols);
    }

    void GenerateBoard(int r, int c)
    {
        rows = r;
        cols = c;
        ClearBoard();

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        int totalCards = rows * cols;
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Board must have even number of cards");
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

    private void AutoScaleCards()
    {
        Vector2 size = boardContainer.rect.size;
        float cellWidth = (size.x - (gridLayout.spacing.x * (cols - 1))) / cols;
        float cellHeight = (size.y - (gridLayout.spacing.y * (rows - 1))) / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }
}
