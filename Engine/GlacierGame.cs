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
    public abstract class GlacierGame : Game
    {        
        protected GraphicsDeviceManager graphics;
        protected GlacierSpriteBatch spriteBatch;
        protected ProviderManager manager;

        StringBuilder debugStrings = new StringBuilder();

        protected GlacierGame(Point Resolution, string ContentDirectory = "Content")
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = ContentDirectory;
            graphics.PreferredBackBufferWidth = Resolution.X;
            graphics.PreferredBackBufferHeight = Resolution.Y; 
                        
            manager = new ProviderManager("GLACIER_SYS");           
        }

        protected void CreateGameResources()
        {
            GameResources.Init(GraphicsDevice);
        }

        protected override void LoadContent()
        {            
            spriteBatch = new GlacierSpriteBatch(GraphicsDevice)
            {
                SortingMode = SpriteSortMode.Deferred,
                SampleState = SamplerState.PointWrap,
                Rasterizer = new RasterizerState() { MultiSampleAntiAlias = true },
            };
            base.LoadContent();
        }

        protected override void OnActivated(object sender, EventArgs args)
        {            
            base.OnActivated(sender, args);
        }        

        /// <summary>
        /// Adds a debug string to the string buffer. See: <see cref="DrawDebugStrings(SpriteFont, Vector2, Color, Color)"/>
        /// </summary>
        /// <param name="Text"></param>
        protected void AddDebugString(string Text) => debugStrings.AppendLine(Text);

        /// <summary>
        /// Draws a debug textbox to the screen using the specified <see cref="SpriteFont"/>
        /// </summary>
        /// <param name="Font"></param>
        /// <param name="Text"></param>
        /// <param name="Position"></param>
        /// <param name="Color"></param>
        /// <param name="BackColor"></param>
        /// <returns></returns>
        protected Rectangle DrawDebugTextbox(SpriteFont Font, string Text, Vector2 Position, Color Color, Color BackColor)
        {
            var str = Text;
            var loc = Position;
            var size = Font.MeasureString(str);
            var rect = new Rectangle(loc.ToPoint(), size.ToPoint());
            spriteBatch.Draw(GameResources.BaseTexture, rect, BackColor);
            spriteBatch.DrawString(
                Font,
                str,
                loc, Color);
            return rect;
        }

        /// <summary>
        /// Draws the <see cref="ProviderManager.GetDebugString"/> function value to the screen
        /// </summary>
        /// <param name="Font"></param>
        /// <param name="Color"></param>
        /// <param name="BackColor"></param>
        /// <returns></returns>
        protected Rectangle DrawProviderDebugInfo(SpriteFont Font, Color Color, Color BackColor)
        {
            return DrawDebugTextbox(Font, ProviderManager.Root.GetDebugString(), new Vector2(10), Color, BackColor);
        }

        /// <summary>
        /// Draws the DebugStrings and clears the string buffer
        /// </summary>
        /// <param name="Font"></param>
        /// <param name="Position"></param>
        /// <param name="Color"></param>
        /// <param name="BackColor"></param>
        /// <returns></returns>
        protected Rectangle DrawDebugStrings(SpriteFont Font, Vector2 Position, Color Color, Color BackColor)
        {
            var rect = DrawDebugTextbox(Font, debugStrings.ToString(), Position, Color, BackColor);
            debugStrings.Clear();
            debugStrings.AppendLine("=====DEBUG STRINGS=====");
            return rect;
        }

        protected void DrawGeneralDebuggingInformation(SpriteFont Default, float DB_OPACITY)
        {
            var debugRect = DrawProviderDebugInfo(Default, Color.White, Color.Black * DB_OPACITY);
            debugRect = DrawDebugStrings(Default, new Vector2(10, debugRect.Height + 20), Color.White, Color.DarkCyan * DB_OPACITY);
            if (manager.TryGet<GameObjectManager>(out var objectManager))
                DrawDebugTextbox(Default,
                    "=====OBJECTS=====\n" + objectManager.GetDebugInfo(),
                    new Vector2(10, debugRect.Y + debugRect.Height + 30), Color.White, Color.DarkBlue * DB_OPACITY);
        }
    }
}
