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
    public static event Action<int> OnBallLineDestroy = delegate { };

    public static BoardManager Instance = null;

    [Header("References")]
    [SerializeField] private GameObject ballPrefabs;
    [SerializeField] private GameObject queueBallPrefabs;
    [SerializeField] private BallQueueVisualizer ballQueueVisualizer;

    [Header("Configuration")]
    [Header("Ball Generation")]
    
    [SerializeField] private Color[] colorArray;
    [SerializeField] private int ballCount;
    [SerializeField] private int queueBallCount;

    [Header("Bal Lines")]
    [SerializeField] private int destroyAmmount = 3;//Number of ball created a line and can be destroy

    [Header("Debugging")]
    [SerializeField] private bool drawPath;
    [SerializeField] private bool drawClick;
    [SerializeField] private bool drawAllEmptyPos;

    #region Cache Value
    //Data structures, collections
    private Queue<Vector2> boardPath = new Queue<Vector2>(); // The path generate from A* Pathfinding in board position
    private Stack<Vector2Int> ballPath = new Stack<Vector2Int>();
    private Dictionary<Vector2Int, BallController> ballDictionary = new Dictionary<Vector2Int, BallController>(); // Use to store ball with its matrix position
    private List<Vector2Int> emptyMatrixPosition = new List<Vector2Int>();
    private Dictionary<Vector2Int, BallQueueController> queueBallDictionary = new Dictionary<Vector2Int, BallQueueController>();

    //Class instances
    private BallController currentSelectedBall = null;

    //Data types
    private int boardRow;
    private int boardCol;
    private Vector2 cellSize;
    private Coroutine enlargeQueueBallCR;
    
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
        GenerateBalls();
        GenerateQueueBall();
    }

    void OnDrawGizmos()
    {
        
        if (drawPath)
        {
            Gizmos.color = Color.black;
            foreach (var center in boardPath)
            {
                Gizmos.DrawSphere(new Vector3(center.x, center.y), 0.1f);
            }
            //Gizmos.DrawSphere(prevBoard, 0.1f);
        }

        if (drawClick)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(_boardPosition, 0.1f);
        }

        if (drawAllEmptyPos)
        {
            Gizmos.color = Color.cyan;
            foreach (var empty in emptyMatrixPosition)
            {
                Gizmos.DrawCube(UtilMapHelpers.MatrixToBoardPosition(empty, cellSize, boardCol, boardRow), Vector3.one * 0.2f);
            }
            
        }
    }

    private void InitValues()
    {
        //Cache value
        boardRow = MapConstants.BOARD_ROW;
        boardCol = MapConstants.BOARD_COL;
        cellSize = MapProvider.Instance.cellSize;

        //Init all matrix bot position
        Vector2Int cur = new Vector2Int();
        for (int i = 0; i < boardRow; i++)
        {
            for (int j = 0; j < boardCol; j++)
            {
                cur.x = i;
                cur.y = j;
                emptyMatrixPosition.Add(cur);
            }
        }
    }

    Vector2Int prevBoard;
    Vector2Int clickMatPos;
    Vector2 _boardPosition;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.GameState != GameState.Playing)
            return;


        //Debug.Log("On Click");
        Vector2 worldPos = MapProvider.Instance.gameCam.ScreenToWorldPoint(eventData.position);

        //Snap to board position
        Vector2 boardPosition = UtilMapHelpers.WorldToBoardPosition(worldPos, cellSize, boardCol, boardRow);
        _boardPosition = boardPosition;
        clickMatPos = UtilMapHelpers.BoardToMatrixPosition(boardPosition, cellSize, boardCol, boardRow);
        bool hasBallOnPos = HasBallOn(boardPosition);
        //Debug.Log(hasBallOnPos);
        //Check if the board postion has the ball in it
        if (hasBallOnPos)
        {
            //Debug.Log("has ball on: " + clickMatPos);

            //If click ball and click on another ball, then cancel previous ball, s
            BallController prevBall = currentSelectedBall;

            //Changed currentSelecedBall to the current ball
            currentSelectedBall = ballDictionary[UtilMapHelpers.BoardToMatrixPosition(boardPosition,cellSize,boardCol,boardRow)];
            Vector2Int selectedBallMatPos = UtilMapHelpers.BoardToMatrixPosition(
                currentSelectedBall.transform.position, cellSize, boardCol, boardRow);

            //Click on the same position and has ball            
            if (prevBoard == clickMatPos)
            {
                Debug.Log("Click on same bal with: " + prevBoard + " and " + selectedBallMatPos);
                return;
            }

        
            if (prevBall != null) prevBall.OnBallCancelSelected();
            currentSelectedBall.OnBallSelected();
        }

        //Else if click on empty cell and the current selected ball is not empty, then generate path and move ball on that path
        else
        {
            //Debug.Log(clickMatPos +  " empty");
            if (currentSelectedBall != null)
            {


                ballPath.Clear();
                boardPath.Clear();


                var ballMatPos = UtilMapHelpers.BoardToMatrixPosition(currentSelectedBall.transform.position, cellSize, boardCol, boardRow);
                //Debug.Log(ballMatPos);
                var destMatPos = UtilMapHelpers.BoardToMatrixPosition(boardPosition, cellSize, boardCol, boardRow);
                ballPath = AStarPathFinding(ballMatPos, destMatPos, boardRow, boardCol);

                //If can find any path, then reset every thing
                if (ballPath == null)
                {
                    Debug.Log("Cant find ball path!");
                    currentSelectedBall.OnBallCancelSelected();
                    ResetValues();
                    return;
                }


                while (ballPath.Count != 0)
                {
                    var waypoint = ballPath.Pop();
                    var boardPos = UtilMapHelpers.MatrixToBoardPosition(waypoint, cellSize, boardCol, boardRow);
                    boardPath.Enqueue(boardPos);
                    //Debug.Log(boardPosition);
                }


                //Set path for current ball to move itself and subcribe to even when ball move completed
                currentSelectedBall.SetPath(boardPath);
                UpdateNormalBallPos(prevBoard);
                currentSelectedBall.OnBallMoveCompleted += CurrentSelectedBall_OnBallMoveCompleted;
            }

        }


        prevBoard = clickMatPos;
        //Debug.Log("save prev board: " + prevBoard);
    }

    void AddNewNormalBall(Vector2Int matPos,BallBase newBall) 
    {
        if (newBall.GetType() == typeof(BallController))
        {
            ballDictionary.Add(matPos,(BallController) newBall);
        }
        else if (newBall.GetType() == typeof(BallQueueController))
        {
            queueBallDictionary.Add(matPos, (BallQueueController)newBall);
        }

        emptyMatrixPosition.Remove(matPos);
    }

    void UpdateNormalBallPos(Vector2Int matPos)
    {
        ballDictionary.Remove(matPos);
        emptyMatrixPosition.Add(matPos);
    }

    private void CurrentSelectedBall_OnBallMoveCompleted()
    {
        //Update the position of the ball dictionary and empty cell matrix
        var currentMatPos = UtilMapHelpers.BoardToMatrixPosition(currentSelectedBall.transform.position, cellSize, boardCol, boardRow);
        AddNewNormalBall(currentMatPos, currentSelectedBall);
        

        //Check if that pos has the queue ball, then replace the queue ball with current ball
        if (queueBallDictionary.ContainsKey(clickMatPos))
        {
            var queueBall = queueBallDictionary[clickMatPos];
            Destroy(queueBall.gameObject);

            queueBallDictionary.Remove(clickMatPos);

        }

        //Enlarge all queue balls, wait for completed all enlargion and create generate new queue ball
        EnlargeQueueBalls(() =>
        {

            //Check if there is no cell to generate ball
            if (emptyMatrixPosition.Count == 0)
            {
                //Then game lose
                GameManager.Instance.LoseGame();
                return;
            }

            ChangeQueueBallsToNormalBalls();
            GenerateQueueBall();

            //Find and destroy ball lines
            DestroyBallLines();

            //End turn,  change to the next ball by reseting values
            currentSelectedBall.OnBallMoveCompleted -= CurrentSelectedBall_OnBallMoveCompleted;
            ResetValues();
            
        });
    }

    private void ChangeQueueBallsToNormalBalls()
    {
        foreach (var queueBall in queueBallDictionary)
        {
            queueBall.Value.gameObject.AddComponent<BallController>();
            
            var ballPref = Instantiate(ballPrefabs, queueBall.Value.transform.position, Quaternion.identity, transform).GetComponent<BallController>();
            ballPref.InitValues();
            ballPref.SetData(queueBall.Value.BallColor);
            ballDictionary.Add(queueBall.Key, ballPref);
            Destroy(queueBall.Value.gameObject);
        }

        queueBallDictionary.Clear();
    }

    private void EnlargeQueueBalls(System.Action completed)
    {

        if (enlargeQueueBallCR != null)
            StopCoroutine(enlargeQueueBallCR);

        enlargeQueueBallCR = StartCoroutine(CR_EnlargeQueueBalls(completed));
    }

    private IEnumerator CR_EnlargeQueueBalls(System.Action completed)
    {
        float enlargeTime = 0f;
        foreach (var queueBall in queueBallDictionary)
        {
            enlargeTime = queueBall.Value.EnlargeTime;
            queueBall.Value.EnlargeToNormal();
        }

        yield return new WaitForSeconds(enlargeTime);
        completed?.Invoke();
    }

    private void ResetValues()
    {
        currentSelectedBall = null;
        prevBoard = new Vector2Int(-1, -1);
        //Debug.Log("reset prev board: " + prevBoard);
    }

    private void BallController_OnPlayerClick(BallController ballController)
    {
        if (currentSelectedBall != ballController)
        {
            currentSelectedBall = ballController;
        }
    }

    //Find the line including with balls
    private void DestroyBallLines()
    {
        //Start at the current ball position
        Color curBallColor = currentSelectedBall.BallColor;
        Vector2Int curMatPos = clickMatPos;
        Queue<BallController> ballLines = new Queue<BallController>();
        ballLines.Enqueue(currentSelectedBall);
        //----Vertical Check ------     
        while (curMatPos.x >= 0  )
        {
            curMatPos.x--;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }
        curMatPos.x = clickMatPos.x;
        while (curMatPos.x < boardRow)
        {
            curMatPos.x++;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }

        //Check if this lines longer than the player desire values
        if (ballLines.Count >= destroyAmmount)
        {
            DestroyBallLines(ballLines);
        }

        //------Horizonal Check------
        ballLines.Clear();
        ballLines.Enqueue(currentSelectedBall);
        curMatPos = clickMatPos;
        while (curMatPos.y >=0)
        {
            curMatPos.y--;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }
        curMatPos.y = clickMatPos.y;
        while (curMatPos.y <boardCol)
        {
            curMatPos.y++;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }

        //Check if this lines longer than the player desire values
        if (ballLines.Count >= destroyAmmount)
        {
            DestroyBallLines(ballLines);
        }

        //----First Diagonal Check----
        ballLines.Clear();
        ballLines.Enqueue(currentSelectedBall);
        curMatPos = clickMatPos;
        while (curMatPos.x >= 0 && curMatPos.y >= 0)
        {
            curMatPos.x--;
            curMatPos.y--;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }

        curMatPos = clickMatPos;
        while (curMatPos.x < boardRow && curMatPos.y < boardCol)
        {
            curMatPos.x++;
            curMatPos.y++;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }


        //Check if this lines longer than the player desire values
        if (ballLines.Count >= destroyAmmount)
        {
            DestroyBallLines(ballLines);
        }

        //----Second Diagonal Check
        ballLines.Clear();
        ballLines.Enqueue(currentSelectedBall);
        curMatPos = clickMatPos;

        while (curMatPos.x >=0 && curMatPos.y < boardCol)
        {
            curMatPos.x--;
            curMatPos.y++;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }

        curMatPos = clickMatPos;
        while (curMatPos.x < boardRow && curMatPos.y >= 0)
        {
            curMatPos.x++;
            curMatPos.y--;
            if (HasBallOn(curMatPos))
            {
                var ballEntry = ballDictionary[curMatPos];
                if (ballEntry.BallColor == curBallColor)
                {
                    ballLines.Enqueue(ballEntry);
                }
                else break;
            }
            else break;
        }

        //Check if this lines longer than the player desire values
        if (ballLines.Count >= destroyAmmount)
        {
            //Debug.Log("Destroy ball diagonal line!");
            DestroyBallLines(ballLines);
        }
    }


    private void DestroyBallLines(Queue<BallController> ballLines)
    {
        int ballCount = ballLines.Count;
        while (ballLines.Count > 0)
        {
            var ball = ballLines.Dequeue();
            Destroy(ball.gameObject);
        }

        //Fire ball line destroy event
        OnBallLineDestroy?.Invoke(ballCount);
    }

    private void GenerateQueueBall()
    {

        int exactQueueBallCount = queueBallCount <= emptyMatrixPosition.Count ? queueBallCount : (queueBallCount - emptyMatrixPosition.Count);
        //If 

        Color randColor = new Color();

        for (int i = 0; i < exactQueueBallCount; i++)
        {
            randColor = colorArray[Random.Range(0, colorArray.Length)];
            var randomMatPos = emptyMatrixPosition[Random.Range(0, emptyMatrixPosition.Count)];
            var boardPos = UtilMapHelpers.MatrixToBoardPosition(randomMatPos, cellSize, boardCol, boardRow);
            var ballPref = Instantiate(queueBallPrefabs, boardPos, Quaternion.identity, transform).GetComponent<BallQueueController>();
            ballPref.InitValues();
            ballPref.SetData(randColor);
            queueBallDictionary.Add(randomMatPos, ballPref);
            emptyMatrixPosition.Remove(randomMatPos);
        }

        ballQueueVisualizer.Visualize(queueBallDictionary);
    }



    private void GenerateBalls()
    {
        Color randColor = new Color();

        for (int i = 0; i < ballCount; i++)
        {
            randColor = colorArray[Random.Range(0, colorArray.Length)];
            var randomMatPos = emptyMatrixPosition[Random.Range(0, emptyMatrixPosition.Count)];
            var boardPos = UtilMapHelpers.MatrixToBoardPosition(randomMatPos, cellSize, boardCol, boardRow);
            var ballPref = Instantiate(ballPrefabs, boardPos, Quaternion.identity, transform).GetComponent<BallController>();
            ballPref.InitValues();
            ballPref.SetData(randColor);
            ballDictionary.Add(randomMatPos, ballPref);
            emptyMatrixPosition.Remove(randomMatPos);
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
        
        while (openList.Count > 0)
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
        {
            path = null;//Unable to found destination
        }

        //Debug.Log(foundDest ? "Found path!" : "Not found path");
            
             
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
