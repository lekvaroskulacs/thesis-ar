using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;


/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class Players<Type>
{
    private readonly List<Type> players = new List<Type>(2);

    /// <summary>
    /// Adds a player. Returns true if added, false if list is full or already contains the player.
    /// </summary>
    public bool Add(Type player)
    {
        if (players.Count >= 2 || players.Contains(player))
        {
            return false;
        }

        players.Add(player);

        return true;
    }

    /// <summary>
    /// Removes a player. Returns true if removed, false if not found.
    /// </summary>
    public bool Remove(Type player)
    {
        bool removed = players.Remove(player);
        return removed;
    }

    /// <summary>
    /// Returns the other player than the one passed in, or default if not found.
    /// </summary>
    public Type Other(Type player)
    {
        if (players.Count != 2) return default;

        if (EqualityComparer<Type>.Default.Equals(players[0], player))
            return players[1];

        if (EqualityComparer<Type>.Default.Equals(players[1], player))
            return players[0];

        return default;
    }

    public List<Type> data
    {
        get
        {
            return players;
        }
    }

    public Type host
    {
        get
        {
            if (players.Count == 0) return default;
            return players[0];
        }
    }
}


public class NetworkManagerImpl : NetworkManager
{

    public Players<MenuPlayer> players = new Players<MenuPlayer>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, playerInstance);

        var menuPlayer = playerInstance.GetComponent<MenuPlayer>();
        menuPlayer.displayName = "";
        players.Add(menuPlayer);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        var playerInstance = conn.identity.GetComponent<MenuPlayer>();

        NetworkServer.DestroyPlayerForConnection(conn);

        players.Remove(playerInstance);
    }

    public void UpdateDisplayNames()
    {
        foreach (var player in players.data)
        {
            var opponent = players.Other(player);
            player.RpcSetOpponentName(opponent?.displayName);
        }
    }

    public void SetPlayerName(NetworkConnection conn, string name)
    {
        var player = conn is null ? players.host : conn.identity.GetComponent<MenuPlayer>();
        if (player != null)
        {
            player.displayName = name;
            UpdateDisplayNames();
        }
    }



}
