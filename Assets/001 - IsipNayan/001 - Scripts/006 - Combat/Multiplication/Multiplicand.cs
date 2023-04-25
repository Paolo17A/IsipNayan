using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Multiplicand : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MultiplicandTMP;
    
    public void SetMultiplicand(int multiplicand)
    {
        MultiplicandTMP.text = multiplicand.ToString();
    }
}
