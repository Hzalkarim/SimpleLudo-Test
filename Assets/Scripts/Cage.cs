using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Cage
{
    private Cell[] _cells;
    private Cell _startingPoint;

    private bool[] _isFilled;

    public Cell[] Cells => _cells;
    public Cell StartingPoint => _startingPoint;

    public Cage(Cell[] cells, Cell startingPoint)
    {
        _cells = cells;
        _startingPoint = startingPoint;

        _isFilled = new bool[cells.Length];
    }

    public bool IsIndexInside(int i)
    {
        return _cells.Any(c => c.Index == i);
    }

    public void Enter(Character character)
    {
        for (int i = 0; i < _isFilled.Length; i++)
        {
            if (!_isFilled[i])
            {
                _isFilled[i] = true;
                character.SetPosition(_cells[i]);
                character.IsInCage = true;
                break;
            }
        }
    }

    public void Exit(Character character)
    {
        int i = 0;
        foreach (Cell cell in _cells)
        {
            if (cell.Index == character.CurrentCellIndex)
            {
                _isFilled[i] = false;
                break;
            }
            i++;
        }
        character.SetPosition(_startingPoint);
        character.IsInCage = false;

    }
}
