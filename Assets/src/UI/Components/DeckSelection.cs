using System.Collections;
using System.Collections.Generic;
using Mirror.BouncyCastle.Security;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DeckSelection : MonoBehaviour
{
    [SerializeField] private GameObject cardButtonPrefab;
    public List<string> creatureIdentifiers = new List<string>();

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SelectDeck(GameObject contentPanel)
    {
        var catalogue = CardCatalogue.GetCatalogue();
        foreach (var card in catalogue)
        {
            var identifier = card.Key;
            if (identifier == "Board")
            {
                continue;
            }

            var buttonObject = Instantiate(cardButtonPrefab, contentPanel.transform);
            var button = buttonObject.GetComponent<CardButton>();
            button.deckSelection = this;
            button.creatureIdentifier = identifier;
            buttonObject.GetComponentInChildren<TMP_Text>().text = identifier;
        }

    }
    
    public void FinalizeDeck()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("MenuPlayer"))
        {
            var p = player.GetComponent<NetworkMenuPlayer>();
            if (p.isLocalPlayer)
            {
                p.RequestFinalizeDeck(creatureIdentifiers);
            }
        }
    }
}