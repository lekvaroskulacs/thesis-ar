using System;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    [SyncVar] public TurnState state;
    [SyncVar] public int mana;
    [SyncVar] public int maxMana;
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

    [TargetRpc]
    public void RpcStartTurn(NetworkConnectionToClient target)
    {
        Debug.Log($"StartTurn on {name}, state: {state}");
        //state = TurnState.PLAYING;
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
    public void RpcCreaturePlayed(bool hostSide, int creatureSlot, string creatureIdentifier)
    {
        if (!board)
        {
            throw new NullReferenceException("Client local player board reference is null!");
        }
        board.CreaturePlayed(hostSide, creatureSlot, creatureIdentifier);
    }

    public void test(bool hostSide, int creatureSlot, string creatureIdentifier)
    {
        Debug.Log(board);
        //GameObject.FindWithTag("Logger").GetComponent<UIManager>().DisplayError(board == null ? board.name : "null");
        if (board) board.CreaturePlayed(hostSide, creatureSlot, creatureIdentifier);
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
        // TODO>check if this is called
        CmdPlayCreature(creature.creatureIdentifier, creature.manaCost, creatureSlot);
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
    public void CmdCommenceAttack()
    {

    }
/*
    [Command]
    public void CmdToggleBlocking(Creature creature)
    {

    }
    */
}