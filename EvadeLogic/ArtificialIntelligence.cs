using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Constants;

namespace Checkers
{
    public class ArtificialIntelligence
    {
        public static string[] MovesLog;
        public static List<string> MoveList { get; set; } = new List<string>();
        public static List<string> OutputMoveList { get; set; } = new List<string>();
        public static List<int> OutputRatingList { get; set; } = new List<int>();
        public static int Count { get; set; } = 0;
        public static int CountedGameScore { get; set; } = 0;
        public static int CountedKingsDistance { get; set; } = 0;
        public static int CountedPawnsDistance { get; set; } = 0;
        public static int OutputRating { get; set; }
        public static string OutputLog { get; set; }
        public enum AILevels
        {
            Random = 0,
            Dumb = 1,
            Smart = 2
        }
        public static int AILevelW { get; set; } = (int) AILevels.Dumb;
        public static int AILevelB { get; set; } = (int) AILevels.Random;
        public static bool IsPlayerWTurn { get; set; }

        public static string FindBestMove(int aILevel, int[,] gameArray, bool isPlayerWTurn)
        {

            if (aILevel == 0)
            {
                return MoveList[AppConstants.Rnd.Next(MoveList.Count)];
            }
            if (aILevel == 1)
            {
                AppConstants.Depth = 1;
            }

            if (aILevel == 2)
            {
                AppConstants.Depth = 2;
            }

            OutputRating = -AppConstants.Max;
            OutputLog = "";
            OutputMoveList.Clear();
            OutputRatingList.Clear();
            IsPlayerWTurn = isPlayerWTurn;

            CountedGameScore = CountGameScore(gameArray);
            CountedKingsDistance = CountKingsDistance(gameArray);
            CountedPawnsDistance = CountPawnsDistance(gameArray);

            Rules.GetAllPossibleMoves(IsPlayerWTurn, gameArray, MoveList);
            MoveList.Shuffle();
            foreach (var move in MoveList.ToList())
            {
                DoAIMove(move, gameArray);
                //int rating = -Negamax(gameArray, AppConstants.Depth, false, move);
                int rating = -AlphaBeta(gameArray, AppConstants.Depth, false, -AppConstants.Max, Dal(AppConstants.Max), move);
                UndoAIMove(move, gameArray);
                //Increases the value for frozing moves
                rating += (HelperMethods.ToInt(move[6]) * (AppConstants.Depth + 1));

                OutputMoveList.Add(move);
                OutputRatingList.Add(rating);
                Console.WriteLine($"Final output move {move} rating {rating}");
                if (rating > OutputRating)
                {
                    OutputRating = rating;
                    OutputLog = move;
                }
            }
            //Console.WriteLine(Count);
            return OutputLog;
        }

        public static int AlphaBeta(int[,] gameArray, int depth, bool maximizingPlayer, int alpha, int beta,
                                    string movex = "")
        {
            int rating = -AppConstants.Max;

            //If Tie
            if (Rules.GameEndTie(gameArray))
                return 0;

            //Is playerW turn
            if ((IsPlayerWTurn && maximizingPlayer) || (!IsPlayerWTurn && !maximizingPlayer))
            {
                //If won
                if (Rules.GameEndPlayerWWin(gameArray, new List<string>()))
                    return AppConstants.Max;
                //If lost
                if (Rules.GameEndPlayerBWin(gameArray, MoveList))
                    return -AppConstants.Max;
            }
            //IS playerB turn
            else
            {
                //If won
                if (Rules.GameEndPlayerBWin(gameArray, new List<string>()))
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
                DoAIMove(move, gameArray);

                rating = -AlphaBeta(gameArray, depth - 1, !maximizingPlayer, Dal(-beta), Dal(-alpha), move);

                //Increases evaluation of frozing moves with respect to the count of moves needed
                rating += HelperMethods.ToInt(move[6]) * depth;
                UndoAIMove(move, gameArray);

                rating = Bliz(rating);
                if (rating > alpha)
                {
                    alpha = rating;
                    if (rating >= beta)
                    {
                        return beta;
                    }
                }
                //Count++;
                //Console.WriteLine($"Depth: {depth} Move: {move} Rating: {rating}");
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


        public static int Negamax(int[,] gameArray, int depth, bool maximizingPlayer, string movex = "")
        {
            int rating = -AppConstants.Max;

            //If Tie
            if (Rules.GameEndTie(gameArray))
                return 0;

            //Is playerW turn
            if ((IsPlayerWTurn && maximizingPlayer) || (!IsPlayerWTurn && !maximizingPlayer))
            {
                //If won
                if (Rules.GameEndPlayerWWin(gameArray, new List<string>()))
                    return AppConstants.Max;
                //If lost
                if (Rules.GameEndPlayerBWin(gameArray, MoveList))
                    return -AppConstants.Max;
            }
            //IS playerB turn
            else
            {
                //If won
                if (Rules.GameEndPlayerBWin(gameArray, new List<string>()))
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
                DoAIMove(move, gameArray);
                rating = Math.Max(rating, -Negamax(gameArray, depth - 1, !maximizingPlayer, move));
                UndoAIMove(move, gameArray);
                //Increases evaluation of frozing moves with respect to the count of moves needed
                rating += HelperMethods.ToInt(move[6]) * depth;

                Count++;
                //Console.WriteLine($"Depth: {depth} Move: {move} Rating: {rating}");
            }

            //Changes evaluation to choose options that more quickly ends the game
            if (rating > AppConstants.Many)
                rating -= 1;
            if (rating < -AppConstants.Many)
                rating += 1;

            return rating;
        }
        public static void DoAIMove(string move, int[,] gameArray)
        {
            GameBoard.SetField(gameArray, HelperMethods.ToInt(move[0]), HelperMethods.ToInt(move[1]));

            if (HelperMethods.ToInt(move[6]) == (int)GameBoard.TurnResults.Moved)
            {
                GameBoard.SetField(gameArray, HelperMethods.ToInt(move[3]), HelperMethods.ToInt(move[4]), HelperMethods.ToInt(move[2]));
            }
            else
            {
                GameBoard.SetField(gameArray, HelperMethods.ToInt(move[3]), HelperMethods.ToInt(move[4]), (int)GameBoard.BoardValues.Frozen);
            }
        }
        public static void UndoAIMove(string move, int[,] gameArray)
        {
            GameBoard.SetField(gameArray, HelperMethods.ToInt(move[0]),
                HelperMethods.ToInt(move[1]), HelperMethods.ToInt(move[2]));
            GameBoard.SetField(gameArray, HelperMethods.ToInt(move[3]),
                HelperMethods.ToInt(move[4]), HelperMethods.ToInt(move[5]));
        }

        public static int HeuristicEvaluation(int[,] gameArray, string move = "")
        {
            var rating = 0;
            rating = CountGameScore(gameArray) - CountedGameScore;
            rating += (CountKingsDistance(gameArray) - CountedKingsDistance)*4;
            rating += (CountPawnsDistance(gameArray) - CountedPawnsDistance)^2;
            
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
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.WhiteKing)
                    {
                        rating += (AppConstants.BoardSize - row + 1);

                    }

                    if (gameArray[col, row] == (int)GameBoard.BoardValues.BlackKing)
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
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.WhitePawn)
                    {
                        rating += CountMovesToKings(gameArray, col, row, true);
                    }

                    if (gameArray[col, row] == (int)GameBoard.BoardValues.BlackPawn)
                    {
                        rating -= CountMovesToKings(gameArray, col, row, false);
                    }
                }
            }

            return rating;
        }
        private static int CountMovesToKings(int[,] gameArray, int colP, int rowP, bool IsWhiteP)
        {
            int rating = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {

                    if (IsWhiteP)
                    {
                        if (gameArray[col, row] == (int)GameBoard.BoardValues.BlackKing)
                        {
                            rating += AppConstants.BoardSize - 1 - CountMovesToField(col, row, colP, rowP);
                        }
                    }
                    else
                    {
                        if (gameArray[col, row] == (int)GameBoard.BoardValues.WhiteKing)
                        {
                            rating += AppConstants.BoardSize - 1 - CountMovesToField(col, row, colP, rowP);
                            //rating += AppConstants.BoardSize - Math.Sqrt(Math.Pow(col - colP, 2) + Math.Pow(row - rowP, 2));

                        }
                    }

                }
            }

            return rating;
        }

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

        public static int CountGameScore(int[,] gameArray)
        {
            int score = 0;
            for (int row = 1; row <= AppConstants.BoardSize; row++)
            {
                for (int col = 1; col <= AppConstants.BoardSize; col++)
                {
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.WhiteKing)
                        score += 100;
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.BlackKing)
                        score -= 100;
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.WhitePawn)
                        score += 0;
                    if (gameArray[col, row] == (int) GameBoard.BoardValues.BlackPawn)
                        score -= 0;
                }
            }

            return score;
        }

    }
}
