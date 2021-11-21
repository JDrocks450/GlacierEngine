using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
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
    public partial class AnimatedObjectProvider : IProvider
    {
        public bool Animate(IAnimationDefinition definition)
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
        public bool StopAnimation(object obj) => animations.Remove(obj);

        public ProviderManager Parent { get; set; }
        private Dictionary<object, IAnimationDefinition> animations = new Dictionary<object, IAnimationDefinition>();

        public void Refresh(GameTime time)
        {
            foreach(var anim in animations)
            {                
                var def = anim.Value;
                if (def.Complete || def.Paused)
                    continue;
                anim.Value.AnimationTimer += time.ElapsedGameTime.TotalSeconds;
                if (anim.Value.Timestep.TotalSeconds <= def.AnimationTimer)
                {
                    def.AnimationTimer = 0;
                    def.Frame+=def.FrameChange;
                    if (def.Frame >= def.FrameCount)
                    {
                        switch (def.StoryboardMode)
                        {
                            case AnimationStoryboard.Forward:
                                def.Frame = 0;
                                if (!def.Infinite)
                                    def.Complete = true;
                                break;
                            case AnimationStoryboard.ForwardReverse:
                                def.FrameChange *= -1;
                                def.Frame = def.FrameCount - 2;
                                break;                            
                        }
                    }
                    else if (def.Frame < 0)
                    {
                        if (def.StoryboardMode == AnimationStoryboard.Reverse ||
                            def.StoryboardMode ==  AnimationStoryboard.ForwardReverse)
                        {
                            def.FrameChange *= -1;
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
