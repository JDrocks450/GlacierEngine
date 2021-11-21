using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public enum HeadlineAnimation
    {
        None,
        Float,
        FadeOut,        
    }

    public interface IHeadlineable
    {
        Vector2 Position
        {
            get;
        }
        Point Size
        {
            get;
        }
    }

    public class HeadlineDefinition
    {
        public string Name
        {
            get; private set;
        }
        public Texture2D Texture
        {
            get; private set;
        }
        public TimeSpan ShownTime
        {
            get; set;
        } = TimeSpan.FromSeconds(2.5);
        public HeadlineAnimation Animation
        {
            get; set;
        } = HeadlineAnimation.FadeOut | HeadlineAnimation.Float;
        public HeadlineDefinition(string Name, Texture2D texture)
        {
            Texture = texture;
            this.Name = Name;
        }
    }

    public class Headline : GameObject
    {
        const float MAX_HEADLINE_FLOAT_HEIGHT = 50f;
        public IHeadlineable Parent
        {
            get; internal set;
        }
        public string Name => Definiton.Name;
        private HeadlineDefinition Definiton;
        private TimeSpan _shownTime;
        private float YOffset = 0, animOpacity = 1;

        public TimeSpan TimeSinceAppeared
        {
            get; private set;
        }
        public TimeSpan ShownTime
        {
            get
            {
                return _shownTime;
            }
            set
            {
                _shownTime = value;
            }
        }
        protected override bool AutoInitialize => false;
        internal Headline(HeadlineDefinition definition, IHeadlineable Subject) : base(default(Texture2D), new Vector2(), new Point())
        {
            Definiton = definition;
            _shownTime = Definiton.ShownTime;
            Parent = Subject;
            Initialize();
        }

        public override void Initialize()
        {
            Texture = Definiton.Texture;
            Size = new Point(50);
            Visible = true;            
        }

        public override void Update(GameTime gt)
        {
            if (Visible)
            {
                TimeSinceAppeared += gt.ElapsedGameTime;
                float percentComplete = (float)((TimeSinceAppeared - TimeSpan.FromSeconds(ShownTime.TotalSeconds / 2)).TotalSeconds / ShownTime.TotalSeconds);
                if (Definiton.Animation.HasFlag(HeadlineAnimation.Float))
                    YOffset = MathHelper.SmoothStep(0, MAX_HEADLINE_FLOAT_HEIGHT, percentComplete);
                if (Definiton.Animation.HasFlag(HeadlineAnimation.FadeOut))
                    animOpacity = MathHelper.SmoothStep(1, 0, percentComplete);
                Opacity = animOpacity;
                Position = Parent.Position /*+ ((Parent.Size.ToVector2() * new Vector2(.5f,0)))*/ - new Vector2(0,Texture.Height) - new Vector2(0, YOffset); //centered above IHeadlinable
                if (TimeSinceAppeared > Definiton.ShownTime)
                    Visible = false;
            }
        }
    }

    /// <summary>
    /// The provider that controls textures appearing above an object to indicate something important to the player. Example: "!" above a character's head indicates he is surprised
    /// </summary>
    public class HeadlineProvider : IProvider, IRenderable
    {
        private Dictionary<IHeadlineable, Headline> Headlines = new Dictionary<IHeadlineable, Headline>();
        private Dictionary<string, HeadlineDefinition> Definitions = new Dictionary<string, HeadlineDefinition>();
        public ProviderManager Parent { get; set; }
        public HeadlineProvider()
        {

        }

        public bool Define(HeadlineDefinition definition)
        {
            try 
            {
                Definitions.Add(definition.Name, definition);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public IEnumerable<HeadlineDefinition> GetDefinitions()
        {
            return Definitions.Values;
        }

        public HeadlineDefinition GetDefinition(string HeadlineKey)
        {
            if (Definitions.TryGetValue(HeadlineKey, out var definition))
                return definition;
            return null;
        }

        public Headline AddHeadline(IHeadlineable Subject, string HeadlineKey)
        {
            var definition = GetDefinition(HeadlineKey);
            if (definition == null)
                return null;
            return AddHeadline(Subject, definition);
        }

        public Headline AddHeadline(IHeadlineable Subject, HeadlineDefinition Headline)
        {
            if (HasHeadline(Subject))
                RemoveHeadline(Subject);
            Headline headLine = new Headline(Headline, Subject);
            Headlines.Add(Subject, headLine);
            return headLine;
        }

        public bool HasHeadline(IHeadlineable Subject) => Headlines.ContainsKey(Subject);

        public bool RemoveHeadline(IHeadlineable Subject)
        {
            return Headlines.Remove(Subject);
        }

        public void Refresh(GameTime gt)
        {
            foreach (var headLine in Headlines.Values)
                headLine.Update(gt);
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (var headLine in Headlines.Values)
                headLine.Draw(batch);
        }
    }
}
