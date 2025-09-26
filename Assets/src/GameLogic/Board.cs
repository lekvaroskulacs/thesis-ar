using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Mirror;
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

public class Board : NetworkBehaviour
{
    [SerializeField] private Corners corners;
    private BoardTracker boardTracker;
    private BoardState state;
    private Action boardReady;


    [SerializeField] private List<CreatureField> creatureFields;
    private Players<GamePlayer> players;


    private Vector3 topLeft;
    private float width;
    private float height;

    public List<GameObject> playableCardGameObjects;


    public void SubscribeToBoardReady(Action callback)
    {
        boardReady += callback;
    }

    void Start()
    {
        boardTracker = GameObject.FindWithTag("BoardTracker").GetComponent<BoardTracker>();
        boardTracker.board = this;
        corners = new Corners();
    }

    void Update()
    {
        if (state == BoardState.NOT_READY)
        {
            UpdateNotReady();
        }
        else if (state == BoardState.READY)
        {
            UpdateReady();
        }
    }

    void UpdateNotReady()
    {
        if (boardTracker.GetTrackedObjectStatus() == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            Debug.Log("Board ready");
            state = BoardState.READY;
            OnBoardReady();
        }
    }

    void UpdateReady()
    {
        foreach (var obj in playableCardGameObjects)
        {
            CreatureField slot = null;
            foreach (var field in creatureFields)
            {
                if (field.IsGameObjectOnCreatureField(obj))
                {
                    slot = field;
                }
            }

            if (slot)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            }
        }
    }

    void OnBoardReady()
    {
        // TODO:
        // Set topleft, width height calculated from corners
        // Then create a field in the middle maybe?
        //var field = new CreatureField();
        //fields.Add(field);
        playableCardGameObjects = boardTracker.allLoadedGameObjects;
    }


    public Corners GetCorners()
    {
        return corners;
    }
}