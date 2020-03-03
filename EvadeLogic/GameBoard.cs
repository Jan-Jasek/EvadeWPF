using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeLogic
{
    public class GameBoard
    {
        #region Private fields

        private readonly int size = AppConstants.BoardSize;

        //Sloupec-řádek-jednotka-sloupec-řádek-jednotka-vysledekTahu
        #endregion

        #region Public props
        public int[,] GameArray = new int[AppConstants.BoardSize + 2, AppConstants.BoardSize + 2];

        public int TurnCounter { get; set; }

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

        public void NewGame()
        {
            for (int row = 0; row <= size + 1; row++)
            {
                for (int col = 0; col <= size + 1; col++)
                {
                    GameArray[col, row] = (row < 1 || row > size || col < 1 || col > size)
                        ? (int)AppConstants.BoardValues.Barrier
                        : (int)AppConstants.BoardValues.Empty;
                    if (GameArray[col, row] == (int)AppConstants.BoardValues.Empty)
                        GameArray[col, row] = (int)NewGameAddUnit(col, row);
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

        private AppConstants.BoardValues NewGameAddUnit(int col, int row)
        {
            if (row == 1)
            {
                if ((col == size / 2) || (col == size / 2 + 1))
                {
                    return AppConstants.BoardValues.BlackKing;
                }

                return AppConstants.BoardValues.BlackPawn;
            }
            if (row == size)
            {
                if ((col == size / 2) || (col == size / 2 + 1))
                {
                    return AppConstants.BoardValues.WhiteKing;
                }

                return AppConstants.BoardValues.WhitePawn;
            }

            return AppConstants.BoardValues.Empty;
        }

        #endregion
        

        public static void SetField(int[,] gameArray, int col, int row, int result = (int)AppConstants.BoardValues.Empty)
        {
            gameArray[col, row] = result;
        }
    }
}
