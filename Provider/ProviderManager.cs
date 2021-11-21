using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Glacier.Common.Provider
{
    public class ProviderManager : IProvider
    {
        private static Dictionary<string, ProviderManager> _currentProviders = new Dictionary<string, ProviderManager>();
        private Dictionary<Type, IProvider> providers = new Dictionary<Type, IProvider>();
        private Stopwatch _debugTimer;
        private Dictionary<IProvider, TimeSpan> _diagnosticInfo = new Dictionary<IProvider, TimeSpan>();
        private Queue<double> _avgTime = new Queue<double>();
        private TimeSpan frameTime;

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
            _debugTimer = new Stopwatch();
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

        public T Get<T>() where T : IProvider
        {
            if(providers.TryGetValue(typeof(T), out var value))
                return (T)value;
            throw new ArgumentException($"A component of this Game is requesting access to a Provider that hasn't been added yet to your Game: " +
                $"{typeof(T).Name}. Use ProviderManager.Register to set up this provider for use in this project.");
        }

        public bool TryGet<T>(out T Provider)
        {
            if (providers.TryGetValue(typeof(T), out var value))
            {
                Provider = (T)value;
                return true;
            }
            Provider = default(T);
            return false;
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
            _diagnosticInfo.Clear();
            _debugTimer.Reset();
            _debugTimer.Start();
            foreach (var provider in providers.Values)
            {
                var milliseconds = _debugTimer.Elapsed;
                if (provider != this)
                    provider.Refresh(time);
                milliseconds = _debugTimer.Elapsed - milliseconds;
                _diagnosticInfo.Add(provider, milliseconds);
            }
            _debugTimer.Stop();
            frameTime = _debugTimer.Elapsed;
            if (_avgTime.Count > 120)
                _avgTime.Dequeue();
            _avgTime.Enqueue(frameTime.TotalMilliseconds);
        }

        public override string ToString()
        {
            return "ProviderManager (" + KeyName + ") " + providers.Count + " providers registered";
        }

        public string GetDebugString()
        {
            return "=====PROVIDERS=====\n" +
                ToString() + "\n"
                + string.Join("\n", providers.Values.Select(
                    x => x.GetType().Name + $" - {(_diagnosticInfo[x].TotalMilliseconds).ToString("F2")}ms" +
                    $" ({(_diagnosticInfo[x].TotalMilliseconds / frameTime.TotalMilliseconds).ToString("P")})"))
                + $"\nFrameTime {frameTime.TotalMilliseconds.ToString("F2")}ms\n" +
                $"Avg. FrameRate (Unrendered) {(1000 / _avgTime.Average()).ToString("F2")} FPS";
        }
    }
}
