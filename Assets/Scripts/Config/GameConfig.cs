
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "CardMatch/Game Config", fileName = "GameConfig")]

public class GameConfig : ScriptableObject
{

    [Header("Gameplay")]
    public float flipAnimationDuration = 0.25f;
    public float mismatchRevealDuration = 0.5f;

    [Header("Scoring")]
    public int basePointPerMatch = 100;
    public float comboMutlplier = 0.2f;
    public float comboWindoSeconds = 3f;

    [Header("Layout")]
    public Vector2 cardSpacing = new Vector2(10f, 10f);

    [Header("Levels")]
    public List<LevelConfig> levels;
}
[System.Serializable]
public class LevelConfig
{
    public int levelNumber;
    public int rows;
    public int cols;
    [Header("Star Thresholds")]
    public int threeStarTurns; 
    public int twoStarTurns;
}
