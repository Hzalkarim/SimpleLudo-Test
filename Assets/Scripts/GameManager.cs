using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Board _board;
    [SerializeField]
    private Dice _dice;
    [SerializeField]
    private Player[] _players;
    [SerializeField]
    [Range(1, 4)]
    private int _characterPerPlayer;

    [Header("Rules")]
    [SerializeField]
    private bool _mandatorySix = false;
    [SerializeField]
    private bool _useBacktrack = false;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI _currentPlayerText;
    [SerializeField]
    private TextMeshProUGUI _diceRollText;
    [SerializeField]
    private Button _moveButton;
    [SerializeField]
    private Button _diceRollButton;
    [SerializeField]
    private Toggle _mandatorySixToggle;
    [SerializeField]
    private Toggle _useBacktrackToggle;
    [SerializeField]
    private Color _selectedCellColor;

    private Character _selectedCharacter;

    private int _diceRoll = 1;
    private int _duplicateSix = 0;
    private int _currentPlayerIndex = 0;

    private int _currentCellIndex = -1;
    private Color _currentCellColor;

    private int _gameRound = 0;

    public bool MandatorySix => _mandatorySix;
    public bool UseBacktrack => _useBacktrack;

    public void Start()
    {
        _board.Init();
        _board.OnCellClick += OnCellClickedListener;

        RegisterAllPlayer();

        _useBacktrackToggle.isOn = _useBacktrack;
        _mandatorySixToggle.isOn = _mandatorySix;

        _useBacktrackToggle.onValueChanged.AddListener(SetUseBacktrack);
        _mandatorySixToggle.onValueChanged.AddListener(SetMandatorySix);

        _moveButton.onClick.AddListener(OnMoveButtonClickListener);
        _diceRollButton.onClick.AddListener(OnRollButtonClickListener);

        _dice.OnStartRolling += OnDiceStartRollListener;
        _dice.OnRolling += OnDiceRollingListener;
        _dice.OnFinishRolling += OnDiceFinishRollListener;
    }

    public void RegisterAllPlayer()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            _board.RegisterPlayer(_players[i], i, _characterPerPlayer);
            _players[i].OnStartMoving += OnPlayerStartTurn;
            _players[i].OnFinishMoving += OnPlayerFinishTurn;
            Debug.Log($"Register Player {_players[i].Name}");
        }
    }

    public void UnregisterAllPlayer()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            _board.UnregisterPlayer(_players[i]);
            _players[i].OnStartMoving -= OnPlayerStartTurn;
            _players[i].OnFinishMoving -= OnPlayerFinishTurn;
            Debug.Log($"Unregister Player {_players[i].Name}");
        }
    }

    public void RestartGame()
    {
        UnregisterAllPlayer();
        RegisterAllPlayer();

        _currentPlayerIndex = 0;
        _gameRound = 0;

        UpdateCurrentPlayerText();
        UpdateRollText(0);

        _moveButton.interactable = false;
        _diceRollButton.interactable = true;
    }

    public void SetMandatorySix(bool val)
    {
        _mandatorySix = val;
    }

    public void SetUseBacktrack(bool val)
    {
        _useBacktrack = val;
    }

    public void OnValueChangeCharacterAmountDropdown(int val)
    {
        _characterPerPlayer = val + 1;
    }

    private void OnCellClickedListener(int index)
    {
        if (_currentCellIndex >= 0)
        {
            _board.Cells[_currentCellIndex].SetFillColor(_currentCellColor);
            StopCoroutine(SelectedCellColorTimeout());
        }

        _currentCellIndex = index;
        _currentCellColor = _board.Cells[index].GetFillColor();
        _board.Cells[index].SetFillColor(_selectedCellColor);
        StartCoroutine(SelectedCellColorTimeout());

    }

    private void OnMoveButtonClickListener()
    {
        if (_diceRoll == 0 || _currentCellIndex < 0) return;

        Player player = _players[_currentPlayerIndex];

        Debug.Log($"Player Move name {player.name}");

        int i = 0;
        for (; i < player.Characters.Length; i++)
        {
            if (player.Characters[i].CurrentCellIndex == _currentCellIndex)
            {
                _selectedCharacter = player.Characters[i];
                break;
            }
        }

        if (_selectedCharacter == null) return;

        if (player.Cage.IsIndexInside(_selectedCharacter.CurrentCellIndex))
        {
            if (_mandatorySix && _gameRound > 0 && _diceRoll != 6) return;

            if (CheckConflictingCell(player.Cage.StartingPoint, _selectedCharacter))
            {
                player.Cage.Exit(_selectedCharacter);
                OnCellClickedListener(player.Cage.StartingPoint.Index);
                Debug.Log($"Exited {_selectedCharacter.name}");
            }
            _selectedCharacter = null;
            return;
        }

        Cell[] cells = _board.GetCellSequence(player, i, _diceRoll, _useBacktrack);

        player.MoveCharacter(i, cells);
    }

    private bool CheckConflictingCell(Cell cell, Character activeCharacter)
    {
        Debug.Log($"Character num {_board.Characters.Count}");
        for (int i = 0; i < _board.Characters.Count; i++)
        {
            Character characterAtCell = _board.Characters[i];
            if (activeCharacter == characterAtCell)
                continue;
            if (!characterAtCell.IsInCage && characterAtCell.CurrentCellIndex == cell.Index)
            {
                if (characterAtCell.PlayerName.Equals(activeCharacter.PlayerName))
                {
                    return false;
                }
                else
                {
                    KnockCharacterToCage(characterAtCell);
                    return true;
                }
            }
        }

        return true;
    }

    private void KnockCharacterToCage(Character character)
    {
        if (character == null) return;
        Player player = _players.First(p => p.Name.Equals(character.PlayerName));
        player.Cage.Enter(character);
    }

    #region DICE METHODS
    private void OnRollButtonClickListener()
    {
        _dice.Roll();
    }

    private void OnDiceStartRollListener()
    {
        _diceRollButton.interactable = false;
    }

    private void OnDiceRollingListener(int num)
    {
        UpdateRollText(num);
    }

    private void OnDiceFinishRollListener(int num)
    {
        if (_mandatorySix && _gameRound > 4 && num != 6)
        {
            bool allCharacterInCage = _players[_currentPlayerIndex].Characters.All(c => c.IsInCage);
            if (allCharacterInCage)
            {
                StartCoroutine(EndPlayerTurnCantMove(num));
                return;
            }
        }
        UpdateRollText(num);
        _moveButton.interactable = true;
    }

    private void UpdateRollText(int num)
    {
        _diceRoll = num;
        _diceRollText.text = _diceRoll.ToString();
    }
    #endregion

    #region PLAYER STATE METHODS
    private void UpdateCurrentPlayerText()
    {
        _currentPlayerText.text = _players[_currentPlayerIndex].Name;
    }

    private void OnPlayerStartTurn(int index)
    {
        _moveButton.interactable = false;
    }

    private void OnPlayerFinishTurn(int index)
    {
        if (CheckWinner(_players[_currentPlayerIndex], _selectedCharacter))
        {
            _currentPlayerText.text = $"{_players[_currentPlayerIndex].Name} wins!";
            _diceRollButton.interactable = false;
            return;
        }

        CheckConflictingCell(_board.Cells[index], _selectedCharacter);

        if (_diceRoll == 6 && _duplicateSix < 2)
        {
            _duplicateSix++;
        }
        else
        {
            EndPlayerTurn();
        }

        ResetState();
    }

    private bool CheckWinner(Player player, Character character)
    {
        return player.House.CellIndex == character.CurrentCellIndex;
    }

    private void ResetState()
    {
        _selectedCharacter = null;
        _diceRoll = 0;
        _diceRollButton.interactable = true;
    }

    private void EndPlayerTurn()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % 4;
        _gameRound++;
        _duplicateSix = 0;
        UpdateCurrentPlayerText();
    }

    private IEnumerator EndPlayerTurnCantMove(int diceRoll)
    {
        _diceRollText.text = $"{diceRoll} (PASS)";
        yield return new WaitForSeconds(2f);

        ResetState();
        EndPlayerTurn();
        UpdateRollText(_diceRoll);
    }
    #endregion

    private IEnumerator SelectedCellColorTimeout()
    {
        yield return new WaitForSeconds(3f);
        _board.Cells[_currentCellIndex].SetFillColor(_currentCellColor);
    }
}