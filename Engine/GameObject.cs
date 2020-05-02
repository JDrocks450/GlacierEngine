using Glacier.Common.Provider;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Engine
{
    public interface IGameComponent
    {
        /// <summary>
        /// Load will be ignored if this is true
        /// </summary>
        bool IsLoaded
        {
            get; set;
        }
        void Initialize();
        void Update(GameTime gt);
    }

    public interface IRenderable
    {
        void Draw(SpriteBatch batch);
    }

    public abstract class GameObject : IGameComponent, IRenderable
    {
        protected Shadow Shadow
        {
            get; set;
        }
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set { X = value.X; Y = value.Y; }
        }
        public float X
        {
            get; set;
        }
        public float Y
        {
            get; set;
        }
        public Point Size
        {
            get => new Point(Width, Height);
            set { Width = value.X; Height = value.Y; }
        }
        public int Width
        {
            get; set;
        }
        public int Height
        {
            get; set;
        }
        public string ID
        {
            get; set;
        }

        public string TextureFileName => Texture?.Name;
        public Texture2D Texture
        { 
            get => _texture;
            set
            {                                   
                _texture = value;
                ResetSafezone();
            } 
        }
        public Rectangle Safezone
        {
            get; set;
        }
        private Rectangle? _hitbox;
        public Rectangle Hitbox
        {
            get
            {
                if (_hitbox == null) return new Rectangle(Position.ToPoint(), (Size.ToVector2() * new Vector2(Scale)).ToPoint());
                else return _hitbox.Value;
            }
            set => _hitbox = value;
        }

        private Rectangle? _textureDestination;
        private Texture2D _texture;
        private string _filename;

        public Rectangle TextureDestination
        {
            get
            {
                if (_textureDestination == null)
                    return new Rectangle(Hitbox.Location, Hitbox.Size);
                return _textureDestination.Value;
            }
            set => _textureDestination = value;
        }

        public SpriteEffects SpriteFlipSetting { get; set; } = SpriteEffects.None;
        public Rectangle? TextureSource
        {
            get; set;
        } = null;

        public Color Color
        {
            get; set;
        } = Color.Silver;

        public virtual Color Ambience
        {
            get; set;
        } = Color.Transparent;

        public virtual float AmbientIntensity
        {
            get; set;
        } = 0f;

        /// <summary>
        /// The origin of the sprite. Zero to One, 1 = Sprite Width/Height, 0 = 0
        /// </summary>
        public Vector2 Origin
        {
            get; set;
        }

        public float Scale
        {
            get; set;
        } = 1f;

        public float Rotation
        {
            get; set;
        }

        public float Opacity
        {
            get; set;
        } = 1f;

        public bool IsLoaded { get; set; }

        public bool Visible { get; set; } = true;

        /// <summary>
        /// Determines whether <see cref="Initialize"/> is called from the base constructor. If false,
        /// the developer will need to call this method manually.
        /// </summary>
        protected virtual bool AutoInitialize { get; } = true;

        public GameObject(string texKey, Vector2 Position, Point Size) : 
            this(ProviderManager.Root.Get<ContentProvider>().GetTexture(texKey), Position, Size)
        {

        }

        public GameObject(Texture2D texture, Vector2 Position, Point Size)
        {
            this.Texture = texture;
            this.Position = Position;
            this.Size = Size;
            if (ID == null)
                ID = GetID();
            this.ID = ID;
            if (AutoInitialize)
                Initialize();
        }

        private void ResetSafezone(string TextureKey)
        {
            Safezone = ProviderManager.Root.Get<ContentProvider>().GetTextureSafezone(TextureKey).Value;
        }

        private void ResetSafezone()
        {
            Safezone = ProviderManager.Root.Get<ContentProvider>().GetTextureSafezone(Texture) ?? default;
        }

        public abstract void Initialize();

        public abstract void Update(GameTime gt);

        public virtual void Draw(SpriteBatch batch)
        {
            if (!Visible || Texture == null) return;
            if (Shadow != null)
                Shadow.DrawShadow(batch);
            var origin = Origin * Size.ToVector2();
            batch.Draw(Texture,
                TextureDestination,
                TextureSource,
                Color.Lerp(Color, Ambience, AmbientIntensity) * Opacity,
                Rotation, origin, SpriteFlipSetting, 0);
        }

        public static string GetID(char prefix = 'O', IEnumerable<GameObject> scope = null)
        {
            string getID()
            {
                return prefix + GameResources.Rand.Next(10000, 99999).ToString();
            }
            if (scope == null)
                return getID();
            var id = getID();
            bool ok = false;
            while (!ok)
            {
                ok = true;
                foreach (var item in scope)
                    if (item.ID == id)
                    {
                        ok = false;
                        break;
                    }
                if (!ok)
                    id = getID();
            }
            return id;
        }
    }
}
