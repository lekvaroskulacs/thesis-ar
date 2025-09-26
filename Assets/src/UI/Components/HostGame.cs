using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HostGame : MonoBehaviour
{
    [Header("Menu screen references")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseNamePanel;

    [Header("Button")]
    [SerializeField] private Button hostGameButton;

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
        hostGameButton.onClick.AddListener(OnHostGameClicked);
    }

    public void OnHostGameClicked()
    {
        mainMenuPanel.SetActive(false);
        chooseNamePanel.SetActive(true);

        networkManager.StartHost();
    }

}