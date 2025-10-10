using System.Collections.Generic;
using UnityEngine;

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
        if (!player.isServer)
        {
            return guestFields;
        }
        else
        {
            return hostFields;
        }
    }


}