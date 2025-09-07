using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MultipleImageTrackingManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    /// <summary>
    /// The prefabs should have the same name as the tracked images in the ReferenceImageLibrary
    /// </summary>
    private Dictionary<string, GameObject> nameToPrefabMap;

    [SerializeField] private List<GameObject> prefabs;



    void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (!trackedImageManager)
        {
            return;
        }

        nameToPrefabMap = new Dictionary<string, GameObject>();

        SetupTrackedPrefabs();
    }

    void SetupTrackedPrefabs()
    {
        foreach (var element in prefabs)
        {
            var gameObject = Instantiate(element);
        }
    }

    void Update()
    {
        
    }
}
