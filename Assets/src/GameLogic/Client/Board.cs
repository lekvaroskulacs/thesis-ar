using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Mirror;
using Mirror.BouncyCastle.Crypto.Modes;
using Mirror.Examples.Basic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
    private NetworkGamePlayer localPlayer;

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
        boardTracker = GameObject.FindWithTag("BoardTracker").GetComponent<BoardTracker>();
        boardTracker.board = this;
        foreach (var gameobj in boardTracker.allLoadedGameObjects)
        {
            var prefab = CardCatalogue.GetPrefabForCard(gameobj.GetComponent<Creature>().creatureIdentifier);
            NetworkClient.RegisterPrefab(prefab);
            playableCardGameObjects.Add(prefab);
        }

        var players = GameObject.FindGameObjectsWithTag("NetworkGamePlayer");
        var player = players.Single(player => player.GetComponent<NetworkGamePlayer>().isLocalPlayer);
        localPlayer = player.GetComponent<NetworkGamePlayer>();
        localPlayer.board = this;
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
    }

    void OnBoardReady()
    {
        playableCardGameObjects = boardTracker.allLoadedGameObjects;
        foreach (var field in battlefield.FieldsOfPlayer(localPlayer))
        {
            field.owningPlayer = localPlayer;
        }
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

        if (battlefield.FieldsOfPlayer(localPlayer).Contains(field))
        {
            creature.owningPlayer = localPlayer;
        }
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

    

}