using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField]
    private PlayerUniqueCellData[] _playerTemplateData;
    [SerializeField]
    private Cell _startFromZero;

    [SerializeField]
    private GameObject _characterContainer;
    [SerializeField]
    private Character _characterPrefab;

    private Cell[] _cells;
    private List<Character> _characters = new List<Character>();

    public event Action<int> OnCellClick;

    public Cell[] Cells => _cells;
    public List<Character> Characters => _characters;

    public void Init()
    {
        _cells = GetComponentsInChildren<Cell>();
        foreach (Cell cell in _cells)
        {
            cell.Init();
            cell.OnClick += OnCellClickedListener;
        }
        //RegisterAllCellsClickListener();
    }

    public Cell[] GetCellSequence(Player player, int characterIdx, int step, bool backtrack = false)
    {
        int[] seq = new int[step + 1];
        Character playerChar = player.GetCharacter(characterIdx);
        seq[0] = playerChar.CurrentCellIndex;

        bool isForward = true;
        for (int i = 1; i <= step; i++)
        {
            if (isForward && seq[i - 1] == player.House.IntersectionCellIndex)
            {
                seq[i] = player.House.EntranceCellIndex;
                continue;
            }

            if (isForward && seq[i - 1] == _startFromZero.Index)
            {
                seq[i] = 0;
                continue;
            }

            if (seq[i - 1] == player.House.CellIndex)
            {
                if (backtrack)
                {
                    seq[i] = player.House.CellIndex - 1;
                    isForward = false;
                }
                else
                {
                    seq[i] = player.House.CellIndex;
                }
                continue;
            }

            if (backtrack &&  !isForward && seq[i - 1] == player.House.EntranceCellIndex)
            {
                seq[i] = player.House.IntersectionCellIndex;
                continue;
            }

            if (isForward)
                seq[i] = seq[i - 1] + 1;
            else if (backtrack && !isForward)
                seq[i] = seq[i - 1] - 1;
        }

        return seq.Select(i => _cells[i]).ToArray();
    }

    public void RegisterPlayer(Player player, int index, int characterCount)
    {
        Cage cage = new Cage(_playerTemplateData[index].cage, _playerTemplateData[index].startingPoint);
        House house = new House(_playerTemplateData[index].House, _playerTemplateData[index].toHouseIntersection);

        Debug.Log($"Cage starting point {player.name}: {_playerTemplateData[index].startingPoint}");
        Character[] chars = new Character[characterCount];
        for (int i = 0; i < characterCount; i++)
        {
            chars[i] = Instantiate(_characterPrefab, _characterContainer.transform);
            chars[i].SetColor(_playerTemplateData[index].color);
            chars[i].name = $"Character_{player.Name}_{i}";
            cage.Enter(chars[i]);
        }

        player.Cage = cage;
        player.House = house;
        player.Characters = chars;

        _characters.AddRange(chars);
    }

    public void UnregisterPlayer(Player player)
    {
        player.Cage = null;
        player.House = null;
        int charCount = player.Characters.Length;
        for (int i = 0; i < charCount; i++)
        {
            _characters.Remove(player.Characters[i]);
            Destroy(player.Characters[i].gameObject);
        }
        player.Characters = null;
    }

    private void RegisterAllCellsClickListener()
    {
        foreach (Cell cell in _cells)
        {
            cell.OnClick += OnCellClickedListener;
        }
    }

    private void UnregisterAllCellsClickListener()
    {
        foreach (Cell cell in _cells)
        {
            cell.OnClick -= OnCellClickedListener;
        }
    }

    private void OnCellClickedListener(int index)
    {
        OnCellClick?.Invoke(index);
    }
}

[System.Serializable]
public class PlayerUniqueCellData
{
    public Cell[] cage;
    public Cell startingPoint;
    public Cell toHouseIntersection;
    public Cell House;

    public Color color;
}
