using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HelpWindow
    {
        public HelpWindow()
        {
            InitializeComponent();
            HelpTextBox.Text = "Evade Help. \n See Help --> Rules for game rules" +
                               "\n Use New Game to start a new game at default state" +
                               "\n Use Save Game to save a state of current state of the game to file, and Load Game to load the game from the file." +
                               "\n Use Best Move for a hint of best possible move at the current state of the game." +
                               "\n Use Undo / Redo for browsing the move history of the game. Playing a new move in one of the past states of the game removes the remaining moves from the history. Undoing an AI turn requires the user to click to the game board in order to start the next AI turn." +
                               "\n Use AI to set whether the players are controlled by AI algorithm or user." +
                               "\n Use Difficulty to set the AI difficulty of the players.";

        }
    }
}
