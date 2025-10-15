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

    private List<Creature> currentAttackers = new List<Creature>();
    
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

    public void AttackByPlayer(NetworkGamePlayer player, List<uint> attackerIds)
    {
        var playerCreatures = battlefield.CreaturesOfPlayer(player);
        foreach (var id in attackerIds)
        {
            var gameObject = NetworkServer.spawned[id].gameObject;
            var creature = gameObject.GetComponent<Creature>();
            if (!creature)
            {
                throw new ArgumentException("Received attacker IDs contain non-creature object!");
            }
            if (!playerCreatures.Contains(creature))
            {
                throw new ArgumentException("Attacking creature is not owned by attacking player!");
            }

            currentAttackers.Add(creature);
        }
        
        foreach (var p in players.data)
        {
            p.RpcAttackCommenced(player.isServer, attackerIds);
        }
    }
}