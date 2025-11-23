using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Battlefield : MonoBehaviour
{
    [SerializeField] private List<CreatureField> _hostFields;

    public List<CreatureField> hostFields
    {
        get
        {
            return _hostFields;
        }
    }

    [SerializeField] private List<CreatureField> _guestFields;
    public List<CreatureField> guestFields
    {
        get
        {
            return _guestFields;
        }
    }

    public List<CreatureField> allFields
    {
        get
        {
            var list = new List<CreatureField>();
            list.AddRange(hostFields);
            list.AddRange(guestFields);
            return list;
        }
    }

    public List<CreatureField> FieldsOfPlayer(NetworkGamePlayer player)
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

}