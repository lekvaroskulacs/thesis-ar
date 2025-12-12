using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using static ReplayLogger;

public class ReplayManager : MonoBehaviour
{
    [SerializeField] private Board board;
    private ReplayEventList eventList;
    private NetworkManagerImpl _networkManager;
    private NetworkManagerImpl networkManager
    {
        get
        {
            if (_networkManager != null)
            {
                return _networkManager;
            }
            return _networkManager = NetworkManager.singleton as NetworkManagerImpl;
        }
    }

    void Start()
    {
        eventList = ReplayLogger.LoadFromFile("game.replay");
        if (eventList.events.Count == 0)
        {
            Debug.LogError("Replay is empty");
            return;
        }
        StartCoroutine(ReplayCoroutine());
    }

    private IEnumerator ReplayCoroutine()
    {
        yield return new WaitUntil(() => networkManager.gamePlayers.data.Count == 2);
        board.InitReplay();
        networkManager.InitReplay(networkManager.gamePlayers.data[Int32.Parse(eventList.events[0].connectionId)]);

        var obj = new GameObject();
        obj.AddComponent<ReplayProcessor>();
        obj.AddComponent<NetworkIdentity>();
        var processor = obj.GetComponent<ReplayProcessor>();
        NetworkServer.Spawn(obj);

        foreach (var evt in eventList.events)
        {
            yield return new WaitUntil(() => processor.canExecuteNext);
            processor.ProcessCommand(evt, true);

            yield return new WaitForSeconds(1f);
        }
    }
}