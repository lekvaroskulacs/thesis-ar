using Mirror;
using UnityEngine;

public class JoinGame : MonoBehaviour
{
    [Header("Menu screen references")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseNamePanel;

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

    public void OnJoinGameClicked()
    {
        mainMenuPanel.SetActive(false);
        chooseNamePanel.SetActive(true);

        networkManager.networkAddress = "192.168.1.13";
        networkManager.StartClient();
    }

}