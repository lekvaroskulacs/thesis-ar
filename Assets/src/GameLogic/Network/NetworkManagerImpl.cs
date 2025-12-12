using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using Telepathy;
using System.Linq;
using Unity.VisualScripting;


/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

enum StartingPlayer
{
    Host,
    Guest,
    Random
}

public class NetworkManagerImpl : NetworkManager
{
    [Header("Scenes")]
    [SerializeField] private string gameScene;
    [SerializeField] private string menuScene;

    [Header("Configuration")]
    [SerializeField] private StartingPlayer startingPlayer;
    [SerializeField] private GameObject serverBoardPrefab;
    [SerializeField] private GameObject networkBoardPrefab;
    [SerializeField] private DeckSelection deckSelection;
    public HashSet<string> spawnableCardIds = new HashSet<string>();
    public ServerBoard serverBoard { get; internal set; }

    public string currentScene;
    public NetworkPlayers<NetworkMenuPlayer> players = new NetworkPlayers<NetworkMenuPlayer>();
    public NetworkPlayers<NetworkGamePlayer> gamePlayers = new NetworkPlayers<NetworkGamePlayer>();
    public TurnManager turnManager;

    public bool devDebug = false;
    public bool loadGame = false;
    public bool replay = false;
    public int boardReadyCnt = 0;

    public override void Awake()
    {
        base.Awake();
        currentScene = menuScene;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var prefab = currentScene != menuScene ?
            Resources.Load<GameObject>("Prefabs/GamePlayer") :
            Resources.Load<GameObject>("Prefabs/MenuPlayer");

        GameObject playerInstance = Instantiate(prefab);
        playerInstance.name = $"{prefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, playerInstance);
        if (currentScene == gameScene)
        {
            var gamePlayer = playerInstance.GetComponent<NetworkGamePlayer>();
            gamePlayers.Add(gamePlayer);
            GameObject networkBoardObj = Instantiate(networkBoardPrefab);
            NetworkServer.Spawn(networkBoardObj, conn);
        }
        else if (currentScene == menuScene)
        {
            var menuPlayer = playerInstance.GetComponent<NetworkMenuPlayer>();
            menuPlayer.displayName = "";
            if (players.data.Count > 0 && players.host.loadGame)
            {
                menuPlayer.loadGame = true;
            }
            players.Add(menuPlayer);
        }
        else
        {
            GameObject player2 = Instantiate(prefab);
            player2.name = $"{prefab.name} [connId=1]";
            var gamePlayer1 = playerInstance.GetComponent<NetworkGamePlayer>();
            var gamePlayer2 = player2.GetComponent<NetworkGamePlayer>();
            gamePlayer1.isHost = true;
            gamePlayer2.isHost = false;
            gamePlayers.Add(gamePlayer1);
            gamePlayers.Add(gamePlayer2);

            NetworkServer.Spawn(player2, conn);
        }

    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (currentScene == menuScene)
        {
            MenuDisconnected(conn);
        }
        else if (currentScene == gameScene)
        {
            GameDisconnected(conn);
        }
    }

    public override void OnClientDisconnect()
    {
        if (currentScene == menuScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void MenuDisconnected(NetworkConnectionToClient conn)
    {
        var playerInstance = conn.identity.GetComponent<NetworkMenuPlayer>();

        NetworkServer.DestroyPlayerForConnection(conn);

        players.Remove(playerInstance);
    }

    // TODO: Somehow we should handle the client trying to reconnect and maybe verifying if its the same one
    void GameDisconnected(NetworkConnectionToClient conn)
    {
        ReplayLogger.SaveToFile("game.replay");
        var playerInstance = conn.identity.GetComponent<NetworkGamePlayer>();

        NetworkServer.DestroyPlayerForConnection(conn);

        gamePlayers.Remove(playerInstance);
    }


    public void SetPlayerName(NetworkConnection conn, string name)
    {
        var player = conn is null ? players.host : conn.identity.GetComponent<NetworkMenuPlayer>();
        if (player != null)
        {
            player.displayName = name;
            UpdateDisplayNames();
        }
    }


    public override void OnServerChangeScene(string newSceneName)
    {
        currentScene = newSceneName;
    }

    public override void OnServerSceneChanged(string newSceneName)
    {
        if (newSceneName == gameScene)
        {
            StartCoroutine(ServerWaitForConfigReady());
        }
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == gameScene)
        {
            StartCoroutine(ClientWaitForConfigReady());
        }
    }

    IEnumerator ServerWaitForConfigReady()
    {
        yield return new WaitUntil(() =>
        {
            bool arReady = ARSession.state == ARSessionState.SessionTracking;
            bool playersReady = gamePlayers.data.Count == 2;
            return arReady && (playersReady || devDebug);
        });
        ServerLoadGame();
    }

    IEnumerator ClientWaitForConfigReady()
    {
        yield return new WaitUntil(() =>
        {
            bool arReady = ARSession.state == ARSessionState.SessionTracking;
            return arReady;
        });
        ClientLoadGame();
    }

    void ServerLoadGame()
    {
        RegisterPrefabs(spawnableCardIds.ToList());

        if (loadGame)
        {
            StartCoroutine(SetupLoadedGame());
            return;    
        }
        
        turnManager = GameObject.FindWithTag("TurnManager").GetComponent<TurnManager>();
        NetworkGamePlayer sPlayer = null;
        switch (startingPlayer)
        {
            case StartingPlayer.Host:
                sPlayer = gamePlayers.host;
                break;
            case StartingPlayer.Guest:
                sPlayer = gamePlayers.Other(gamePlayers.host);
                break;
            case StartingPlayer.Random:
                sPlayer = gamePlayers.data[UnityEngine.Random.value > 0.5f ? 1 : 0];
                break;
        }
        turnManager.Init(gamePlayers, sPlayer);
        InitServerBoard();
    }

    void ClientLoadGame()
    {
        RegisterPrefabs(spawnableCardIds.ToList());

        var list = new List<ReferenceImageInfo>();
        var catalogue = CardCatalogue.GetCatalogue();
        var tex = Resources.Load<Texture2D>(catalogue["Board"].trackedImagePath);
        list.Add(new ReferenceImageInfo(tex, "Board", 0.1f));

        foreach (var id in deckSelection.creatureIdentifiers)
        {
            tex = Resources.Load<Texture2D>(catalogue[id].trackedImagePath);
            list.Add(new ReferenceImageInfo(tex, id, 0.1f));
        }
        
        var library = GameObject.FindGameObjectWithTag("Origin").GetComponent<MutableLibrary>();
        library.AddReferenceImages(list);
        library.StartTrackingImages();

        var idList = new List<string> { "Board" };
        idList.AddRange(deckSelection.creatureIdentifiers);
        var imgManager = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();
        imgManager.SetTrackedEntities(idList);
    }

    public void InitServerBoard()
    {
        var board = Instantiate(serverBoardPrefab);
        serverBoard = board.GetComponent<ServerBoard>();
        serverBoard.players = gamePlayers;
    }

    public void InitReplay(NetworkGamePlayer startingPlayer)
    {
        replay = true;
        turnManager = GameObject.FindWithTag("TurnManager").GetComponent<TurnManager>();
        turnManager.Init(gamePlayers, startingPlayer);
        InitServerBoard();
    }

    public void StartGame()
    {
        StartCoroutine(CollectCardIdsCoroutine());
    }

    IEnumerator CollectCardIdsCoroutine()
    {
        yield return new WaitUntil(() =>
        {
            foreach (var player in players.data)
            {
                if (!player.deckFinalized)
                {
                    return false;
                }
            }
            return true;
        });

        foreach (var player in players.data)
        {
            spawnableCardIds.AddRange(player.spawnableCardIds);
        }

        ReplayLogger.LogPrefabsToLoad(spawnableCardIds.ToList());
        ServerChangeScene("GameScene");
    }
    
    public void RegisterPrefabs(List<string> cardIds)
    {
        foreach (var player in gamePlayers.data)
        {
            player.RpcRegisterCardPrefabs(cardIds);
        }
    }

    public void LoadGame()
    {
        loadGame = true;
        StartGame();
    }

    public IEnumerator SetupLoadedGame()
    {
        yield return new WaitUntil(() =>
        {
            return boardReadyCnt >= 2;
        });

        ReplayLogger.ReplayEventList eventList;
        eventList = ReplayLogger.LoadFromFile("game.replay");
        if (eventList.events.Count == 0)
        {
            Debug.LogError("Replay is empty");
        }


        turnManager = GameObject.FindWithTag("TurnManager").GetComponent<TurnManager>();
        turnManager.Init(gamePlayers,  gamePlayers.data.Find((p) => p.connectionToClient.connectionId == Convert.ToInt32(eventList.events[0]?.connectionId ?? "0")));
        InitServerBoard();


        var obj = new GameObject();
        obj.AddComponent<ReplayProcessor>();
        obj.AddComponent<NetworkIdentity>();
        var processor = obj.GetComponent<ReplayProcessor>();
        NetworkServer.Spawn(obj);

        yield return new WaitUntil(() =>
        {
            return gamePlayers.data[0].mana == 1 || gamePlayers.data[1].mana == 1;
        });

        foreach (var evt in eventList.events)
        {
            yield return new WaitUntil(() => processor.canExecuteNext);
            processor.ProcessCommand(evt);
        }
    }

    public NetworkIdentity GetSpawnedObject(uint id)
    {
        if (replay || loadGame)
        {
            return FindFirstObjectByType<ReplayProcessor>().replayIdToNetObj[id];
        }
        else
        {
            return NetworkServer.spawned[id];
        }
    }

    public NetworkIdentity GetSpawnedObjectClient(uint id)
     {
        if (replay)
        {
            return FindFirstObjectByType<ReplayProcessor>().replayIdToNetObj[id];
        }
        else
        {
            return NetworkClient.spawned[id];
        }
    }

    #region RPCs

    public void GameStartable()
    {
        players.host.RpcGameStartable();
    }

    public void UpdateDisplayNames()
    {
        foreach (var player in players.data)
        {
            var opponent = players.Other(player);
            player.RpcSetOpponentName(opponent?.displayName);
        }
    }

    public void EndGame(NetworkGamePlayer loser)
    {
        loser.RpcLoseGame();
        gamePlayers.Other(loser).RpcWinGame();
    }

    #endregion

}
