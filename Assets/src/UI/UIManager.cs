using UnityEditor.PackageManager;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    /// Components
    private ErrorDisplay errorDisplay;

    void Start()
    {
        errorDisplay = GetComponentInChildren<ErrorDisplay>();
    }

    public void DisplayError(string msg)
    {
        errorDisplay.Display(msg);
    }
}
