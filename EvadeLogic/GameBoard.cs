using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;

namespace Checkers
{
    public class GameBoard
    {
        #region Private fields

        private int size = AppConstants.BoardSize;

        //Sloupec-řádek-jednotka-sloupec-řádek-jednotka-vysledekTahu
        #endregion

        #region Public props
        public int[,] GameArray = new int[AppConstants.BoardSize + 2, AppConstants.BoardSize + 2];

        public int TurnCounter { get; set; }

        public enum BoardValues
        {
            Empty = 0, WhitePawn = 1, BlackPawn = 2, WhiteKing = 3, BlackKing = 4, Frozen = 5, Barrier = 7

        }

        public enum TurnResults
        {
            Moved = 0,
            Frozen = 1
        }

        #endregion

        #region Constructors

        public GameBoard()
        {
        }

        #endregion
        /// <summary>
        /// starts new game
        /// </summary>
        #region NewGame

        public void NewGame_Start()
        {
            for (int row = 0; row <= size + 1; row++)
            {
                for (int col = 0; col <= size + 1; col++)
                {
                    GameArray[col, row] = (row < 1 || row > size || col < 1 || col > size)
                        ? (int)BoardValues.Barrier
                        : (int)BoardValues.Empty;
                    if (GameArray[col, row] == (int)BoardValues.Empty)
                        GameArray[col, row] = (int)NewGame_AddUnit(col, row);
                    //GameArray[1, 6] = (int)BoardValues.WhiteKing;
                    //GameArray[3, 3] = (int)BoardValues.WhitePawn;
                    //GameArray[1, 1] = (int)BoardValues.BlackPawn;
                    //GameArray[1, 2] = (int)BoardValues.WhiteKing;
                    //GameArray[1, 3] = (int)BoardValues.BlackKing;
                    //GameArray[1, 1] = (int)BoardValues.BlackKing;
                    //GameArray[6, 4] = (int)BoardValues.WhiteKing;
                    //GameArray[4, 1] = (int)BoardValues.BlackPawn;
                }

            }
        }

        private BoardValues NewGame_AddUnit(int col, int row)
        {
            if (row == 1)
            {
                if ((col == size / 2) || (col == size / 2 + 1))
                {
                    return BoardValues.BlackKing;
                }

                return BoardValues.BlackPawn;
            }
            if (row == size)
            {
                if ((col == size / 2) || (col == size / 2 + 1))
                {
                    return BoardValues.WhiteKing;
                }

                return BoardValues.WhitePawn;
            }

            return BoardValues.Empty;
        }

        #endregion

        /// <summary>
        /// Checks whether the input is within the game board size
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public bool IsOutOfBoard(string message = "")
        {

            if (HelperMethods.ToInt(message[0]) < 1 || HelperMethods.ToInt(message[0]) > AppConstants.BoardSize
                                                    || HelperMethods.ToInt(message[1]) < 1 
                                                    || HelperMethods.ToInt(message[1]) > AppConstants.BoardSize)
            {
                return true;
            }

            return false;
        }


        public static void SetField(int[,] gameArray, int col, int row, int result = (int)GameBoard.BoardValues.Empty)
        {
            gameArray[col, row] = result;
        }
    }
}
