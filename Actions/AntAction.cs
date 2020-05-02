using AntFarm.Common.Engine;
using AntFarm.Common.Provider;
using AntFarm.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntFarm.Common.Actions
{
    public interface IActionRunnable
    {
        ActionObjectGroup<IActionRunnable> ParentGroup { get; set; }
        Queue<AntAction> ActionQueue
        {
            get;
        }
        T EnqueueAction<T>(T action) where T : AntAction;
    }
    public interface IDelayable
    {
        double SecondsDelay { get; set; }
        void Delay(double seconds);
    }
    public abstract class AntAction : ICloneable
    {
        /// <summary>
        /// Handles action completed event calls
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="completed"></param>
        public delegate void OnActionCompletedEventHandler(Ant subject, AntAction completed);
        /// <summary>
        /// Raised when this action determines itself to be completed.
        /// </summary>
        public abstract event OnActionCompletedEventHandler OnActionCompleted;

        public Ant Subject
        {
            get;set;
        }
        public AntAction()
        {

        }
        public AntAction(Ant subject) : this()
        {
            Subject = subject;
        }

        /// <summary>
        /// The action is completed?
        /// </summary>
        public bool Complete
        {
            get; protected set;
        }

        public abstract void Run(GameTime time);

        public virtual void OnForceEnded()
        {
            //do nothing.
            return;
        }

        public void End()
        {
            Complete = true;
        }

        public abstract object Clone();
    }

    public class AntRoutingAction : AntAction, IDelayable
    {
        bool exactPositioning = false;
        public AntRoutingAction(ITileObject destination) : this(null, destination)
        {

        }
        public AntRoutingAction(Ant subject, ITileObject destination) : base(subject)
        {
            SetDestination(destination);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="startPoint"></param>
        /// <param name="secondsDelay">The time, in seconds, to delay this action. Used for walking in lines!</param>
        public AntRoutingAction(Vector2 startPoint, Vector2 endPoint) : base(null)
        {
            StartingPoint = startPoint;
            this.EndPoint = endPoint;
        }

        private Vector2? getRandomPointInSafezone(ITileObject tile)
        {
            if (tile == null) return null;
            var safezone = ProviderManager.Root.Get<LazerContentManager>().GetTextureSafezone(tile.ThisObject.TextureFileName);
            if (!safezone.HasValue)
                return tile.Position;
            var rect = safezone.Value;
            var x = GameResources.Rand.Next(0, rect.Width) + tile.Position.X;
            var y = GameResources.Rand.Next(0, rect.Height) + tile.Position.Y;
            x += rect.X;
            y += rect.Y;
            return new Vector2(x, y);
        }
        
        private Vector2 getRandomPointOnBottom(ITileObject tile)
        {
            var newPos = new Vector2(0, tile.Size.Y + tile.Position.Y);
            newPos.X = tile.Position.X + GameResources.Rand.Next(0, tile.Size.X);
            return newPos;
        }

        public double Speed
        {
            get; set;
        } = Ant.SPEED_SECONDSPERTILE;

        public ITileObject RoutingObject;
        public bool Routing => RoutingObject != null;

        public double SecondsDelay { get; set; } = -1;

        private DateTime startRoutingTime;
        public Vector2 StartingPoint;
        public Vector2 EndPoint;

        private bool isRunning = false;
        private float routingDirection;
        private double elapsed = 0;

        public delegate void OnRoutingCompletedEventHandler(Ant subject, ITileObject destination);
        public event OnRoutingCompletedEventHandler OnRoutingCompleted;
        public override event OnActionCompletedEventHandler OnActionCompleted;

        public void SetDestination(ITileObject destination)
        {            
            StartingPoint = Subject?.Position ?? new Vector2(float.NaN, float.NaN);
            RoutingObject = destination;
            EndPoint = getRandomPointInSafezone(destination) ?? EndPoint;
        }

        public float GetTravelAngle()
        {
            Vector2 p1 = StartingPoint, p2 = EndPoint;
            Vector2 diff = p2 - p1;
            var angle = MathHelper.ToDegrees((float)Math.Atan2(diff.Y, diff.X));
            if (angle < 0)
                angle = 360 + angle;
            return angle;
        }

        public Direction GetTravelDirection(float angle)
        {
            if (angle < 0)
                angle = 360 + angle;
            if (Enum.IsDefined(typeof(Direction), (short)angle))
                return (Direction)(short)angle;
            if (angle > (short)Direction.E && angle < (short)Direction.N)
            {
                var step = 90 / 4.0;
                if (angle < step)
                    return Direction.E;
                else if (angle > step * 3)
                    return Direction.N;
                else
                    return Direction.NE;
            }
            if (angle > (short)Direction.N && angle < (short)Direction.W)
            {
                var step = 90 / 4.0;
                if (angle < step + (double)Direction.N)
                    return Direction.N;
                else if (angle > step * 3 + (double)Direction.N)
                    return Direction.W;
                else
                    return Direction.NW;
            }
            if (angle > (short)Direction.W && angle < (short)Direction.S)
            {
                var step = 90 / 4.0;
                if (angle < step + (double)Direction.W)
                    return Direction.W;
                else if (angle > step * 3 + (double)Direction.W)
                    return Direction.S;
                else
                    return Direction.SW;
            }
            if (angle > (short)Direction.S && angle < 360)
            {
                var step = 90 / 4.0;
                if (angle < step + (double)Direction.S)
                    return Direction.S;
                else if (angle > step * 3 + (double)Direction.S)
                    return Direction.E;
                else
                    return Direction.SE;
            }
            return Direction.N;
        }

        public override void Run(GameTime time)
        {
            if (Subject == null)
                return;
            elapsed += time.ElapsedGameTime.TotalSeconds;
            if (elapsed < SecondsDelay)
            {
                startRoutingTime = DateTime.Now;
                return;
            }
            if (!isRunning)
            {
                StartingPoint = Subject?.Position ?? new Vector2(float.NaN, float.NaN);
                startRoutingTime = DateTime.Now;
                isRunning = true;
            }
            if (Complete) // skips most calculation and places the object where it needs to be.
            {
                OnRoutingCompleted?.Invoke(Subject, RoutingObject); // a more user-friendly event
                OnActionCompleted?.Invoke(Subject, this);
                Subject.Position = EndPoint; // always in case    
                return;
            }
            else if (float.IsNaN(StartingPoint.X))
                StartingPoint = Subject.Position;
            var distanceToTravel = (StartingPoint - EndPoint).Length();
            var ETA = distanceToTravel * Speed;
            var elapsedTime = DateTime.Now - startRoutingTime;
            if (elapsedTime.TotalSeconds / ETA > 1)
            {
                OnRoutingCompleted?.Invoke(Subject, RoutingObject); // a more user-friendly event
                OnActionCompleted?.Invoke(Subject, this);
                Subject.Position = EndPoint; // always in case
            }
            else
                Subject.Position = Vector2.Lerp(StartingPoint, EndPoint, (float)(elapsedTime.TotalSeconds / ETA));
        }

        public override object Clone()
        {
            return new AntRoutingAction(RoutingObject);
        }

        public void Delay(double seconds)
        {
            SecondsDelay = seconds;
        }
    }

    /// <summary>
    /// Calls a method multiple times over a set interval
    /// </summary>
    public class AntRepetitiveAction : AntAction
    {
        public enum RepeatMode
        {
            /// <summary>
            /// Calls every action, in order, every time the interval is passed
            /// </summary>
            Iterative,
            /// <summary>
            /// Calls each action sequentially, in order, each time the interval is passed, one after another
            /// </summary>
            Cascade
        }
        public struct RepeatOptions
        {
            /// <summary>
            /// The current repeat mode
            /// </summary>
            public RepeatMode Mode;
            /// <summary>
            /// The max amount of time this action should take.
            /// </summary>
            public double TimeLimit;
            /// <summary>
            /// The interval, in seconds, for each repeat of this AntAction
            /// </summary>
            public double Interval;
            /// <summary>
            /// The amount of times to repeat before this AntAction is completed
            /// </summary>
            public int RepeatTimes;
        }
        public RepeatOptions Options
        {
            get; set;
        }

        public delegate void OnActionRepeatHandler(Ant ant, AntRepetitiveAction action, GameTime time);

        private double elapsed, timeSinceStart;
        private int actionIndex, repeatedAmount;
        private OnActionRepeatHandler[] Actions;        

        public AntRepetitiveAction(double secondsInterval, int repeatAmount, RepeatMode mode = RepeatMode.Iterative, params OnActionRepeatHandler[] actions) : base()
        {
            Actions = actions;
            Options = new RepeatOptions()
            {
                Interval = secondsInterval,
                TimeLimit = -1,
                RepeatTimes = repeatAmount,
                Mode = mode
            };
        }

        public AntRepetitiveAction(double secondsInterval, double timeLimit, RepeatMode mode = RepeatMode.Iterative, params OnActionRepeatHandler[] actions) : base()
        {
            Actions = actions;
            Options = new RepeatOptions()
            {
                Interval = secondsInterval,
                RepeatTimes = -1,
                TimeLimit = timeLimit,
                Mode = mode
            };
        }

        public AntRepetitiveAction(RepeatOptions options, params OnActionRepeatHandler[] actions)
            : this(0, 0, RepeatMode.Iterative, actions)
        {
            Options = options;
        }

        public override event OnActionCompletedEventHandler OnActionCompleted;

        public override void Run(GameTime time)
        {
            if (Complete)
                return;
            elapsed += time.ElapsedGameTime.TotalSeconds;
            if (elapsed > Options.Interval)
            {
                elapsed = 0;
                switch (Options.Mode)
                {
                    case RepeatMode.Cascade:
                        Actions[actionIndex].Invoke(Subject, this, time);
                        actionIndex++;
                        if (actionIndex >= Actions.Length)
                            actionIndex = 0;
                        break;
                    case RepeatMode.Iterative:
                        foreach (var action in Actions)                            
                            action.Invoke(Subject, this, time);
                        break;
                }
                repeatedAmount++;
            }
            timeSinceStart += time.ElapsedGameTime.TotalSeconds;
            if (timeSinceStart > Options.TimeLimit && Options.TimeLimit > -1)
            {
                Complete = true;
                OnActionCompleted?.Invoke(Subject, this);
            }
            if (repeatedAmount >= Options.RepeatTimes && Options.RepeatTimes > -1)
            {
                Complete = true;
                OnActionCompleted?.Invoke(Subject, this);
            }
        }

        public override object Clone()
        {
            return new AntRepetitiveAction(Options, Actions);
        }
    }
}
