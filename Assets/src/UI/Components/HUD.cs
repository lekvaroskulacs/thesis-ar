using System.Collections;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private NetworkGamePlayer player;

    [Header("UI elements")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button commenceAttackButton;
    [SerializeField] private Button confirmBlockersButton;
    [SerializeField] private TMP_Text manaDisplay;

    [SerializeField] private TMP_Text turnStateDisplay;

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
        endTurnButton.onClick.AddListener(OnEndTurn);
        commenceAttackButton.onClick.AddListener(OnCommenceAttack);
        confirmBlockersButton.onClick.AddListener(OnConfirmBlockers);
    }

    public void RegisterNetworkPlayer(NetworkGamePlayer player)
    {
        this.player = player; 
    }

    void Update()
    {
        if (player)
        {
            string text = "";
            switch (player.state)
            {
                case TurnState.PLAYING:
                    endTurnButton.enabled = true;
                    confirmBlockersButton.gameObject.SetActive(false);
                    commenceAttackButton.gameObject.SetActive(true);
                    commenceAttackButton.enabled = false;
                    text = "Playing";
                    break;
                case TurnState.BLOCKING:
                    endTurnButton.enabled = false;
                    confirmBlockersButton.gameObject.SetActive(true);
                    commenceAttackButton.gameObject.SetActive(false);
                    text = "Blocking";
                    break;
                case TurnState.OPPONENT_TURN:
                    endTurnButton.enabled = false;
                    confirmBlockersButton.gameObject.SetActive(false);
                    commenceAttackButton.gameObject.SetActive(false);
                    text = "Opponent turn";
                    break;
                case TurnState.OPPONENT_BLOCKING:
                    endTurnButton.enabled = false;
                    confirmBlockersButton.gameObject.SetActive(false);
                    commenceAttackButton.gameObject.SetActive(false);
                    text = "Opponent blocking";
                    break;
                case TurnState.ATTACKING:
                    endTurnButton.enabled = false;
                    confirmBlockersButton.gameObject.SetActive(false);
                    commenceAttackButton.gameObject.SetActive(true);
                    commenceAttackButton.enabled = true;
                    text = "Attacking";
                    break;
                case TurnState.PLAYING_AFTER_ATTACK:
                    endTurnButton.enabled = true;
                    confirmBlockersButton.gameObject.SetActive(false);
                    commenceAttackButton.gameObject.SetActive(false);
                    text = "Attacking";
                    break;
            }

            turnStateDisplay.text = text;
            manaDisplay.text = $"{player.mana}/{player.maxMana}";
        }
    }

    void OnEndTurn()
    {
        player.RequestEndTurn();
    }

    void OnCommenceAttack()
    {
        
    }

    void OnConfirmBlockers()
    {
        
    }


}