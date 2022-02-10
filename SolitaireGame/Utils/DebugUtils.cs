using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils : MonoBehaviour
{
    [Range(0,10)]
    public float timeScale = 1f;
    
    void Update()
    {
        Time.timeScale = timeScale;
    }

    public static void Log(string message)
    {
#if DEBUG
        Debug.Log("SOU: " + message);
#endif
    }

    public static void LogError(string message)
    {
#if DEBUG
        Debug.LogError("SOU: " + message);
#endif
    }
}
