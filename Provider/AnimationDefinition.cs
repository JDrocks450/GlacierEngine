using Glacier.Common.Engine;
using Glacier.Common.Primitives;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Glacier.Common.Provider
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

    public interface IAnimationDefinition
    {
        /// <summary>
        /// The behavior of this storyboard, including functionality like FillBehavior
        /// </summary>
        AnimationStoryboard StoryboardMode {  get; set; }
        /// <summary>
        /// The object this Animation is acting on. 
        /// <para>There is no contact as to what <see cref="Type"/> this has to be, as long as it has valid <see cref="TextureProperty"/> and
        /// <see cref="SizeProperty"/></para>
        /// </summary>
        object Object { get; }
        /// <summary>
        /// The property that dictates the <see cref="Texture2D"/> reference the <see cref="Object"/> is currently using.
        /// <para>This is down to individual animation implementation how and when this is used.</para>
        /// </summary>
        PropertyInfo TextureProperty { get; }
        /// <summary>
        /// The property that dictates the <see cref="Point"/> value the <see cref="Object"/> is currently using.
        /// <para>This is down to individual animation implementation how and when this is used.</para>
        /// </summary>
        PropertyInfo SizeProperty { get; }

        bool Paused {  get; set; }
        /// <summary>
        /// The time in between frames
        /// </summary>
        TimeSpan Timestep
        {
            get; set;
        }

        /// <summary>
        /// Dictates whether this animation will run an infinite amount of times
        /// </summary>
        bool Infinite
        {
            get; set;
        }

        /// <summary>
        /// The amount of times to repeat this animation. Ignored if <see cref="Infinite"/> is <c>true</c>
        /// </summary>
        int RepeatAmount
        {
            get; set;
        }

        /// <summary>
        /// Dictates whether the animation has completed
        /// </summary>
        bool Complete
        {
            get; set;
        }

        /// <summary>
        /// The current frame of animation
        /// </summary>
        int Frame 
        { 
            get; set;
        }

        /// <summary>
        /// The animation time value of this animation -- measured in Seconds
        /// </summary>
        double AnimationTimer
        {
            get; set;
        }

        /// <summary>
        /// The amount of frames to change over one interval -- usually 1 or -1
        /// </summary>
        int FrameChange { get; set; }
        /// <summary>
        /// The amount of frames for this animation definition
        /// </summary>
        int FrameCount { get; }
    }

    /// <summary>
    /// See: <see cref="SourceFrameAnimationDefinition"/> for usage instructions
    /// </summary>
    public enum AnimationDimension
    {
        /// <summary>
        /// Animate over the rows of the TextureAtlas object
        /// </summary>
        Rows,
        /// <summary>
        /// Animate over the columns of the TextureAtlas object
        /// </summary>
        Columns
    }
    
    /// <summary>
    /// Defines an <see cref="IAnimationDefinition"/> that animates over the <see cref="TextureSourceProperty"/> of this definition -- 
    /// changing what the Texture on the object appears like in-game, allowing you to make animations using <see cref="TextureAtlas"/> objects.
    /// </summary>
    public class SourceFrameAnimationDefinition : IAnimationDefinition
    {
        private int _frame;
        private AnimationStoryboard _mode;        

        /// <summary>
        /// See: <see cref="IAnimationDefinition.StoryboardMode"/>
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
                        FrameChange = 1;
                        Frame = 0;
                        break;
                    case AnimationStoryboard.Reverse:
                        FrameChange = -1;
                        Frame = Frames.Count - 1;
                        break;
                }
                _mode = value;
            }
        }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Object"/>
        /// </summary>
        public object Object { get; internal set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Paused"/>
        /// </summary>
        public bool Paused { get; set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Timestep"/>
        /// </summary>
        public TimeSpan Timestep { get; set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Infinite"/>
        /// </summary>
        public bool Infinite { get; set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.RepeatAmount"/>
        /// </summary>
        public int RepeatAmount { get; set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Complete"/>
        /// </summary>
        public bool Complete { get; set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.Frame"/>
        /// </summary>
        public int Frame
        {
            get => _frame; set
            {
                if (value >= 0 && value < Frames.Count)
                {
                    TextureProperty.SetValue(Object, Atlas.Texture);
                    SizeProperty.SetValue(Object, Atlas.CellSize);
                    TextureSourceProperty.SetValue(Object, Frames.ElementAt(value));
                }
                _frame = value;
            }
        }

        public Queue<Rectangle> Frames { get; set; } = new Queue<Rectangle>();
        /// <summary>
        /// The <see cref="TextureAtlas"/> reference to use for this animation. Guaranteed Read-Only.
        /// </summary>
        public TextureAtlas Atlas { get; }
        /// <summary>
        /// The coordinate in the <see cref="Atlas"/> to start at.
        /// </summary>
        public GridCoordinate StartingCoordinate { get; }
        /// <summary>
        /// The coordinate in the <see cref="Atlas"/> to end at.
        /// </summary>
        public GridCoordinate EndingCoordinate { get; }
        /// <summary>
        /// The Axis of a <see cref="GridCoordinate"/> to animate over -- Rows or Columns
        /// </summary>
        public AnimationDimension Dimension { get; }
        public double AnimationTimer { get; set; }
        public int FrameChange { get; set; } = 1;
        public int FrameCount => Frames.Count;

        /// <summary>
        /// See: <see cref="IAnimationDefinition.TextureProperty"/>
        /// </summary>
        public PropertyInfo TextureProperty { get; private set; }
        /// <summary>
        /// See: <see cref="IAnimationDefinition.SizeProperty"/>
        /// </summary>
        public PropertyInfo SizeProperty { get; private set; }
        /// <summary>
        /// The Property of the <see cref="Object"/> that dictates the Source texture it uses
        /// </summary>
        public PropertyInfo TextureSourceProperty { get; private set; }

        public SourceFrameAnimationDefinition(GameObject Object,
            TextureAtlas Atlas,
            GridCoordinate StartingCoordinate,
            GridCoordinate EndingCoordinate, AnimationDimension Dimension)
            : this(Object,
                  GameObject.TextureProperty,
                  GameObject.SizeProperty,
                  GameObject.TextureSourceProperty,
                  Atlas,
                  StartingCoordinate,
                  EndingCoordinate,
                  Dimension)
        {

        }

        /// <summary>
        /// Creates a new animation definition using the supplied parameters
        /// </summary>
        /// <param name="Atlas">The atlas to use as a source for the animations</param>
        /// <param name="StartingCoordinate">The first frame of animation's location in the Atlas</param>
        /// <param name="EndingCoordinate">The last frame of animation's location in the Atlas</param>
        /// <param name="Dimension">Which component of <see cref="GridCoordinate"/> to animate over</param>
        public SourceFrameAnimationDefinition(object Object,
            PropertyInfo TextureProperty, PropertyInfo SizeProperty, PropertyInfo TextureSourceProperty,
            TextureAtlas Atlas, 
            GridCoordinate StartingCoordinate, 
            GridCoordinate EndingCoordinate, AnimationDimension Dimension)
        {
            this.Object = Object;
            this.TextureProperty = TextureProperty;
            this.SizeProperty = SizeProperty;
            this.TextureSourceProperty = TextureSourceProperty;
            this.Atlas = Atlas;
            this.StartingCoordinate = StartingCoordinate;
            this.EndingCoordinate = EndingCoordinate;
            this.Dimension = Dimension;

            GenerateApply();
        }

        private void GenerateApply()
        {
            Frames.Clear();
            switch (Dimension)
            {
                case AnimationDimension.Rows:
                    {
                        int amount = EndingCoordinate.Row - StartingCoordinate.Row;
                        amount = Math.Abs(amount);
                        int change = 1;
                        for(int i = 0; i < amount; i++)
                        {
                            if (StartingCoordinate.Row > EndingCoordinate.Row)
                                change = -1;
                            GridCoordinate coord = new GridCoordinate(StartingCoordinate.Row + (change * i), StartingCoordinate.Column);
                            Frames.Enqueue(Atlas.GetFrame(coord));
                        }
                    }
                    break;
                case AnimationDimension.Columns:
                    {
                        int amount = EndingCoordinate.Column - StartingCoordinate.Column;
                        amount = Math.Abs(amount);
                        int change = 1;
                        for (int i = 0; i < amount; i++)
                        {
                            if (StartingCoordinate.Column > EndingCoordinate.Column)
                                change = -1;
                            GridCoordinate coord = new GridCoordinate(StartingCoordinate.Row, StartingCoordinate.Column + (change * i));
                            Frames.Enqueue(Atlas.GetFrame(coord));
                        }
                    }
                    break;
            }
        }
    }

    public class AnimationDefinition : IAnimationDefinition
    {       
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
                        FrameChange = 1;
                        Frame = 0;
                        break;
                    case AnimationStoryboard.Reverse:
                        FrameChange = -1;
                        Frame = Frames.Count - 1;
                        break;
                }
                _mode = value;
            }
        }

        /// <summary>
        /// The object being animated
        /// </summary>
        public object Object
        {
            get; internal set;
        }

        public bool Paused
        {
            get;set;
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
                    var texture = Frames.ElementAt(value);
                    TextureProperty.SetValue(Object, Frames.ElementAt(value));
                    SizeProperty.SetValue(Object, new Point(texture.Width, texture.Height));
                }
                _frame = value;
            }
        }
        
        public double AnimationTimer { get; set; }
        public int FrameChange { get; set; }
        public int FrameCount => Frames.Count;

        public PropertyInfo TextureProperty { get; }
        public PropertyInfo SizeProperty { get; }

        private AnimationStoryboard _mode;
        private int _frame;

        /// <summary>
        /// Creates a new <see cref="AnimationDefinition"/> using the predefined properties on a <see cref="GameObject"/> instance
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="textures"></param>
        public AnimationDefinition(
            GameObject Object, params Texture2D[] textures
            ) : this(Object, GameObject.TextureProperty, GameObject.SizeProperty, textures)
        {

        }
        /// <summary>
        /// Creates a new <see cref="AnimationDefinition"/> using the predefined properties on a <see cref="GameObject"/> instance
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="textures"></param>
        public AnimationDefinition(
            GameObject Object, params string[] textures
            ) : this(Object, GameObject.TextureProperty, GameObject.SizeProperty, textures)
        {

        }
        /// <summary>
        /// Creates a new <see cref="AnimationDefinition"/> with the given textures
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="TextureProperty"></param>
        /// <param name="SizeProperty"></param>
        /// <param name="textures"></param>
        public AnimationDefinition(
            object Object, 
            PropertyInfo TextureProperty, PropertyInfo SizeProperty,
            params Texture2D[] textures)
        {
            this.Object = Object;
            this.TextureProperty = TextureProperty;
            this.SizeProperty = SizeProperty;
            AddFrame(textures);
        }
        /// <summary>
        /// Creates a new <see cref="AnimationDefinition"/> with the given texture names
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="TextureProperty"></param>
        /// <param name="SizeProperty"></param>
        /// <param name="textures"></param>
        public AnimationDefinition(object Object,
            PropertyInfo TextureProperty, PropertyInfo SizeProperty,
            params string[] textureKeys)
        {
            this.Object = Object;
            this.TextureProperty = TextureProperty;
            this.SizeProperty = SizeProperty;
            AddFrame(textureKeys);
        }

        public void AddFrame(params Texture2D[] textures)
        {
            foreach (var t in textures)
                Frames.Enqueue(t);
        }

        public void AddFrame(params string[] TextureKeys)
        {
            foreach (var key in TextureKeys)
                AddFrame(GameResources.GetTexture(key));
        }
    }
}
