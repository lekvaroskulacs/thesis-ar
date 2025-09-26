using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class BoardTracker : MonoBehaviour, ITracker
{
    private MultipleImageTrackingManager imageTracker;
    private Board _board;
    public Board board
    {
        get
        {
            return _board;
        }
        set
        {
            _board = value;
            OnBoardInitialized();
        }
    }

    public List<GameObject> allLoadedGameObjects;

    void Start()
    {
        imageTracker = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();
        imageTracker.SubscribeToPrefabLoad(OnPrefabsLoaded);
    }

    public void OnPrefabsLoaded()
    {
        allLoadedGameObjects = imageTracker.allLoadedGameObjects;
    }

    public TrackingState GetTrackedObjectStatus()
    {
        return imageTracker.GetTrackingStateByReferenceImageName(board.name);
    }

    void OnBoardInitialized()
    {
        // allLoadedGameObjects will return every single gameobject, so might need a system to get only the cards from it
        // this wil be good enough for the original scope of the game though
        allLoadedGameObjects.Remove(_board.gameObject);
    }
}