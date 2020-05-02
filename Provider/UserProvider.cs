using Glacier.Common.Engine;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public sealed class UserProvider<T> : IProvider where T : IUser, new()
    {
        private Dictionary<string, T> users = new Dictionary<string, T>();
        public T CurrentUser { get; private set; }
        public ProviderManager Parent { get; set; }

        public UserProvider(T CurrentUser)
        {
            this.CurrentUser = CurrentUser;
        }

        public T Get(string key)
        {
            if (users.TryGetValue(key, out var camera))
                return camera;
            else
                return default;
        }

        public T Create(string name = "root") => Add(new T() { Name = name });

        public T Add(T user)
        {
            if (Get(user.Name) == null)
                users.Add(user.Name, user);
            return user;
        }

        public void Refresh(GameTime time)
        {
            ;
        }
    }
}
