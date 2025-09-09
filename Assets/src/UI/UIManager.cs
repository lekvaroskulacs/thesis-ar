using UnityEngine;

public class UIManager : MonoBehaviour
{
    /// Components
    private ErrorDisplay errorDisplay;
    private StartGame startGame;

    void Start()
    {
        errorDisplay = GetComponentInChildren<ErrorDisplay>();
        startGame = GetComponentInChildren<StartGame>();
    }

    public void DisplayError(string msg)
    {
        errorDisplay.Display(msg);
    }

    public void GameStarted()
    {
        startGame.GameStarted();
    }
}
