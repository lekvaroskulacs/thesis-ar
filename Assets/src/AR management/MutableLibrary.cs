using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReferenceImageInfo
{
    public Texture2D texture;
    public string name;
    public float widthInMeters;

    public ReferenceImageInfo(Texture2D texture, string name, float widthInMeters)
    {
        this.texture = texture;
        this.name = name;
        this.widthInMeters = widthInMeters;
    }
}

public class MutableLibrary : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    private UIManager logger;

    private MutableRuntimeReferenceImageLibrary library;

    void Start()
    {
        logger = GameObject.FindGameObjectWithTag("Logger").GetComponent<UIManager>();
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (!trackedImageManager.descriptor.supportsMutableLibrary)
        {
            logger.DisplayError("This device does not support runtime mutable image libraries!");
            return;
        }

        library = trackedImageManager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;

        if (library == null)
        {
            logger.DisplayError("Could not create reference image library!");
            return;
        }
    }

    public void AddReferenceImages(List<ReferenceImageInfo> imageList)
    {
        foreach (var image in imageList)
        {
            library.ScheduleAddImageWithValidationJob(image.texture, image.name, image.widthInMeters);
        }
    }

    public void StartTrackingImages()
    {
        trackedImageManager.enabled = false;
        trackedImageManager.referenceLibrary = library;
        trackedImageManager.enabled = true;
    }
}
