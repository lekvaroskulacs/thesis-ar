using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text errorText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;

    private float timer = 0f;

    void Awake()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (errorText != null && errorText.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                errorText.gameObject.SetActive(false);
            }
        }
    }
    public void Display(string msg)
    {
        if (errorText == null)
        {
            Debug.LogError("ErrorDisplay: No errorText assigned!");
            return;
        }

        errorText.text = msg;
        errorText.gameObject.SetActive(true);
        timer = displayDuration;
    }
}
