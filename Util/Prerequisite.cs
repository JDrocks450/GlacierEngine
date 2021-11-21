using Glacier.Common.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Util
{
    /// <summary>
    /// Represents a task that can be fulfilled by the player
    /// </summary>
    public class Prerequisite
    {
        public enum PrerequisiteType
        {
            /// <summary>
            /// This cannot be completed
            /// </summary>
            Unattainable,
            /// <summary>
            /// This will be complete once the <see cref="IUser"/> has the item in his inventory
            /// </summary>
            InventoryCheck,
            UserDefined,
        }

        public PrerequisiteType Type
        {
            get;set;
        }
        /// <summary>
        /// The expression that dictates this <see cref="Prerequisite"/>
        /// </summary>
        public Expression<Func<Boolean>> PrerequisiteExpression
        {
            get;set;
        }
        /// <summary>
        /// Runs the <see cref="PrerequisiteExpression"/> and determines whether the requirements are fulfilled or not
        /// </summary>
        public bool Completed => PrerequisiteExpression.Compile().Invoke();
        /// <summary>
        /// Creates an empty <see cref="Prerequisite"/>
        /// </summary>
        /// <param name="type"></param>
        public Prerequisite(PrerequisiteType type)
        {
            Type = type;
        }
        /// <summary>
        /// Creates a <see cref="Prerequisite"/> that is <see cref="PrerequisiteType.UserDefined"/>
        /// </summary>
        /// <param name="UserDefinedPrereq">The expression dictating whether this is fulfilled</param>
        public Prerequisite(Expression<Func<bool>> UserDefinedPrereq)
        {
            PrerequisiteExpression = UserDefinedPrereq;
        }
        /// <summary>
        /// Creates a <see cref="Prerequisite"/> that is <see cref="PrerequisiteType.InventoryCheck"/>
        /// </summary>
        /// <param name="user">The user that this applies to</param>
        /// <param name="InventoryItem">The item required</param>
        public Prerequisite(IUser user, IUserOwnable InventoryItem)
        {
            PrerequisiteExpression = () => user.InventoryContains(InventoryItem);
        }
        /// <summary>
        /// Creates a <see cref="Prerequisite"/> that is <see cref="PrerequisiteType.InventoryCheck"/>, this checks the inventory for items that match the type T
        /// </summary>
        /// <param name="user">The user that this applies to</param>
        public static Prerequisite CreateInventoryTypeMatch<T>(IUser user) where T : IUserOwnable =>
            new Prerequisite(() => !user.InventorySearch<T>().Equals(default(T))) { Type = PrerequisiteType.InventoryCheck };
    }
}
