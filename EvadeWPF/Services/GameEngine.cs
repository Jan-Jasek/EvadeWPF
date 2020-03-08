using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AppShared;
using EvadeLogic;
using EvadeWPF.Interfaces;
using EvadeWPF.Models;

namespace EvadeWPF.Services
{
    public class GameEngine : IGameEngine
    {
        public GameManager gameManager { get; private set; }

        public List<int> TempMove { get; set; }

        public event Action<string> OutputMessage;
        public event Action<string> RaiseEndGameTriggered;
        public event Action<bool> EngineThinkingChanged;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public bool IsEngineThinking
        {
            get;
            set;
            //EngineThinkingChanged(value);
        }

        public void PlayBestMove()
        {
            CheckAITurn(cancellationTokenSource.Token, true);
        }

        public void StartEngine()
        {
            gameManager = new GameManager();
            gameManager.RaiseEndGameTriggered += GameEnded;
        }

        public void StopEngine()
        {
            gameManager = null;
        }

        public void GameTurn()
        {
            gameManager.DoGameTurn();
            IsEngineThinking = false;
            EngineThinkingChanged(false);

            PrintBoard(gameManager.GameBoard.GameArray);
            CheckAITurn(cancellationTokenSource.Token);
        }

        public async void CheckAITurn(CancellationToken cancellationToken, bool isTrue = false)
        {
            if (gameManager.IsPlayerOnTurnAI || isTrue)
            {
                IsEngineThinking = true;
                EngineThinkingChanged(true);
                gameManager.Move = await GetAITurn();
                if (cancellationToken.IsCancellationRequested)
                {
                    IsEngineThinking = false;
                    EngineThinkingChanged(false);
                    gameManager.IsGameEndTriggered(gameManager.GameBoard.GameArray);
                    PrintBoard(gameManager.GameBoard.GameArray);

                    cancellationTokenSource = new CancellationTokenSource();
                    CheckAITurn(cancellationToken);
                    return;
                }

                GameTurn();
            }
        }
        public async Task<List<int>> GetAITurn()
        {
            var myTask = Task.Run(() => gameManager.GetAITurn());
            var Move = await myTask;
            return Move;
        }

        //Validates selected unit piece
        public bool IsSelectValid(IBoardItem boardItem)
        {
            if (gameManager.IsPlayerOnTurnAI)
                return false;
            TempMove = new List<int> {boardItem.Col, boardItem.Row, (int) boardItem.PieceType};
            if (gameManager.IsSelectValid(TempMove))
            {
                gameManager.Move.Clear();
                //OutputMessage("Valid select!");
                gameManager.Move.AddRange(TempMove);
                return true;
            }

            OutputMessage("Invalid select!");
            return false;
        }

        public bool IsMoveValid(IBoardItem boardItem)
        {
            if (gameManager.IsPlayerOnTurnAI)
                return false;
            gameManager.Move.AddRange(new List<int> {boardItem.Col, boardItem.Row, (int) boardItem.PieceType});
            if (gameManager.IsMoveValid(gameManager.Move))
                //OutputMessage("Valid move!");
                return true;

            OutputMessage("Invalid move!");
            gameManager.Move.Clear();
            gameManager.Move.AddRange(TempMove);
            return false;
        }

        //Starts NewGame
        public void NewGame()
        {
            gameManager.NewGame();
            gameManager.IsGameEndTriggered(gameManager.GameBoard.GameArray);
            CheckAITurn(cancellationTokenSource.Token);
        }

        public void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems)
        {
            if (gameManager != null)
            {
                foreach (var itemToRemove in boardItems.Where(x => x is BoardPiece).ToList())
                    boardItems.Remove(itemToRemove);
                var gameBoard = gameManager.GameBoard.GameArray;
                for (var i = 1; i <= 6; i++)
                for (var j = 1; j <= 6; j++)
                    if (HelperMethods.EqualsAny(gameBoard[i, j],
                        (int) BoardValues.BlackKing, (int) BoardValues.BlackPawn, (int) BoardValues.WhiteKing,
                        (int) BoardValues.WhitePawn, (int) BoardValues.Frozen))
                        boardItems.Add(new BoardPiece {Col = i, Row = j, PieceType = (BoardValues) gameBoard[i, j]});
            }
        }
        
        public void GameEnded(string message)
        {
            RaiseEndGameTriggered(message);
        }

        public void PrintBoard(int[,] consoleArray)
        {
            for (var row = 1; row <= AppConstants.BoardSize; row++)
            {
                Console.Write($"\t{row}\t");
                for (var col = 1; col <= AppConstants.BoardSize; col++)
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

        public void UndoMove()
        {
            cancellationTokenSource.Cancel();
            gameManager.UndoLastMove();

            IsEngineThinking = false;
            EngineThinkingChanged(false);
            cancellationTokenSource = new CancellationTokenSource();

            PrintBoard(gameManager.GameBoard.GameArray);
            CheckAITurn(cancellationTokenSource.Token);
        }

        public void RedoMove()
        {
            cancellationTokenSource.Cancel();
            gameManager.GetMoveForRedo();
            cancellationTokenSource = new CancellationTokenSource();
            GameTurn();
        }
    }
}