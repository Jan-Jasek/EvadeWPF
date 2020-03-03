using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            engine = ge;
        }

        private IGameEngine engine;

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

        private IBoardItem _selectedBoardItem;
        public IBoardItem SelectedBoardItem
        {
            set
            {

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
            OutputTextBox = "NewGame started";

            for (int i = 3; i <= 35; i++)
            {
                BoardItems.Add(new BoardSquare());
            }

            BoardItems.Add(new Unit() { Col = 0, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new Unit() { Col = 1, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new Unit() { Col = 2, Row = 0, PieceType = BoardValues.BlackKing });
            BoardItems.Add(new Unit() { Col = 3, Row = 0, PieceType = BoardValues.BlackKing });
            BoardItems.Add(new Unit() { Col = 4, Row = 0, PieceType = BoardValues.BlackPawn });
            BoardItems.Add(new Unit() { Col = 5, Row = 0, PieceType = BoardValues.BlackPawn });




        }
    }
}
