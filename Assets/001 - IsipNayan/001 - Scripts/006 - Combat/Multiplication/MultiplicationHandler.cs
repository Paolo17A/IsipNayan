using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplicationHandler : MonoBehaviour
{
    [SerializeField] List<Multiplicand> ThisMultiplicands;

    public void DisplayProperMultiplicands(int multiplicand, int multiplier)
    {
        for(int i = 0; i < ThisMultiplicands.Count; i++)
        {
            if (i < multiplier)
            {
                ThisMultiplicands[i].gameObject.SetActive(true);
                ThisMultiplicands[i].SetMultiplicand(multiplicand);
            }
            else
                ThisMultiplicands[i].gameObject.SetActive(false);
        }
    }
}
