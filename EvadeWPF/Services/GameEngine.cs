using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
        public List<int> RecommendedMove;
        public bool IsGameEnded { get; set; }
        public string GameWonBy { get; set; }
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public bool IsEngineThinking
        {
            get;
            set;
            //EngineThinkingChanged(value);
        }

        public GameEngine()
        {

        }

        public void PlayBestMove()
        {
            CheckAITurn(AILevels.Smart, true);
        }

        public void StartEngine()
        {
            cancellationTokenSource.Cancel();
            gameManager = new GameManager();
            gameManager.RaiseEndGameTriggered += GameEnded;
        }

        public void StopEngine()
        {
            gameManager = null;
        }

        public void GameTurn(bool redo = false)
        {
            gameManager.DoGameTurn();

            IsEngineThinking = false;
            EngineThinkingChanged(false);



            OutputMessage(TurnToOutput(gameManager.Move));
            PrintBoard(gameManager.GameBoard.GameArray);


            gameManager.Move.Clear();



            if (IsGameEnded == true)
            {
                RaiseEndGameTriggered(GameWonBy);
            }
            if(!redo)
                CheckAITurn(gameManager.IsPlayerWTurn ? ArtificialIntelligence.AILevelW : ArtificialIntelligence.AILevelB);
        }

        public string TurnToOutput(List<int> move)
        {
            string playerOrAI = gameManager.IsPlayerOnTurnAI ? "AI" : "Player";
            string moveOutput = move[6] == (int)TurnResults.Frozen ? "The field is now frozen." : "";
            string selectedColumn = AppConstants.ParseColumnValues(move[0].ToString());
            string targetColumn = AppConstants.ParseColumnValues(move[3].ToString());
            string selectedPiece = AppConstants.ParsePieceValues(move[2].ToString());

            return $"{playerOrAI} moved {selectedPiece} from {selectedColumn}{move[1]} to {targetColumn}{move[4]}. {moveOutput}";
        }

        public void CheckAITurn(AILevels aILevel, bool isTrue = false)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CheckAITurn(cancellationTokenSource.Token, isTrue, aILevel);
        }

        public void PlayMoveHistory(List<List<int>> moveHistory)
        {
            foreach (var move in moveHistory)
            {
                gameManager.Move = new List<int>(move);
                gameManager.DoGameTurn();
                OutputMessage(TurnToOutput(gameManager.Move));
                gameManager.Move.Clear();
            }

            IsEngineThinking = false;
            EngineThinkingChanged(false);

            if (IsGameEnded == true)
            {
                RaiseEndGameTriggered(GameWonBy);
            }
        }

        public void AsyncCancelledInUI()
        {
            cancellationTokenSource.Cancel();
        }

        public async void CheckAITurn(CancellationToken cancellationToken,
            bool find, AILevels aiLevel)
        {
            if (gameManager.IsPlayerOnTurnAI || find)
            {
                IsEngineThinking = true;
                EngineThinkingChanged(true);
                gameManager.Move = await GetAITurn(cancellationToken, aiLevel);
                if (cancellationToken.IsCancellationRequested)
                {
                    IsEngineThinking = false;
                    EngineThinkingChanged(false);
                    gameManager.IsGameEndTriggered(gameManager.GameBoard.GameArray);
                    PrintBoard(gameManager.GameBoard.GameArray);

                    cancellationTokenSource = new CancellationTokenSource();
                    return;
                }

                if (find)
                {
                    RecommendedMove = new List<int>(gameManager.Move);
                    OutputMessage(SendBestMoveToOutput(RecommendedMove));
                    IsEngineThinking = false;   
                    EngineThinkingChanged(false);
                    RecommendedMove.Clear();
                    gameManager.Move.Clear();
                    gameManager.IsGameEndTriggered(gameManager.GameBoard.GameArray);
                    return;
                }

                GameTurn();
            }
        }

        public async Task<List<int>> GetAITurn(CancellationToken cancellationToken, AILevels aiLevel)
        {
            var myTask = Task.Run(() => gameManager.GetAITurn(cancellationToken, aiLevel));
            var Move = await myTask;
            return Move;
        }

        private string SendBestMoveToOutput(List<int> move)
        {
            string selectedColumn = AppConstants.ParseColumnValues(move[0].ToString());
            string targetColumn = AppConstants.ParseColumnValues(move[3].ToString());
            string selectedPiece = AppConstants.ParsePieceValues(move[2].ToString());

            return $"Best move: Select {selectedPiece} from {selectedColumn}{move[1]} to {targetColumn}{move[4]}.";
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

            //OutputMessage("Invalid select!");
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
            gameManager.Move.RemoveRange(3,3);
            return false;
        }

        //Starts NewGame
        public void NewGame()
        {
            gameManager.NewGame();
            IsGameEnded = false;
            GameWonBy = "";
            gameManager.IsGameEndTriggered(gameManager.GameBoard.GameArray);
        }

        public void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems)
        {
            if (gameManager != null)
            {
                foreach (var itemToRemove in boardItems.Where(x => x is BoardPiece || x.PieceType==BoardValues.Recommended).ToList())
                {
                    boardItems.Remove(itemToRemove);
                }

                if (RecommendedMove != null && RecommendedMove.Count > 0)
                {

                    boardItems.Add(new BoardPiece
                    {
                        Col = RecommendedMove[0],
                        Row = RecommendedMove[1],
                        PieceType = BoardValues.Recommended
                    });
                    
                
                    boardItems.Add(new BoardSquare
                    {
                        Col = RecommendedMove[3],
                        Row = RecommendedMove[4],
                        PieceType = BoardValues.Recommended

                    });
                

                    /*
                    foreach (var item in boardItems)
                    {
                        item.RecommendedMove = false;
                        if (item.Col == RecommendedMove[0] && item.Row == RecommendedMove[1]
                            || item.Col == RecommendedMove[3] && item.Row == RecommendedMove[4])
                        {
                            item.RecommendedMove = true;
                        }
                    }
                    */
                }


                var gameBoard = gameManager.GameBoard.GameArray;

                for (var i = 1; i <= 6; i++)
                {
                    for (var j = 1; j <= 6; j++)
                    {
                        if (HelperMethods.EqualsAny(gameBoard[i, j],
                            (int)BoardValues.BlackKing, (int)BoardValues.BlackPawn, (int)BoardValues.WhiteKing,
                            (int)BoardValues.WhitePawn, (int)BoardValues.Frozen))
                            boardItems.Add(new BoardPiece { Col = i, Row = j, PieceType = (BoardValues)gameBoard[i, j] });
                    }

                }

            }
        }
        
        public void GameEnded(string message)
        {
            cancellationTokenSource.Cancel();
            GameWonBy = message;
            IsGameEnded = true;
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
            var move = new List<int>(gameManager.MoveHistory[gameManager.GameBoard.TempTurnCounter - 1]);
            gameManager.UndoLastMove();

            OutputMessage(TurnToOutput(move));
            IsGameEnded = false;
            GameWonBy = "";
            IsEngineThinking = false;
            EngineThinkingChanged(false);

            PrintBoard(gameManager.GameBoard.GameArray);
        }

        public void RedoMove()
        {
            cancellationTokenSource.Cancel();
            gameManager.GetMoveForRedo();
            GameTurn(true);
        }
    }
}