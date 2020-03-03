using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EvadeWPF.Interfaces;

namespace EvadeWPF.Services
{
    public class GameEngine : IGameEngine
    {
        //private GameManager _gameManager;
        public List<List<int>> MoveList { get; set; } = new List<List<int>>();
        public List<int> Move { get; set; }
        public bool IsGameRunning { get; set; } = false;
        public bool IsPlayerWTurn { get; set; } = true;
        public bool IsPlayerWAI { get; set; } = false;
        public bool IsPlayerBAI { get; set; } = true;
        public event Action<string> OutputMessage;

        public void StartEngine()
        {
            //_gameManager = new GameManager();
            //_gameManager.NewGameStart();
            IsGameRunning = true;

        }

        public void StopEngine()
        {
            //_gameManager = null;
            IsGameRunning = false;
        }

        public bool IsSelectValid()
        {
            throw new NotImplementedException();
        }

        public bool IsMoveValid()
        {

            return true;
        }

        public void NewGame()
        {
            throw new NotImplementedException();
        }

        public void SendCommandToGame()
        {

        }

        public int[,] SendGameBoardToGameWindow()
        {
            //return _gameManager.GameBoard.GameArray;
            return new int[1,1];
        }

    }
}
