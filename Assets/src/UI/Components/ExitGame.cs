using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
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
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Clicked);
    }

    void Clicked()
    {
        networkManager.StopClient();
        networkManager.StopServer();
    }
}