using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MutableLibrary : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    private UIManager logger;

    void Start()
    {
        logger = GameObject.FindGameObjectWithTag("Logger").GetComponent<UIManager>();
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (!trackedImageManager.descriptor.supportsMutableLibrary)
        {
            Debug.Log("This device does not support runtime mutable image libraries!");
            logger.DisplayError("This device does not support runtime mutable image libraries!");
            return;
        }
        else
        {
            logger.DisplayError("No errors");
        }
    }

    void Update()
    {
        
    }
}
