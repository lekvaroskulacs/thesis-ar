using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ReplayButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button replayButton;

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
        replayButton.onClick.AddListener(ReplayClicked);
    }

    public void ReplayClicked()
    {
        networkManager.StartHost();
        networkManager.ServerChangeScene("ReplayScene");
    }

}