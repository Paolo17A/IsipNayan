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

    public void ResetPlayerData()
    {
        Debug.Log("resetting all data");
        PlayFabID = "";
        Username = "";
        EmailAddress = "";
    }
}
