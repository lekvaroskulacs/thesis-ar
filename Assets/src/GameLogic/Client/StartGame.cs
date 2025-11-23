using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

public class StartGame : NetworkBehaviour
{
    [SerializeField] private DeckSelection deckSelection;

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
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
        startGameButton.onClick.AddListener(GameStarted);
        loadGameButton.onClick.AddListener(LoadGameStarted);
    }

    public void GameStarted()
    {
        deckSelection.FinalizeDeck();
        networkManager.StartGame();
    }

    public void LoadGameStarted()
    {
        deckSelection.creatureIdentifiers = ReplayLogger.LoadFromFile("game.replay").prefabsToLoad;
        deckSelection.FinalizeDeck();
        networkManager.LoadGame();
    }

}
