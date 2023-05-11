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
    [field: SerializeField] public Image BadgeImage { get; set; }
    [field: SerializeField] public TextMeshProUGUI ScoreTMP { get; set; }

    [Header("GAMES")]
    [SerializeField] private Sprite RacecarSprite;
    [SerializeField] private Sprite QuizSprite;

    [Header("BADGES")]
    [SerializeField] private Sprite BronzeBadge;
    [SerializeField] private Sprite SilverBadge;
    [SerializeField] private Sprite GoldBadge;
    //=========================================================================================================

    public void InitializeThisHistory(PlayerData.GameHistory gameHistory)
    {
        if(gameHistory.gameType == PlayerData.GameType.RACECAR)
        {
            GameImage.sprite = RacecarSprite;
        }
        else if (gameHistory.gameType == PlayerData.GameType.QUIZ)
        {
            GameImage.sprite = QuizSprite;
            GameImage.transform.localScale = new Vector3(2, 2, 2);
        }

        ResultTMP.text = gameHistory.gameResult.ToString();
        ScoreTMP.text = gameHistory.gameScore.ToString();

        if(gameHistory.gameScore >=0 && gameHistory.gameScore <= 8)
            BadgeImage.sprite = BronzeBadge;
        else if (gameHistory.gameScore >= 9 && gameHistory.gameScore <= 14)
            BadgeImage.sprite = SilverBadge;
        else
            BadgeImage.sprite = GoldBadge;
    }
}
