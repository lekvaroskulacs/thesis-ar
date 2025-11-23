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

        foreach (var evt in eventList.events)
        {
            var player = networkManager.gamePlayers.data[Int32.Parse(evt.connectionId)];

            ReplayHelper.ProcessCommand(evt);

            yield return new WaitForSeconds(1f);
        }
    }
}