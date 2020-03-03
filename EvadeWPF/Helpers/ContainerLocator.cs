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
    public static class ContainerLocator
    {
        public static UnityContainer UContainer = new UnityContainer();
    }
}
