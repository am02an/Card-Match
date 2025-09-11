using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SaveManager handles saving/loading player progress (name, level, coins, lives)
/// using JSON stored in PlayerPrefs.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SAVE_KEY = "PLAYER_DATA";

    public PlayerData playerData;
    public delegate void OnDataChanged();
    public static event OnDataChanged DataChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        
            LoadData(); // Load when game starts
    }

    /// <summary>
    /// Save player data to PlayerPrefs as JSON.
    /// </summary>
    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        DataChanged?.Invoke(); // Notify UI
        Debug.Log($"[SaveManager] Data Saved: {json}");
    }


    /// <summary>
    /// Load player data from PlayerPrefs, or create a new save if none exists.
    /// </summary>
    public void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"[SaveManager] Data Loaded: {json}");
            UiManager.Instance.UpdatePlayerUI();
        }
        else
        {
            Debug.Log("[SaveManager] No Save Found, Creating New Data...");
            playerData = new PlayerData
            {
                playerName = "Player"+"_"+Random.Range(99,999),
                currentLevel = 1,
                coins = 0,
            };
            SaveData();
        }
    }

    /// <summary>
    /// Clears saved data (useful for debugging).
    /// </summary>
    public void ClearData()
    {
        playerData.currentLevel = 1;
        playerData.coins = 0;
        playerData.allTurn = 0;           

        SaveData();
        Debug.Log("[SaveManager] Data Cleared!");
    }
}

/// <summary>
/// Serializable PlayerData class for easy JSON save/load.
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int currentLevel;
    public int coins;
    public int allTurn;
}
