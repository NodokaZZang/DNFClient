using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExceptionLogger : MonoBehaviour
{
    System.IO.StreamWriter sw;

    public string LogFileName = "ErrorLog.txt";

    void Start()
    {
        sw = new System.IO.StreamWriter(Application.persistentDataPath + "/" + LogFileName);
        Debug.Log($"Error LogPath {Application.persistentDataPath}/{LogFileName}");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void OnDestroy()
    {
        sw.Close();
    }

    void HandleLog(string logString, string stackTrace, LogType type) 
    {
        //if (type == LogType.Exception || type == LogType.Error) 
        //{
        //    sw.WriteLine("Logged at: " + System.DateTime.Now.ToString() + " - Log : " + logString + " - Trace " + stackTrace + " - Type " + type.ToString());
        //}
    }
}
