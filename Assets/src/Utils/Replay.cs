using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Newtonsoft.Json;

[Serializable]
public class ReplayEvent
{
    public string commandName;
    public float timestamp;
    public string connectionId;
    public Dictionary<string, object> parameters;
}

public static class ReplayLogger
{
    private static List<ReplayEvent> events = new List<ReplayEvent>();

    public static void LogCommand(string commandName, int connectionId, Dictionary<string, object> parameters)
    {
        events.Add(new ReplayEvent
        {
            commandName = commandName,
            timestamp = Time.time,
            connectionId = connectionId.ToString(),
            parameters = parameters
        });
    }

    public static void SaveToFile(string path)
    {
        string json = JsonConvert.SerializeObject(new ReplayEventList { events = events }, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    [Serializable]
    private class ReplayEventList
    {
        public List<ReplayEvent> events;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class LogReplayAttribute : Attribute { }

public static class ReplayHelper
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
            parameters);
    }
}