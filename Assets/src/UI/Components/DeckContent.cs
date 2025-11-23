using UnityEngine;

public class DeckContent : MonoBehaviour
{
    [SerializeField] private DeckSelection deckSelection;
    void OnEnable()
    {
        deckSelection.SelectDeck(gameObject);
    }
}