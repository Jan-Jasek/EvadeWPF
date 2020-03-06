using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppShared;
using EvadeLogic;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;
using EvadeWPF.Models;
using EvadeWPF.ViewModels;

namespace EvadeWPF.Services
{
    public class GameEngine : IGameEngine
    {
        private GameManager _gameManager;
        public List<int> TempMove { get; set; }
        private Task<List<int>> _myTask;
        private TaskFactory _myTaskFactory;

        public event Action<string> OutputMessage;
        public event Action<string> RaiseEndGameTriggered;
        public event Action<bool> EngineThinkingChanged; 
        public bool IsEngineThinking { get; set; }

        public GameEngine()
        {
            _myTaskFactory = Task.Factory;
        }

        public void StartEngine()
        {
            _gameManager = new GameManager();
            _gameManager.RaiseEndGameTriggered += GameEnded;
        }

        public void StopEngine()
        {
            _gameManager = null;
        }

        public async void Move()
        {
            _gameManager.DoGameTurn();
            PrintBoard(_gameManager.GameBoard.GameArray);
            if (_gameManager.IsPlayerOnTurnAI)
            {
                IsEngineThinking = true;
                EngineThinkingChanged(IsEngineThinking);
                _gameManager.Move = await CheckAITurn();
                _gameManager.DoGameTurn();
            }

            IsEngineThinking = false;
            EngineThinkingChanged(IsEngineThinking);
            PrintBoard(_gameManager.GameBoard.GameArray);

        }

        public async Task<List<int>> CheckAITurn()
        {
            var myTask = Task.Run(() => _gameManager.GetAITurn());
            IsEngineThinking = true;
            var Move = await myTask;
            return Move;
        }

        //Validates selected unit piece
        public bool IsSelectValid(IBoardItem boardItem)
        {
            if (_gameManager.IsPlayerOnTurnAI)
                return false;
            TempMove = new List<int>() { boardItem.Col, boardItem.Row, (int)boardItem.PieceType };
            if (_gameManager.IsSelectValid(TempMove))
            {
                _gameManager.Move.Clear();
                //OutputMessage("Valid select!");
                _gameManager.Move.AddRange(TempMove);
                return true;
            }
            else
            {
                OutputMessage("Invalid select!");
                return false;
            }
        }

        public bool IsMoveValid(IBoardItem boardItem)
        {
            if (_gameManager.IsPlayerOnTurnAI)
                return false;
            _gameManager.Move.AddRange(new List<int>() { boardItem.Col, boardItem.Row, (int)boardItem.PieceType });
            if (_gameManager.IsMoveValid(_gameManager.Move))
            {
                //OutputMessage("Valid move!");
                return true;
            }
            else
            {
                OutputMessage("Invalid move!");
                _gameManager.Move.Clear();
                _gameManager.Move.AddRange(TempMove);
                return false;
            }

        }
        //Starts NewGame
        public void NewGame()
        {
            _gameManager.NewGame();
            _gameManager.IsGameEndTriggered(_gameManager.GameBoard.GameArray);
            CheckAITurn();
        }

        public void GameEnded(string message)
        {
            RaiseEndGameTriggered(message);
        }

        public void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems)
        {
            if(_gameManager != null)
            {
                foreach (var itemToRemove in boardItems.Where(x => x is BoardPiece).ToList())
                {
                    boardItems.Remove(itemToRemove);
                }
                var gameBoard = _gameManager.GameBoard.GameArray;
                for (int i = 1; i <= 6; i++)
                {
                    for (int j = 1; j <= 6; j++)
                    {
                        if (AppShared.HelperMethods.EqualsAny(gameBoard[i, j],
                            (int)BoardValues.BlackKing, (int)BoardValues.BlackPawn, (int)BoardValues.WhiteKing, (int)BoardValues.WhitePawn, (int)BoardValues.Frozen))
                        {
                            boardItems.Add(new BoardPiece() { Col = i, Row = j, PieceType = (BoardValues)gameBoard[i, j] });
                        }

                    }
                }
            }

        }

        public void PrintBoard(int [,] consoleArray)
        {
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                Console.Write($"\t{row}\t");
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (consoleArray[col, row] == 9)
                    {
                        Console.Write("* ");
                        continue;
                    }
                    if (consoleArray[col, row] == 5)
                    {
                        Console.Write("X ");
                        continue;
                    }
                    Console.Write($"{consoleArray[col, row]} ");
                }
                if (row == 1)
                    Console.Write("\t\tBlack Team");
                if (row == 2)
                    Console.Write("\t\t2 = Black Pawn");
                if (row == 3)
                    Console.Write("\t\t4 = Black King");
                if (row == 4)
                    Console.Write("\t\t1 = White Pawn");
                if (row == 5)
                    Console.Write("\t\t3 = White King");
                if (row == AppConstants.BoardSize)
                    Console.Write("\t\tWhite Team");
                Console.WriteLine();
            }
            Console.Write("\n\t\tA B C D E F\n");
        }
    }
}
