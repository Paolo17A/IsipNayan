using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionData", menuName = "IsipNayan/Data/QuestionData")]

public class QuestionData : ScriptableObject
{
    [field: SerializeField] public string Question;
    [field: SerializeField] public List<string> Choices;
    [field: SerializeField] public string Answer;
}
