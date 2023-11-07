using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasUICameraSetter : MonoBehaviour
{
    public Canvas canvas;

    public bool isMainCamera;

    private void OnEnable()
    {
        if (!isMainCamera)
            canvas.worldCamera = GameManager.Instance.MyUICamera;
        else
            canvas.worldCamera = GameManager.Instance.MainCamera;
    }
}
