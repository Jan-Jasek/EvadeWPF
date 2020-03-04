using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AppShared;
using EvadeLogic;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;
using EvadeWPF.Models;
using EvadeWPF.ViewModels;

namespace EvadeWPF.Services
{
    public class GameEngine : IGameEngine
    {
        private GameManager _gameManager;
        public List<int> MoveLog { get; set; } = new List<int>();
        public List<int> TempMove { get; set; }

        public event Action<string> OutputMessage;

        public GameEngine()
        {
        }

        public void StartEngine()
        {
            _gameManager = new GameManager();
            _gameManager.NewGame();

        }

        public void StopEngine()
        {
            _gameManager = null;
        }

        public void Move(IBoardItem boardItem)
        {
            MoveLog.Clear();
        }

        //Validates selected unit piece
        public bool IsSelectValid(IBoardItem boardItem)
        {
            if (_gameManager.IsPlayerOnTurnAI)
                return false;
            TempMove = new List<int>() { boardItem.Col, boardItem.Row, (int)boardItem.PieceType };
            if (_gameManager.IsSelectValid(TempMove))
            {
                OutputMessage("Valid select!");
                MoveLog.AddRange(TempMove);
                return true;
            }
            else
            {
                OutputMessage("Invalid select!");
                return false;
            }
            
        }

        public bool IsMoveValid(IBoardItem boardItem)
        {
            if (_gameManager.IsPlayerOnTurnAI)
                return false;
            MoveLog.AddRange(new List<int>() { boardItem.Col, boardItem.Row, (int)boardItem.PieceType });
            if (_gameManager.IsMoveValid(MoveLog))
            {
                OutputMessage("Valid move!");
                return true;
            }
            else
            {
                OutputMessage("Invalid move!");
                MoveLog.RemoveRange(3,3);
                return false;
            }

        }
        //Starts NewGame
        public void NewGame()
        {
            _gameManager.NewGame();
        }

        public void SendCommandToGame()
        {

        }

        public void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems)
        {
            var gameBoard = _gameManager.GameBoard.GameArray;

            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    if (AppShared.HelperMethods.EqualsAny(gameBoard[i, j],
                        (int)BoardValues.BlackKing,(int)BoardValues.BlackPawn,(int)BoardValues.WhiteKing,(int)BoardValues.WhitePawn))
                    {
                        boardItems.Add(new BoardPiece() { Col = i-1, Row = j-1, PieceType = (BoardValues)gameBoard[i, j] });
                    }
                }
            }
        }


    }
}
