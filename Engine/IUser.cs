using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Engine
{
    public interface IUser
    {
        string Name { get; set; }
        Dictionary<IUserOwnable, int> Inventory { get; }
        bool InventoryContains(IUserOwnable item);
        T InventorySearch<T>() where T : IUserOwnable;
        uint InventoryTypeAbundance<T>() where T : IUserOwnable;
    }
}
