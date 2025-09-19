using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseName : NetworkBehaviour
{
    [Header("Host references")]
    [SerializeField] private GameObject chooseHostNamePanel;
    [SerializeField] private GameObject hostLobbyPanel;
    [SerializeField] private TMP_InputField hostNameInputField;

    [Header("Client references")]
    [SerializeField] private GameObject chooseClientNamePanel;
    [SerializeField] private GameObject clientLobbyPanel;
    [SerializeField] private TMP_InputField clientNameInputField;

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
    private const string playerPrefsNameKey = "PlayerName";

    void Awake()
    {
        hostNameInputField.onValueChanged.AddListener(OnValueChangedHost);
        clientNameInputField.onValueChanged.AddListener(OnValueChangedClient);
    }

    public void OnHostConfirmClicked()
    {
        chooseHostNamePanel.SetActive(false);
        hostLobbyPanel.SetActive(true);

        CmdSetPlayerName(hostNameInputField.text);
    }


    public void OnClientConfirmClicked()
    {
        chooseClientNamePanel.SetActive(false);
        clientLobbyPanel.SetActive(true);
        
        CmdSetPlayerName(clientNameInputField.text);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetPlayerName(string name, NetworkConnectionToClient sender = null)
    {
        networkManager.SetPlayerName(sender, name);
    }

    public void OnValueChangedHost(string value)
    {

    }

    public void OnValueChangedClient(string value)
    {

    }

}