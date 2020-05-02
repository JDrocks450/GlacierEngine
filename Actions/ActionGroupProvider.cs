using AntFarm.Common.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common.Actions
{
    public class ActionGroupProvider<T> : IProvider where T : IActionRunnable
    {
        public ProviderManager Parent { get; set; }
        private HashSet<ActionObjectGroup<T>> ActionObjectGroups = new HashSet<ActionObjectGroup<T>>();
        private ActionObjectGroup<T> _current;

        public ActionObjectGroup<T> CurrentGroup
        {
            get => _current;
            set => SetSelectedGroup(value);
        }

        public void SetSelectedGroup(ActionObjectGroup<T> group)
        {
            _current = group;
            if (!ActionObjectGroups.Contains(CurrentGroup))
                ActionObjectGroups.Add(CurrentGroup);
        }
        public ActionObjectGroup<T> CreateGroup(params T[] objects)
        {
            var group = new ActionObjectGroup<T>(objects);
            ActionObjectGroups.Add(group);
            return group;
        }
        public bool RemoveGroup(ActionObjectGroup<T> group)
        {
            if (group.Equals(CurrentGroup))
                CurrentGroup = null;
            return ActionObjectGroups.Remove(group);
        }
    }
}
