using UnityEngine;

public class BoardTracker : MonoBehaviour
{
    public Corners corners;
    private MultipleImageTrackingManager imageTracker;


    void Start()
    {
        corners = new Corners();
        imageTracker = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();
        imageTracker.SubscribeToPrefabLoad(OnPrefabsLoaded);
    }

    void OnPrefabsLoaded()
    {
        corners.BottomLeft = imageTracker.GetGameObjectByReferenceImageName("BottomLeft");
        corners.BottomRight = imageTracker.GetGameObjectByReferenceImageName("BottomRight");
        corners.TopLeft = imageTracker.GetGameObjectByReferenceImageName("TopLeft");
        corners.TopRight = imageTracker.GetGameObjectByReferenceImageName("TopRight");
    }

    public bool CornersReady()
    {
        if (corners.BottomLeft is null ||
            corners.BottomRight is null ||
            corners.TopLeft is null ||
            corners.TopRight is null)
        {
            return false;
        }

        return corners.BottomLeft.activeSelf &&
               corners.BottomRight.activeSelf &&
               corners.TopLeft.activeSelf &&
               corners.TopRight.activeSelf;
    }

    public Corners GetCorners()
    {
        return corners;
    }
}