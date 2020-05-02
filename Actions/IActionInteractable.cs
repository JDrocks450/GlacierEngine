using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common.Actions
{
    public interface IActionInteractable<T> where T : IActionRunnable
    {
        void Interact(ActionObjectGroup<T> Group);
    }
}
