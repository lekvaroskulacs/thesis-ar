using Mirror;
using Mirror.Examples.Basic;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine;
using System;


public class CreatureField : MonoBehaviour
{
    [HideInInspector] public NetworkGamePlayer owningPlayer;
    [HideInInspector] public Creature creature;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer field;

    [Header("Interface")]
    [SerializeField] public TMP_Text creatureHealthDisplay;
    [SerializeField] public TMP_Text creatureAttackDisplay;
    [SerializeField] public Button toggleAttackButton;
    [SerializeField] public Button toggleBlockButton;
    [SerializeField] public GameObject attackIcon;
    [SerializeField] public GameObject blockIcon;

    [SerializeField] public Button fieldSelector;

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
            creatureAttackDisplay.text = creature.attack.ToString();
            creatureHealthDisplay.text = creature.health.ToString();

            if (creature.attacking)
            {
                toggleAttackButton.GetComponentInChildren<TMP_Text>().text = "X";
            }
            else
            {
                toggleAttackButton.GetComponentInChildren<TMP_Text>().text = "Attack";
            }

            if (creature.blocking)
            {
                toggleBlockButton.GetComponentInChildren<TMP_Text>().text = "X";
            }
            else
            {
                toggleBlockButton.GetComponentInChildren<TMP_Text>().text = "Block";
            }

            if (creature.attackConfirmed)
            {
                attackIcon.SetActive(true);
            }
            else
            {
                attackIcon.SetActive(false);
            }
        }
        else
        {
            attackIcon.SetActive(false);
            creatureAttackDisplay.gameObject.SetActive(false);
            creatureHealthDisplay.gameObject.SetActive(false);
            toggleAttackButton.gameObject.SetActive(false);
            toggleBlockButton.gameObject.SetActive(false);
        }

        if (owningPlayer)
        {
            if (owningPlayer.state == TurnState.MOVING_CREATURE)
            {
                fieldSelector.gameObject.SetActive(true);
            }
            else
            {
                fieldSelector.gameObject.SetActive(false);
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