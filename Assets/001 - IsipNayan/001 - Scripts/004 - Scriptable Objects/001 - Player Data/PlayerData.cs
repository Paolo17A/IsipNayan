using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "IsipNayan/Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    [field: Header("PLAYFAB VARIABLES")]
    [field: SerializeField] public string PlayFabID { get; set; }
    [field: SerializeField] public string Username { get; set; }
    [field: SerializeField] public string EmailAddress { get; set; }

    [field: SerializeField] public List<GameHistory> PlayerHistory { get; set; }
    public void ResetPlayerData()
    {
        PlayFabID = "";
        Username = "";
        EmailAddress = "";
        PlayerHistory.Clear();
    }

    #region GAME HISTORY
    public enum GameType { NONE, RACECAR, QUIZ}
    [Serializable]
    public class GameHistory
    {
        [SerializeField] public GameType gameType;
        [SerializeField] public GameManager.Result gameResult;
        [SerializeField] public int gameScore;
    }

    public void AddGameHistory(GameType _type, GameManager.Result _result, int _score)
    {
        GameHistory history = new GameHistory();
        history.gameType = _type;
        history.gameResult = _result;
        history.gameScore = _score;

        PlayerHistory.Add(history);
    }
    #endregion
}
