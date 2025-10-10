using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkMenuPlayer : NetworkBehaviour
{
    MenuPlayerUIHandler ui;
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

    [SyncVar]
    public bool isReady;

    [SyncVar]
    public string displayName;

    public bool isHost { get; set; } = false;


    void Awake()
    {
        ui = GameObject.FindWithTag("Logger").GetComponentInChildren<MenuPlayerUIHandler>();
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("LocalPlayerStart");
        isHost = isServer;
        ui.ClientStart(isHost);
        CmdUpdateDisplayNames();
    }

    [Command]
    public void CmdUpdateDisplayNames()
    {
        networkManager.UpdateDisplayNames();
    }

    [ClientRpc]
    public void RpcSetOpponentName(string name)
    {
        if (isLocalPlayer)
        {
            SetOpponentName(name);
        }
    }

    public void SetOpponentName(string name)
    {
        ui.opponentDisplayName = name;
        Debug.Log($"SetOpponentName called with name {name}");
    }

    public bool ReadyUp()
    {
        if (isHost)
        {
            Debug.Log("Something went wrong! Readying up is not a defined action for the host.");
            return false;
        }

        CmdSetReadyStatus(true);
        return true;
    }

    [Command]
    public void CmdSetReadyStatus(bool ready)
    {
        isReady = ready;
        networkManager.GameStartable();
    }

    [ClientRpc]
    public void RpcGameStartable()
    {
        if (isHost)
        {
            ui.GameStartable();
        }
    }

}
