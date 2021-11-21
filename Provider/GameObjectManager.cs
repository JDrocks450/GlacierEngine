using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    /// <summary>
    /// A generic provider that manages a set of <see cref="GameObject"/> instances.
    /// <para>Handles <see cref="GameObject.Update(GameTime)"/> and <see cref="GameObject.Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch)"/></para>
    /// </summary>
    public class GameObjectManager : IProvider
    {
        public ProviderManager Parent { get; set; }
        private Dictionary<GameObject, int> gameObjects = new Dictionary<GameObject, int>();
        private Stopwatch debugWatch = new Stopwatch();
        private StringBuilder debugStrings = new StringBuilder();

        public T Add<T>(T Object, int layer = 0) where T : GameObject
        {
            if (gameObjects.ContainsKey(Object)) return Object;
            gameObjects.Add(Object, layer);
            return Object;
        }
        public bool Remove(GameObject Object) => gameObjects.Remove(Object);
        /// <summary>
        /// Gets the first <see cref="GameObject"/> instance in this manager of type <c>T</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() => GetAll<T>().FirstOrDefault();
        /// <summary>
        /// Gets all <see cref="GameObject"/> instances in this object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<T> GetAll<T>() => gameObjects.OfType<T>();

        public void Refresh(GameTime time)
        {
            debugStrings.Clear();
            debugWatch.Reset();
            debugWatch.Start();
            Dictionary<Type, int> types = new Dictionary<Type, int>();
            for(int i = 0; i < gameObjects.Count; i++)
            {
                GameObject gameObject = gameObjects.Keys.ElementAt(i);
                if (gameObject == null)
                    continue;
#if DEBUG
                var type = gameObject.GetType();
                if (!types.ContainsKey(type))
                    types.Add(type, 1);
                else types[type]++;
#endif
                gameObject.Update(time);                               
            }
            debugWatch.Stop();
            foreach (var type in types)            
                debugStrings.AppendLine(type.Key.Name + " [" + type.Value + " object(s)]");
            debugStrings.AppendLine("CPUTime: " + debugWatch.Elapsed.TotalMilliseconds.ToString("F2")+"ms");
        }

        public string GetDebugInfo()
        {
            return debugStrings.ToString();
        }

        public void Draw(SpriteBatch Batch)
        {
            var ordered = gameObjects.OrderBy(x => x.Value);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                KeyValuePair<GameObject, int> gameObject = ordered.ElementAt(i);
                if (gameObject.Key == null)
                    continue;
                gameObject.Key.Draw(Batch);
            }
        }
    }
}
