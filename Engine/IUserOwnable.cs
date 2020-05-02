using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Engine
{
    public interface IUserOwnable
    {
        IUser Owner
        {
            get; set;
        }
        bool ChangeOwnership(IUser newOwner);        
    }
}
