using System;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    [SyncVar] public TurnState state;
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
    public void RpcEndTurn(NetworkConnection target)
    {
        state = TurnState.OPPONENT_TURN;
        
    }

    public void RequestEndTurn()
    {
        CmdEndTurn();
    }

    [Command]
    public void CmdEndTurn()
    {
        networkManager.turnManager.EndTurn(this);
    }

    [Command]
    public void CmdPlayCard(Card card)
    {

    }

    [Command]
    public void CmdToggleAttacking(Creature creature)
    {

    }

    [Command]
    public void CmdCommenceAttack()
    {

    }

    [Command]
    public void CmdToggleBlocking(Creature creature)
    {

    }
}