using Glacier.Common.Engine;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{   
    /// <summary>
    /// An <see cref="IProvider"/> that handles object animation over a specified timestep
    /// </summary>
    public class AnimatedObjectProvider : IProvider
    {
        public class AnimationDefinition<T> where T : GameObject
        {
            public enum AnimationStoryboard
            {
                /// <summary>
                /// Plays the animation frames sequentially
                /// </summary>
                Forward,
                /// <summary>
                /// Plays the animation forward then again in reverse order
                /// </summary>
                ForwardReverse,
                /// <summary>
                /// Plays the animation in reverse order
                /// </summary>
                Reverse,
                /// <summary>
                /// Plays the animation in a random order
                /// </summary>
                Random,
                /// <summary>
                /// A user defined storyboard
                /// </summary>
                UserDefined
            }

            /// <summary>
            /// The current pre-defined storyboard mode to use to play the animation
            /// </summary>
            public AnimationStoryboard StoryboardMode
            {
                get => _mode;
                set
                {
                    switch (value)
                    {
                        case AnimationStoryboard.ForwardReverse:
                        case AnimationStoryboard.Forward:
                            frameChange = 1;
                            Frame = 0;
                            break;
                        case AnimationStoryboard.Reverse:
                            frameChange = -1;
                            Frame = Frames.Count - 1;
                            break;
                    }
                    _mode = value;
                }
            }

            /// <summary>
            /// The object being animated
            /// </summary>
            public T Object
            {
                get; set;
            }

            /// <summary>
            /// The time in between frames
            /// </summary>
            public TimeSpan Timestep
            {
                get; set;
            }

            /// <summary>
            /// Dictates whether this animation will run an infinite amount of times
            /// </summary>
            public bool Infinite
            {
                get; set;
            } = true;

            /// <summary>
            /// The amount of times to repeat this animation. Ignored if <see cref="Infinite"/> is <c>true</c>
            /// </summary>
            public int RepeatAmount
            {
                get; set;
            }

            /// <summary>
            /// Dictates whether the animation has completed
            /// </summary>
            public bool Complete
            {
                get; set;
            }

            /// <summary>
            /// The frames to play in the animation
            /// </summary>
            public Queue<Texture2D> Frames
            {
                get; set;
            } = new Queue<Texture2D>();

            public int Frame
            {
                get => _frame;
                set
                {
                    if (value >= 0 && value < Frames.Count)
                    {
                        Object.Texture = Frames.ElementAt(value);
                        Object.Size = new Point(Object.Texture.Width, Object.Texture.Height);
                    }
                    _frame = value;
                }
            }

            internal int frameChange = 1;
            internal double animationTimer;
            private AnimationStoryboard _mode;
            private int _frame;

            public AnimationDefinition(T Object, params Texture2D[] textures)
            {
                this.Object = Object;
                AddFrame(textures);
            }
            public AnimationDefinition(T Object, params string[] textureKeys)
            {
                this.Object = Object;
                AddFrame(textureKeys);
            }

            public void AddFrame(params Texture2D[] textures)
            {
                foreach(var t in textures)
                    Frames.Enqueue(t);
            }

            public void AddFrame(params string[] TextureKeys)
            {
                foreach(var key in TextureKeys)
                    AddFrame(GameResources.GetTexture(key));
            }
        }

        public bool Animate(AnimationDefinition<GameObject> definition)
        {
            try
            {
                animations.Add(definition.Object, definition);
                definition.Frame = 0;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public ProviderManager Parent { get; set; }
        private Dictionary<GameObject, AnimationDefinition<GameObject>> animations = new Dictionary<GameObject, AnimationDefinition<GameObject>>();

        public void Refresh(GameTime time)
        {
            foreach(var anim in animations)
            {                
                var def = anim.Value;
                if (def.Complete)
                    continue;
                anim.Value.animationTimer += time.ElapsedGameTime.TotalSeconds;
                if (anim.Value.Timestep.TotalSeconds <= def.animationTimer)
                {
                    def.animationTimer = 0;
                    def.Frame+=def.frameChange;
                    if (def.Frame >= def.Frames.Count)
                    {
                        switch (def.StoryboardMode)
                        {
                            case AnimationDefinition<GameObject>.AnimationStoryboard.Forward:
                                def.Frame = 0;
                                if (!def.Infinite)
                                    def.Complete = true;
                                break;
                            case AnimationDefinition<GameObject>.AnimationStoryboard.ForwardReverse:
                                def.frameChange *= -1;
                                def.Frame = def.Frames.Count - 2;
                                break;                            
                        }
                    }
                    else if (def.Frame < 0)
                    {
                        if (def.StoryboardMode == AnimationDefinition<GameObject>.AnimationStoryboard.Reverse ||
                            def.StoryboardMode == AnimationDefinition<GameObject>.AnimationStoryboard.ForwardReverse)
                        {
                            def.frameChange *= -1;
                            def.Frame = 1;
                            if (!def.Infinite)
                                def.Complete = true;
                        }
                    }
                }
            }
        }
    }
}
