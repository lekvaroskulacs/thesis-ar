using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using System.Threading;

[Serializable]
public class ReplayEvent
{
    public string commandName;
    public float timestamp;
    public string connectionId;
    public uint netId;
    public Dictionary<string, object> parameters;
}

public static class ReplayLogger
{
    private static ReplayEventList eventList = new ReplayEventList();

    public static void LogPrefabsToLoad(List<string> prefabs)
    {
        eventList.prefabsToLoad = prefabs;
    }
    
    public static void LogCommand(string commandName, int connectionId, uint netId, Dictionary<string, object> parameters)
    {
        eventList.events.Add(new ReplayEvent
        {
            commandName = commandName,
            timestamp = Time.time,
            connectionId = connectionId.ToString(),
            netId = netId,
            parameters = parameters
        });
    }

    public static void SaveToFile(string fileName)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        string path = Path.Combine(Application.persistentDataPath, fileName);
    #else
        string path = fileName;
    #endif
        string json = JsonConvert.SerializeObject(eventList, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static ReplayEventList LoadFromFile(string fileName)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        string path = Path.Combine(Application.persistentDataPath, fileName);
    #else
        string path = fileName;
    #endif
        if (!File.Exists(path))
        {
            Debug.LogError("Replay file not found: " + path);
            return null;
        }
        var str = File.ReadAllText(path);
        ReplayEventList replayEvents = JsonConvert.DeserializeObject<ReplayEventList>(str);
        return replayEvents;
    }

    [Serializable]
    public class ReplayEventList
    {
        public List<string> prefabsToLoad = new List<string>();
        public List<ReplayEvent> events = new List<ReplayEvent>();
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class LogReplayAttribute : Attribute { }

public  class ReplayHelper : MonoBehaviour
{
   

    public static void LogCommandAuto(string commandName, object instance, params object[] args)
    {
        var parameters = new Dictionary<string, object>();
        var method = instance.GetType().GetMethod(commandName);
        var paramInfos = method.GetParameters();
        for (int i = 0; i < paramInfos.Length; i++)
        {
            parameters[paramInfos[i].Name] = args[i];
        }
        ReplayLogger.LogCommand(commandName, 
            (instance as NetworkBehaviour)?.connectionToClient?.connectionId ?? -1, 
            (instance as NetworkBehaviour)?.netId ?? 0,
            parameters);
    }

    
}