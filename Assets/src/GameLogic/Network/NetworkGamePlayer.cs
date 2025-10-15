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
    [SyncVar] public HashSet<Creature> attackingCreatures = new HashSet<Creature>();
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

    [Command]
    public void CmdChangeState(TurnState state)
    {
        // It would be great to have some validation e.g. you can only go to ATTACK from PLAYING
        this.state = state;
    }
    
    [Command]
    public void CmdEndTurn()
    {
        networkManager.turnManager.EndTurn(this);
    }

    [Command]
    public void CmdPlayCreature(string creatureIdentifier, int manaCost, int creatureSlot)
    {
        if (manaCost > mana)
        {
            throw new UnauthorizedAccessException("This shouldn't have been called.");
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
    [Command]
    public void CmdCommenceAttack(List<uint> creatureNetIds)
    {
        networkManager.serverBoard.AttackByPlayer(this, creatureNetIds);
        networkManager.turnManager.ConfirmAttackers(this);
    }
/*
    [Command]
    public void CmdToggleBlocking(Creature creature)
    {

    }
    */
}