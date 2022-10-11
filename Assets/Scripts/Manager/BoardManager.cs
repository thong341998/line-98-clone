using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonConstants;
using System;
using Random = UnityEngine.Random;

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

struct PPair
{
    public float f;
    public Vector2Int coord;
    public void SetData(float iF, Vector2Int iCoord)
    {
        f = iF;
        coord = iCoord;
    }
    
}



public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    public static event Action<Queue<Vector2>> OnGeneratePathComplete = delegate { };

    public static BoardManager Instance = null;

    [Header("Configuration")]
    [Header("Ball Generation")]
    [SerializeField] private GameObject ballPrefabs;
    [SerializeField] private Color[] colorArray;
    [SerializeField] private int ballCount;

    [Header("Debugging")]
    [SerializeField] private bool drawPath;

    private BallController currentSelectedBall = null;
    private Dictionary<Vector2Int, BallController> ballDictionary = new Dictionary<Vector2Int, BallController>(); // Use to store ball with its matrix position
    private Stack<Vector2Int> ballPath = new Stack<Vector2Int>();

    #region Cache Value
    private int boardRow;
    private int boardCol;
    private Vector2 cellSize;
    private Queue<Vector2> boardPath = new Queue<Vector2>(); // The path generate from A* Pathfinding in board position

    #endregion  

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
        InitValues();
        GenerateBalls(ballCount);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (drawPath)
        {
            foreach (var center in boardPath)
            {
                Gizmos.DrawSphere(new Vector3(center.x, center.y), 0.1f);
            }
            //Gizmos.DrawSphere(prevBoard, 0.1f);
        }
    }

    private void InitValues()
    {
        boardRow = MapConstants.BOARD_ROW;
        boardCol = MapConstants.BOARD_COL;
        cellSize = MapProvider.Instance.cellSize;
    }

    Vector2Int prevBoard;
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("On Click");
        Vector2 worldPos = MapProvider.Instance.gameCam.ScreenToWorldPoint(eventData.position);

        //Snap to board position
        Vector2 boardPosition = UtilMapHelpers.WorldToBoardPosition(worldPos, cellSize, boardCol, boardRow);
        Vector2Int clickMatPos = UtilMapHelpers.BoardToMatrixPosition(boardPosition, cellSize, boardCol, boardRow);
        bool hasBallOnPos = HasBallOn(boardPosition);
        //Debug.Log(hasBallOnPos);
        //Check if the board postion has the ball in it
        if (hasBallOnPos)
        {
            //If click ball and click on another ball, then cancel previous ball, s
            BallController prevBall = currentSelectedBall;

            //Changed currentSelecedBall to the current ball
            currentSelectedBall = ballDictionary[UtilMapHelpers.BoardToMatrixPosition(boardPosition,cellSize,boardCol,boardRow)];
            Vector2Int selectedBallMatPos = UtilMapHelpers.BoardToMatrixPosition(
                currentSelectedBall.transform.position, cellSize, boardCol, boardRow);

            //Click on the same position and has ball            
            if (prevBoard == selectedBallMatPos)
            {
                Debug.Log("Click on same ball!");
                return;
            }

        
            if (prevBall != null) prevBall.OnBallCancelSelected();
            currentSelectedBall.OnBallSelected();
        }

        //Else if click on empty cell and the current selected ball is not empty, then generate path and move ball on that path
        else if (currentSelectedBall != null) 
        {
          

            ballPath.Clear();
            boardPath.Clear();

           
            var ballMatPos = UtilMapHelpers.BoardToMatrixPosition(currentSelectedBall.transform.position, cellSize, boardCol, boardRow);
            //Debug.Log(ballMatPos);
            var destMatPos = UtilMapHelpers.BoardToMatrixPosition(boardPosition, cellSize, boardCol, boardRow);
            ballPath = AStarPathFinding(ballMatPos,destMatPos, boardRow, boardCol);

            while (ballPath.Count != 0)
            {
                var waypoint = ballPath.Pop();
                var boardPos = UtilMapHelpers.MatrixToBoardPosition(waypoint, cellSize, boardCol, boardRow);
                boardPath.Enqueue(boardPos);
                //Debug.Log(boardPosition);
            }


            //Set path for current ball to move itself and subcribe to even when ball move completed
            currentSelectedBall.SetPath(boardPath);
            currentSelectedBall.OnBallMoveCompleted += CurrentSelectedBall_OnBallMoveCompleted;
        }

        prevBoard = clickMatPos;

        
    }

    private void CurrentSelectedBall_OnBallMoveCompleted()
    {
        //Update the position of the ball
        ballDictionary.Remove(prevBoard);
        ballDictionary.Add(UtilMapHelpers.BoardToMatrixPosition(currentSelectedBall.transform.position, cellSize, boardCol, boardRow),currentSelectedBall);

        currentSelectedBall.OnBallMoveCompleted -= CurrentSelectedBall_OnBallMoveCompleted;
        currentSelectedBall = null;
       
    }

    private void BallController_OnPlayerClick(BallController ballController)
    {
        if (currentSelectedBall != ballController)
        {
            currentSelectedBall = ballController;
        }
    }


    private void GenerateBalls(int ballCount)
    {
        Color randColor = new Color();
        int count = 0;
        Vector2Int matPos = new Vector2Int();
        while (count <= ballCount)
        {
           
            matPos.Set(Random.Range(0, boardRow), Random.Range(0, boardCol));
            if (!ballDictionary.ContainsKey(matPos))
            {
                randColor = colorArray[Random.Range(0, colorArray.Length)];
                var boardPos = UtilMapHelpers.MatrixToBoardPosition(matPos, cellSize, boardCol, boardRow);
                var ballPref = Instantiate(ballPrefabs, boardPos, Quaternion.identity, transform).GetComponent<BallController>();
                ballPref.InitValues();
                ballPref.SetData(randColor);
                ballDictionary.Add(matPos, ballPref);
                count++;
            }
        }
        
    }

    /// <summary>
    /// Use A* Pathfinding to create a path from source to destination, Notice that the Vector2Int is the coordinate in matrix
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <param name="boardRow"></param>
    /// <param name="boardCol"></param>
    /// <returns>A Stack of Vector2Int, which is coordinate in matrix created by boardRow and boardCol, 
    /// with the top is the destinationn</returns>
    
    private Stack<Vector2Int> AStarPathFinding(Vector2Int source, Vector2Int dest, int boardRow, int boardCol)
    {
        
        //Create a closed list
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

        //Initialize the parameters of the starting node
        iRow = source.x;
        iCol = source.y;
        cellMatrix[iRow, iCol].SetData(iRow, iCol, 0, 0, 0);


        //Create an open list and add the first starting node
        List<PPair> openList = new List<PPair>();
        PPair firstPair = new PPair();
        firstPair.SetData(0, new Vector2Int(iRow, iCol));
        openList.Add(firstPair);

        bool foundDest = false;
        Vector2Int currentSuccessor = Vector2Int.zero;
        Stack<Vector2Int> path = new Stack<Vector2Int>();
        
        while (openList.Count >= 0)
        {
            var pPair = openList[0];

            openList.RemoveAt(0);

            //Add this source to the close list
            iRow = pPair.coord.x;
            iCol = pPair.coord.y;
            closeList[iRow, iCol] = true;

            //Generating 4 succesor of each cell, because only ball cant allow to move digonally
            //Cell-- > Popped Cell(i, j)
            //N-- > North(iRow - 1, iCol)
            //S-- > South(iRow + 1, iCol)
            //E-- > East(iRow, iCol + 1)
            //W-- > West(iRow, iCol - 1)

            float gNew, hNew, fNew;

            //----North Succesor----
            currentSuccessor.x = iRow - 1;
            currentSuccessor.y = iCol;
            if (IsValid(currentSuccessor))
            {
                //If this is the destination cell
                if (IsDestination(dest, currentSuccessor))
                {
                    // Set the Parent of the destination cell
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].rowParent = iRow;
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].colParent = iCol;
                    path = TracePath(cellMatrix, dest);

                    foundDest = true;
                    break;
                }
                //If the successor is already in the closed list or if it is blocked, then ignore it.Else do the following
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !HasBallOn(currentSuccessor))
                {
                    gNew = cellMatrix[currentSuccessor.x, currentSuccessor.y].g + 1.0f;
                    hNew = CalculateHeuristics(dest,currentSuccessor);
                    fNew = gNew + hNew;

                    // If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. 
                    //Record the f, g, and h costs of the square cell
                    //                OR
                    // If it is on the open list already, check
                    // to see if this path to that square is
                    // better, using 'f' cost as the measure.
                    if (cellMatrix[currentSuccessor.x, currentSuccessor.y].h == float.MaxValue || cellMatrix[currentSuccessor.x, currentSuccessor.y].f > fNew)
                    {
                        PPair newPPair = new PPair();
                        newPPair.SetData(fNew, currentSuccessor);
                        openList.Add(newPPair);

                        //Save the data of this succescor
                        cellMatrix[currentSuccessor.x, currentSuccessor.y].SetData(iRow, iCol, fNew, gNew, hNew);
                    }
                }


            }

            //----South Succesor----
            currentSuccessor.x = iRow + 1;
            currentSuccessor.y = iCol;
            if (IsValid(currentSuccessor))
            {
                //If this is the destination cell
                if (IsDestination(dest, currentSuccessor))
                {
                    // Set the Parent of the destination cell
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].rowParent = iRow;
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].colParent = iCol;
                    path = TracePath(cellMatrix, dest);

                    foundDest = true;
                    break;
                }
                //If the successor is already in the closed list or if it is blocked, then ignore it.Else do the following
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !HasBallOn(currentSuccessor))
                {
                    gNew = cellMatrix[currentSuccessor.x, currentSuccessor.y].g + 1.0f;
                    hNew = CalculateHeuristics(dest, currentSuccessor);
                    fNew = gNew + hNew;

                    // If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. 
                    //Record the f, g, and h costs of the square cell
                    //                OR
                    // If it is on the open list already, check
                    // to see if this path to that square is
                    // better, using 'f' cost as the measure.
                    if (cellMatrix[currentSuccessor.x, currentSuccessor.y].h == float.MaxValue || cellMatrix[currentSuccessor.x, currentSuccessor.y].f > fNew)
                    {
                        PPair newPPair = new PPair();
                        newPPair.SetData(fNew, currentSuccessor);
                        openList.Add(newPPair);

                        //Save the data of this succescor
                        cellMatrix[currentSuccessor.x, currentSuccessor.y].SetData(iRow, iCol, fNew, gNew, hNew);
                    }
                }


            }

            //----East Succesor----
            currentSuccessor.x = iRow;
            currentSuccessor.y = iCol + 1;
            if (IsValid(currentSuccessor))
            {
                //If this is the destination cell
                if (IsDestination(dest, currentSuccessor))
                {
                    // Set the Parent of the destination cell
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].rowParent = iRow;
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].colParent = iCol;
                    path = TracePath(cellMatrix, dest);

                    foundDest = true;
                    break;
                }
                //If the successor is already in the closed list or if it is blocked, then ignore it.Else do the following
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !HasBallOn(currentSuccessor))
                {
                    gNew = cellMatrix[currentSuccessor.x, currentSuccessor.y].g + 1.0f;
                    hNew = CalculateHeuristics(dest, currentSuccessor);
                    fNew = gNew + hNew;

                    // If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. 
                    //Record the f, g, and h costs of the square cell
                    //                OR
                    // If it is on the open list already, check
                    // to see if this path to that square is
                    // better, using 'f' cost as the measure.
                    if (cellMatrix[currentSuccessor.x, currentSuccessor.y].h == float.MaxValue || cellMatrix[currentSuccessor.x, currentSuccessor.y].f > fNew)
                    {
                        PPair newPPair = new PPair();
                        newPPair.SetData(fNew, currentSuccessor);
                        openList.Add(newPPair);

                        //Save the data of this succescor
                        cellMatrix[currentSuccessor.x, currentSuccessor.y].SetData(iRow, iCol, fNew, gNew, hNew);
                    }
                }


            }

            //----West Succesor----
            currentSuccessor.x = iRow;
            currentSuccessor.y = iCol - 1;
            if (IsValid(currentSuccessor))
            {
                //If this is the destination cell
                if (IsDestination(dest, currentSuccessor))
                {
                    // Set the Parent of the destination cell
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].rowParent = iRow;
                    cellMatrix[currentSuccessor.x, currentSuccessor.y].colParent = iCol;
                    path = TracePath(cellMatrix, dest);

                    foundDest = true;
                    break;
                }
                //If the successor is already in the closed list or if it is blocked, then ignore it.Else do the following
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !HasBallOn(currentSuccessor))
                {
                    gNew = cellMatrix[currentSuccessor.x, currentSuccessor.y].g + 1.0f;
                    hNew = CalculateHeuristics(dest, currentSuccessor);
                    fNew = gNew + hNew;

                    // If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. 
                    //Record the f, g, and h costs of the square cell
                    //                OR
                    // If it is on the open list already, check
                    // to see if this path to that square is
                    // better, using 'f' cost as the measure.
                    if (cellMatrix[currentSuccessor.x, currentSuccessor.y].h == float.MaxValue || cellMatrix[currentSuccessor.x, currentSuccessor.y].f > fNew)
                    {
                        PPair newPPair = new PPair();
                        newPPair.SetData(fNew, currentSuccessor);
                        openList.Add(newPPair);

                        //Save the data of this succescor
                        cellMatrix[currentSuccessor.x, currentSuccessor.y].SetData(iRow, iCol, fNew, gNew, hNew);
                    }
                }
            }

        }

        if (!foundDest)
            path = null;//Unable to found destination
             
        return path;
    }
    #region A* Pathfinding Utility Functions
    //Stack<Vector2Int> path = null;
    //We will use dynamic programming to trace the path
    Stack<Vector2Int> TracePath(Cell[,] cellMatrix, Vector2Int dest)
    {
        int row = dest.x;
        int col = dest.y;

        Stack<Vector2Int> path = new Stack<Vector2Int>();
        

        //Trace the cell matric to create path
        while (!(cellMatrix[row, col].rowParent == row && cellMatrix[row, col].colParent == col))
        {
            path.Push(new Vector2Int(row, col));
            int tempR = cellMatrix[row, col].rowParent;
            int tempC = cellMatrix[row, col].colParent;

            row = tempR;
            col = tempC;
        }

        return path;
    }



    private float CalculateHeuristics(Vector2Int dest, Vector2Int curSuccessor)
    {
        return Mathf.Abs(curSuccessor.x - dest.x) + Mathf.Abs(curSuccessor.y - dest.y);
    }

    private bool IsDestination(Vector2Int dest, Vector2Int curSuccessor)
    {
        return dest == curSuccessor;
    }

    private bool IsValid(Vector2Int curSuccessor)
    {
        return (curSuccessor.x >= 0) && (curSuccessor.x < MapConstants.BOARD_COL) && (curSuccessor.y >= 0) && (curSuccessor.y < MapConstants.BOARD_ROW);
    }

    

    private bool HasBallOn(Vector2 boardPosition)
    {
        //foreach (var ball in ballDictionary)
        //{
        //    if (Vector2.SqrMagnitude(ball.Key - boardPosition) < float.Epsilon)
        //        return true;
        //}
        //return false;
        return ballDictionary.ContainsKey(UtilMapHelpers.BoardToMatrixPosition(boardPosition,cellSize,boardCol,boardRow));
    }

    private bool HasBallOn(Vector2Int matPosition)
    {
        return ballDictionary.ContainsKey(matPosition);
    }

    #endregion



   
   
}
