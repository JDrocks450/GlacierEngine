using AntFarm.Common.Actions;
using AntFarm.Common.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common.Actions
{
    public class ActionObjectGroup<T> : IDisposable where T : IActionRunnable
    {
        private HashSet<T> items = new HashSet<T>();
        public IEnumerable<T> Items => items;
        public bool IsActiveGroup => Provider.CurrentGroup.Equals(this);
        public static ActionGroupProvider<T> Provider => ProviderManager.Root.Get<ActionGroupProvider<T>>();
        public ActionObjectGroup()
        {

        }
        public ActionObjectGroup(params T[] AddRange) : this()
        {
            if (AddRange != null)
                foreach(var o in AddRange)
                    Add(o);
        }
        public T Add(T obj)
        {
            items.Add(obj);
            obj.ParentGroup = this as ActionObjectGroup<IActionRunnable>;
            return obj;
        }
        public bool Remove(T obj)
        {
            obj.ParentGroup = null;
            return items.Remove(obj);
        }
        public Action EnqueueActionSequentially<Action>(Action action, double interval) where Action : AntAction, IDelayable
        {
            int count = 0;
            foreach (var t in items)
            {
                var clone = action.Clone() as Action;
                if (action is AntRoutingAction)
                {
                    var Clone = clone as AntRoutingAction;
                    Clone.EndPoint = (action as AntRoutingAction).EndPoint;
                    Clone.StartingPoint = (action as AntRoutingAction).StartingPoint;
                }        
                clone.Delay(interval * count);
                t.EnqueueAction(clone);
                count++;
            }  
            return action;             
        }
        public Action EnqueueAction<Action>(Action action) where Action : AntAction
        {
            foreach(var t in items)            
                t.EnqueueAction(action.Clone() as Action);            
            return action;
        }
        public void ClearAllQueues()
        {
            foreach (var t in items)
                t.ActionQueue.Clear();                    
        }        
        public ActionObjectGroup<T> Split(int divisor = 2)
        {
            var remaining = Items.Take(Items.Count() / 2);
            items = new HashSet<T>();
            foreach (var item in Items.Skip(Items.Count() / 2)) Add(item);
            return Provider.CreateGroup(remaining.ToArray());
        }
        public void Dispose()
        {
            Provider.RemoveGroup(this);
        }
    }
}
