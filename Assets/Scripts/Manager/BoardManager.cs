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
    [SerializeField] private GameObject ballPrefs;
    [SerializeField] private int ballCount;
    public static BoardManager Instance = null;

    private Dictionary<Vector2, BallController> ballDictionary = new Dictionary<Vector2, BallController>(); // Use to store ball with its board position

    
    

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 worldPos = MapProvider.Instance.gameCam.ScreenToWorldPoint(eventData.position);

        //Snap to board position
        Vector2 boardPosition = UtilMapHelpers.WorldToBoardPosition(worldPos, MapProvider.Instance.cellSize, MapConstants.BOARD_COL, MapConstants.BOARD_ROW);

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


    private void GenerateBalls(int ballCount)
    {
        var balls = GetComponentsInChildren<BallController>();
        for (int i = 0; i < balls.Length; i++)
        {
            ballDictionary.Add(balls[i].transform.position, balls[i]);
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
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !IsBlocked(currentSuccessor))
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
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !IsBlocked(currentSuccessor))
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
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !IsBlocked(currentSuccessor))
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
                else if (closeList[currentSuccessor.x, currentSuccessor.y] == false && !IsBlocked(currentSuccessor))
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

        path.Push(new Vector2Int(row, col));
     
        
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

    private bool IsBlocked(Vector2Int matrixPosition)
    {
        var boardPosition = UtilMapHelpers.MatrixToBoardPosition(matrixPosition, MapProvider.Instance.cellSize, MapConstants.BOARD_COL, MapConstants.BOARD_ROW);
        return ballDictionary.ContainsKey(boardPosition);//NOTICE: Test this function carefully
    }

    #endregion



   
   
}
