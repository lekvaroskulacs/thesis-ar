using UnityEngine;

class Arena : MonoBehaviour
{
    private Corners corners;
    private Board board;

    [SerializeField] private GameObject leftWall;
    [SerializeField] private Wall rightWall;
    [SerializeField] private Wall topWall;
    [SerializeField] private Wall bottomWall;

    void Start()
    {
        corners = new Corners();
        board = GetComponent<Board>();
        board.SubscribeToBoardReady(OnBoardReady);
    }

    void OnBoardReady()
    {
        corners = board.GetCorners();
        leftWall.GetComponent<Wall>().start = corners.BottomLeft;
        leftWall.GetComponent<Wall>().end = corners.TopLeft;
        leftWall.SetActive(true);
    }

}