using UnityEngine.UI;
using UnityEngine;

public class CardButton : MonoBehaviour
{
    public DeckSelection deckSelection;
    public string creatureIdentifier;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Clicked);
    }

    void Update()
    {
        if (deckSelection.creatureIdentifiers.Contains(creatureIdentifier))
        {
            var image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f); 
        }
        else
        {
            var image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f); 
        }
    }

    void Clicked()
    {
        if (!deckSelection.creatureIdentifiers.Contains(creatureIdentifier))
        {
            deckSelection.creatureIdentifiers.Add(creatureIdentifier);
        }
        else
        {
            deckSelection.creatureIdentifiers.Remove(creatureIdentifier);
        }
    }
}