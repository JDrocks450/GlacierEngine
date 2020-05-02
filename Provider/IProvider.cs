using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public interface IProvider
    {
        ProviderManager Parent { get; set; }

        void Refresh(GameTime time);
    }
}
