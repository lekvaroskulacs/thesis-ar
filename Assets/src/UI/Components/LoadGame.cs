using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    [Header("Menu screen references")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadGamePanel;

    [Header("Button")]
    [SerializeField] private Button loadGameButton;

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
        loadGameButton.onClick.AddListener(Clicked);
    }

    public void Clicked()
    {
        mainMenuPanel.SetActive(false);
        loadGamePanel.SetActive(true);

        networkManager.StartHost();
        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        yield return new WaitUntil(() =>
        {
            return GameObject.FindWithTag("MenuPlayer") != null;
        });

        GameObject.FindWithTag("MenuPlayer").GetComponent<NetworkMenuPlayer>().CmdSetLoadGame(true);
    }

}