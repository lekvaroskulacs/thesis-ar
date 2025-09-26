using Mirror.Examples.Basic;
using Unity.VisualScripting;
using UnityEngine;


class CreatureField : MonoBehaviour
{
    public GamePlayer owningPlayer;

    [SerializeField] private SpriteRenderer field;

    public Vector3 topLeft { get; set; }
    public float width { get; set; }
    public float height { get; set; }


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