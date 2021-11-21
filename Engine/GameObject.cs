using Glacier.Common.Provider;
using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Engine
{
    public interface IGameComponent : IDisposable
    {
        /// <summary>
        /// Load will be ignored if this is true
        /// </summary>
        bool IsLoaded
        {
            get; set;
        }
        bool Destroyed
        {
            get;
        }
        void Initialize();
        void Update(GameTime gt);
    }

    public interface IRenderable
    {
        void Draw(SpriteBatch batch);
    }

    /// <summary>
    /// The base class of every object in Glacier
    /// </summary>
    public abstract class GameObject : IGameComponent, IRenderable
    {
        /// <summary>
        /// The <see cref="Position"/> property of a GameObject
        /// </summary>
        public static PropertyInfo PositionProperty => typeof(GameObject).GetProperty("Position");
        /// <summary>
        /// The <see cref="Scale"/> property of a GameObject
        /// </summary>
        public static PropertyInfo ScaleProperty => typeof(GameObject).GetProperty("Scale");
        /// <summary>
        /// The <see cref="Size"/> property of a GameObject
        /// </summary>
        public static PropertyInfo SizeProperty => typeof(GameObject).GetProperty("Size");
        /// <summary>
        /// The <see cref="Texture"/> property of a GameObject
        /// </summary>
        public static PropertyInfo TextureProperty => typeof(GameObject).GetProperty("Texture");
        /// <summary>
        /// The <see cref="TextureSource"/> property of a GameObject
        /// </summary>
        public static PropertyInfo TextureSourceProperty => typeof(GameObject).GetProperty("TextureSource");

        /// <summary>
        /// <see cref="GameObject"/> can have a shadow appearing beneath them using any texture
        /// </summary>
        protected Shadow Shadow
        {
            get; set;
        }
        /// <summary>
        /// The position of this game object in 2D space
        /// </summary>
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set { OnPositionChanged(Position, value); X = value.X; Y = value.Y;  }
        }
        /// <summary>
        /// The X component of the <see cref="Position"/>
        /// </summary>
        public float X
        {
            get; set;
        }
        /// <summary>
        /// The Y component of the <see cref="Position"/>
        /// </summary>
        public float Y
        {
            get; set;
        }
        /// <summary>
        /// The size of this game object. This is the UNSCALED size, <see cref="Scale"/> property is applied during <see cref="Draw(SpriteBatch)"/> and collision detection.
        /// </summary>
        public Point Size
        {
            get => new Point(Width, Height);
            set { Width = value.X; Height = value.Y; }
        }
        /// <summary>
        /// The width component of the <see cref="Size"/>
        /// </summary>
        public int Width
        {
            get; set;
        }
        /// <summary>
        /// The height component of the <see cref="Size"/>
        /// </summary>
        public int Height
        {
            get; set;
        }
        /// <summary>
        /// The unique-identifier used by this object
        /// </summary>
        public string ID
        {
            get; set;
        }
        /// <summary>
        /// The asset-name of the current texture. See: <see cref="Texture"/>
        /// </summary>
        public string TextureFileName => Texture?.Name;
        /// <summary>
        /// This object's texture reference. Use the <see cref="ContentProvider"/> to avoid memory leaks and unnecessary copies of this reference.
        /// </summary>
        public Texture2D Texture
        { 
            get => _texture;
            set
            {                                   
                _texture = value;
                ResetSafezone();
            } 
        }
        /// <summary>
        /// This is the viewable region around the <see cref="Texture"/> -- updated whenever the Texture is changed, this allows you
        /// to get a rectangle around the SCALED boundary of this <see cref="GameObject"/>
        /// <para>See: <see cref="UnscaledSafezone"/> to avoid scaling</para>
        /// </summary>
        public Rectangle Safezone
        {
            get => 
                new Rectangle((_safezone.Location.ToVector2() * new Vector2(Scale)).ToPoint(),
                    (_safezone.Size.ToVector2() * new Vector2(Scale)).ToPoint()); 
            set => _safezone = value;
        }
        /// <summary>
        /// The unscaled safezone of this <see cref="Texture"/>
        /// </summary>
        public Rectangle UnscaledSafezone => new Rectangle((_safezone.Location),
                    (_safezone.Size)); 
        protected Rectangle? _hitbox;
        /// <summary>
        /// The collision-box around this <see cref="GameObject"/>, calculated using <see cref="Scale"/>, <see cref="Position"/>, and <see cref="Safezone"/>
        /// </summary>
        public Rectangle Hitbox
        {
            get
            {
                if (_hitbox == null) return new Rectangle((Position - (Origin * Scale)).ToPoint() + Safezone.Location,
                    Safezone.Size);
                else return _hitbox.Value;
            }
            set => _hitbox = value;
        }

        protected Rectangle? _textureDestination;
        private Texture2D _texture;
        private string _filename;
        private Rectangle _safezone;
        private Vector2 _originRatio;

        /// <summary>
        /// This is the area where the texture will be rendered, filling this box with the texture, factoring in the <see cref="TextureSource"/>
        /// </summary>
        public Rectangle TextureDestination
        {
            get
            {
                if (_textureDestination == null)
                    return new Rectangle(Position.ToPoint(), (Size.ToVector2() * new Vector2(Scale)).ToPoint());
                return _textureDestination.Value;
            }
            set => _textureDestination = value;
        }        

        public SpriteEffects SpriteFlipSetting { get; set; } = SpriteEffects.None;
        /// <summary>
        /// The amount of this texture to show in the <see cref="TextureDestination"/>
        /// </summary>
        public Rectangle? TextureSource
        {
            get; set;
        } = null;
        /// <summary>
        /// The color-tint to apply to this Texture
        /// </summary>
        public Color Color
        {
            get; set;
        } = Color.White;
        /// <summary>
        /// This is a helper-property, used to apply a secondary tint on top of the <see cref="Color"/> property
        /// </summary>
        public virtual Color Ambience
        {
            get; set;
        } = Color.Transparent;
        /// <summary>
        /// The intensity of the <see cref="Ambience"/> property, by default is 0
        /// </summary>
        public virtual float AmbientIntensity
        {
            get; set;
        } = 0f;
        /// <summary>
        /// The texture coordinate-space to use as the origin of the visual transformation.
        /// </summary>
        public Vector2 OriginRatio
        {
            get
            {
                return _originRatio;
            }
            set => _originRatio = value;
        }
        /// <summary>
        /// The calculated pixel-coordinates of the <see cref="OriginRatio"/> property
        /// </summary>
        public Vector2 Origin => new Vector2(Texture?.Width ?? 0, Texture?.Height ?? 0) * OriginRatio;
        /// <summary>
        /// The scale to apply to this <see cref="GameObject"/>. See: <see cref="Size"/>
        /// </summary>
        public float Scale
        {
            get; set;
        } = 1f;        
        /// <summary>
        /// The visual-rotation of this object. Does not affect any other properties. Affected by <see cref="OriginRatio"/>
        /// </summary>
        public float Rotation
        {
            get; set;
        }
        /// <summary>
        /// The Opacity of this object, only affects the visual portion of this object. 
        /// </summary>
        public float Opacity
        {
            get; set;
        } = 1f;
        /// <summary>
        /// Dictates whether <see cref="Initialize"/> has been called.
        /// </summary>
        public bool IsLoaded { get; set; }
        /// <summary>
        /// Is this object visible? Sets <see cref="Opacity"/> to 0 when true, 1 when false.
        /// </summary>
        public bool Visible
        {
            get => Opacity != 0f;
            set => Opacity = (value) ? 1f : 0f;
        }

        /// <summary>
        /// Determines whether <see cref="Initialize"/> is called from the base constructor. If false,
        /// the developer will need to call this method manually.
        /// </summary>
        protected virtual bool AutoInitialize { get; } = true;
        public bool Destroyed { get; protected set; }

        public static Texture2D GetTexture(string TextureName) => ProviderManager.Root.Get<ContentProvider>().GetTexture(TextureName);
        public void SetTexture(string TextureName) => Texture = GetTexture(TextureName);

        public GameObject(string texKey, Vector2 Position, Point Size) : 
            this(GetTexture(texKey), Position, Size)
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
            batch.Draw(Texture,
                TextureDestination,
                TextureSource,
                Color.Lerp(Color, Ambience, AmbientIntensity) * Opacity,
                Rotation, Origin, SpriteFlipSetting, 0);
            if (GameResources.Debug_HighlightHitboxes)
                batch.Draw(GameResources.BaseTexture, Hitbox, Color.Green * .5f);
        }

        public virtual void Dispose()
        {
            Destroyed = true;
        }

        protected virtual void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {

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
