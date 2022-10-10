using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilMapHelpers
{
    public static Vector2 WorldToBoardPosition(Vector2 worldPosition, Vector2 cellSize, int maxCol, int maxRow)
    {
        int iCol = Mathf.RoundToInt(worldPosition.x / cellSize.x);
        int iRow = Mathf.RoundToInt(worldPosition.y / cellSize.y);

        Vector2 boardPos = Vector2.zero;
        boardPos.x = iCol * cellSize.x;
        boardPos.y = iRow * cellSize.y;
        return boardPos;
    }
}
