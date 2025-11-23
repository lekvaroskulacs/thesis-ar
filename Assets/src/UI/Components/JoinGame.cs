using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class JoinGame : MonoBehaviour
{
    [Header("Menu screen references")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chooseNamePanel;
    [SerializeField] private GameObject loadGameLobby;

    [Header("Button")]
    [SerializeField] private Button joinGameButton;

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
        joinGameButton.onClick.AddListener(OnJoinGameClicked);
    }

    public void OnJoinGameClicked()
    {
        mainMenuPanel.SetActive(false);

        networkManager.networkAddress = "192.168.1.13";
        networkManager.StartClient();

        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() =>
        {
            return GameObject.FindWithTag("MenuPlayer") != null;
        });

        if (GameObject.FindWithTag("MenuPlayer").GetComponent<NetworkMenuPlayer>().loadGame)
        {
            loadGameLobby.SetActive(true);
        }
        else
        {
            chooseNamePanel.SetActive(true);
        }
    }

}