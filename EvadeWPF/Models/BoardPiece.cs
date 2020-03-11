using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppShared;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;

namespace EvadeWPF.Models
{
    public class BoardPiece : NotifyPropertyChanged, IBoardItem
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public BoardValues PieceType {get; set; }
        public bool RecommendedMove { get; set; } = false;
    }
}
