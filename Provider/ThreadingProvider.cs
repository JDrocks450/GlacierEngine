using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public class ThreadingProvider : IProvider
    {
        public ProviderManager Parent { get; set; }
        private ConcurrentStack<Action> workQueue = new ConcurrentStack<Action>();

        public void EnqueueWork(Action Work) => 
            workQueue.Push(Work);

        public void Refresh(GameTime time)
        {
            while(workQueue.Count > 0)
            {
                Action action = null;
                for (int i = 0; i < 15; i++)
                {
                    if (workQueue.TryPop(out action))
                        break;
                }
                if (action == null)
                    throw new InvalidOperationException("The action was not started successfully as too many concurrent read/write operations" +
                        "are happening proventing this collection from being accessed in a timely manner.");
                action?.Invoke();

            }
        }
    }
}
