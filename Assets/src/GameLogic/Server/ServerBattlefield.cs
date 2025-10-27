using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class ServerBattlefield : MonoBehaviour
{
    [SerializeField] private List<ServerCreatureField> _hostFields;
    public List<ServerCreatureField> hostFields
    {
        get
        {
            return _hostFields;
        }
    }

    [SerializeField] private List<ServerCreatureField> _guestFields;
    public List<ServerCreatureField> guestFields
    {
        get
        {
            return _guestFields;
        }
    }

    public List<ServerCreatureField> allFields
    {
        get
        {
            var list = new List<ServerCreatureField>();
            list.AddRange(hostFields);
            list.AddRange(guestFields);
            return list;
        }
    }

    public List<ServerCreatureField> FieldsOfPlayer(NetworkGamePlayer player)
    {
        if (!player.isHost)
        {
            return guestFields;
        }
        else
        {
            return hostFields;
        }
    }

    public List<Creature> CreaturesOfPlayer(NetworkGamePlayer player)
    {
        var playerCreatures = new List<Creature>();
        foreach (var field in FieldsOfPlayer(player))
        {
            playerCreatures.Add(field.creature);
        }
        return playerCreatures;
    }

    public Creature GetOpposingCreature(Creature creature)
    {
        int slot = -1;
        bool hostSide = false;
        foreach (var field in hostFields)
        {
            if (field.creature == creature)
            {
                slot = hostFields.IndexOf(field);
                hostSide = true;
            }
        }
        foreach (var field in guestFields)
        {
            if (field.creature == creature)
            {
                slot = guestFields.IndexOf(field);
                hostSide = false;
            }
        }

        int numSlots = 3;
        if (hostSide)
        {
            return guestFields[numSlots - slot].creature;
        }
        else
        {
            return hostFields[numSlots - slot].creature;
        }
    }

}