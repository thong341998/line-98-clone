using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonConstants;
public class MapProvider : MonoBehaviour
{
    public static MapProvider Instance = null;

    [SerializeField]
    private GameObject board;
    public Camera gameCam;


    private SpriteRenderer boardSprite;

    [HideInInspector]
    public Vector2 boardSize = new Vector2();

    [HideInInspector]
    public Vector2 cellSize = new Vector2();

    [HideInInspector]
    public Vector2 firstCellPos = new Vector2();

    public int HalfCol => MapConstants.BOARD_COL / 2;
    public int HalfRow => MapConstants.BOARD_ROW / 2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            DestroyImmediate(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        boardSprite = board.GetComponent<SpriteRenderer>();

        boardSize.x = boardSprite.sprite.bounds.size.x * board.transform.lossyScale.x;
        boardSize.y = boardSprite.sprite.bounds.size.y * board.transform.lossyScale.y;

        cellSize.x = boardSize.x /  MapConstants.BOARD_COL;
        cellSize.y = boardSize.y /  MapConstants.BOARD_ROW;

        int halfCol = MapConstants.BOARD_COL/2 ;
        int halfRow = MapConstants.BOARD_ROW / 2;

        firstCellPos.x = board.transform.position.x - cellSize.x * halfCol;
        firstCellPos.y = board.transform.position.y + cellSize.y * halfRow;
       // Debug.Log(string.Format(
            //"board size: ({0},{1}), cell size:({2},{3}), first cell pos:({4},{5})",boardSize.x,boardSize.y,cellSize.x,cellSize.y, firstCellPos.x,firstCellPos.y));

        
    }

   
}
