using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AppShared;
using EvadeWPF.Annotations;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;
using EvadeWPF.Models;

namespace EvadeWPF.ViewModels
{
    public class GameWindowViewModel : NotifyPropertyChanged
    {

        public GameWindowViewModel(IGameEngine ge)
        {
            BoardItems = new ObservableCollection<IBoardItem>();
            _engine = ge;
            _engine.OutputMessage += OutputMessage;
            _engine.RaiseEndGameTriggered += EndGame;
            _engine.EngineThinkingChanged += EngineThinkingChanged;
            NewGame();
        }

        private void EngineThinkingChanged(bool obj)
        {
            IsEngineThinking = obj;
        }

        private bool _isEngineThinking;
        public bool IsEngineThinking
        {
            get
            {
                return _isEngineThinking;
            }
            set
            {
                _isEngineThinking = value;
                RaisePropertyChanged();
                if (value == false)
                {
                    IsPanelLoading = false;
                    _engine.AddUnitsFromGameBoard(BoardItems);
                    IsListBoxEnabled = true;
                    PlayerOnTurn = _engine.gameManager.IsPlayerWTurn ? "White" : "Black";
                    TurnCounterLabel = _engine.gameManager.GameBoard.TempTurnCounter.ToString() + "/" + _engine.gameManager.GameBoard.TurnCounter.ToString();
                }

                if (value == true)
                {

                    IsPanelLoading = true;
                    IsListBoxEnabled = false;
                }
            }
        }

        private bool _isPanelLoading;
        public bool IsPanelLoading
        {
            get => _isPanelLoading;
            set
            {
                _isPanelLoading = value;
                RaisePropertyChanged();
            }
        }

        private IGameEngine _engine;

        private ObservableCollection<IBoardItem> _boardItems;
        public ObservableCollection<IBoardItem> BoardItems
        {
            get { return _boardItems; }
            set
            {
                _boardItems = value;
                RaisePropertyChanged();
            }
        }

        private IBoardItem _selectedBoardPiece;
        private IBoardItem _targetBoardItem;
        private IBoardItem _selectedBoardItem;
        public IBoardItem SelectedBoardItem
        {
            set
            {
                _selectedBoardItem = value;

                if (_selectedBoardItem != null && IsListBoxEnabled)
                {
                    if (_selectedBoardItem is BoardPiece && _engine.IsSelectValid(value))
                    {
                        _selectedBoardPiece = value;
                    }
                    else
                    {
                        if (_selectedBoardPiece != null && _engine.IsMoveValid(value) )
                        {
                            _targetBoardItem = value;
                            SendMoveToEngine();
                        }
                    }
                }
                
            }
        }

        public ICommand UndoMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                OutputMessage("UndoMove");
                _engine.UndoMove();
            });}
        }
        public ICommand RedoMoveCommand
        {
            get { return new DelegateCommand(() => _engine.RedoMove()); }
        }
        public ICommand PlayBestMoveCommand
        {
            get { return new DelegateCommand(() => _engine.PlayBestMove());}
        }
        
        public ICommand NewGameCommand
        {
            get { return new DelegateCommand(() => NewGame()); }
        }

        private bool _isUndoButtonEnabled = true;
        public bool IsUndoButtonEnabled
        {
            get => _isUndoButtonEnabled;
            set
            {
                _isListBoxEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isRedoButtonEnabled = true;
        public bool IsRedoButtonEnabled
        {
            get => _isRedoButtonEnabled;
            set
            {
                _isListBoxEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isListBoxEnabled = true;
        public bool IsListBoxEnabled
        {
            get => _isListBoxEnabled;
            set
            {
                _isListBoxEnabled = value;
                RaisePropertyChanged();
            }
        }

        private string _playerOnTurn;
        public string PlayerOnTurn
        {
            get => _playerOnTurn;
            set
            {
                _playerOnTurn = value + " player on turn" + (_engine.gameManager.IsPlayerOnTurnAI ? "(AI)" : "");
                RaisePropertyChanged();
            }
        }

        private string _turnCounterLabel = "0/0";
        public string TurnCounterLabel
        {
            get => _turnCounterLabel;
            set
            {
                _turnCounterLabel = value;
                RaisePropertyChanged();
            }
        }

        private string _outputTextBox;
        public string OutputTextBox
        {
            get => _outputTextBox;
            set
            {
                _outputTextBox = value;
                RaisePropertyChanged();
            }
        }

        private void SendMoveToEngine()
        {
            IsListBoxEnabled = false;
            SelectedBoardItem = null;
            _engine.GameTurn();
            ResetAfterMove();
        }

        private void NewGame()
        {
            OutputMessage(AppConstants.NewGameStarted);
            BoardItems.Clear();
            _engine.StartEngine();
            IsListBoxEnabled = true;
            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    BoardItems.Add(new BoardSquare() { Col = i, Row = j, PieceType = BoardValues.Empty });
                }
            }

            _engine.NewGame();
            _engine.AddUnitsFromGameBoard(BoardItems);
            PlayerOnTurn = _engine.gameManager.IsPlayerWTurn ? "White" : "Black";
        }

        private void EndGame(string message)
        {
            _engine.StopEngine();
            IsListBoxEnabled = false;
            OutputTextBox = "Game won by" + message;
        }

        private void OutputMessage(string message)
        {
            OutputTextBox = OutputTextBox + message + "\n";
        }

        public void ResetAfterMove()
        {
            _selectedBoardPiece = null;
            _targetBoardItem = null;
            _selectedBoardItem = null;
        }
    }
}
