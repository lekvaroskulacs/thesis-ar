using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using System.Linq;

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

    public static void SaveToFile(string path)
    {
        string json = JsonConvert.SerializeObject(eventList, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static ReplayEventList LoadFromFile(string path)
    {
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
            (instance as NetworkBehaviour)?.netId ?? 0,
            parameters);
    }

    public static void ProcessCommand(ReplayEvent evt)
    {   
        NetworkGamePlayer player = NetworkServer.spawned[evt.netId].GetComponent<NetworkGamePlayer>();
        if (evt.commandName == "CmdPlayCreature")
        {
            player.PlayCreature(
                evt.parameters["creatureIdentifier"].ToString(),
                Convert.ToInt32(evt.parameters["manaCost"]),
                Convert.ToInt32(evt.parameters["creatureSlot"])
            );
        }
        
        if (evt.commandName == "CmdEndTurn")
        {
            player.EndTurn();
        }

        if (evt.commandName == "CmdChangeState")
        {
            player.ChangeState(
                (TurnState) Convert.ToInt32(evt.parameters["state"])
            );
        }

        if (evt.commandName == "CmdCommenceAttack")
        {
            player.CommenceAttack(
                (evt.parameters["creatureNetIds"] as JArray).ToObject<List<uint>>()
            );
        }

        if (evt.commandName == "CmdConfirmBlock")
        {
           player.ConfirmBlock(
                (evt.parameters["creatureNetIds"] as JArray).ToObject<List<uint>>()
            );
        }

        if (evt.commandName == "CmdResolveCombat")
        {
           player.ResolveCombat();
        }

        Creature cr = NetworkServer.spawned[evt.netId].GetComponent<Creature>();
        if (evt.commandName == "CmdToggleCanAttack")
        {
            cr.CmdToggleCanAttack(
                (bool) evt.parameters["canAttack"]
            );
        }

        if (evt.commandName == "CmdToggleAttack")
        {
            cr.CmdToggleAttack();
        }

        if (evt.commandName == "CmdToggleCanBlock")
        {
            cr.CmdToggleAttack();
        }

        if (evt.commandName == "CmdConfirmAttack")
        {
            cr.CmdConfirmAttack();
        }

        if (evt.commandName == "CmdConfirmBlock")
        {
            cr.CmdConfirmBlock();
        }

        if (evt.commandName == "CmdResetCombatState")
        {
            cr.CmdResetCombatState();
        }
    }
}