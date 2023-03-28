using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryDataHandler : MonoBehaviour
{
    //=========================================================================================================
    [field: Header("HISTORY")]
    [field: SerializeField] public Image GameImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI ResultTMP { get; set; }
    [field: SerializeField] public TextMeshProUGUI ScoreTMP { get; set; }

    [Header("GAMES")]
    [SerializeField] private Sprite RacecarSprite;
    [SerializeField] private Sprite QuizSprite;
    //=========================================================================================================

    public void InitializeThisHistory(PlayerData.GameHistory gameHistory)
    {
        if(gameHistory.gameType == PlayerData.GameType.RACECAR)
            GameImage.sprite = RacecarSprite;
        else if (gameHistory.gameType == PlayerData.GameType.QUIZ)
            GameImage.sprite = QuizSprite;

        ResultTMP.text = gameHistory.gameResult.ToString();
        ScoreTMP.text = gameHistory.gameScore.ToString();
    }
}
