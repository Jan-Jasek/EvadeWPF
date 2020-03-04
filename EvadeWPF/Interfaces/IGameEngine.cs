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
        bool IsSelectValid(IBoardItem boardItem);
        bool IsMoveValid(IBoardItem boardItem);
        void Move(IBoardItem boardItem);
        void NewGame();
        void AddUnitsFromGameBoard(ObservableCollection<IBoardItem> boardItems);

    }
}
