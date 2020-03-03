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

        private BoardPiece _selectedBoardPiece;
        private IBoardItem _selectedBoardItem;
        public IBoardItem SelectedBoardItem
        {
            set
            {
                if(value is BoardPiece)
                {
                    var boardPiece = (BoardPiece) value;
                    if (_selectedBoardPiece == null)
                    {
                        if (_engine.IsMoveValid() == true)
                        {
                            _selectedBoardPiece = boardPiece;
                        }

                    }
                    else
                    {
                        _engine.IsMoveValid();
                    }
                }
                else
                {
                    _engine.IsMoveValid();
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

        private void NewGame()
        {
            OutputMessage(AppConstants.NewGameStarted);
            //_engine.NewGame();

            for (int i = 3; i <= 35; i++)
            {
                BoardItems.Add(new BoardSquare());
            }

            BoardItems.Add(new BoardPiece() { Col = 0, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new BoardPiece() { Col = 1, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new BoardPiece() { Col = 2, Row = 0, PieceType = BoardValues.BlackKing });
            BoardItems.Add(new BoardPiece() { Col = 3, Row = 0, PieceType = BoardValues.BlackKing });
            BoardItems.Add(new BoardPiece() { Col = 4, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new BoardPiece() { Col = 5, Row = 0, PieceType = BoardValues.BlackPawn });



        }

        private void OutputMessage(string message)
        {
            OutputTextBox = _outputTextBox + message;
        }
    }
}
