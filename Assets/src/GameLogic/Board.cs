using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Mirror;
using Mirror.BouncyCastle.Crypto.Modes;
using UnityEditor;
using UnityEngine;

internal enum BoardState
{
    NOT_READY, READY, PLAYING
}

public class Board : NetworkBehaviour
{
    private BoardTracker boardTracker;
    private BoardState state;
    private Action boardReady;


    [SerializeField] private Battlefield battlefield;
    private NetworkPlayers<NetworkGamePlayer> players;


    public List<GameObject> playableCardGameObjects;

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


    public void SubscribeToBoardReady(Action callback)
    {
        boardReady += callback;
    }

    void Start()
    {
        boardTracker = GameObject.FindWithTag("BoardTracker").GetComponent<BoardTracker>();
        boardTracker.board = this;
        if (networkManager.players.data.Count == 2)
        {
            players = networkManager.gamePlayers;
        }
        else
        {
            throw new Exception("Players not connected yet!");
        }
    }

    void Update()
    {
        if (state == BoardState.NOT_READY)
        {
            UpdateNotReady();
        }
        else if (state == BoardState.READY)
        {
            UpdateReady();
        }
    }

    void UpdateNotReady()
    {
        if (boardTracker.GetTrackedObjectStatus() == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            Debug.Log("Board ready");
            state = BoardState.READY;
            OnBoardReady();
        }
    }

    void UpdateReady()
    {
        foreach (var obj in playableCardGameObjects)
        {
            CreatureField slot = null;
            foreach (var field in battlefield.allFields)
            {
                if (field.IsGameObjectOnCreatureField(obj))
                {
                    slot = field;
                }
            }

            if (slot)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    void OnBoardReady()
    {
        playableCardGameObjects = boardTracker.allLoadedGameObjects;
        foreach (var field in battlefield.hostFields)
        {
            field.owningPlayer = players.host;
            Debug.Log(field.owningPlayer);
        }
        foreach (var field in battlefield.guestFields)
        {
            field.owningPlayer = players.Other(players.host);
            Debug.Log(field.owningPlayer);
        }
    }

}