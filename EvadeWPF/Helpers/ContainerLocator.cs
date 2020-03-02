using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvadeWPF.Interfaces;
using EvadeWPF.Services;
using EvadeWPF.ViewModels;
using Unity;

namespace EvadeWPF.Helpers
{
    public class ContainerLocator
    {
        private UnityContainer _container;
        public ContainerLocator()
        {
            _container = new UnityContainer();
            _container.RegisterType<IGameEngine, GameEngine>();
        }

        public GameWindowViewModel GameVM => _container.Resolve<GameWindowViewModel>();
    }
}
