using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _fill;
    [SerializeField]
    private SpriteRenderer _border;

    public int Index { get; set; }

    public event Action<int> OnClick;

    public void Init()
    {
        Index = int.Parse(name.Split('_')[1]);
    }

    public Color GetFillColor()
    {
        return _fill.color;
    }

    public Color GetBorderColor()
    {
        return _border.color;
    }

    public void SetFillColor(Color color)
    {
        _fill.color = color;
    }

    public void SetBorderColor(Color color)
    {
        _border.color = color;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnMouseDown()
    {
        Debug.Log($"{name} clicked!");
        OnClick?.Invoke(Index);
    }
}
