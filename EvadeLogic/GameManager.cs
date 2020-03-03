using System;
using System.Collections.Generic;
using System.Linq;

namespace EvadeLogic
{
    public class GameManager
    {
        #region Private fields

        private GameBoard gameBoard;
        private CheckersConsole.CheckersConsole checkersConsole;
        //FormatLog = Column-row-unit-column-row-unit-turnResult
        private string tempLog;
        private string outputLog;

        private List<int> Move = new List<int>();
        #endregion

        #region Public properties
        public List<List<int>> MoveList { get; set; } = new List<List<int>>();

        public bool IsGameRunning { get; set; } = false;
        public bool IsPlayerWTurn { get; set; } = true;

        public bool IsPlayerWAI { get; set; } = true;
        public bool IsPlayerBAI { get; set; } = true;

        #endregion

        #region Constructors

        public GameManager()
        {
            gameBoard = new GameBoard();
            checkersConsole = new CheckersConsole.CheckersConsole {consoleArray = gameBoard.GameArray, size = AppConstants.BoardSize};
        }

        #endregion

        #region NewGame
        /// <summary>
        /// starts new game
        /// </summary>
        public void NewGame()
        {
            gameBoard.NewGame();
            checkersConsole.NewGame();
            IsGameRunning = true;
            gameBoard.TurnCounter = 0;

            GameLoop();
        }

        #endregion


        /// <summary>
        /// Main game loop, awaits user input and count turns.
        /// </summary>
        private void GameLoop()
        {
            //If game is running
            while (IsGameRunning)
            {
                Move.Clear();
                outputLog = "";
                tempLog = "";

                IsPlayerWTurn = (gameBoard.TurnCounter % 2 != 1);
                checkersConsole.PrintBoard();

                if (IsGameEndTriggered(gameBoard.GameArray))
                    break;

                GameTurn();

                gameBoard.TurnCounter++;
            }

            //Starts new game if no game is running
            checkersConsole.NoGameRunning();
            NewGame();
        }

        private void GameTurn()
        {

            if (IsPlayerOnTurnAI())
            {
                AIGameTurn();
            }

            if (!IsPlayerOnTurnAI())
            {
                PlayerGameTurn();
            }

        }

        private bool IsPlayerOnTurnAI()
        {
            return (IsPlayerWTurn && IsPlayerWAI) || (!IsPlayerWTurn && IsPlayerBAI);
        }

        /// <summary>
        /// AI game turn
        /// </summary>
        private void AIGameTurn()
        {
            string inputMessage;
            while (true)
            {
                PrintTeamsTurn();

                inputMessage = checkersConsole.PromptUser(AppConstants.AITurn);
                if (!InputOptions(inputMessage))
                    continue;

                if (!IsGameRunning)
                    return;

                break;
            }

            if (!IsGameRunning)
                return;
            var aILevel = IsPlayerWTurn ? ArtificialIntelligence.AILevelW : ArtificialIntelligence.AILevelB;
            ArtificialIntelligence.MoveList = new List<List<int>>(MoveList);

            Move = ArtificialIntelligence.FindBestMove(aILevel,gameBoard.GameArray,IsPlayerWTurn);
            tempLog = string.Join("", Move);
            //Console.WriteLine(tempLog);
            outputLog = HelperMethods.ParseOutputLog(tempLog);
            DoMove();
        }


        #region EndGame

        /// <summary>
        /// Checks ending criteria of the game
        /// </summary>
        private bool IsGameEndTriggered(int[,] gameArray)
        {
            if (Rules.IsGameEndTie(gameBoard.GameArray))
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
            checkersConsole.PrintBoard();
            checkersConsole.PrintMessage(AppConstants.GameEndTie);
            IsGameRunning = false;
        }

        private bool GameEndWin()
        {
            string message = "";
            bool end = false;
            if (Rules.GameEndPlayerWWin(gameBoard.GameArray, MoveList))
            {
                message = AppConstants.White;
                end = true;
            }

            if (Rules.GameEndPlayerBWin(gameBoard.GameArray, MoveList))
            {
                message = AppConstants.Black;
                end = true;
            }
            if(end)
            {
                checkersConsole.PrintBoard();
                checkersConsole.GameWon(message);
                IsGameRunning = false;
                return true;
            }

            return false;
        }

        private void GameEndNoPossibleTurns()
        {
            checkersConsole.PrintBoard();
            checkersConsole.PrintMessage(AppConstants.NoPossibleTurns);
            IsGameRunning = false;
        }


        #endregion


        #region InputHandlers
        /// <summary>
        /// Validates user input and prints help if the input is not recognized.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ValidateInput(string message)
        {
            //If Empty
            if (message == "")
            {
                checkersConsole.PrintMessage(AppConstants.InvalidInput);
                checkersConsole.PrintMessage(AppConstants.WriteHelp);
                return false;
            }
            //OptionsMenu
            if (message[0] == '!')
            {
                return InputOptions(message);
            }
            //If Valid move
            if (AppConstants.ColumnValues.Any(n => n.Key == message?[0].ToString()) && message.Length == 2)
            {
                if (!Rules.IsOutOfBoard(HelperMethods.ToInt(HelperMethods.ParseInputLog(message)[0]), HelperMethods.ToInt(HelperMethods.ParseInputLog(message)[1])))
                    return true;
            }

            checkersConsole.PrintMessage(AppConstants.InvalidInput);
            checkersConsole.PrintMessage(AppConstants.WriteHelp);   
            return false;
        }

        private bool InputOptions(string message)
        {
            int temp;
            switch (message)
            {
                default:
                    checkersConsole.PrintMessage(AppConstants.InvalidInput);
                    checkersConsole.PrintMessage(AppConstants.WriteHelp);
                    return false;
                case AppConstants.Help:
                    PrintHelp();
                    return false;
                case "":
                    return true;
                case AppConstants.PlayerB:
                    IsPlayerBAI = !IsPlayerBAI;
                    checkersConsole.PrintMessage($"Black player switched. Is it AI? {IsPlayerBAI}.");
                    GameLoop();
                    return false;
                case AppConstants.PlayerW:
                    IsPlayerWAI = !IsPlayerWAI;
                    checkersConsole.PrintMessage($"White player switched. Is it AI? {IsPlayerWAI}.");
                    GameLoop();
                    return false;
                case AppConstants.PrintBoard:
                    checkersConsole.PrintBoard();
                    return false;
                case AppConstants.DifficultyW:
                    temp = SwitchDifficulty();
                    if (temp != -1)
                        ArtificialIntelligence.AILevelW = (AppConstants.AILevels) temp;
                    return false;
                case AppConstants.DifficultyB:
                    temp = SwitchDifficulty();
                    if (temp != -1)
                        ArtificialIntelligence.AILevelB = (AppConstants.AILevels) temp;
                    return false; 
                case AppConstants.Hint:
                    HintBestMove();
                    return false;  
                case AppConstants.EndGame:
                    IsGameRunning = false;
                    return true;
            }
        }

        private void HintBestMove()
        {
            checkersConsole.PrintMessage(string.Join("",ArtificialIntelligence.FindBestMove(AppConstants.AILevels.Smart,gameBoard.GameArray,IsPlayerWTurn)));
        }

        private int SwitchDifficulty()
        {
            string diff = checkersConsole.PromptUser(AppConstants.SelectDifficulty);
            switch (diff)
            {
                default:
                    checkersConsole.PrintMessage(AppConstants.InvalidInput);
                    return -1;
                case "0":
                    return (int)AppConstants.AILevels.Random;
                case "1":
                    return (int)AppConstants.AILevels.Dumb;
                case "2":
                    return (int)AppConstants.AILevels.Smart;
            }
            
        }

        #endregion


        #region PlayerGameTurn
        /// <summary>
        /// Single game turn, prints game board, empties logs, then selects unit, moves it and check whether the game has been won.
        /// </summary>
        private void PlayerGameTurn()
        {
            SelectUnit();
            checkersConsole.PrintBoard();
            PlayerMove();
        }
        /// <summary>
        /// Prompts user to select new unit to move. Then checks if the selection is valid.
        /// </summary>
        private void SelectUnit()
        {
            //Prompts user for selecting unit
            string selectedPosition;
            int selectedValue;
            string inputMessage;

            while (true)
            {
                PrintTeamsTurn();

                inputMessage = checkersConsole.PromptUser(AppConstants.SelectUnit);
                if (!ValidateInput(inputMessage))
                    continue;

                if (!IsGameRunning)
                    return;

                selectedPosition = HelperMethods.ParseInputLog(inputMessage);
                selectedValue = gameBoard.GameArray[HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1])];

                var move = new List<int>()
                    {HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1]), selectedValue};

                if (Rules.SelectedUnit_Check(move, gameBoard.TurnCounter))
                {
                    Move.AddRange(move);
                    break;

                }
                checkersConsole.PrintMessage(AppConstants.WrongFieldSelected);
            }


            outputLog = inputMessage + selectedValue;

            GameBoard.SetField(gameBoard.GameArray,HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1]), 9);
            checkersConsole.SelectedUnit(outputLog);
        }

        /// <summary>
        /// Prints who's turn it is
        /// </summary>
        private void PrintTeamsTurn()
        {
            if (IsPlayerWTurn)
            {
                checkersConsole.PrintMessage(AppConstants.WhiteTeamTurn);
            }
            else
            {
                checkersConsole.PrintMessage(AppConstants.BlackTeamTurn);
            }
        }

        /// <summary>
        /// Prompts user for a new position to move to
        /// </summary>
        /// <returns></returns>
        private void GetNewPosition()
        {
            //Prompts user for new position
            while (true)
            {
                PrintTeamsTurn();

                var inputMessage = checkersConsole.PromptUser(AppConstants.SelectPosition);
                if (!ValidateInput(inputMessage))
                    continue;

                if (!IsGameRunning)
                    return;

                var newPosition = HelperMethods.ParseInputLog(inputMessage);
                var oldValue = gameBoard.GameArray[HelperMethods.ToInt(newPosition[0]), HelperMethods.ToInt(newPosition[1])];

                var move = new List<int>(Move.Select(i => (i)));
                move.AddRange( new List<int>() { HelperMethods.ToInt(newPosition[0]), HelperMethods.ToInt(newPosition[1]), oldValue});
                outputLog += inputMessage + oldValue;

                if (Rules.ValidateMove(move))
                {
                    Move = move;
                    break;
                }

                checkersConsole.PrintMessage(AppConstants.WrongMoveSelected);
            }

        }
        /// <summary>
        /// Performs movement of the selected unit
        /// </summary>
        private void PlayerMove()
        {
            GetNewPosition();
            DoMove();

        }

        private void DoMove()
        {
            outputLog += Move[6].ToString();
            GameBoard.SetField(gameBoard.GameArray,Move[0], Move[1]);

            if (IsPlayerOnTurnAI())
            {
                if (Move[6] == (int)AppConstants.TurnResults.Moved)
                {
                    GameBoard.SetField(gameBoard.GameArray,Move[3], Move[4], Move[2]);
                    checkersConsole.LogTurn(outputLog, AppConstants.PressEnter);
                }
                else
                {
                    GameBoard.SetField(gameBoard.GameArray,Move[3], Move[4], (int)AppConstants.BoardValues.Frozen);
                    checkersConsole.LogTurn(outputLog, AppConstants.FieldFrozen + AppConstants.PressEnter);
                }

            }
            else
            {
                if (Move[6] == (int)AppConstants.TurnResults.Moved)
                {
                    GameBoard.SetField(gameBoard.GameArray, Move[3], Move[4], Move[2]);
                    checkersConsole.LogTurn(outputLog);
                }
                else
                {
                    GameBoard.SetField(gameBoard.GameArray, Move[3], Move[4], (int)AppConstants.BoardValues.Frozen);
                    checkersConsole.LogTurn(outputLog, AppConstants.FieldFrozen);
                }
            }
        }

        #endregion


        /// <summary>
        /// Prints help options to the console.
        /// </summary>
        private void PrintHelp()
        {
            foreach (var n in AppConstants.OptionsValues)
            {
                checkersConsole.PrintMessage($"{n.Key} : {n.Value}");
            }
        }



    }
}
