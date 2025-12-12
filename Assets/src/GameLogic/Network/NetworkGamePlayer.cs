using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mirror;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    [SyncVar] public TurnState state;
    [SyncVar] public int mana;
    [SyncVar] public int maxMana;
    [SyncVar] public int playerHealth;
    [SyncVar] public bool isHost;
    [SyncVar] public HashSet<Creature> attackingCreatures = new HashSet<Creature>();
    [SyncVar] public HashSet<Creature> blockingCreatures = new HashSet<Creature>();
    public Board board;
    HUD hud;
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
        hud = FindFirstObjectByType<HUD>();
        if (hud == null && networkManager.currentScene != "ReplayScene")
        {
            throw new Exception("HUD doesn't exist for this player");
        }
        hud?.RegisterNetworkPlayer(this);
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
        if (playerHealth <= 0)
        {
            LoseGame();
        }
    }

    public void LoseGame()
    {
        networkManager.EndGame(this);
    }


    [TargetRpc]
    public void RpcRegisterCardPrefabs(List<string> cardIds)
    {
        foreach (var id in cardIds)
        {
            NetworkClient.RegisterPrefab(CardCatalogue.GetPrefabForCard(id));
            Debug.Log(id);
        }
    }
    
    [TargetRpc]
    public void RpcStartTurn(NetworkConnectionToClient target)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Debug.Log($"StartTurn on {name}, state: {state}");
        if (!board)
        {
            return;
        }
        board.NewTurn();
    }

    [TargetRpc]
    public void RpcStartGameAsSecond(NetworkConnectionToClient target)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Debug.Log($"StartAsSecond on {name}, state: {state}");
        //state = TurnState.OPPONENT_TURN;
    }

    [TargetRpc]
    public void RpcEndTurn(NetworkConnectionToClient target)
    {
        if (!isLocalPlayer)
        {
            return;
        }
    }

    [TargetRpc]
    public void RpcCreaturePlayed(bool hostSide, int creatureSlot, uint netId)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        if (networkManager.loadGame && !NetworkClient.spawned.ContainsKey(netId))
        {
            //maybe solution is for loadgame to just work on server? prob not good either
            Debug.LogError("Spawned creature not found. This can happen when server side loading is too fast, and the creature already died");
            return;
        }
        var creatureObj = NetworkClient.spawned[netId].gameObject;
        var creature = creatureObj.GetComponent<Creature>();    
        board.CreaturePlayed(hostSide, creatureSlot, creature);
    }

    [TargetRpc]
    public void RpcCreatureDestroyed(bool hostSide, int creatureSlot)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.CreatureDestroyed(hostSide, creatureSlot);
    }

    [TargetRpc]
    public void RpcAttackCommenced(bool hostSide, List<uint> netIds)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.AttackCommenced(hostSide, netIds);
    }

    [TargetRpc]
    public void RpcBlockConfirmed(bool hostSide, List<uint> netIds)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.BlockConfirmed(hostSide, netIds);
    }

    [TargetRpc]
    public void RpcWinGame()
    {
        hud.WinScreen();
    }

    [TargetRpc]
    public void RpcLoseGame()
    {
        hud.LoseScreen();
    }

    [TargetRpc]
    public void RpcUpdateCreatureLocations(List<uint> netIds)
    {
        board.UpdateCreatureLocations(netIds);
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
        CmdCommenceBlock(blockers);
    }

    public void RequestResolveCombat()
    {
        CmdResolveCombat();
    }
    
    [Command][LogReplay]
    public void CmdChangeState(TurnState state)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdChangeState), this, state);
        ChangeState(state);
    }

    public void ChangeState(TurnState state)
    {
        // It would be great to have some validation e.g. you can only go to ATTACK from PLAYING
        this.state = state;
    }

    [Command][LogReplay]
    public void CmdEndTurn()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdEndTurn), this);
        EndTurn();
    }

    public void EndTurn()
    {
        networkManager.turnManager.EndTurn(this);
    }

    [Command][LogReplay]
    public void CmdPlayCreature(string creatureIdentifier, int manaCost, int creatureSlot)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdPlayCreature), this, creatureIdentifier, manaCost, creatureSlot);
        PlayCreature(creatureIdentifier, manaCost, creatureSlot);
    }

    public void PlayCreature(string creatureIdentifier, int manaCost, int creatureSlot)
    {
        if (manaCost > mana)
        {
            Debug.LogError("PlayCreature called without enough mana");
            return;
        }
        if (!networkManager.serverBoard.CreaturePlayed(this, creatureSlot, creatureIdentifier))
        {
            Debug.LogError("Creature already exists in this slot or player in wrong state!");
            return;
        }
        mana -= manaCost;
    }

    [Command][LogReplay]
    public void CmdCommenceAttack(List<uint> creatureNetIds)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdCommenceAttack), this, creatureNetIds);
        CommenceAttack(creatureNetIds);
    }

    public void CommenceAttack(List<uint> creatureNetIds)
    {
        networkManager.serverBoard.AttackByPlayer(this, creatureNetIds);
        networkManager.turnManager.ConfirmAttackers(this);
    }

    [Command][LogReplay]
    public void CmdCommenceBlock(List<uint> creatureNetIds)
    {
        ReplayHelper.LogCommandAuto(nameof(CmdCommenceBlock), this, creatureNetIds);
        ConfirmBlock(creatureNetIds);
    }

    public void ConfirmBlock(List<uint> creatureNetIds)
    {
        networkManager.serverBoard.BlockByPlayer(this, creatureNetIds);
        networkManager.turnManager.ConfirmBlockers(this);
    }

    [Command][LogReplay]
    public void CmdResolveCombat()
    {
        ReplayHelper.LogCommandAuto(nameof(CmdResolveCombat), this);
        ResolveCombat();
    }

    public void ResolveCombat()
    {
        networkManager.serverBoard.ResolveCombat(this);
    }

    [Command]
    public void CmdBoardReady()
    {
        networkManager.boardReadyCnt++;
    }

/*
    [Command]
    public void CmdToggleBlocking(Creature creature)
    {

    }
    */
}