using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF.Interfaces
{
    public interface IBoardItem
    {
        int Row { get; set; }
        int Col { get; set; }
    }
}
