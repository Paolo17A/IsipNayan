using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryPanelData", menuName = "IsipNayan/Data/StoryPanelData")]
public class StoryPanelData : ScriptableObject
{
    [field: SerializeField] public Sprite StorySprite { get; set; }
    [field: SerializeField][field: TextArea] public string StoryText { get; set; }
}
