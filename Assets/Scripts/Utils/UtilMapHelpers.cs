using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilMapHelpers
{
    /// <summary>
    /// Giving a wolrd position, return a board position
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <param name="cellSize"></param>
    /// <param name="maxCol"></param>
    /// <param name="maxRow"></param>
    /// <returns></returns>
    public static Vector2 WorldToBoardPosition(Vector2 worldPosition, Vector2 cellSize, int maxCol, int maxRow)
    {
        int iCol = Mathf.RoundToInt(worldPosition.x / cellSize.x);
        int iRow = Mathf.RoundToInt(worldPosition.y / cellSize.y);

        Vector2 boardPos = Vector2.zero;
        boardPos.x = iCol * cellSize.x;
        boardPos.y = iRow * cellSize.y;
        return boardPos;
    }

    /// <summary>
    /// Gving a board position, return a position in matrix (starting at (0,0) in the top left)
    /// </summary>
    /// <returns></returns>
    public static Vector2Int BoardToMatrixPosition(Vector2 boardPosition, Vector2 cellSize, int maxCol, int maxRow)
    {
        //Reverse because the matrix is [irow,icol]
        Vector2Int matrixPosition = Vector2Int.zero;
        matrixPosition.x = maxRow /2 - Mathf.RoundToInt(boardPosition.y / cellSize.y);
        matrixPosition.y = maxCol/2 + Mathf.RoundToInt(boardPosition.x / cellSize.x);


        return matrixPosition;
    }

    public static Vector2 MatrixToBoardPosition(Vector2Int matrixPosition, Vector2 cellSize, int maxCol, int maxRow)
    {
        Vector2 boardPosition = Vector2.zero;

        return boardPosition;
    }

}
