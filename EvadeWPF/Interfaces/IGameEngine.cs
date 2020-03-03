using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF.Interfaces
{
    public interface IGameEngine
    {

        void StartEngine();
        void StopEngine();
        event Action<string> OutputMessage;
        bool IsSelectValid();
        bool IsMoveValid();
        void NewGame();
    }
}
