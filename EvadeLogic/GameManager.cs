using System;
using System.Collections.Generic;
using System.Linq;
using AppShared;

namespace EvadeLogic
{
    public class GameManager
    {
        #region Private fields

        public GameBoard GameBoard { get; private set; }
        //FormatLog = Column-row-unit-column-row-unit-turnResult
        private string tempLog;
        private string outputLog;

        private List<int> Move = new List<int>();
        #endregion

        #region Public properties
        public List<List<int>> MoveList { get; set; } = new List<List<int>>();
        public bool IsGameRunning { get; set; } = false;
        public bool IsPlayerWTurn { get; set; } = true;
        public bool IsPlayerWAI { get; set; } = false;
        public bool IsPlayerBAI { get; set; } = false;

        #endregion

        #region Constructors

        public GameManager()
        {
            GameBoard = new GameBoard();
        }

        #endregion

        #region NewGame
        /// <summary>
        /// starts new game
        /// </summary>
        public void NewGame()
        {
            GameBoard.NewGame();
            IsGameRunning = true;
            GameBoard.TurnCounter = 0;
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

                IsPlayerWTurn = (GameBoard.TurnCounter % 2 != 1);
                //checkersConsole.PrintBoard();

                if (IsGameEndTriggered(GameBoard.GameArray))
                    break;

                GameTurn();

                GameBoard.TurnCounter++;
            }

            //Starts new game if no game is running
            //checkersConsole.NoGameRunning();
            NewGame();
        }

        private void GameTurn()
        {

            if (IsPlayerOnTurnAI)
            {
                AIGameTurn();
            }

            if (!IsPlayerOnTurnAI)
            {
                PlayerGameTurn();
            }

        }

        public bool IsPlayerOnTurnAI => (IsPlayerWTurn && IsPlayerWAI) || (!IsPlayerWTurn && IsPlayerBAI);


        /// <summary>
        /// AI game turn
        /// </summary>
        private void AIGameTurn()
        {
            string inputMessage;
            while (true)
            {
                PrintTeamsTurn();

                inputMessage = "";//checkersConsole.PromptUser(AITurn);
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

            Move = ArtificialIntelligence.FindBestMove(aILevel,GameBoard.GameArray,IsPlayerWTurn);
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
            //checkersConsole.PrintBoard();
            //checkersConsole.PrintMessage(GameEndTie);
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
                //checkersConsole.PrintBoard();
                //checkersConsole.GameWon(message);
                IsGameRunning = false;
                return true;
            }

            return false;
        }

        private void GameEndNoPossibleTurns()
        {
            //checkersConsole.PrintBoard();
            //checkersConsole.PrintMessage(NoPossibleTurns);
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
                //checkersConsole.PrintMessage(InvalidInput);
                //checkersConsole.PrintMessage(WriteHelp);
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

            //checkersConsole.PrintMessage(InvalidInput);
            //checkersConsole.PrintMessage(WriteHelp);   
            return false;
        }

        private bool InputOptions(string message)
        {
            int temp;
            switch (message)
            {
                default:
                    //checkersConsole.PrintMessage(InvalidInput);
                    //checkersConsole.PrintMessage(WriteHelp);
                    return false;
                case AppConstants.Help:
                    PrintHelp();
                    return false;
                case "":
                    return true;
                case AppConstants.PlayerB:
                    IsPlayerBAI = !IsPlayerBAI;
                    //checkersConsole.PrintMessage($"Black player switched. Is it AI? {IsPlayerBAI}.");
                    GameLoop();
                    return false;
                case AppConstants.PlayerW:
                    IsPlayerWAI = !IsPlayerWAI;
                    //checkersConsole.PrintMessage($"White player switched. Is it AI? {IsPlayerWAI}.");
                    GameLoop();
                    return false;
                case AppConstants.PrintBoard:
                    //checkersConsole.PrintBoard();
                    return false;
                case AppConstants.DifficultyW:
                    temp = SwitchDifficulty();
                    if (temp != -1)
                        ArtificialIntelligence.AILevelW = (AILevels) temp;
                    return false;
                case AppConstants.DifficultyB:
                    temp = SwitchDifficulty();
                    if (temp != -1)
                        ArtificialIntelligence.AILevelB = (AILevels) temp;
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
            //checkersConsole.PrintMessage(string.Join("",ArtificialIntelligence.FindBestMove(AILevels.Smart,GameBoard.GameArray,IsPlayerWTurn)));
        }

        private int SwitchDifficulty()
        {
            string diff = "";//checkersConsole.PromptUser(SelectDifficulty);
            switch (diff)
            {
                default:
                    //checkersConsole.PrintMessage(InvalidInput);
                    return -1;
                case "0":
                    return (int)AILevels.Random;
                case "1":
                    return (int)AILevels.Dumb;
                case "2":
                    return (int)AILevels.Smart;
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
            //checkersConsole.PrintBoard();
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

                inputMessage = "";//checkersConsole.PromptUser(SelectUnit);
                if (!ValidateInput(inputMessage))
                    continue;

                if (!IsGameRunning)
                    return;

                selectedPosition = HelperMethods.ParseInputLog(inputMessage);
                selectedValue = GameBoard.GameArray[HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1])];

                var move = new List<int>()
                    {HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1]), selectedValue};

                if (Rules.SelectedUnit_Check(move, GameBoard.TurnCounter))
                {
                    Move.AddRange(move);
                    break;

                }
                //checkersConsole.PrintMessage(WrongFieldSelected);
            }


            outputLog = inputMessage + selectedValue;

            GameBoard.SetField(GameBoard.GameArray,HelperMethods.ToInt(selectedPosition[0]), HelperMethods.ToInt(selectedPosition[1]), 9);
            //checkersConsole.SelectedUnit(outputLog);
        }

        /// <summary>
        /// Prints who's turn it is
        /// </summary>
        private void PrintTeamsTurn()
        {
            if (IsPlayerWTurn)
            {
                //checkersConsole.PrintMessage(WhiteTeamTurn);
            }
            else
            {
                //checkersConsole.PrintMessage(BlackTeamTurn);
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

                var inputMessage = "";//checkersConsole.PromptUser(SelectPosition);
                if (!ValidateInput(inputMessage))
                    continue;

                if (!IsGameRunning)
                    return;

                var newPosition = HelperMethods.ParseInputLog(inputMessage);
                var oldValue = GameBoard.GameArray[HelperMethods.ToInt(newPosition[0]), HelperMethods.ToInt(newPosition[1])];

                var move = new List<int>(Move.Select(i => (i)));
                move.AddRange( new List<int>() { HelperMethods.ToInt(newPosition[0]), HelperMethods.ToInt(newPosition[1]), oldValue});
                outputLog += inputMessage + oldValue;

                if (Rules.ValidateMove(move))
                {
                    Move = move;
                    break;
                }

                //checkersConsole.PrintMessage(WrongMoveSelected);
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
            GameBoard.SetField(GameBoard.GameArray,Move[0], Move[1]);

            if (IsPlayerOnTurnAI)
            {
                if (Move[6] == (int)TurnResults.Moved)
                {
                    GameBoard.SetField(GameBoard.GameArray,Move[3], Move[4], Move[2]);
                    //checkersConsole.LogTurn(outputLog, PressEnter);
                }
                else
                {
                    GameBoard.SetField(GameBoard.GameArray,Move[3], Move[4], (int)BoardValues.Frozen);
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

        #endregion


        /// <summary>
        /// Prints help options to the console.
        /// </summary>
        private void PrintHelp()
        {
            foreach (var n in AppConstants.OptionsValues)
            {
                //checkersConsole.PrintMessage($"{n.Key} : {n.Value}");
            }
        }



    }
}
