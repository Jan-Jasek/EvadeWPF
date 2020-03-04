using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppShared;

namespace EvadeLogic
{
    public static class Rules
    {
        #region Validations

        /// <summary>
        /// Checks whether the selected unit is valid
        /// </summary>
        /// <returns></returns>
        internal static bool SelectedUnit_Check(List<int> move, int turnCounter)
        {
            if (HelperMethods.EqualsAny(move[2], (int)BoardValues.Empty, (int)BoardValues.Barrier, (int)BoardValues.Frozen))
                return false;
            if (turnCounter % 2 == 1)
            {
                if (HelperMethods.EqualsAny(move[2], (int)BoardValues.WhiteKing, (int)BoardValues.WhitePawn))
                    return false;
            }
            else
            {
                if (HelperMethods.EqualsAny(move[2], (int)BoardValues.BlackKing, (int)BoardValues.BlackPawn))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Validates proposed move action
        /// </summary>
        /// <returns></returns>
        internal static bool ValidateMove(List<int> move)
        {
            return CheckMoveLength(move) && CheckField(move);
        }

        /// <summary>
        /// Checks distance of the move action
        /// </summary>
        /// <returns></returns>
        internal static bool CheckMoveLength(List<int> move)
        {
            return Math.Sqrt(Math.Pow(move[0] - move[3], 2) +
                             Math.Pow(move[1] - move[4], 2)).IsBetween(0, 2);
        }

        internal static bool GetAllPossibleMoves(bool teamWhiteTurn, int[,] gameArray, List<List<int>> moveList = null)
        {
            moveList?.Clear();

            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (teamWhiteTurn)
                    {
                        if (HelperMethods.EqualsAny(gameArray[col, row], (int)BoardValues.WhiteKing,
                            (int)BoardValues.WhitePawn))
                        {
                            GetAllPositionMoves(col, row, gameArray, moveList);
                        }
                    }
                    else
                    {
                        if (HelperMethods.EqualsAny(gameArray[col, row], (int)BoardValues.BlackKing,
                            (int)BoardValues.BlackPawn))
                        {
                            GetAllPositionMoves(col, row, gameArray, moveList);
                        }
                    }
                }
            }
            return (moveList?.Count > 0);
        }

        internal static bool GetAllPositionMoves(int col, int row, int[,] gameArray, List<List<int>> moveList)
        {
            List<int> tryListI = new List<int>(){ -1, 0, 1 };
            List<int> tryListJ = new List<int>(){ -1, 0, 1 };

            foreach (var i in tryListI)
            {
                foreach (var j in tryListJ)
                {
                    if (Rules.IsOutOfBoard(col + j,row + i))
                        continue;

                    var move = new List<int>()
                        {col, row, gameArray[col, row], col + j, row + i, gameArray[col + j, row + i]};

                    if (CheckField(move))
                    {
                        moveList?.Add(move);
                    }
                }
            }

            return (moveList?.Count > 0);
        }


        /// <summary>
        /// Checks what the result of move action should be
        /// </summary>
        /// <returns></returns>
        internal static bool CheckField(List<int> move)
        {
            //Check whether new field is valid for movement
            if (!HelperMethods.EqualsAny(move[5], (int)BoardValues.Barrier, (int)BoardValues.Frozen))
            {
                //if empty, move is valid
                if (move[5] == (int)BoardValues.Empty)
                {
                    move.Add((int)TurnResults.Moved);
                    return true;
                }

                //Determines turn result depending on what is the selected unit ańd content of new field
                switch (move[2])
                {
                    case (int)BoardValues.BlackPawn:
                        {
                            if (HelperMethods.EqualsAny(move[5],(int)BoardValues.WhitePawn, (int)BoardValues.WhiteKing))
                            {
                                move.Add((int)TurnResults.Frozen);
                                return true;

                            }
                            return false;
                        }
                    case (int)BoardValues.WhitePawn:
                        {
                            if (HelperMethods.EqualsAny(move[5], (int)BoardValues.BlackPawn, (int)BoardValues.BlackKing))
                            {
                                move.Add((int)TurnResults.Frozen);
                                return true;
                            }

                            return false;
                        }

                    default: return false;

                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the input is within the game board size
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public static bool IsOutOfBoard(int col, int row)
        {

            if (col < 1 || col > AppConstants.BoardSize
                                                    || row < 1
                                                    || row > AppConstants.BoardSize)
            {
                return true;
            }

            return false;
        }


        #endregion

        #region Endgame

        public static bool IsGameEndTie(int[,] gameArray)
        {
            int WhiteKings = 0;
            int BlackKings = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int)BoardValues.WhiteKing)
                        WhiteKings++;
                    if (gameArray[col, row] == (int)BoardValues.BlackKing)
                        BlackKings++;
                }
            }

            if (WhiteKings == 0 && BlackKings == 0)
            {
                return true;
            }

            return false;
        }

        public static bool GameEndPlayerWWin(int[,] gameArray, List<List<int>> moveList)
        {
            if (GameEndWin(gameArray, true))
                return true;
            if (!Rules.GetAllPossibleMoves(false, gameArray, moveList))
                return true;
            return false;
        }

        public static bool GameEndPlayerBWin(int[,] gameArray, List<List<int>> moveList)
        {
            if (GameEndWin(gameArray, false))
                return true;
            if (!Rules.GetAllPossibleMoves(true, gameArray, moveList))
                return true;
            return false;
        }

        public static bool GameEndWin(int[,] gameArray, bool PlayerW)
        {
            for (int col = 1; col <= AppConstants.BoardSize; col++)
            {
                
                if (PlayerW && gameArray[col, 1] == (int)BoardValues.WhiteKing)
                {
                    return true;
                }

                if (!PlayerW && gameArray[col, AppConstants.BoardSize] == (int)BoardValues.BlackKing)
                {
                    return true;
                }
            }

            return false;
        }


        #endregion


    }
}
