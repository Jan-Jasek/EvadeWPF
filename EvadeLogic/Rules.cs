using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;

namespace Checkers
{
    public static class Rules
    {
        public static GameBoard GameBoard;

        #region Validations

        /// <summary>
        /// Checks whether the selected unit is valid
        /// </summary>
        /// <returns></returns>
        internal static bool SelectedUnit_Check(ref string turnLog, int turnCounter)
        {
            if (HelperMethods.EqualsAny(turnLog[2].ToString(), ((int)GameBoard.BoardValues.Empty).ToString(), ((int)GameBoard.BoardValues.Barrier).ToString(), ((int)GameBoard.BoardValues.Frozen).ToString()))
                return false;
            if (turnCounter % 2 == 1)
            {
                if (HelperMethods.EqualsAny(turnLog[2].ToString(), ((int)GameBoard.BoardValues.WhiteKing).ToString(), ((int)GameBoard.BoardValues.WhitePawn).ToString()))
                    return false;
            }
            else
            {
                if (HelperMethods.EqualsAny(turnLog[2].ToString(), ((int)GameBoard.BoardValues.BlackKing).ToString(), ((int)GameBoard.BoardValues.BlackPawn).ToString()))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Validates proposed move action
        /// </summary>
        /// <returns></returns>
        internal static bool ValidateMove(ref string tempLog)
        {
            return CheckMoveLength(ref tempLog) && CheckField(ref tempLog);
        }

        /// <summary>
        /// Checks distance of the move action
        /// </summary>
        /// <returns></returns>
        internal static bool CheckMoveLength(ref string tempLog)
        {
            return Math.Sqrt(Math.Pow(HelperMethods.ToInt(tempLog[0]) - HelperMethods.ToInt(tempLog[3]), 2) +
                             Math.Pow(HelperMethods.ToInt(tempLog[1]) - HelperMethods.ToInt(tempLog[4]), 2)).IsBetween(0, 2);
        }

        internal static bool GetAllPossibleMoves(bool teamWhiteTurn, int[,] gameArray, List<string> moveList)
        {
            moveList?.Clear();

            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (teamWhiteTurn)
                    {
                        if (HelperMethods.EqualsAny(gameArray[col, row].ToString(), ((int)GameBoard.BoardValues.WhiteKing).ToString(),
                            ((int)GameBoard.BoardValues.WhitePawn).ToString()))
                        {
                            GetAllPositionMoves(col, row, gameArray, moveList);
                        }
                    }
                    else
                    {
                        if (HelperMethods.EqualsAny(gameArray[col, row].ToString(), ((int)GameBoard.BoardValues.BlackKing).ToString(),
                            ((int)GameBoard.BoardValues.BlackPawn).ToString()))
                        {
                            GetAllPositionMoves(col, row, gameArray, moveList);
                        }
                    }
                }
            }
            return (moveList?.Count > 0);
        }

        internal static bool GetAllPositionMoves(int col, int row, int[,] gameArray, List<string> moveList)
        {
            List<int> tryListI = new List<int>(){ -1, 0, 1 };
            List<int> tryListJ = new List<int>(){ -1, 0, 1 };

            foreach (var i in tryListI)
            {
                foreach (var j in tryListJ)
                {
                    if (GameBoard.IsOutOfBoard($"{col + j}{row + i}"))
                        continue;

                    var turnLog = col.ToString() + row.ToString() + gameArray[col, row].ToString();
                    turnLog += (col + j).ToString() + (row + i).ToString() +
                               gameArray[col + j, row + i].ToString();
                    if (CheckField(ref turnLog))
                    {
                        moveList?.Add(turnLog);
                    }
                }
            }

            return (moveList?.Count > 0);
        }


        /// <summary>
        /// Checks what the result of move action should be
        /// </summary>
        /// <returns></returns>
        internal static bool CheckField(ref string tempLog)
        {
            //Check whether new field is valid for movement
            if (!HelperMethods.EqualsAny(tempLog[5].ToString(), ((int)GameBoard.BoardValues.Barrier).ToString(), ((int)GameBoard.BoardValues.Frozen).ToString()))
            {
                //if empty, move is valid
                if (HelperMethods.ToInt(tempLog[5]) == (int)GameBoard.BoardValues.Empty)
                {
                    tempLog += ((int)GameBoard.TurnResults.Moved).ToString();
                    return true;
                }

                //Determines turn result depending on what is the selected unit ańd content of new field
                switch (HelperMethods.ToInt(tempLog[2]))
                {
                    case (int)GameBoard.BoardValues.BlackPawn:
                        {
                            if (HelperMethods.EqualsAny(tempLog[5].ToString(), ((int)GameBoard.BoardValues.WhitePawn).ToString(),
                                ((int)GameBoard.BoardValues.WhiteKing).ToString()))
                            {
                                tempLog += ((int)GameBoard.TurnResults.Frozen).ToString();
                                return true;

                            }
                            return false;
                        }
                    case (int)GameBoard.BoardValues.WhitePawn:
                        {
                            if (HelperMethods.EqualsAny(tempLog[5].ToString(), ((int)GameBoard.BoardValues.BlackPawn).ToString(),
                                ((int)GameBoard.BoardValues.BlackKing).ToString()))
                            {
                                tempLog += ((int)GameBoard.TurnResults.Frozen).ToString();
                                return true;
                            }

                            return false;
                        }

                    default: return false;

                }
            }

            return false;
        }



        #endregion

        #region Endgame

        public static bool GameEndTie(int[,] gameArray)
        {
            int WhiteKings = 0;
            int BlackKings = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int)GameBoard.BoardValues.WhiteKing)
                        WhiteKings++;
                    if (gameArray[col, row] == (int)GameBoard.BoardValues.BlackKing)
                        BlackKings++;
                }
            }

            if (WhiteKings == 0 && BlackKings == 0)
            {
                return true;
            }

            return false;
        }

        public static bool GameEndPlayerWWin(int[,] gameArray, List<string> moveList)
        {
            if (GameEndWin(gameArray, true))
                return true;
            if (!Rules.GetAllPossibleMoves(false, gameArray, moveList))
                return true;
            return false;
        }

        public static bool GameEndPlayerBWin(int[,] gameArray, List<string> moveList)
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
                
                if (PlayerW && gameArray[col, 1] == (int)GameBoard.BoardValues.WhiteKing)
                {
                    return true;
                }

                if (!PlayerW && gameArray[col, AppConstants.BoardSize] == (int)GameBoard.BoardValues.BlackKing)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
