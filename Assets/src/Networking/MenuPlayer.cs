using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class MenuPlayer : NetworkBehaviour
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

    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    private bool isReady;

    public bool isHost { get; set; } = false;
    [SyncVar]
    public string displayName;


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

    void OnReadyStatusChanged(bool oldValue, bool newValue)
    {

    }

}
