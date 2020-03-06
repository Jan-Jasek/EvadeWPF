using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using AppShared;

namespace EvadeLogic
{
    public class ArtificialIntelligence
    {
        public static List<List<int>> MoveList { get; set; } = new List<List<int>>();
        public static int Count { get; set; } = 0;
        public static int CountedGameScore { get; set; } = 0;
        public static int CountedKingsDistance { get; set; } = 0;
        public static int CountedPawnsDistance { get; set; } = 0;
        public static int OutputRating { get; set; }
        public static List<int> OutputMove { get; set; }
        public static AILevels AILevelW { get; set; } = AILevels.Smart;
        public static AILevels AILevelB { get; set; } = AILevels.Smart;
        public static bool IsPlayerWTurn { get; set; }

        public static List<int> FindBestMove(AILevels aILevel, int[,] gameArray, bool isPlayerWTurn)
        {

            if (aILevel == 0)
            {
                return MoveList[AppConstants.Rnd.Next(MoveList.Count)];
            }

            AppConstants.Depth = (int)aILevel;

            OutputRating = -AppConstants.Max;
            OutputMove?.Clear();
            IsPlayerWTurn = isPlayerWTurn;

            CountedGameScore = CountGameScore(gameArray);
            CountedKingsDistance = CountKingsDistance(gameArray);
            CountedPawnsDistance = CountPawnsDistance(gameArray);

            MoveList.Shuffle();
            foreach (var move in MoveList.ToList())
            {
                DoTempMove(move, gameArray);
                int rating = -AlphaBeta(gameArray, AppConstants.Depth, false, -AppConstants.Max, Dal(AppConstants.Max));
                UndoTempMove(move, gameArray);
                //Increases the value for frozing moves
                rating += (Hunting((move[5]), (move[6])) * (AppConstants.Depth + 1));

                //Console.WriteLine($"Final output move {string.Join("",move)} rating {rating}");
                if (rating > OutputRating)
                {
                    OutputRating = rating;
                    OutputMove = move;
                }
            }
            //Console.WriteLine(Count);
            return OutputMove;
        }

        public static int Hunting(int unit, int result)
        {
            if ((unit == (int) BoardValues.BlackKing || unit == (int) BoardValues.WhiteKing) 
                && result == (int)BoardValues.Frozen)
                return 1;
            return 0;
        }

        public static int AlphaBeta(int[,] gameArray, int depth, bool maximizingPlayer, int alpha, int beta,
                                    string movex = "")
        {
            int rating = -AppConstants.Max;

            //If Tie
            if (Rules.IsGameEndTie(gameArray))
                return 0;

            //Is playerW turn
            if ((IsPlayerWTurn && maximizingPlayer) || (!IsPlayerWTurn && !maximizingPlayer))
            //if maximizingPlayer)
            {
                //If won
                if (Rules.GameEndPlayerWWin(gameArray, new List<List<int>>()))
                    return AppConstants.Max;
                //If lost
                if (Rules.GameEndPlayerBWin(gameArray, MoveList))
                    return -AppConstants.Max;
            }
            //IS playerB turn
            else
            {
                //If won
                if (Rules.GameEndPlayerBWin(gameArray, new List<List<int>>()))
                    return AppConstants.Max;
                //If lost
                if (Rules.GameEndPlayerWWin(gameArray, MoveList))
                    return -AppConstants.Max;
            }

            //If reached the depth level
            if (depth == 0)
            {
                //Return turn evaluation
                if ((IsPlayerWTurn && maximizingPlayer) || (!IsPlayerWTurn && !maximizingPlayer))
                //if maximizingPlayer
                {
                    rating = HeuristicEvaluation(gameArray, movex);
                }
                else
                {
                    rating = -HeuristicEvaluation(gameArray, movex);
                }

                return rating;
            }

            MoveList.Shuffle();
            foreach (var move in MoveList.ToList())
            {
                //int[,] gameArray2 = HelperMethods.CloneArray(gameArray);
                DoTempMove(move, gameArray);

                rating = -AlphaBeta(gameArray, depth - 1, !maximizingPlayer, Dal(-beta), Dal(-alpha));

                //Increases evaluation of frozing moves with respect to the count of moves needed
                //rating += HelperMethods.ToInt(move[6]) * depth;
                rating += (Hunting((move[5]), (move[6])) * depth);

                UndoTempMove(move, gameArray);
                //Count++;

                rating = Bliz(rating);
                if (rating > alpha)
                {
                    alpha = rating;
                    if (rating >= beta)
                    {
                        return beta;
                    }
                }
                //Console.WriteLine($"Depth: depth Move: {move} Rating: {rating}");
            }


            return alpha;
        }

        public static int Dal(int rating)
        {
            if(rating > AppConstants.Many)
                return rating + 1;
            if (rating < -AppConstants.Many)
                return rating - 1;
            return rating;
        }

        public static int Bliz(int rating)
        {
            if (rating > AppConstants.Many)
                return rating - 1;
            if (rating < -AppConstants.Many)
                return rating + 1;
            return rating;
        }
        
        public static void DoTempMove(List<int> move, int[,] gameArray)
        {
            GameBoard.SetField(gameArray, (move[0]), (move[1]));

            if ((move[6]) == (int)TurnResults.Moved)
            {
                GameBoard.SetField(gameArray, (move[3]), (move[4]), (move[2]));
            }
            else
            {
                GameBoard.SetField(gameArray, (move[3]), (move[4]), (int)BoardValues.Frozen);
            }
        }
        public static void UndoTempMove(List<int> move, int[,] gameArray)
        {
            GameBoard.SetField(gameArray, (move[0]),
                (move[1]), (move[2]));
            GameBoard.SetField(gameArray, (move[3]),
                (move[4]), (move[5]));
        }

        /// <summary>
        /// Evaluates the current state of the GameBoard
        /// </summary>
        /// <param name="gameArray"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static int HeuristicEvaluation(int[,] gameArray, string move = "")
        {
            var rating = 0;
            rating += (CountGameScore(gameArray) - CountedGameScore);
            rating += (CountKingsDistance(gameArray) - CountedKingsDistance)*4;
            rating += (CountPawnsDistance(gameArray) - CountedPawnsDistance);
            
            return rating;
        }
        /// <summary>
        /// Evaluate kings distance to the end of the board
        /// </summary>
        /// <param name="gameArray"></param>
        /// <returns></returns>
        private static int CountKingsDistance(int[,] gameArray)
        {
            int rating = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int) BoardValues.WhiteKing)
                    {
                        rating += (AppConstants.BoardSize - row + 1);

                    }

                    if (gameArray[col, row] == (int)BoardValues.BlackKing)
                    {
                        rating -= (row);

                    }
                }
            }

            return rating;
        }
        /// <summary>
        /// Evaluates pawns distance to kings
        /// </summary>
        /// <param name="gameArray"></param>
        /// <returns></returns>
        private static int CountPawnsDistance(int[,] gameArray)
        {
            int rating = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int) BoardValues.WhitePawn)
                    {
                        rating += CountMovesToKings(gameArray, col, row, true);
                    }

                    if (gameArray[col, row] == (int)BoardValues.BlackPawn)
                    {
                        rating -= CountMovesToKings(gameArray, col, row, false);
                    }
                }
            }

            return rating;
        }

        /// <summary>
        /// Counts pawns moves to kings
        /// </summary>
        /// <param name="gameArray"></param>
        /// <param name="colP"></param>
        /// <param name="rowP"></param>
        /// <param name="IsWhiteP"></param>
        /// <returns></returns>
        private static int CountMovesToKings(int[,] gameArray, int colP, int rowP, bool IsWhiteP)
        {
            int rating = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {

                    if (IsWhiteP)
                    {
                        if (gameArray[col, row] == (int)BoardValues.BlackKing)
                        {
                            rating += AppConstants.BoardSize - 1 - CountMovesToField(col, row, colP, rowP);
                        }
                    }
                    else
                    {
                        if (gameArray[col, row] == (int)BoardValues.WhiteKing)
                        {
                            rating += AppConstants.BoardSize - 1 - CountMovesToField(col, row, colP, rowP);
                            //rating += AppConstants.BoardSize - Math.Sqrt(Math.Pow(col - colP, 2) + Math.Pow(row - rowP, 2));

                        }
                    }

                }
            }

            return rating;
        }

        /// <summary>
        /// Counts moves to get to specific field
        /// </summary>
        /// <param name="colK"></param>
        /// <param name="rowK"></param>
        /// <param name="colP"></param>
        /// <param name="rowP"></param>
        /// <returns></returns>
        private static int CountMovesToField(int colK, int rowK, int colP, int rowP)
        {
            var colT = colP;
            var rowT = rowP;
            var count = 0;

            while (!(colT == colK && rowT == rowK))
            {
                if (colT < colK)
                    colT++;
                else if (colT > colK)
                    colT--;

                if (rowT < rowK)
                    rowT++;
                else if (rowT > rowK)
                    rowT--;

                count++;
            }

            return count;
        }


        /// <summary>
        /// Counts current GameScore based on present units
        /// </summary>
        /// <param name="gameArray"></param>
        /// <returns></returns>
        public static int CountGameScore(int[,] gameArray)
        {
            int score = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int) BoardValues.WhiteKing)
                        score += 100;
                    if (gameArray[col, row] == (int) BoardValues.BlackKing)
                        score -= 100;
                    if (gameArray[col, row] == (int) BoardValues.WhitePawn)
                        score += 0;
                    if (gameArray[col, row] == (int) BoardValues.BlackPawn)
                        score -= 0;
                }
            }

            return score;
        }
    }
}
