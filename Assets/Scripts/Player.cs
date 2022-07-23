using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private int _index;

    private Character[] _characters;
    private Cage _cage;
    private House _house;

    public event Action<int> OnStartMoving;
    public event Action<int> OnFinishMoving;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public int Index
    {
        get { return _index; }
        set { _index = value; }
    }

    public Character[] Characters
    {
        get { return _characters; }
        set { _characters = value; }
    }
    public Cage Cage
    {
        get { return _cage; }
        set { _cage = value; }
    }
    public House House
    {
        get { return _house; }
        set { _house = value; }
    }

    public bool IsMoving { get; private set; }

    public void MoveCharacter(int characterIdx, Cell[] sequence)
    {
        StartCoroutine(MoveCharacterCoroutine(characterIdx, sequence));
    }


    public Character GetCharacter(int index)
    {
        return _characters[index];
    }

    private IEnumerator MoveCharacterCoroutine(int characterIdx, Cell[] sequence)
    {
        IsMoving = true;
        OnStartMoving?.Invoke(sequence[0].Index);
        foreach (Cell cell in sequence)
        {
            yield return _characters[characterIdx].MoveToCell(cell);
        }
        IsMoving = false;
        OnFinishMoving?.Invoke(sequence[sequence.Length - 1].Index);
    }
}
