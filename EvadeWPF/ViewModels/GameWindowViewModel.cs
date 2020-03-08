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
using System.Xml;
using AppShared;
using EvadeLogic;
using EvadeWPF.Annotations;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;
using EvadeWPF.Models;
using Microsoft.Win32;

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

        public bool IsGameEnded { get; set; }

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

                if (!IsGameEnded)
                {
                    if (value == false)
                    {
                        IsPanelLoading = false;
                        _engine.AddUnitsFromGameBoard(BoardItems);
                        IsListBoxEnabled = true;
                        PlayerOnTurn = _engine.gameManager.IsPlayerWTurn ? "White" : "Black";
                        TurnCounterLabel = _engine.gameManager.GameBoard.TempTurnCounter.ToString() + "/" + _engine.gameManager.GameBoard.TurnCounter.ToString();
                        IsUndoButtonEnabled = (_engine.gameManager.GameBoard.TempTurnCounter > 0);
                        IsRedoButtonEnabled = (_engine.gameManager.GameBoard.TempTurnCounter <
                                               _engine.gameManager.GameBoard.TurnCounter);
                    }

                    if (value == true)
                    {
                        IsPanelLoading = true;
                        IsListBoxEnabled = false;
                    }
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
        private IBoardItem _selectedBoardItem;
        public IBoardItem SelectedBoardItem
        {
            set
            {
                if (_engine.gameManager.IsPlayerOnTurnAI && IsListBoxEnabled)
                {
                    _engine.CheckAITurn(_engine.gameManager.IsPlayerWTurn ? ArtificialIntelligence.AILevelW : ArtificialIntelligence.AILevelB);
                }

                else
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
                            if (_selectedBoardPiece != null && _engine.IsMoveValid(value))
                            {
                                SendMoveToEngine();
                            }
                        }
                    }
                }

            }
        }

        public ICommand UndoMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                if (IsGameEnded)
                {
                    IsGameEnded = false;
                }
                OutputMessage("UndoMove");
                _engine.UndoMove();
            });}
        }
        public ICommand RedoMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                OutputMessage("RedoMove");
                _engine.RedoMove();
            }); }
        }
        public ICommand PlayBestMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                OutputMessage("PlayBestMove");
                _engine.PlayBestMove();
            });}
        }

        public ICommand NewGameCommand
        {
            get { return new DelegateCommand(() => NewGame()); }
        }

        private void NewGame()
        {
            IsGameEnded = false;
            IsListBoxEnabled = false;
            OutputMessage(AppConstants.NewGameStarted);
            BoardItems.Clear();
            _engine.StartEngine();
            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    BoardItems.Add(new BoardSquare() { Col = i, Row = j, PieceType = BoardValues.Empty });
                }
            }

            _engine.NewGame();
            PlayerOnTurn = _engine.gameManager.IsPlayerWTurn ? "White" : "Black";
            IsBlackPlayerAI = _engine.gameManager.IsPlayerBAI;
            IsWhitePlayerAI = _engine.gameManager.IsPlayerWAI;
            IsUndoButtonEnabled = false;
            TurnCounterLabel = "0/0";
            _engine.AddUnitsFromGameBoard(BoardItems);

            if (ArtificialIntelligence.AILevelW == AILevels.Random)
            {
                IsWhiteDifficultyRandom = true;
            }
            if (ArtificialIntelligence.AILevelW == AILevels.Dumb)
            {
                IsWhiteDifficultyDumb = true;
            }
            if (ArtificialIntelligence.AILevelW == AILevels.Smart)
            {
                IsWhiteDifficultySmart = true;
            }
            IsListBoxEnabled = true;
        }

        public ICommand SaveGameCommand
        {
            get { return new DelegateCommand(() => SaveGame()); }
        }

        private void SaveGame()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML document|*.xml";
            saveFileDialog.FileName = "Game";
            saveFileDialog.DefaultExt = "xml";

            using (XmlWriter writer = XmlWriter.Create(@saveFileDialog.FileName))
            {

            }

            if (saveFileDialog.ShowDialog() == true)
            {

            }
        }

        public ICommand LoadGameCommand
        {
            get { return new DelegateCommand(() => LoadGame()); }
        }

        private void LoadGame()
        {
            throw new NotImplementedException();
        }

        private bool _isUndoButtonEnabled = false;
        public bool IsUndoButtonEnabled
        {
            get => _isUndoButtonEnabled;
            set
            {
                _isUndoButtonEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isRedoButtonEnabled = false;
        public bool IsRedoButtonEnabled
        {
            get => _isRedoButtonEnabled;
            set
            {
                _isRedoButtonEnabled = value;
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

        private bool _isWhitePlayerAI;
        public bool IsWhitePlayerAI
        {
            get => _isWhitePlayerAI;
            set
            {
                _isWhitePlayerAI = value;
                _engine.gameManager.IsPlayerWAI = value;
                RaisePropertyChanged();
            }
        }

        private bool _isBlackPlayerAI;
        public bool IsBlackPlayerAI
        {
            get => _isBlackPlayerAI;
            set
            {
                _isBlackPlayerAI = value;
                _engine.gameManager.IsPlayerBAI = value;
                RaisePropertyChanged();
            }
        }

        private bool _isWhiteDifficultyRandom;
        public bool IsWhiteDifficultyRandom
        {
            get => _isWhiteDifficultyRandom;
            set
            {
                _isWhiteDifficultyRandom = value;
                if (value == true)
                {
                    IsWhiteDifficultyDumb = false;
                    IsWhiteDifficultySmart = false;
                    SetWDifficulty(AILevels.Random);
                }
                RaisePropertyChanged();
            }
        }

        private bool _isWhiteDifficultyDumb;
        public bool IsWhiteDifficultyDumb
        {
            get => _isWhiteDifficultyDumb;
            set
            {
                _isWhiteDifficultyDumb = value;
                if (value == true)
                {
                    IsWhiteDifficultyRandom = false;
                    IsWhiteDifficultySmart = false;
                    SetWDifficulty(AILevels.Dumb);
                }
                RaisePropertyChanged();
            }
        }

        private bool _isWhiteDifficultySmart;
        public bool IsWhiteDifficultySmart
        {
            get => _isWhiteDifficultySmart;
            set
            {
                _isWhiteDifficultySmart = value;
                if (value == true)
                {
                    IsWhiteDifficultyRandom = false;
                    IsWhiteDifficultyDumb = false;
                    SetWDifficulty(AILevels.Smart);
                }
                RaisePropertyChanged();
            }
        }

        public void SetWDifficulty(AILevels aILevel)
        {
            ArtificialIntelligence.AILevelW = aILevel;
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

        private void EndGame(string message)
        {
            IsListBoxEnabled = false;
            IsGameEnded = true;
            OutputTextBox = OutputTextBox + "Game won by" + message;
        }

        private void OutputMessage(string message)
        {
            OutputTextBox = OutputTextBox + message + "\n";
        }

        public void ResetAfterMove()
        {
            _selectedBoardPiece = null;
            _selectedBoardItem = null;
        }
    }
}
