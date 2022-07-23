using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House
{
    private Cell _cell;
    private Cell _intersectionCell;

    public int CellIndex => _cell.Index;
    public int EntranceCellIndex => _cell.Index - 5;
    public int IntersectionCellIndex => _intersectionCell.Index;

    public House(Cell cell, Cell intersection)
    {
        _cell = cell;
        _intersectionCell = intersection;
    }

}
