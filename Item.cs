using AntFarm.Common.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public interface IItemHolder
    {
        Rectangle Hitbox { get; set; }
        Vector2 Position { get; set; }
        HashSet<Item> HoldingItems { get; set; }
        Item Equip(Item item);
        bool Drop(Item item);
        bool DropAll();
    }
    public abstract class Item : GameObject
    {
        public IItemHolder Parent { get; protected set; }
        public Item(Texture2D texture, Point Size, IItemHolder parent) : base(texture, parent.Position, Size)
        {
            Parent = parent;            
        }

        public override void Update(GameTime gt)
        {
            Position = Parent.Position - new Vector2(0, Height - Parent.Hitbox.Height);
        }
    }
}
