using Mirror;
using Mirror.Examples.Basic;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine;


public class CreatureField : MonoBehaviour
{
    [HideInInspector] public NetworkGamePlayer owningPlayer;
    [HideInInspector] public Creature creature;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer field;

    [Header("Interface")]
    [SerializeField] public OrderableText creatureHealthDisplay;
    [SerializeField] public OrderableText creatureAttackDisplay;
    [SerializeField] public Button toggleAttackButton;
    [SerializeField] public Button toggleBlockButton;

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

    void Awake()
    {
        toggleAttackButton.onClick.AddListener(ToggleAttack);
        toggleBlockButton.onClick.AddListener(ToggleBlock);
    }

    void Update()
    {
        if (creature)
        {
            creatureAttackDisplay.gameObject.SetActive(true);
            creatureHealthDisplay.gameObject.SetActive(true);
            creatureAttackDisplay.Text = creature.attack.ToString();
            creatureHealthDisplay.Text = creature.health.ToString();

            if (creature.attacking)
            {
                toggleAttackButton.GetComponentInChildren<TMP_Text>().text = "X";
            }
            else
            {
                toggleAttackButton.GetComponentInChildren<TMP_Text>().text = "Attack";
            }
        }
    }

    void ToggleAttack()
    {
        creature.RequestToggleAttack();
    }

    void ToggleBlock()
    {
        creature.RequestToggleBlock();
    }


}