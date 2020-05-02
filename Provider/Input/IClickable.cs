using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Glacier.Common.Provider.Input.InputProvider;

namespace Glacier.Common.Provider.Input
{
    public interface IClickable
    {
        Texture2D Texture { get; }
        float Scale { get; }
        Rectangle Hitbox { get; }
        bool IsMouseOver { get; set; }
        bool PerpixelHitDetection { get; }
        bool Enabled { get; }
        void MouseEnter(GameTime time,InputEventArgs args);
        void Clicked(GameTime time, InputEventArgs args);
        void MouseDown(GameTime time, InputEventArgs args);
        void MouseLeave(GameTime time, InputEventArgs args);
    }
}
