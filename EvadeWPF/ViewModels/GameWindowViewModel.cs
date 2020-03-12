using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
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

        public void Help()
        {
            Thread viewThread = new Thread(() =>
            {
                HelpWindow view = new HelpWindow();
                //CodeRequestViewModel viewModel = new CodeRequestViewModel();
                view.ShowDialog();
            });
            viewThread.SetApartmentState(ApartmentState.STA);
            viewThread.Start();
        }


        public ICommand HelpCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Help();
                });
            }
        }

        public void Rules()
        {
            Thread viewThread = new Thread(() =>
            {
                RulesWindow view = new RulesWindow();
                //CodeRequestViewModel viewModel = new CodeRequestViewModel();
                view.ShowDialog();
            });
            viewThread.SetApartmentState(ApartmentState.STA);
            viewThread.Start();
        }

        public ICommand RulesCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Rules();
                });
            }
        }

        public ICommand MoveHistoryDoubleClickCommand
        {
            get
            {
                return new RelayCommand(x =>
                {
                    var targetTurn = (int)x + 1;
                    if ((int)targetTurn >= 1 && targetTurn != _engine.gameManager.GameBoard.TempTurnCounter)
                    {
                        MoveHistoryDoubleClick(targetTurn);
                    }
                });
            }
        }

        public ICommand MoveHistoryKeyDownCommand
        {
            get
            {
                return new RelayCommand(x =>
                {
                    if ((Key)x == Key.Enter)
                    {
                        var targetTurn = CurrentMoveFromHistory+1;
                        if ((int)targetTurn >= 1 && targetTurn != _engine.gameManager.GameBoard.TempTurnCounter)
                        {
                            MoveHistoryDoubleClick(targetTurn);
                        }

                    }
                });
            }
        }

        private void MoveHistoryDoubleClick(int x)
        {
            int currentTurn = _engine.gameManager.GameBoard.TempTurnCounter;
            int difference = currentTurn - x;
            if (difference > 0)
            {
                while (difference != 0)
                {
                    _engine.UndoMove();
                    difference--;
                }
            }
            if (difference < 0)
            {
                while (difference != 0)
                {
                    _engine.RedoMove();
                    difference++;
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
                OutputMessage("UndoMove:");
                _engine.UndoMove();
            });}
        }
        public ICommand RedoMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                OutputMessage("RedoMove:");
                _engine.RedoMove();
            }); }
        }
        public ICommand PlayBestMoveCommand
        {
            get { return new DelegateCommand(() =>
            {
                _engine.PlayBestMove();
            });}
        }

        public ICommand NewGameCommand
        {
            get { return new DelegateCommand(() => NewGame()); }
        }

        private void NewGame()
        {
            _engine.AsyncCancelledInUI();
            IsGameEnded = false;
            IsListBoxEnabled = false;
            OutputLogTextBox = AppConstants.NewGameStarted;
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
            ArtificialIntelligence.AILevelB = AILevels.Random;
            ArtificialIntelligence.AILevelW = AILevels.Smart;
            IsUndoButtonEnabled = false;
            TurnCounterLabel = "0/0";
            _engine.AddUnitsFromGameBoard(BoardItems);

            if (ArtificialIntelligence.AILevelB == AILevels.Random)
            {
                IsBlackDifficultyRandom = true;
            }
            if (ArtificialIntelligence.AILevelB == AILevels.Dumb)
            {
                IsBlackDifficultyDumb = true;
            }
            if (ArtificialIntelligence.AILevelB == AILevels.Smart)
            {
                IsBlackDifficultySmart = true;
            }

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

            XDocument xmlDoc = new XDocument(
                new XElement("Game",
                    new XElement("Setting",
                        new XElement("IsWhiteAI", IsWhitePlayerAI),
                        new XElement("IsBlackAI", IsBlackPlayerAI),
                        new XElement("WhiteDifficulty", (int)ArtificialIntelligence.AILevelW),
                        new XElement("BlackDifficulty", (int)ArtificialIntelligence.AILevelB)
                    ),
                    ExportMoveHistoryToXML().Element("MoveHistory"))                
            );


            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    xmlDoc.Save(saveFileDialog.FileName);
                }
            }
            catch(Exception e)
            { 
                MessageBox.Show($"Unable to save file \n{e.Message}", "Save game error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private XDocument ExportMoveHistoryToXML()
        {
            XDocument xmlDoc = new XDocument(new XElement("MoveHistory"));
            //var characters = document.Descendants(ns + "Characters").FirstOrDefault();

            string output = "";
            foreach (var list in _engine.gameManager.MoveHistory)
            {
                list.ForEach((i) => output += i.ToString());
                xmlDoc.Root.Add(new XElement("Move",output));
                output = "";
            }

            return xmlDoc;
        }

        public ICommand LoadGameCommand
        {
            get { return new DelegateCommand(() => LoadGame()); }
        }

        private void LoadGame()
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.DefaultExt = ".xml";
            openFileDlg.Filter = "XML Files (*.xml)|*.xml";

            List<List<int>> moveList = new List<List<int>>();
            XDocument xmlDoc = new XDocument();

            try
            {
                if (openFileDlg.ShowDialog() == true)
                {
                    xmlDoc = XDocument.Load(openFileDlg.FileName);
                    _engine.AsyncCancelledInUI();
                    moveList.AddRange(xmlDoc.Descendants("Move")
                        .Select(element => element.Value
                            .Select(x => int.Parse(x.ToString()))
                            .ToList<int>()));
                    NewGame();
                    OutputLogTextBox = $"Loaded game {openFileDlg.FileName} \n";

                    IsWhitePlayerAI = xmlDoc.Descendants("IsWhiteAI")
                        .Select(element => bool.Parse(element.Value.ToString()))
                        .FirstOrDefault();
                    IsBlackPlayerAI = xmlDoc.Descendants("IsBlackAI")
                        .Select(element => bool.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    var aiLevelW = xmlDoc.Descendants("WhiteDifficulty")
                        .Select(element => (AILevels)int.Parse(element.Value.ToString()))
                        .FirstOrDefault();

                    var aiLevelB = xmlDoc.Descendants("BlackDifficulty")
                        .Select(element => (AILevels)int.Parse(element.Value.ToString()))
                        .FirstOrDefault();
                    _engine.PlayMoveHistory(moveList);


                    if (aiLevelB == AILevels.Random)
                    {
                        IsBlackDifficultyRandom = true;
                    }
                    if (aiLevelB == AILevels.Dumb)
                    {
                        IsBlackDifficultyDumb = true;
                    }
                    if (aiLevelB == AILevels.Smart)
                    {
                        IsBlackDifficultySmart = true;
                    }

                    if (aiLevelW == AILevels.Random)
                    {
                        IsWhiteDifficultyRandom = true;
                    }
                    if (aiLevelW == AILevels.Dumb)
                    {
                        IsWhiteDifficultyDumb = true;
                    }
                    if (aiLevelW  == AILevels.Smart)
                    {
                        IsWhiteDifficultySmart = true;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to load file \n{e.Message}", "Load game error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                if (value == false)
                {
                    _isWhiteDifficultyRandom = value;

                    if (!IsWhiteDifficultySmart && !IsWhiteDifficultyDumb)
                    {
                        _isWhiteDifficultyRandom = true;
                    }
                }
                if (value == true)
                {
                    _isWhiteDifficultyRandom = value;
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
                if (value == false)
                {
                    _isWhiteDifficultyDumb = value;

                    if (!IsWhiteDifficultyRandom && !IsWhiteDifficultySmart)
                    {
                        _isWhiteDifficultyDumb = true;
                    }
                }

                if (value == true)
                {
                    _isWhiteDifficultyDumb = value;
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
                if (value == false)
                {
                    _isWhiteDifficultySmart = value;

                    if (!IsWhiteDifficultyRandom && !IsWhiteDifficultyDumb)
                    {
                        _isWhiteDifficultySmart = true;
                    }
                }
                if (value == true)
                {
                    _isWhiteDifficultySmart = value;
                    IsWhiteDifficultyRandom = false;
                    IsWhiteDifficultyDumb = false;
                    SetWDifficulty(AILevels.Smart);
                }
                RaisePropertyChanged();
            }
        }

        private bool _isBlackDifficultyRandom;
        public bool IsBlackDifficultyRandom
        {
            get => _isBlackDifficultyRandom;
            set
            {
                if (value == false)
                {
                    _isBlackDifficultyRandom = value;
                    if (!IsBlackDifficultyDumb && !IsBlackDifficultySmart)
                    {
                        _isBlackDifficultyRandom = true;
                    }
                }

                if (value == true)
                {
                    _isBlackDifficultyRandom = value;
                    IsBlackDifficultyDumb = false;
                    IsBlackDifficultySmart = false;
                    SetBDifficulty(AILevels.Random);
                }
                RaisePropertyChanged();
            }
        }

        private bool _isBlackDifficultyDumb;
        public bool IsBlackDifficultyDumb
        {
            get => _isBlackDifficultyDumb;
            set
            {
                if (value == false)
                {
                    _isBlackDifficultyDumb = value;

                    if (!IsBlackDifficultyRandom && !IsBlackDifficultySmart)
                    {
                        _isBlackDifficultyDumb = true;
                    }
                }
                if (value == true)
                {
                    _isBlackDifficultyDumb = value;
                    IsBlackDifficultyRandom = false;
                    IsBlackDifficultySmart = false;
                    SetBDifficulty(AILevels.Dumb);
                }
                RaisePropertyChanged();
            }
        }

        private bool _isBlackDifficultySmart;
        public bool IsBlackDifficultySmart
        {
            get => _isBlackDifficultySmart;
            set
            {
                if (value == false)
                {
                    _isBlackDifficultySmart = value;

                    if (!IsBlackDifficultyRandom && !IsBlackDifficultyDumb)
                    {
                        _isBlackDifficultySmart = true;
                    }
                }

                if (value == true)
                {
                    _isBlackDifficultySmart = value;
                    IsBlackDifficultyRandom = false;
                    IsBlackDifficultyDumb = false;
                    SetBDifficulty(AILevels.Smart);
                }
                RaisePropertyChanged();
            }
        }

        public void SetWDifficulty(AILevels aILevel)
        {
            ArtificialIntelligence.AILevelW = aILevel;
        }
        public void SetBDifficulty(AILevels aILevel)
        {
            ArtificialIntelligence.AILevelB = aILevel;
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

        private int _currentMoveFromHistory;
        public int CurrentMoveFromHistory
        {
            get => _currentMoveFromHistory;
            set
            {
                _currentMoveFromHistory = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> MoveHistory { get; set; } = new ObservableCollection<string>();

        public void SynchronizeMoveHistory()
        {
            MoveHistory.Clear();
            int i = 1;

            foreach (var move in _engine.gameManager.MoveHistory)
            {
                string selectedColumn = AppConstants.ParseColumnValues(move[0].ToString());
                string targetColumn = AppConstants.ParseColumnValues(move[3].ToString());
                string selectedPiece = AppConstants.ParsePieceValues(move[2].ToString());

                MoveHistory.Add(i
                                + ": "
                                + (i % 2 == 1 ? "White" : "Black")
                                + " from "
                                + $"{selectedColumn}{move[1]}"
                                + " to "
                                + $"{targetColumn}{move[4]}");
                i++;
            }

            CurrentMoveFromHistory = _engine.gameManager.GameBoard.TempTurnCounter -1;
        }

        private string _moveHistoryTextBlock;
        public string MoveHistoryTextBlock
        {
            get => _moveHistoryTextBlock;
            set
            {
                _moveHistoryTextBlock = value;
                RaisePropertyChanged();
            }
        }

        private string _outputLogTextBox;
        public string OutputLogTextBox
        {
            get => _outputLogTextBox;
            set
            {
                _outputLogTextBox = value;
                RaisePropertyChanged();
            }
        }

        private void SendMoveToEngine()
        {
            IsListBoxEnabled = false;
            SelectedBoardItem = null;
            _engine.GameTurn(false);
            ResetAfterMove();
        }

        private void EndGame(string message)
        {
            IsListBoxEnabled = false;
            IsGameEnded = true;
            OutputLogTextBox = OutputLogTextBox + "Game won by" + message;
        }

        private void OutputMessage(string message)
        {
            SynchronizeMoveHistory();
            OutputLogTextBox = OutputLogTextBox + message + "\n";
        }

        public void ResetAfterMove()
        {
            _selectedBoardPiece = null;
            _selectedBoardItem = null;
        }
    }
}
