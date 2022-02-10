using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerHelper : MonoBehaviour
{
    void Start()
    {
        var res = Screen.currentResolution;
        var ratio = (float)res.height / res.width;
        var factor = ratio >= (16f/9f) ? 0f : 1f;
        GetComponent<CanvasScaler>().matchWidthOrHeight = factor;
    }
}
