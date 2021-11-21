using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Primitives
{
    public class TextureAtlas
    {
        public Texture2D Texture { get; set; }
        public Point TextureSize => new Point(Texture.Width, Texture.Height);
        public int Rows { get; set; }
        public int Columns { get; set; }
        public Point CellSize => (TextureSize.ToVector2() / new Vector2(Columns, Rows)).ToPoint();

        public TextureAtlas(Texture2D atlas, int rows, int columns)
        {
            Texture = atlas;
            Rows = rows;
            Columns = columns;
        }

        public Rectangle GetFrame(GridCoordinate Position) => new Rectangle((Point)Position * CellSize, CellSize);

        public void ApplyFrame<T>(T Object, GridCoordinate Frame) where T : GameObject
        {
            Object.Texture = Texture;
            Object.Size = CellSize;
            Object.TextureSource = GetFrame(Frame);
        }        
    }
}
