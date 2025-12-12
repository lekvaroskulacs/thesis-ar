using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Mirror;
using Mirror.BouncyCastle.Crypto.Modes;
using Mirror.Examples.Basic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

internal enum BoardState
{
    NOT_READY, READY, REPLAY, PLAYING
}

public class Board : NetworkBehaviour
{
    [SerializeField] private TMP_Text hostPlayerHealth;
    [SerializeField] private TMP_Text guestPlayerHealth;
    private BoardTracker boardTracker;
    private BoardState state;
    private Action boardReady;


    [SerializeField] public Battlefield battlefield;
    private NetworkGamePlayer localPlayer;
    private NetworkGamePlayer opponentPlayer;

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

    private List<Creature> currentAttackers = new List<Creature>();


    public void SubscribeToBoardReady(Action callback)
    {
        boardReady += callback;
    }

    public List<CreatureField> GetCreatureFieldsForLocalPlayer()
    {
        return battlefield.FieldsOfPlayer(localPlayer);
    }

    void Start()
    {
        var players = GameObject.FindGameObjectsWithTag("NetworkGamePlayer");
        var player = players.Single(player => player.GetComponent<NetworkGamePlayer>().isLocalPlayer);
        localPlayer = player.GetComponent<NetworkGamePlayer>();
        localPlayer.board = this;
        opponentPlayer = players.Single(player => !player.GetComponent<NetworkGamePlayer>().isLocalPlayer).GetComponent<NetworkGamePlayer>();
        
        if (networkManager.currentScene == "ReplayScene")
        {
            return;
        }
        
        boardTracker = GameObject.FindWithTag("BoardTracker").GetComponent<BoardTracker>();
        boardTracker.board = this;

        var ids = new List<string>();
        foreach (var gameobj in boardTracker.allLoadedGameObjects)
        {
            var creatureId = gameobj.GetComponent<Creature>().creatureIdentifier;
            var prefab = CardCatalogue.GetPrefabForCard(creatureId);
            //NetworkClient.RegisterPrefab(prefab);
            playableCardGameObjects.Add(prefab);
            ids.Add(creatureId);
        }

    }

    void Update()
    {
        if (networkManager.currentScene == "ReplayScene")
        {
            if (localPlayer.isHost)
            {
                hostPlayerHealth.text = localPlayer.playerHealth.ToString();
                guestPlayerHealth.text = opponentPlayer.playerHealth.ToString();
            }
            else
            {
                guestPlayerHealth.text = localPlayer.playerHealth.ToString();
                hostPlayerHealth.text = opponentPlayer.playerHealth.ToString();
            }
            return;
        }

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
        if (networkManager.currentScene == "ReplayScene")
        {
            return;
        }

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
            foreach (var field in battlefield.FieldsOfPlayer(localPlayer))
            {
                if (field.IsGameObjectOnCreatureField(obj))
                {
                    slot = field;
                }
            }

            if (slot)
            {
                if (!slot.creature)
                {
                    PlayCreature(obj.GetComponent<Creature>(), battlefield.FieldsOfPlayer(localPlayer).IndexOf(slot));
                }
                else
                {
                    Debug.Log($"Creature already exists on slot {slot}");
                }
            }
        }

        if (localPlayer.isHost)
        {
            hostPlayerHealth.text = localPlayer.playerHealth.ToString();
            guestPlayerHealth.text = opponentPlayer.playerHealth.ToString();
        }
        else
        {
            guestPlayerHealth.text = localPlayer.playerHealth.ToString();
            hostPlayerHealth.text = opponentPlayer.playerHealth.ToString();
        }
    }

    void OnBoardReady()
    {
        playableCardGameObjects = boardTracker.allLoadedGameObjects;
        foreach (var field in battlefield.FieldsOfPlayer(localPlayer))
        {
            field.owningPlayer = localPlayer;
        }
        localPlayer.CmdBoardReady();
    }

    void PlayCreature(Creature creature, int creatureSlot)
    {
        localPlayer.RequestPlayCreature(creature, creatureSlot);
    }

    public void CreaturePlayed(bool hostSide, int creatureSlot, Creature creature)
    {
        Debug.Log($"Creature played: {creature} on hostside: {hostSide} at slot: {creatureSlot}");
        var field = hostSide ? battlefield.hostFields[creatureSlot] : battlefield.guestFields[creatureSlot];
        field.creature = creature;
        creature.transform.position = field.transform.position;
        creature.transform.rotation = field.transform.rotation;
        creature.transform.parent = field.transform;
/*
        if (battlefield.FieldsOfPlayer(localPlayer).Contains(field))
        {
            creature.owningPlayer = localPlayer;
        }
*/
    }

    public void CreatureDestroyed(bool hostSide, int creatureSlot)
    {
        var field = hostSide ? battlefield.hostFields[creatureSlot] : battlefield.guestFields[creatureSlot];
        field.creature = null;
    }

    public void NewTurn()
    {
        foreach (var field in battlefield.FieldsOfPlayer(localPlayer))
        {
            if (field.creature)
            {
                field.creature.RequestBeginTurn();
            }
        }
    }

    public void AttackCommenced(bool hostSide, List<uint> attackerIds)
    {
        foreach (var id in attackerIds)
        {
            var gameObject = NetworkClient.spawned[id].gameObject;
            var creature = gameObject.GetComponent<Creature>();
            if (!creature)
            {
                throw new ArgumentException("Received attacker IDs contain non-creature object!");
            }

            currentAttackers.Add(creature);
            creature.RequestConfirmAttack();
        }
    }

    public void BlockConfirmed(bool hostSide, List<uint> blockerIds)
    {
        foreach (var id in blockerIds)
        {
            var gameObject = NetworkClient.spawned[id].gameObject;
            var creature = gameObject.GetComponent<Creature>();
            if (!creature)
            {
                throw new ArgumentException("Received blocker IDs contain non-creature object!");
            }

            currentAttackers.Add(creature);
            creature.RequestConfirmBlock();
        }

        if (localPlayer.isHost == hostSide)
        {
            localPlayer.CmdResolveCombat();
        }
    }

    public void InitReplay()
    {
        var cardIds = ReplayLogger.LoadFromFile("game.replay").prefabsToLoad;
        foreach (var card in cardIds)
        {
            var prefab = CardCatalogue.GetPrefabForCard(card);
            NetworkClient.RegisterPrefab(prefab);
        }
        state = BoardState.REPLAY;

        localPlayer = networkManager.gamePlayers.host;
        localPlayer.board = this;
        networkManager.gamePlayers.Other(localPlayer).board = this;
    }

    public void UpdateCreatureLocations(List<uint> netIds)
    {
        var fields = battlefield.allFields;
        for (int i = 0; i < fields.Count; ++i)
        {
            Creature creature = null;
            if (netIds[i] != 0)
            {
                creature = NetworkClient.spawned[netIds[i]].gameObject.GetComponent<Creature>();
            }

            if (creature == null)
            {
                fields[i].creature = null;
                continue;
            }

/*
            CreatureField originalField = null;
            foreach (var field in fields)
            {
                if (field.creature == creature)
                {
                    originalField = field;
                }
            }

            if (originalField == null)
            {
                Debug.LogError("Original field not found when moving. This shouldn't happen");
            }
            originalField.creature = null;
*/
            fields[i].creature = creature;
            creature.transform.SetParent(fields[i].transform);
            creature.transform.position = fields[i].transform.position;
        }
    }
}