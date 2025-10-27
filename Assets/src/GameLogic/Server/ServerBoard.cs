using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;
using Mirror.BouncyCastle.Crypto.Modes;


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
    private List<Creature> currentBlockers = new List<Creature>();
    
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
            p.RpcCreaturePlayed(player.isHost, creatureSlot, creature.GetComponent<NetworkIdentity>().netId);
        }
    }

    public void CreatureDestroyed(NetworkGamePlayer player, Creature creature)
    {
        var fields = player.isHost ? battlefield.hostFields : battlefield.guestFields;
        foreach (var field in fields)
        {
            if (field.creature == creature)
            {
                field.creature = null;
                foreach (var p in players.data)
                {
                    p.RpcCreatureDestroyed(player.isHost, fields.IndexOf(field));
                }
            }
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
            p.RpcAttackCommenced(player.isHost, attackerIds);
        }
    }

    public void BlockByPlayer(NetworkGamePlayer player, List<uint> blockerIds)
    {
        var playerCreatures = battlefield.CreaturesOfPlayer(player);
        foreach (var id in blockerIds)
        {
            var gameObject = NetworkServer.spawned[id].gameObject;
            var creature = gameObject.GetComponent<Creature>();
            if (!creature)
            {
                throw new ArgumentException("Received blcoker IDs contain non-creature object!");
            }
            if (!playerCreatures.Contains(creature))
            {
                throw new ArgumentException("Blocking creature is not owned by blocking player!");
            }

            currentBlockers.Add(creature);
        }

        foreach (var p in players.data)
        {
            p.RpcBlockConfirmed(player.isHost, blockerIds);
        }
    }

    public void ResolveCombat(NetworkGamePlayer blockingPlayer)
    {
        foreach (var attacker in currentAttackers)
        {
            if (!attacker.attackConfirmed)
            {
                throw new Exception($"Something went wrong. Attacker is not confirmed to attack: {attacker}");
            }

            var blocker = battlefield.GetOpposingCreature(attacker);
            if (blocker)
            {
                blocker.TakeDamage(attacker.attack);
                attacker.TakeDamage(blocker.attack);
            }
            else
            {
                blockingPlayer.TakeDamage(attacker.attack);
            }
        }
        currentAttackers.Clear();
        currentBlockers.Clear();
        foreach (var field in battlefield.allFields)
        {
            if (field.creature)
            {
                field.creature.ResetCombatState();
            }
        }
    }
    
    
}