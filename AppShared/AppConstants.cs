using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AppShared
{

    public enum BoardValues
    {
        Empty = 0, WhitePawn = 1, BlackPawn = 2, WhiteKing = 3, BlackKing = 4, Frozen = 5, Barrier = 7, Recommended = 9

    }

    public enum TurnResults
    {
        Moved = 0,
        Frozen = 1
    }

    public enum AILevels
    {
        Random = 0,
        Dumb = 2,
        Smart = 4
    }

    public static class AppConstants
    {
        public static int BoardSize = 6;
        public static int Depth = 3;
        public static int Max = 1000;
        public static int Many = 900;

        public static Dictionary<string, int> ColumnValues = new Dictionary<string, int>()
        {
            {"A",1},
            {"B",2},
            {"C",3},
            {"D",4},
            {"E",5},
            {"F",6}
        };

        public static Dictionary<string, int> PieceValues = new Dictionary<string, int>()
        {
            {"White Pawn",1},
            {"Black Pawn",2},
            {"White King",3},
            {"Black King",4}
        };

        public static Dictionary<string, string> OptionsValues = new Dictionary<string, string>()
        {
            {"A3","Select or move to field A3"},
            {"!PrintBoard","Prints current GameBoard." },
            {"!PlayerW","Switch white player from human to AI or vice versa."},
            {"!PlayerB","Switch black player from human to AI or vice versa."},
            {"!DifficultyW","Select AI difficulty for white player" },
            {"!DifficultyB","Select AI difficulty for black player" },
            {"!Hint","Hints best possible move." },
            {"!EndGame","Ends current game." }
        };

        public static string NewGameStarted = "New game has started!\n";
        public static string WrongFieldSelected = "Invalid field selected. Please select a valid field.\n";
        public static string WrongMoveSelected = "Invalid move.\n";
        public static string FieldFrozen = "The field is now frozen. ";
        public static string InvalidInput = "Invalid input.\n";
        public static string BlackTeamTurn = "\nBlack team's turn.";
        public static string WhiteTeamTurn = "\nWhite team's turn.";
        public static string WriteHelp = "Write '!help' for help!";
        public static string GameEndTie = "Game ended in tie!";
        public static string NoPossibleTurns = "No possible turns. Game Ends.";
        public static string Black = "Black";
        public static string White = "White";
        public static string AITurn = "AI turn. Press enter to play.";
        public static string SelectUnit = "Select unit by entering number of column and row(e.q.H3) followed by enter.";
        public static string SelectPosition = "Select new position you wish to move to (e.q. H3) followed by enter.";
        public static string SelectDifficulty = "Please select AI difficulty. 0 = random, 1 = dumb, 2 = smart.";
        public static string PressEnter = "Press enter to continue.";

        public const string Help = "!HELP";
        public const string PlayerW = "!PLAYERW";
        public const string PlayerB = "!PLAYERB";
        public const string PrintBoard = "!PRINTBOARD";
        public const string DifficultyW = "!DIFFICULTYW";
        public const string DifficultyB = "!DIFFICULTYB";
        public const string Hint = "!HINT";
        public const string EndGame = "!ENDGAME";

        public static Random Rnd { get; set; } = new Random();


        public static string ParseColumnValues(string columnNumber)
        {
            foreach (KeyValuePair<string, int> item in ColumnValues)
            {
                if (item.Value == int.Parse(columnNumber))
                {
                    return item.Key;
                }
            }

            return "";
        }

        public static string ParsePieceValues(string pieceNumber)
        {
            foreach (KeyValuePair<string, int> item in AppConstants.PieceValues)
            {
                if (item.Value == int.Parse(pieceNumber))
                {
                    return item.Key;
                }
            }

            return "";
        }

    }

    public static class RelativeFontSize
    {
        public static readonly DependencyProperty RelativeFontSizeProperty =
            DependencyProperty.RegisterAttached("RelativeFontSize", typeof(Double), typeof(RelativeFontSize), new PropertyMetadata(1.0, HandlePropertyChanged));

        public static Double GetRelativeFontSize(Control target)
            => (Double)target.GetValue(RelativeFontSizeProperty);

        public static void SetRelativeFontSize(Control target, Double value)
            => target.SetValue(RelativeFontSizeProperty, value);

        static Boolean isInTrickery = false;

        public static void HandlePropertyChanged(Object target, DependencyPropertyChangedEventArgs args)
        {
            if (isInTrickery) return;

            if (target is Control control)
            {
                isInTrickery = true;

                try
                {
                    control.SetValue(Control.FontSizeProperty, DependencyProperty.UnsetValue);

                    var unchangedFontSize = control.FontSize;

                    var value = (Double)args.NewValue;

                    control.FontSize = unchangedFontSize * value;

                    control.SetValue(Control.FontSizeProperty, unchangedFontSize * value);
                }
                finally
                {
                    isInTrickery = false;
                }
            }
        }
    }
}

