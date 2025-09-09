using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;

public class MultipleImageTrackingManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    private GameObject instantiatedObjectsParent;

    private List<string> entityNames;
    private Dictionary<string, GameObject> nameToGameObject;
    public event Action OnPrefabsLoaded;

    private Dictionary<string, string> catalogue = new Dictionary<string, string>
    {
        { "BottomLeft", "Prefabs/Board/BottomLeft" },
        { "BottomRight", "Prefabs/Board/BottomRight" },
        { "TopLeft", "Prefabs/Board/TopLeft"},
        { "TopRight", "Prefabs/Board/TopRight"},
        { "Fireball", "Prefabs/Cards/Fireball" },
        { "HealingPotion", "Prefabs/Cards/HealingPotion" }
        // Add more here as needed
    };

    public GameObject GetGameObjectByReferenceImageName(string name)
    {
        return nameToGameObject[name];
    }

    public void SubscribeToPrefabLoad(Action callback)
    {
        OnPrefabsLoaded += callback;
    }

    void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (!trackedImageManager)
        {
            return;
        }

        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);

        nameToGameObject = new Dictionary<string, GameObject>();
    }

    void OnDestroy()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    public void SetTrackedEntities(List<string> names)
    {
        entityNames = names;
        SetupTrackedPrefabs();
    }

    void SetupTrackedPrefabs()
    {
        if (instantiatedObjectsParent != null)
        {
            Destroy(instantiatedObjectsParent);
        }

        instantiatedObjectsParent = new GameObject("Instantiated Objects Parent");

        foreach (var name in entityNames)
        {
            var gameObject = Instantiate(LoadPrefab(name), instantiatedObjectsParent.transform);
            Debug.Log(gameObject);
            gameObject.name = name;
            gameObject.SetActive(false);
            nameToGameObject[name] = gameObject;
        }

        OnPrefabsLoaded?.Invoke();
    }

    GameObject LoadPrefab(string cardName)
    {
        if (!catalogue.TryGetValue(cardName, out string path))
        {
            Debug.LogWarning($"CardCatalogue: No entry found for '{cardName}'");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"CardCatalogue: Failed to load prefab at '{path}' for '{cardName}'");
        }

        return prefab;
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            GameObjectAdded(trackedImage);
            Debug.Log($"Image tracked: {trackedImage.referenceImage.name}");
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            GameObjectUpdated(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            GameObjectRemoved(trackedImage.Value);
            Debug.Log($"Image removed: {trackedImage.Value.referenceImage.name}");
        }
    }

    void GameObjectAdded(ARTrackedImage trackedImage)
    {
        if (trackedImage == null)
        {
            return;
        }

        var gameObject = nameToGameObject[trackedImage.referenceImage.name];
        gameObject.SetActive(true);
        gameObject.transform.position = trackedImage.transform.position;
        gameObject.transform.rotation = trackedImage.transform.rotation;
    }

    void GameObjectUpdated(ARTrackedImage trackedImage)
    {
        if (trackedImage == null)
        {
            return;
        }
        var gameObject = nameToGameObject[trackedImage.referenceImage.name];

        gameObject.transform.position = trackedImage.transform.position;
        gameObject.transform.rotation = trackedImage.transform.rotation;
    }

    void GameObjectRemoved(ARTrackedImage trackedImage)
    {
        if (trackedImage == null)
        {
            return;
        }

        var gameObject = nameToGameObject[trackedImage.referenceImage.name];
        gameObject.SetActive(false);
    }
}
