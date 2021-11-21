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
    public class Shadow
    {
        public GameObject Object
        {
            get; private set;
        }
        public Texture2D Texture
        {
            get; private set;
        } 
        public Vector2 Offset
        {
            get;set;
        }
        public Vector2 Scale { get; set; } = new Vector2(1f);
        /// <summary>
        /// Creates a shadow with the texture specified.
        /// </summary>
        /// <param name="Object">The object this shadow is being applied to</param>
        public Shadow(Texture2D texture, GameObject Object)
        {
            Texture = texture;
            this.Object = Object;
            Offset = new Vector2((Object.Width / 2f) - (texture.Width/2f),
                -(texture.Height / 2f));
        }
        /// <summary>
        /// Creates a shadow with the texture found at the specified path.
        /// </summary>
        /// <param name="ShadowName">The keyname of the shadow. Note that the shadow texture must be in the Shadows folder!</param>
        /// <param name="Object">The object this shadow is being applied to</param>
        public Shadow(string ShadowName, GameObject Object) : this(GameResources.GetTexture("Shadows/" + ShadowName), Object)
        {
            
        }

        public void DrawShadow(SpriteBatch batch)
        {
            batch.Draw(Texture,
                       Object.Position + new Vector2(0, Object.Height) + Offset,
                       null,
                       Color.White,
                       0,
                       new Vector2(0),
                       Scale,
                       SpriteEffects.None,
                       1);
        }
    }
}
