using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EvadeWPF.Helpers;
using EvadeWPF.ViewModels;
using Microsoft.Xaml.Behaviors;
using Unity;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GameWindow
    {
        private Boolean _autoScroll = true;

        public GameWindow()
        {
            InitializeComponent();
            DataContext = ContainerLocator.UContainer.Resolve<GameWindowViewModel>();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (sv.VerticalOffset == sv.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    _autoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    _autoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (_autoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                sv.ScrollToVerticalOffset(sv.ExtentHeight);
            }
        }

    }

    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {
        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox listBox = (sender as ListBox);
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();
                            if (listBox.SelectedItem !=
                                null)
                                listBox.ScrollIntoView(
                                    listBox.SelectedItem);
                        }));
                }
            }
        }
        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -=
                AssociatedObject_SelectionChanged;

        }
    }

    public class KeyDownWithArgsBehavior : Behavior<UIElement>
    {
        public ICommand KeyDownCommand
        {
            get { return (ICommand)GetValue(KeyDownCommandProperty); }
            set { SetValue(KeyDownCommandProperty, value); }
        }

        public static readonly DependencyProperty KeyDownCommandProperty =
            DependencyProperty.Register("KeyDownCommand", typeof(ICommand), typeof(KeyDownWithArgsBehavior), new UIPropertyMetadata(null));


        protected override void OnAttached()
        {
            AssociatedObject.KeyDown += new KeyEventHandler(AssociatedObjectKeyUp);
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= new KeyEventHandler(AssociatedObjectKeyUp);
            base.OnDetaching();
        }

        private void AssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            if (KeyDownCommand != null)
            {
                KeyDownCommand.Execute(e.Key);
            }
        }
    }
}
