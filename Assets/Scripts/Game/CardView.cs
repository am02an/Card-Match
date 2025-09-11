using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    #region Variables
    [Header("Reference")]
    public Image frontImage;
    public Image backImage;
    public Button button;

    public CardModel model;
    private bool isAnimating = false;

    [Header("Rotation Settings")]
    public Transform backFaceRotate;
    public Vector3 rotationAxis = Vector3.right; // Default = X axis
    public float rotationSpeed = 90f;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        button.onClick.AddListener(OnCardClicked);
    }
    private void Start()
    {
        button.onClick.AddListener(() => AudioManager.Instance.PlaySFX("Flip"));

    }
    private void Update()
    {
        RotateBackFace();
    }
    #endregion

    #region Public Methods
    public void Bind(CardModel cardModel, Sprite faceSprite)
    {
        model = cardModel;
        frontImage.sprite = faceSprite;
    }

    public IEnumerator PlayFlipAnimation(bool faceUp, float duration = 0.25f)
    {
        isAnimating = true;
        float time = 0;
        Quaternion startRot = transform.rotation;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.identity;

        // Rotate to halfway point
        while (time < duration / 2f)
        {
            transform.rotation = Quaternion.Slerp(startRot, midRot, time / (duration / 2f));
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = midRot;

        // Flip card state and update visuals
        model.isFaceUp = faceUp;
        UpdateVisual();

        // Rotate to final point
        time = 0f;
        while (time < duration / 2f)
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

        if (model != null)
        {
            model.isFaceUp = false;
            model.isMatched = false;
        }

        UpdateVisual();
        gameObject.SetActive(true);
    }
    #endregion

    #region Private Methods
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

    private void RotateBackFace()
    {
        // Rotate around the chosen axis
        backFaceRotate.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
    #endregion
}
