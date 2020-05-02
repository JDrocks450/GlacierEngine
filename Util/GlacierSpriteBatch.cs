using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Util
{
    public class DrawCall
    {
        public Texture2D Texture { get; set; }
        public Rectangle Destination { get; set; }
        public Rectangle? Source { get; set; }
        public Color Color { get; set; }
        public float Rotation { get; set; } = 0;
        public Vector2 Origin { get; set; }
        public SpriteEffects Effects { get; set; }
        public float Layer { get; set; } = 0;

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Texture, Destination, Source, Color, Rotation, Origin, Effects, Layer);
        }
    }
    public class GlacierSpriteBatch : SpriteBatch
    {
        public GlacierSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        public SpriteSortMode SortingMode { get; set; }
        public BlendState BlendingState { get; set; }
        public SamplerState SampleState { get; set; }
        public DepthStencilState DepthStencil { get; set; }
        public RasterizerState Rasterizer { get; set; }
        public Effect Effects { get;  set; }
        public Matrix? Transform { get; set; }

        /// <summary>
        /// Begins this <see cref="SpriteBatch"/> with the chosen settings
        /// </summary>
        public void Begin()
        {
            Begin(SortingMode, BlendingState, SampleState, DepthStencil, Rasterizer, Effects, Transform);
        }

        public void Begin(Matrix Transform)
        {
            this.Transform = Transform;
            this.Begin();
        }
    }
}
