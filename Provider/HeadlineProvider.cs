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
        public HeadlineDefinition(string Name, Texture2D texture)
        {
            Texture = texture;
            this.Name = Name;
        }
    }

    internal class Headline : GameObject
    {
        public IHeadlineable Parent
        {
            get; set;
        }
        public string Name => Definiton.Name;
        private HeadlineDefinition Definiton;
        public TimeSpan TimeSinceAppeared
        {
            get; private set;
        }        
        internal Headline(HeadlineDefinition definition) : base(default(Texture2D), new Vector2(), new Point())
        {
            
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
                Position = Parent.Position + ((Parent.Size / new Point(2)) - new Point(25)).ToVector2(); //centered above IHeadlinable
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

        public bool AddHeadline(IHeadlineable Subject, string HeadlineKey)
        {
            var definition = GetDefinition(HeadlineKey);
            if (definition == null)
                return false;
            AddHeadline(Subject, definition);
            return true;
        }

        public void AddHeadline(IHeadlineable Subject, HeadlineDefinition Headline)
        {
            Headline headLine = new Headline(Headline);
            Headlines.Add(Subject, headLine);
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
