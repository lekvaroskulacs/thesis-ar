using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class BoardTracker : MonoBehaviour, ITracker
{
    private MultipleImageTrackingManager imageTracker;

    void Start()
    {
        imageTracker = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();
        imageTracker.SubscribeToPrefabLoad(OnPrefabsLoaded);
    }

    void OnPrefabsLoaded()
    {
    }

    public TrackingState GetTrackedObjectStatus()
    {
        return imageTracker.GetTrackingStateByReferenceImageName(name);
    }
}