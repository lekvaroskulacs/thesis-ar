using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

public class DevStart : NetworkBehaviour
{

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;

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

    void Awake()
    {
        startGameButton.onClick.AddListener(GameStarted);
    }

    public void GameStarted()
    {
        networkManager.StartHost();
        networkManager.ServerChangeScene("GameScene");
    }

}