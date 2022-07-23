using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private int _currentCellIndex;
    private string _teamName;

    [SerializeField]
    private Color _color;
    [SerializeField]
    private SpriteRenderer[] _colorStripe;

    public int CurrentCellIndex => _currentCellIndex;
    public string PlayerName
    {
        get
        {
            if (_teamName == null)
                _teamName = name.Split('_')[1];
            return _teamName;
        }
    }
    public bool IsInCage { get; set; }

    public IEnumerator MoveToCell(Cell cell)
    {
        SetPosition(cell);
        yield return new WaitForSeconds(.1f);
    }

    public void SetPosition(Cell cell)
    {
        //Debug.Log($"Move {name} from {_currentCellIndex} to {cell.Index}");
        transform.position = cell.GetPosition();
        _currentCellIndex = cell.Index;
    }

    public void SetColor(Color color)
    {
        foreach (var stripe in _colorStripe)
        {
            stripe.color = color;
        }
    }
}
