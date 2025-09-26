using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUp : NetworkBehaviour
{
    [SerializeField] private Button readyButton;


    void Awake()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    public void OnReadyClicked()
    {
        var player = NetworkClient.connection.identity.GetComponent<NetworkMenuPlayer>();
        if (player.ReadyUp())
        {
            readyButton.interactable = false;
        }
    }
}