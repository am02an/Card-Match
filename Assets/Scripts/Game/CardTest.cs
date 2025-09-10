using UnityEngine;

public class CardTest : MonoBehaviour
{
    public CardView cardView;
    public Sprite testSprite;

    void Start()
    {
        CardModel model = new CardModel("apple", 1);
        cardView.Bind(model, testSprite);

        CardEvents.CardFlipped += (m, v) => Debug.Log("Card flipped: " + m.id);
    }
}
