using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonConstants;

struct Cell
{
   public int colParent;
   public int rowParent;

     public float f, g, h;

    public void SetData(int rowPar, int colPar, float iF, float iG, float iH)
    {
        colParent = colPar;
        rowParent = rowPar;
        f = iF;
        g = iG;
        h = iH;
    }
}

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject ballPrefs;
    [SerializeField] private int ballCount;
    public static BoardManager Instance = null;

    private Dictionary<Vector2, BallController> ballDictionary = new Dictionary<Vector2, BallController>(); // Use to store ball with its board position

    
    

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 worldPos = MapProvider.Instance.gameCam.ScreenToWorldPoint(eventData.position);

        //Snap to board position
        Vector2 boardPosition = UtilMapHelpers.WorldToBoardPosition(worldPos, MapProvider.Instance.cellSize, MapConstants.BOARD_COL, MapConstants.BOARD_ROW);

        //Check if has ball then 
        Debug.Log(UtilMapHelpers.BoardToMatrixPosition(boardPosition, MapProvider.Instance.cellSize, MapConstants.BOARD_COL, MapConstants.BOARD_ROW));

        
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null)
            DestroyImmediate(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateBalls(ballCount);
    }

    

    private bool IsBlocked(Vector2 position)
    {
        return ballDictionary.ContainsKey(position);
    }

    //use A* Pathfinding to create a path from the ball to the clicking destination
    private List<Vector2> AStarPathFinding(Vector2 source, Vector2 des, int boardRow, int boardCol)
    {
        //Create an closed list
        bool[,] closeList = new bool[boardRow, boardCol];

        //Create and init a cell matrix
        Cell[,] cellMatrix = new Cell[boardRow, boardCol];
        int iCol, iRow;
        for (iRow = 0; iRow < boardRow; iRow++)
        {
            for (iCol = 0; iCol < boardCol; iCol++)
            {
                cellMatrix[iRow, iCol].SetData(-1, -1, float.MaxValue, float.MaxValue, float.MaxValue);
            }
        }

        //Initialzie the parameters of the starting node



        return null;
    }

    private void GenerateBalls(int ballCount)
    {
        var balls = GetComponentsInChildren<BallController>();
        for (int i = 0; i < balls.Length; i++)
        {
            ballDictionary.Add(balls[i].transform.position, balls[i]);
        }
    }
   
}
