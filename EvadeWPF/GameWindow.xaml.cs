using System.Windows;
using System.Windows.Controls;
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

        private void MenuItemWithRadioButtons_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                RadioButton rb = mi.Icon as RadioButton;
                if (rb != null)
                {
                    rb.IsChecked = true;
                }
            }
        }
    }
}
