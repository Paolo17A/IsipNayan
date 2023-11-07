using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissIndicatorController : MonoBehaviour
{
    private void Awake()
    {
    }

    public void PlayAnimation(float pos)
    {
        LeanTween.moveY(gameObject, pos, 1f).setEase(LeanTweenType.easeInOutCirc).setOnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void DestroyMe() => Destroy(gameObject);
}
