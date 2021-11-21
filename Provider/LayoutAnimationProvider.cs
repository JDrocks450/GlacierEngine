using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public interface ILayoutAnimationDefinition
    {
        bool Paused { get; }
        bool Completed { get; }
        bool Repeating { get; }
        PropertyInfo AnimatedProperty { get; set; }

        void AnimateOne(GameTime time, object Object);
        void Restart();
    }

    /// <summary>
    /// Represents a layout-based animation, using Keyframes.
    /// </summary>
    public class KeyframedLayoutAnimationDefinition : ILayoutAnimationDefinition
    {
        /// <summary>
        /// A keyframe in a layout animation
        /// </summary>
        public class LayoutAnimationKeyframe
        {
            /// <summary>
            /// The name of this Keyframe
            /// </summary>
            public string Name {  get; set; }
            /// <summary>
            /// The position to animate to for this frame
            /// </summary>
            public Vector2 Position {  get; set; }
            /// <summary>
            /// The time at which to be at the specified <see cref="Position"/> in this Animation
            /// </summary>
            public TimeSpan Time {  get; set; }            

            /// <summary>
            /// Creates a <see cref="LayoutAnimationKeyframe"/> using the specified values.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="position"></param>
            /// <param name="time"></param>
            public LayoutAnimationKeyframe(string name, Vector2 position, TimeSpan time)
            {
                Name = name;
                Position = position;
                Time = time;
            }
        }        

        public List<LayoutAnimationKeyframe> Keyframes { get; set; } = new List<LayoutAnimationKeyframe>();

        public TimeSpan CurrentTime { get; private set;  } = TimeSpan.Zero;

        public TimeSpan AnimationTime => Keyframes?.LastOrDefault()?.Time ?? TimeSpan.Zero;

        public int KeyframesAmount => Keyframes?.Count ?? 0;

        public LayoutAnimationKeyframe Current { get; private set; }  

        public LayoutAnimationKeyframe Next => Keyframes.ElementAtOrDefault(Keyframes.IndexOf(Current) + 1);

        public bool Completed { get; private set; } = false;

        public bool Paused { get; private set; } = false;

        /// <summary>
        /// The property of the object to apply the animation to
        /// </summary>
        public System.Reflection.PropertyInfo AnimatedProperty
        {
            get; set;
        }

        /// <summary>
        /// Will this animation be repeating?
        /// </summary>
        public bool Repeating { get; set; }

        public KeyframedLayoutAnimationDefinition()
        {
            Restart(); // REFRESH ANIM
        }

        public KeyframedLayoutAnimationDefinition(params LayoutAnimationKeyframe[] Frames) : this()
        {
            foreach (var Frame in Frames)
                AddFrame(Frame);
        }

        /// <summary>
        /// Adds an existing keyframe to this animation
        /// </summary>
        /// <param name="Frame"></param>
        public void AddFrame(LayoutAnimationKeyframe Frame) => Keyframes.Add(Frame);
        /// <summary>
        /// Creates a new <see cref="LayoutAnimationKeyframe"/> with the given information
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Time"></param>
        public void AddFrame(Vector2 Position, TimeSpan Time) => AddFrame(new LayoutAnimationKeyframe("Keyframe", Position, Time));

        /// <summary>
        /// Moves the animation forward based on the amount of Elapsed time since the last call
        /// </summary>
        public void AnimateOne(GameTime time, object Object)
        {
            if (Keyframes == null) return;
            if (Keyframes.Count == 0) return;
            if (Completed || Paused) return;

            //START ANIMATION
            CurrentTime += time.ElapsedGameTime;
            if (Current == default)
                Current = Keyframes.First();
            if (Current == null) return; // WHAT??
            if (Next == null)
            { // THERE IS NO OTHER KEYFRAME
                AnimatedProperty.SetValue(Object, Current.Position);
                Completed = true;
                return;
            }
            if (Next.Time < CurrentTime)
                Current = Next;
            if (Next == null)
            { // THERE IS NO OTHER KEYFRAME
                AnimatedProperty.SetValue(Object, Current.Position);
                Completed = true;
                return;
            }
            AnimatedProperty.SetValue(Object, 
                Vector2.Lerp(Current.Position, Next.Position, (float)(CurrentTime.TotalSeconds / Next.Time.TotalSeconds)));            
        }

        /// <summary>
        /// Restarts this animation
        /// </summary>
        public void Restart()
        {
            Current = null;
            CurrentTime = TimeSpan.Zero;
            Completed = false;
            Paused = false;
        }
    }

    /// <summary>
    /// Provides functionality for animations in layout-based transformations.
    /// <para>For example, moving a GameObject from one coordinate on the screen to another.</para>
    /// </summary>
    public class LayoutAnimationProvider : IProvider
    {
        public ProviderManager Parent { get; set; }

        private List<KeyValuePair<object, ILayoutAnimationDefinition>> _animatedObjects =
            new List<KeyValuePair<object, ILayoutAnimationDefinition>>();

        /// <summary>
        /// Enters this object into the animation system, and applies the given animation.
        /// <para>Objects can have multiple animations running at one time, simply call this function for every animation required on this object.</para>
        /// <para>NOTE: The animation definition reference is maintained. You can use the <see cref="ILayoutAnimationDefinition{T}"/> 
        /// handle to control when the animation is paused and monitor when the animation completes using properties on that object at any time.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Object"></param>
        /// <param name="AnimationDefinition"></param>
        public void Animate(object Object, PropertyInfo AnimatedProperty, ILayoutAnimationDefinition AnimationDefinition)
        {
            AnimationDefinition.AnimatedProperty = AnimatedProperty;
            _animatedObjects.Add(new KeyValuePair<object, ILayoutAnimationDefinition>
                (Object, AnimationDefinition));
        }

        /// <summary>
        /// Returns whether or not the specified object has at least ONE animation currently running on it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Object"></param>
        /// <returns></returns>
        public bool IsObjectAnimating<T>(T Object) where T : GameObject
        {
            return _animatedObjects.Where(x => x.Key == Object).Any();
        }

        public void Refresh(GameTime time)
        {
            for(int i = 0; i < _animatedObjects.Count; i++)
            {
                var animationTuple = _animatedObjects.ElementAt(i);
                var animation = animationTuple.Value;
                if (animation == null) continue;
                if (animation.Completed)
                {
                    if (animation.Repeating)
                        animation.Restart();
                    else
                        _animatedObjects.RemoveAt(i);
                }
                animation.AnimateOne(time, animationTuple.Key);
            }
        }
    }
}
