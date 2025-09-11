using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{

    [Header("Reference")]
    public Image frontImage;
    public Image backImage;
    public Button button;

    public CardModel model;
    private bool isAnimating = false;

    private void Awake()
    {
        button.onClick.AddListener(OnCardClicked);
    }
    public void Bind (CardModel cardModel, Sprite faceSprite)
    {
        model = cardModel;
        frontImage.sprite = faceSprite;
    }
    private void OnCardClicked()
    {
        if (isAnimating || model.isMatched) return;
        StartCoroutine(PlayFlipAnimation(!model.isFaceUp));
        CardEvents.CardFlipped?.Invoke(model, this);

    }
    private void UpdateVisual()
    {
        frontImage.gameObject.SetActive(model.isFaceUp);
        backImage.gameObject.SetActive(!model.isFaceUp && !model.isMatched);
        gameObject.SetActive(!model.isMatched);
    }
    public IEnumerator PlayFlipAnimation(bool faceUp,float duration=0.25f)
    {
        isAnimating = true;
        float time = 0;
        Quaternion StartRot = transform.rotation;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.identity;

        while(time<duration/2f)
        {
            transform.rotation = Quaternion.Slerp(StartRot, midRot, time / (duration / 2f));
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = midRot;

        model.isFaceUp = faceUp;
        //UpdateVisual
        UpdateVisual();
        time = 0f;
        while(time<duration/2f)
        {
            transform.rotation = Quaternion.Slerp(midRot, endRot, time / (duration / 2f));

            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
        isAnimating = false;
    }
    public void FlipDown()
    {
        StartCoroutine(PlayFlipAnimation(false));
    }
    public void ResetCard()
    {
        StopAllCoroutines();
        isAnimating = false;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        if(model!=null)
        {
            model.isFaceUp = false;
            model.isMatched = false;
        }
        UpdateVisual();
        gameObject.SetActive(true);
    }
}
