using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EvadeWPF.Helpers;
using EvadeWPF.Interfaces;
using EvadeWPF.Services;
using EvadeWPF.ViewModels;
using Unity;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ContainerLocator.UContainer
                .RegisterType<IGameEngine, GameEngine>()
                .RegisterType<GameWindowViewModel>();

        }

    }
}
