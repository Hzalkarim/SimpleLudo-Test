using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private bool _isRolling;
    private int[] _dummyRollFace = new int[]
    {
        2, 1, 3, 4, 6,
        5, 6, 3, 2, 1,
        6, 1, 4, 5, 3,
        5, 4, 1, 2, 6,
        1, 6, 5, 2, 3,
        6, 5, 3, 4, 1,
    };

    private WaitForSeconds _wait = new WaitForSeconds(.02f);

    public bool IsRolling => _isRolling;
    public int FinalRoll { get; private set; }

    public event Action OnStartRolling;
    public event Action<int> OnRolling;
    public event Action<int> OnFinishRolling;

    public void Roll()
    {
        StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        _isRolling = true;
        OnStartRolling?.Invoke();
        for (int i = 0; i < _dummyRollFace.Length; i++)
        {
            OnRolling?.Invoke(_dummyRollFace[i]);
            yield return _wait;
        }

        FinalRoll = UnityEngine.Random.Range(1, 7);
        _isRolling = false;
        OnFinishRolling?.Invoke(FinalRoll);
    }
}
