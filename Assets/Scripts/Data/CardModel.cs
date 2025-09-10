using System;

[System.Serializable]
public class CardModel
{
    public string id;                 
    public int uniqueInstanceId;      
    public bool isMatched;             
    public bool isFaceUp;            

    public CardModel(string id, int instanceId)
    {
        this.id = id;
        this.uniqueInstanceId = instanceId;
        this.isMatched = false;
        this.isFaceUp = false;
    }
}
