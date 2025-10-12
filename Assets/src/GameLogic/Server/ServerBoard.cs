using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public class ServerBoard : NetworkBehaviour
{
    public ServerBattlefield battlefield;
    public NetworkPlayers<NetworkGamePlayer> players;

    private Dictionary<string, string> catalogue = new Dictionary<string, string>
    {
        { "Skeleton", "Prefabs/Cards/Creatures/Skeleton" },
        { "Fairy", "Prefabs/Cards/Creatures/GreenFairy" },
        { "Cactus", "Prefabs/Cards/Creatures/Cactus"},
        { "Beholder", "Prefabs/Cards/Creatures/Beholder"}
        // Add more here as needed
    };
    
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


    public void CreaturePlayed(NetworkGamePlayer player, int creatureSlot, string creatureIdentifier)
    {
        var creature = Instantiate(Resources.Load<GameObject>(catalogue[creatureIdentifier]));
        NetworkServer.Spawn(creature, player.netIdentity.connectionToClient);
        var c = creature.GetComponent<Creature>();
        c.owningPlayer = player;
        battlefield.FieldsOfPlayer(player)[creatureSlot].creature = c;

        foreach (var p in players.data)
        {
            p.RpcCreaturePlayed(player.isServer, creatureSlot, creature.GetComponent<NetworkIdentity>().netId);
        }
    }

    public void CreatureDestroyed(NetworkGamePlayer player, int creatureSlot)
    {
        var creature = battlefield.FieldsOfPlayer(player)[creatureSlot].creature;
        Destroy(creature.gameObject);
        battlefield.FieldsOfPlayer(player)[creatureSlot].creature = null;

        foreach (var p in players.data)
        {
            p.RpcCreatureDestroyed(player.isServer, creatureSlot);
        }
    }

    public Creature CreatureAtSlot(NetworkGamePlayer player, int creatureSlot)
    {
        throw new NotImplementedException();
    }
}