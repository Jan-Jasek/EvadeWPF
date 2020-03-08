using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvadeLogic;
using EvadeWPF.Models;

namespace EvadeWPF.Interfaces
{
    public interface IGameEngine
    {
        void StartEngine();
        GameManager gameManager { get; }
        void StopEngine();
        event Action<string> OutputMessage;
        event Action<string> RaiseEndGameTriggered;
        event Action<bool> EngineThinkingChanged;
        bool IsSelectValid(IBoardItem boardItem);
        bool IsMoveValid(IBoardItem boardItem);
        void GameTurn();
        void NewGame();
        void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems);
        bool IsEngineThinking { get; set; }
        void PlayBestMove();
        void UndoMove();
        void RedoMove();
    }
}
