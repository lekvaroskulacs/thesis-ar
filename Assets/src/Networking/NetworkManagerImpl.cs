using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;
using Mirror.Examples.Basic;
using UnityEditor;


/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/



public class NetworkManagerImpl : NetworkManager
{
    [Header("Scenes")]
    [SerializeField] private string gameScene;
    [SerializeField] private string menuScene;

    private string currentScene;

    public NetworkPlayers<NetworkMenuPlayer> players = new NetworkPlayers<NetworkMenuPlayer>();
    public NetworkPlayers<NetworkGamePlayer> gamePlayers = new NetworkPlayers<NetworkGamePlayer>();

    public override void Awake()
    {
        base.Awake();
        currentScene = menuScene;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var prefab = currentScene == gameScene ?
            Resources.Load<GameObject>("Prefabs/GamePlayer") :
            Resources.Load<GameObject>("Prefabs/MenuPlayer");

        GameObject playerInstance = Instantiate(prefab);
        playerInstance.name = $"{prefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, playerInstance);

        if (currentScene == gameScene) return;

        var menuPlayer = playerInstance.GetComponent<NetworkMenuPlayer>();
        menuPlayer.displayName = "";
        players.Add(menuPlayer);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (currentScene == menuScene)
        {
            MenuDisconnected(conn);
        }
        else
        {
            GameDisconnected(conn);
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
            Invoke("LoadGame", 1f);
        }
    }

    void LoadGame()
    {
        var list = new List<ReferenceImageInfo>();
        var tex = Resources.Load<Texture2D>("Images/Board/board");
        list.Add(new ReferenceImageInfo(
            tex,
            "Board",
            0.1f
        ));


        tex = Resources.Load<Texture2D>("Images/Cards/Creatures/skeleton");
        list.Add(new ReferenceImageInfo(
            tex,
            "Skeleton",
            0.1f
        ));


        var library = GameObject.FindGameObjectWithTag("Origin").GetComponent<MutableLibrary>();

        library.AddReferenceImages(list);
        library.StartTrackingImages();

        var imgManager = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();

        imgManager.SetTrackedEntities(new List<string> { "Board", "Skeleton" });
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

    #endregion
}
