using System.Windows;
using EvadeWPF.Helpers;
using EvadeWPF.ViewModels;
using Unity;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();
            DataContext = ContainerLocator.UContainer.Resolve<GameWindowViewModel>();
        }

    }
}
