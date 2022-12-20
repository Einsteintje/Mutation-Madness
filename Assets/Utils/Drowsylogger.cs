using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Drowsylogger
{
    private static void DoLog(System.Action<string, Object> LogFunction, string prefix,Object myObj,  params object[] msg){
#if UNITY_EDITOR
        LogFunction($"{prefix}[<color=lightblue>{myObj.name}</color>]: {System.String.Join(" ; ", msg)}", myObj);
#endif
    }

    public static void Log(this Object myObj, params object[] msg){
        DoLog(Debug.Log, "",myObj,  msg);
        
    }
    public static void LogError(this Object myObj, params object[] msg){
        DoLog(Debug.LogError,"<color=red><!></color>",myObj,  msg);
    }

    public static void LogWarning(this Object myObj,params object[] msg){
        DoLog(Debug.LogWarning,"<color=yellow>⚠️</color>",myObj,  msg);
    }

    public static void LogSuccess(this Object myObj, params object[] msg){
        DoLog(Debug.Log,"<color=green>🙂</color>",myObj,  msg);
    }
}
