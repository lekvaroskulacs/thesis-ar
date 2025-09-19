using TMPro;
using UnityEngine;

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
            opponentDisplay.text = $"Opponent: {value}";
        }
    }

    public bool isHost { get; set; } = false;

    [SerializeField] TMP_Text opponentDisplayHost;
    [SerializeField] TMP_Text opponentDisplayClient;
    private TMP_Text opponentDisplay;

    public void ClientStart(bool isHost)
    {
        this.isHost = isHost;
        opponentDisplay = isHost ? opponentDisplayHost : opponentDisplayClient;
    }
}