using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glacier.Common.Provider
{
    public class ProviderManager : IProvider
    {
        private static Dictionary<string, ProviderManager> _currentProviders = new Dictionary<string, ProviderManager>();
        private Dictionary<Type, IProvider> providers = new Dictionary<Type, IProvider>();
        public string KeyName
        {
            get;
            private set;
        }
        public ProviderManager Parent { get; set; }
        public static ProviderManager Root => GetRoot();

        private ProviderManager()
        {

        }
        public ProviderManager(string key)
        {
            _currentProviders.Add(key, this);
            KeyName = key;
            Console.WriteLine($"ProviderManager: Scope [{key}] Created");
        }

        public static ProviderManager Get(string key)
        {
            if(_currentProviders.TryGetValue(key, out var value))
                return value;
            return null;
        }

        public static ProviderManager GetRoot()
        {
            return _currentProviders.FirstOrDefault().Value;
        }

        public static bool Retire(string key)
        {
            Console.WriteLine($"ProviderManager: Scope [{key}] Retired");
            return _currentProviders.Remove(key);
        }

        public T Get<T>()
        {
            if(providers.TryGetValue(typeof(T), out var value))
                return (T)value;
            return default;
        }

        /// <summary>
        /// Gets all the Types of providers currently registered
        /// </summary>
        /// <returns></returns>
        public Type[] GetAll()
        {
            return providers.Keys.ToArray();
        }
       
        public T Register<T>(T Provider) where T : IProvider
        {
            providers.Add(Provider.GetType(), Provider);
            Provider.Parent = this;
            Console.WriteLine($"ProviderManager: Scope [{KeyName}] Registered: {Provider.GetType().Name}");
            return Provider;
        }

        /// <summary>
        /// Causes all registered Providers to run <see cref="IProvider.Refresh(GameTime)"/> in the order they were registered in
        /// </summary>
        /// <param name="time"></param>
        public void Refresh(GameTime time)
        {
            foreach (var provider in providers.Values)
                if (provider != this)
                    provider.Refresh(time);
        }
    }
}
