using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppShared;

namespace EvadeLogic
{
    public class GameManager
    {
        #region Private fields

        public GameBoard GameBoard { get; private set; }
        //FormatLog = Column-row-unit-column-row-unit-turnResult

        public List<int> Move = new List<int>();
        public List<List<int>> MoveHistory { get; set; } = new List<List<int>>();
        #endregion
        #region Public properties
        public List<List<int>> MoveList { get; set; } = new List<List<int>>();
        public bool IsGameRunning { get; set; } = false;
        public bool IsPlayerWTurn { get; set; } = true;
        public bool IsPlayerWAI { get; set; } = false;
        public bool IsPlayerBAI { get; set; } = true;
        public bool IsNextMoveRedo { get; set; } = false;
        public event Action<string> RaiseEndGameTriggered;
        public bool IsPlayerOnTurnAI => (IsPlayerWTurn && IsPlayerWAI) || (!IsPlayerWTurn && IsPlayerBAI);

        #endregion

        #region Constructors

        public GameManager()
        {
        }

        #endregion

        #region NewGame
        /// <summary>
        /// starts new game
        /// </summary>
        public void NewGame()
        {
            GameBoard = new GameBoard();
            GameBoard.NewGame();
            IsGameRunning = true;
            GameBoard.TurnCounter = 0;
            GameBoard.TempTurnCounter = 0;
        }

        #endregion

        #region Validations

        public bool IsSelectValid(List<int> move)
        {
            return Rules.SelectedUnit_Check(move, GameBoard.TurnCounter);
        }

        public bool IsMoveValid(List<int> move)
        {
            return Rules.ValidateMove(move);
        }

        #endregion

        public void DoGameTurn()
        {
            DoMove();
            //If new move is replacing previous move without redo
            if (GameBoard.TurnCounter != GameBoard.TempTurnCounter && !IsNextMoveRedo)
            {
                MoveHistory.RemoveAll(item =>
                {
                    int index = MoveHistory.IndexOf(item);
                    return index > GameBoard.TempTurnCounter-1;
                });
                GameBoard.TurnCounter = GameBoard.TempTurnCounter;
            }
            //If redo move replayed
            if (IsNextMoveRedo)
            {
                GameBoard.TempTurnCounter += 1;
                IsPlayerWTurn = (GameBoard.TempTurnCounter % 2 != 1);
            }
            //If standard move
            else
            {
                GameBoard.TurnCounter += 1;
                GameBoard.TempTurnCounter = GameBoard.TurnCounter;
                IsPlayerWTurn = (GameBoard.TurnCounter % 2 != 1);
                MoveHistory.Add(new List<int>(Move));
            }
            Move.Clear();
            IsNextMoveRedo = false;
            IsGameEndTriggered(GameBoard.GameArray);
        }

        public List<int> GetAITurn()
        {
            var aILevel = IsPlayerWTurn ? ArtificialIntelligence.AILevelW : ArtificialIntelligence.AILevelB;
            ArtificialIntelligence.MoveList = new List<List<int>>(MoveList);
            int[,] testArray = CloneArray(GameBoard.GameArray);
            Move = ArtificialIntelligence.FindBestMove(aILevel, testArray, IsPlayerWTurn);
            return Move;
        
        }

        public int[,] CloneArray(int[,] gameArray)
        {
            int[,] copyArray = new int[AppConstants.BoardSize+2,AppConstants.BoardSize+2];
            for (int row = 0; row <= AppConstants.BoardSize + 1; row++)
            {
                for (int col = 0; col <= AppConstants.BoardSize + 1; col++)
                {
                    copyArray[col, row] = gameArray[col, row];
                }
            }

            return copyArray;
        }
        #region EndGame

        /// <summary>
        /// Checks ending criteria of the game
        /// </summary>
        public bool IsGameEndTriggered(int[,] gameArray)
        {
            if (Rules.IsGameEndTie(GameBoard.GameArray))
            {
                GameEndTie();
                return true;
            }
            if (GameEndWin())
                return true;

            if (!Rules.GetAllPossibleMoves(IsPlayerWTurn, gameArray, MoveList))
            {
                GameEndNoPossibleTurns();
                return true;
            }

            return false;
        }

        private void GameEndTie()
        {
            RaiseEndGameTriggered(AppConstants.GameEndTie);
            IsGameRunning = false;
        }

        private bool GameEndWin()
        {
            string message = "";
            bool end = false;
            if (Rules.GameEndPlayerWWin(GameBoard.GameArray, MoveList))
            {
                message = AppConstants.White;
                end = true;
            }

            if (Rules.GameEndPlayerBWin(GameBoard.GameArray, MoveList))
            {
                message = AppConstants.Black;
                end = true;
            }
            if(end)
            {
                RaiseEndGameTriggered(message);
                IsGameRunning = false;
                return true;
            }

            return false;
        }

        private void GameEndNoPossibleTurns()
        {
            RaiseEndGameTriggered(AppConstants.NoPossibleTurns);
            IsGameRunning = false;
        }

        #endregion

        public void DoMove()
        {
            GameBoard.SetField(GameBoard.GameArray, Move[0], Move[1]);

            if (IsPlayerOnTurnAI)
            {
                if (Move[6] == (int)TurnResults.Moved)
                {
                    GameBoard.SetField(GameBoard.GameArray, Move[3], Move[4], Move[2]);
                    //checkersConsole.LogTurn(outputLog, PressEnter);
                }
                else
                {
                    GameBoard.SetField(GameBoard.GameArray, Move[3], Move[4], (int)BoardValues.Frozen);
                    //checkersConsole.LogTurn(outputLog, FieldFrozen + PressEnter);
                }

            }
            else
            {
                if (Move[6] == (int)TurnResults.Moved)
                {
                    GameBoard.SetField(GameBoard.GameArray, Move[3], Move[4], Move[2]);
                    //checkersConsole.LogTurn(outputLog);
                }
                else
                {
                    GameBoard.SetField(GameBoard.GameArray, Move[3], Move[4], (int)BoardValues.Frozen);
                    //checkersConsole.LogTurn(outputLog, FieldFrozen);
                }
            }
        }

        public void DoUndoMove()
        {
            GameBoard.SetField(GameBoard.GameArray, (Move[0]),
                (Move[1]), (Move[2]));
            GameBoard.SetField(GameBoard.GameArray, (Move[3]),
                (Move[4]), (Move[5]));

            GameBoard.TempTurnCounter -= 1;
            IsPlayerWTurn = (GameBoard.TempTurnCounter % 2 != 1);
            IsGameEndTriggered(GameBoard.GameArray);
        }

        public void UndoLastMove()
        {
            Move = new List<int>(MoveHistory[GameBoard.TempTurnCounter - 1]);
            DoUndoMove();
        }

        public void GetMoveForRedo()
        {
            Move = new List<int>(MoveHistory[GameBoard.TempTurnCounter]);
            IsNextMoveRedo = true;
        }
    }
}
