using System;
using System.Collections.Generic;
using Mirror;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    [SyncVar] public TurnState state;
    [SyncVar] public int mana;
    [SyncVar] public int maxMana;
    [SyncVar] public int playerHealth = 20;
    [SyncVar] public bool isHost;
    [SyncVar] public HashSet<Creature> attackingCreatures = new HashSet<Creature>();
    [SyncVar] public HashSet<Creature> blockingCreatures = new HashSet<Creature>();
    public Board board;
    NetworkManagerImpl _networkManager;
    NetworkManagerImpl networkManager
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

    public override void OnStartLocalPlayer()
    {
        var hud = FindFirstObjectByType<HUD>();
        if (hud == null)
        {
            throw new Exception("HUD doesn't exist for this player");
        }
        hud.RegisterNetworkPlayer(this);
        isHost = NetworkServer.connections.Count > 0;
    }

    void Update()
    {
        if (state == TurnState.PLAYING && attackingCreatures.Count > 0)
        {
            CmdChangeState(TurnState.ATTACKING);
        }

        if (state == TurnState.ATTACKING && attackingCreatures.Count == 0)
        {
            CmdChangeState(TurnState.PLAYING);
        }
    }

    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        //die
    }

    [TargetRpc]
    public void RpcStartTurn(NetworkConnectionToClient target)
    {
        Debug.Log($"StartTurn on {name}, state: {state}");
        //state = TurnState.PLAYING;
        if (!board)
        {
            return;
        }
        board.NewTurn();
    }

    [TargetRpc]
    public void RpcStartGameAsSecond(NetworkConnectionToClient target)
    {
        Debug.Log($"StartAsSecond on {name}, state: {state}");
        //state = TurnState.OPPONENT_TURN;
    }

    [TargetRpc]
    public void RpcEndTurn(NetworkConnectionToClient target)
    {
        state = TurnState.OPPONENT_TURN;
        
    }

    [TargetRpc]
    public void RpcCreaturePlayed(bool hostSide, int creatureSlot, uint netId)
    {
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        var creatureObj = NetworkClient.spawned[netId].gameObject;
        var creature = creatureObj.GetComponent<Creature>();    
        board.CreaturePlayed(hostSide, creatureSlot, creature);
    }

    [TargetRpc]
    public void RpcCreatureDestroyed(bool hostSide, int creatureSlot)
    {
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.CreatureDestroyed(hostSide, creatureSlot);
    }

    [TargetRpc]
    public void RpcAttackCommenced(bool hostSide, List<uint> netIds)
    {
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.AttackCommenced(hostSide, netIds);
    }

    [TargetRpc]
    public void RpcBlockConfirmed(bool hostSide, List<uint> netIds)
    {
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.BlockConfirmed(hostSide, netIds);
    }

    public void RequestEndTurn()
    {
        if (state != TurnState.PLAYING && state != TurnState.PLAYING_AFTER_ATTACK)
        {
            Debug.Log($"Ending turn is not possible in state {state}");
            return;
        }
        CmdEndTurn();
    }

    public void RequestPlayCreature(Creature creature, int creatureSlot)
    {
        if (creature.manaCost > mana)
        {
            Debug.Log("Not enough mana!");
            return;
        }
        CmdPlayCreature(creature.creatureIdentifier, creature.manaCost, creatureSlot);
    }

    public void RequestAttack()
    {
        List<uint> attackers = new List<uint>();
        foreach (var creature in attackingCreatures)
        {
            attackers.Add(creature.netId);
        }
        CmdCommenceAttack(attackers);
    }

    public void RequestBlock()
    {
        List<uint> blockers = new List<uint>();
        foreach (var creature in blockingCreatures)
        {
            blockers.Add(creature.netId);
        }
        CmdConfirmBlock(blockers);
    }

    public void RequestResolveCombat()
    {
        CmdResolveCombat();
    }

    [Command][LogReplay]
    public void CmdChangeState(TurnState state)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdChangeState), this, state);
        // It would be great to have some validation e.g. you can only go to ATTACK from PLAYING
        this.state = state;
    }
    
    [Command][LogReplay]
    public void CmdEndTurn()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdEndTurn), this);
        networkManager.turnManager.EndTurn(this);
    }

    [Command][LogReplay]
    public void CmdPlayCreature(string creatureIdentifier, int manaCost, int creatureSlot)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdPlayCreature), this, creatureIdentifier, manaCost, creatureSlot);
        if (manaCost > mana)
        {
            Debug.LogError("PlayCreature called without enough mana");
            return;
        }
        mana -= manaCost;
        networkManager.serverBoard.CreaturePlayed(this, creatureSlot, creatureIdentifier);
    }

    /*
        [Command]
        public void CmdToggleAttacking(Creature creature)
        {

        }
    */
    [Command][LogReplay]
    public void CmdCommenceAttack(List<uint> creatureNetIds)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdCommenceAttack), this, creatureNetIds);
        networkManager.serverBoard.AttackByPlayer(this, creatureNetIds);
        networkManager.turnManager.ConfirmAttackers(this);
    }

    [Command][LogReplay]
    public void CmdConfirmBlock(List<uint> creatureNetIds)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdConfirmBlock), this, creatureNetIds);
        networkManager.serverBoard.BlockByPlayer(this, creatureNetIds);
        networkManager.turnManager.ConfirmBlockers(this);
    }

    [Command][LogReplay]
    public void CmdResolveCombat()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdResolveCombat), this);
        networkManager.serverBoard.ResolveCombat(this);
    }
/*
    [Command]
    public void CmdToggleBlocking(Creature creature)
    {

    }
    */
}