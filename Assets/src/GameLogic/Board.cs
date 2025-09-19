using System;
using System.Data.SqlTypes;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Corners
{
    public GameObject BottomLeft;
    public GameObject BottomRight;
    public GameObject TopLeft;
    public GameObject TopRight;
}

internal enum BoardState
{
    NOT_READY, READY, PLAYING
}

public class Board : MonoBehaviour
{
    private BoardTracker boardTracker;
    [SerializeField] private Corners corners;
    private BoardState state;

    private Action boardReady;

    public void SubscribeToBoardReady(Action callback)
    {
        boardReady += callback;
    }

    void Start()
    {
        boardTracker = GetComponent<BoardTracker>();
        corners = new Corners();
    }

    void Update()
    {
        if (state == BoardState.NOT_READY)
        {
            HandleNotReady();
        }
        else if (state == BoardState.READY)
        {
            HandleReady();
        }
    }

    void HandleNotReady()
    {
        if (boardTracker.GetTrackedObjectStatus() == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            Debug.Log("Board ready");
            state = BoardState.READY;
        }
    }

    void HandleReady()
    {

    }
    
    public Corners GetCorners()
    {
        return corners;
    }
}