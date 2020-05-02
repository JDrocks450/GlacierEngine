using AntFarm.Common.Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public enum IndicatorStyle
    {
        WALKING
    }
    public class AntIndicator : GameObject, ITileObject
    {
        public WorldTile Parent { get; set; }

        public GameObject ThisObject => this;

        public Vector2 Offset { get; set; }

        public AntIndicator() : base("Objects/indicator", new Vector2(), new Point())
        {
            Size = new Point(Texture.Width, Texture.Height);
        }

        public override void Initialize()
        {
            
        }

        public override void Update(GameTime gt)
        {
            
        }
    }
}
