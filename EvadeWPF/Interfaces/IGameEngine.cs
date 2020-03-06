using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvadeWPF.Models;

namespace EvadeWPF.Interfaces
{
    public interface IGameEngine
    {
        void StartEngine();
        void StopEngine();
        event Action<string> OutputMessage;
        event Action<string> RaiseEndGameTriggered;
        event Action<bool> EngineThinkingChanged;
        bool IsSelectValid(IBoardItem boardItem);
        bool IsMoveValid(IBoardItem boardItem);
        void Move();
        void NewGame();
        void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems);
        bool IsEngineThinking { get; set; }
    }
}
