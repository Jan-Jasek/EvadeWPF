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
            _engine.StartEngine();
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
                    IsListBoxEnabled = true;
                    _engine.AddUnitsFromGameBoard(BoardItems);
                }

                if (value == true)
                {
                    IsListBoxEnabled = false;
                }
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

                if (_selectedBoardItem != null)
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
                        //todo obsolete
                        else
                        {
                            _selectedBoardItem = value;
                        }

                    }
                }
                
            }
        }

        private ICommand _newGameCommand;
        public ICommand NewGameCommand
        {
            get { return _newGameCommand ?? (_newGameCommand = new RelayCommand(p => NewGame())); }
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
            _engine.Move();
            ResetAfterMove();
            _engine.IsEngineThinking = false;
            _engine.AddUnitsFromGameBoard(BoardItems);
        }

        private void NewGame()
        {
            OutputMessage(AppConstants.NewGameStarted);
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
