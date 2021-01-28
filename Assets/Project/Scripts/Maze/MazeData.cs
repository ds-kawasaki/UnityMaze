using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 迷路情報
/// </summary>
public class MazeData
{
    public enum CellType
    {
        Yuka,
        Wall,
        Start,
        Goal,
    }

    private List<CellType> cells;

    public Vector2Int Size
    {
        private set;
        get;
    }

    public MazeData(Vector2Int size)
    {
        this.Size = size;

        int cellNum = this.Size.x * this.Size.y;
        this.cells = new List<CellType>(cellNum);
        for (int i = 0; i < cellNum; ++i)
        {
            this.cells.Add(CellType.Yuka);
        }
    }

    public void SetCell(Vector2Int pos, CellType cell)
    {
        if (pos.x < 0
            || pos.x >= this.Size.x
            || pos.y < 0
            || pos.y >= this.Size.y)
        {
            return;
        }

        this.cells[pos.x + pos.y * this.Size.x] = cell;
    }

    public CellType GetCell(Vector2Int pos)
    {
        if (pos.x < 0) { pos.x = 0; }
        if (pos.x >= this.Size.x) { pos.x = this.Size.x - 1; }
        if (pos.y < 0) { pos.y = 0; }
        if (pos.y >= this.Size.y) { pos.y = this.Size.y - 1; }

        return this.cells[pos.x + pos.y * this.Size.x];
    }
}
