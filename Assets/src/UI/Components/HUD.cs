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

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

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
                    confirmBlockersButton.enabled = true;
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
                    text = "Playing (After attack)";
                    break;
            }

            turnStateDisplay.text = text;
            manaDisplay.text = $"{player.mana}/{player.maxMana}";

            if (player.board)
            {
                UpdateCreatureHUDs();
            }
        }
    }

    void UpdateCreatureHUDs()
    {
        var fields = player.board.GetCreatureFieldsForLocalPlayer();
        foreach (var field in fields)
        {
            switch (player.state)
            {
                case TurnState.PLAYING:
                    if (!field.creature)
                    {
                        break;
                    }
                    field.toggleAttackButton.gameObject.SetActive(true);
                    field.toggleAttackButton.enabled = field.creature.canAttack;
                    field.toggleBlockButton.gameObject.SetActive(false);
                    break;
                case TurnState.BLOCKING:
                    if (!field.creature)
                    {
                        break;
                    }
                    field.toggleBlockButton.gameObject.SetActive(true);
                    field.toggleAttackButton.enabled = true;
                    field.toggleAttackButton.gameObject.SetActive(false);
                    break;
                case TurnState.OPPONENT_TURN:

                    break;
                case TurnState.OPPONENT_BLOCKING:

                    break;
                case TurnState.ATTACKING:

                    break;
                case TurnState.PLAYING_AFTER_ATTACK:
                    if (!field.creature)
                    {
                        break;
                    }
                    field.toggleBlockButton.gameObject.SetActive(false);
                    field.toggleAttackButton.gameObject.SetActive(false);
                    break;
            }
        }
    }

    void OnEndTurn()
    {
        player.RequestEndTurn();
    }

    void OnCommenceAttack()
    {
        player.RequestAttack();
    }

    void OnConfirmBlockers()
    {
        player.RequestBlock();
    }


    public void WinScreen()
    {
        winScreen.SetActive(true);
    }
    
    public void LoseScreen()
    {
        loseScreen.SetActive(true);
    }

}