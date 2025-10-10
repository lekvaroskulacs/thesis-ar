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

    public void CreaturePlayed(NetworkGamePlayer player, int creatureSlot, string creatureIdentifier)
    {
        var creature = Instantiate(Resources.Load<GameObject>(catalogue[creatureIdentifier]));
        battlefield.FieldsOfPlayer(player)[creatureSlot].creature = creature.GetComponent<Creature>();

        foreach (var p in players.data)
        {
            p.RpcCreaturePlayed(player.isServer, creatureSlot, creatureIdentifier);
        }
    }

    public void CreatureRemoved(NetworkGamePlayer player, int creatureSlot)
    {

    }

    public Creature CreatureAtSlot(NetworkGamePlayer player, int creatureSlot)
    {
        throw new NotImplementedException();
    }
}