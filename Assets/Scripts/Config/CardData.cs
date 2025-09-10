using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu (menuName ="CardMatch/Card Data",fileName ="NewCardData")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string id;
    public Sprite faceSprite;
}
