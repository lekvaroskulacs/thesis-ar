using Mirror;
using Mirror.Examples.Basic;
using Unity.VisualScripting;
using UnityEngine;


public class CreatureField : MonoBehaviour
{
    public NetworkGamePlayer owningPlayer;

    [SerializeField] private SpriteRenderer field;

    public bool IsGameObjectOnCreatureField(GameObject obj)
    {
        var pos = obj.transform.position;
        var min = field.bounds.min;
        var max = field.bounds.max;

        if (min.x < pos.x && pos.x < max.x &&
            min.z < pos.z && pos.z < max.z)
        {
            return true;
        }

        return false;
    }

    
}