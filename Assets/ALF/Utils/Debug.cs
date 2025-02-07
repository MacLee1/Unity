
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngineInternal;

public static class Debug 
{
    public static bool isDebugBuild
    {
    get { return UnityEngine.Debug.isDebugBuild; }
    }

#if FTM_LIVE
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void Log (object message)
    {   
        UnityEngine.Debug.Log (message);
    }
#if FTM_LIVE
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void Log (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.Log (message, context);
    }
        
    // [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError (object message)
    {   
        UnityEngine.Debug.LogError (message);
    }

    // [System.Diagnostics.Conditional("UNITY_EDITOR")]	
    public static void LogError (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.LogError (message, context);
    }

#if FTM_LIVE
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void LogWarning (object message)
    {   
        UnityEngine.Debug.LogWarning (message.ToString ());
    }

#if FTM_LIVE
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void LogWarning (object message, UnityEngine.Object context)
    {   
        UnityEngine.Debug.LogWarning (message.ToString (), context);
    }

#if FTM_LIVE
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
    public static void LogWarningFormat (string format, params object[] args)
    {   
        UnityEngine.Debug.LogWarningFormat (format,args);
    }

    // [System.Diagnostics.Conditional("UNITY_EDITOR")] 
    // public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    // {
    // UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
    // } 
    
    // [System.Diagnostics.Conditional("UNITY_EDITOR")]
    // public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    // {
    // UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    // }
    
    // [System.Diagnostics.Conditional("UNITY_EDITOR")]
    // public static void Assert(bool condition)
    // {
    // if (!condition) throw new Exception();
    // }
}