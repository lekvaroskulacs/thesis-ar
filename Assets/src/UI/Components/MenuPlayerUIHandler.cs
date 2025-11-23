using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerUIHandler : MonoBehaviour
{
    private string _opponentDisplayName;
    public string opponentDisplayName
    {
        get
        {
            return _opponentDisplayName;
        }
        set
        {
            _opponentDisplayName = value;
            opponentDisplay.text = string.IsNullOrEmpty(value) ? "Waiting for opponent to connect..." : $"Opponent: {value}";
        }
    }

    public bool isHost { get; set; } = false;

    [SerializeField] TMP_Text opponentDisplayHost;
    [SerializeField] TMP_Text opponentDisplayClient;
    private TMP_Text opponentDisplay;

    [SerializeField] Button startGameButton;
    [SerializeField] Button loadGameButton;

    public void ClientStart(bool isHost)
    {
        this.isHost = isHost;
        opponentDisplay = isHost ? opponentDisplayHost : opponentDisplayClient;
    }

    public void GameStartable()
    {
        if (!isHost)
        {
            Debug.Log("Only host can start the game!");
            return;
        }

        startGameButton.interactable = true;
        loadGameButton.interactable = true;
    }
}