using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GasAnswerHandler : MonoBehaviour
{
    //===========================================================================================================
    [SerializeField] private RacecarCore RacecarCore;
    [SerializeField] private DriverCore DriverCore;

    [Header("ANSWER VARIABLES")]
    [SerializeField] private TextMeshPro AnswerTMP;
    [field: SerializeField][field: ReadOnly] public bool IsCorrectAnswer { get; set; }
    //===========================================================================================================

    public void AssignAnswer(string answer)
    {
        AnswerTMP.text = int.Parse(answer).ToString("n0");
    }

    public void ProcessAnswer()
    {
        if (IsCorrectAnswer)
        {
            DriverCore.ProcessCorrectAnswer();
        }
        else
        {
            DriverCore.ProcessIncorrectAnswer();
        }
        RacecarCore.ToggleGasAnswers(false);
        RacecarCore.WillResetQuestion = true;
        //RacecarCore.HideAllGasAnswers();
    }
}
