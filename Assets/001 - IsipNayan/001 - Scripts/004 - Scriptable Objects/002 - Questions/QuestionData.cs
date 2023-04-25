using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionData", menuName = "IsipNayan/Data/QuestionData")]

public class QuestionData : ScriptableObject
{
    [field: Header("RACECAR GAME")]
    [field: SerializeField][field: TextArea(minLines: 10, maxLines: 20)] public string Question;

    [field: Header("QUIZ GAME")]
    [field: SerializeField] public List<Multiply> MultiplicationQuestion;

    [field: Header("UNIVERSAL")]
    [field: SerializeField] public List<string> Choices;
    [field: SerializeField] public string Answer;

    

    [Serializable]
    public struct Multiply
    {
        public int multipilicand;
        public int multiplier;
    }
}
