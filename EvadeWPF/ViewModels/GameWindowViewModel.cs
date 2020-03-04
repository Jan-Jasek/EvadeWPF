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
                    if (_engine.IsSelectValid(value))
                    {
                        _selectedBoardPiece = value;
                    }
                    else
                    {
                        if (_selectedBoardPiece != null && _engine.IsMoveValid(value) )
                        {
                            _targetBoardItem = value;
                            SendMoveToEngine(value);
                        }
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
            get
            {
                if(_newGameCommand == null)
                    _newGameCommand = new RelayCommand(p => NewGame());
                return _newGameCommand;
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

        private void SendMoveToEngine(IBoardItem boardItem)
        {
            _engine.Move(boardItem);
            ResetAfterMove();
        }

        private void NewGame()
        {
            OutputMessage(AppConstants.NewGameStarted);

            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    BoardItems.Add(new BoardSquare() { Col = i, Row = j, PieceType = BoardValues.Empty });
                }
            }

            _engine.NewGame();
            _engine.AddUnitsFromGameBoard(BoardItems);
        }


        private void OutputMessage(string message)
        {
            OutputTextBox = _outputTextBox + "\n" + message;
        }

        public void ResetAfterMove()
        {
            _selectedBoardItem = null;
            _selectedBoardPiece = null;
            _targetBoardItem = null;
        }
    }
}
