using Glacier.Common.Engine;
using Glacier.Common.Primitives;
using Glacier.Common.Provider;
using Glacier.Common.Provider.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Glacier.Common.Util
{
    public static class GameResources
    {
        public static Random Rand = new Random();
        public static Viewport CurrentViewport => Device.Viewport;
        public static Rectangle Screen => CurrentViewport.Bounds;
        public static InputProvider InputProvider => ProviderManager.GetRoot().Get<InputProvider>();

        public static GraphicsDevice Device;
        public static Texture2D BaseTexture { get; private set; }
        public static Point MouseWorldPosition => ProviderManager.Root.Get<CameraProvider>().Default.WorldMousePosition;
        public static bool Debug_GUIDebuggingActivated { get; set; } = false;

        public static Stopwatch Stopwatch = new Stopwatch();

        public static void Init(GraphicsDevice device)
        {
            Device = device;
            BaseTexture = new Texture2D(device, 1, 1);
            BaseTexture.SetData(new Color[] { Color.White });
        }

        /// <summary>
        /// Gets a random color, always with 100% opacity
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColor()
        {
            var r = (byte)Rand.Next(0, 256);
            var g = (byte)Rand.Next(0, 256);
            var b = (byte)Rand.Next(0, 256);
            return new Color(r, g, b, (byte)255);
        }

        /// <summary>
        /// Gets a random color similar to the color provided, with a similarity threshold
        /// </summary>
        /// <param name="Similar"></param>
        /// <param name="Threshold">The amount of difference from the similar color to apply to the random color</param>
        /// <returns></returns>
        public static Color GetRandomColor(Color Similar, int Threshold)
        {
            var r = Rand.Next(Similar.R - (Threshold/2), Similar.R + (Threshold/2)+1);
            var g = Rand.Next(Similar.G - (Threshold/2), Similar.G + (Threshold/2)+1);
            var b = Rand.Next(Similar.B - (Threshold/2), Similar.B + (Threshold/2)+1);
            if (r < 0)
                r = 0;
            if (g < 0)
                g = 0;
            if (b < 0)
                b = 0;
            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;
            return new Color(r, g, b, 255);
        }

        public static Color GetRandomColor(Color[] Source) => Source[Rand.Next(0, Source.Length)];

        public static Vector2 GetRandomVector2(float XLower, double XRange, float YLower, double YRange)
        {
            return GetRandomVector2(XLower, (float)(XLower + XRange), YLower, (float)(YLower + YRange));
        }

        public static Vector2 GetRandomVector2(float XLower, float XUpper, float YLower, float YUpper)
        {
            var X = XLower + ((float)Rand.NextDouble() * (XUpper - XLower));
            var Y = YLower + ((float)Rand.NextDouble() * (YUpper - YLower));
            return new Vector2(X, Y);
        }
        public static GridCoordinate GetRandomPoint(int XLower, int XUpper, int YLower, int YUpper)
        {
            var point = GetRandomVector2(XLower, XUpper, YLower, YUpper);
            return new GridCoordinate((int)point.Y, (int)point.X);
        }

        public static T GetCurrentUser<T>() where T : IUser, new()
        {
            return ProviderManager.Root.Get<UserProvider<T>>().CurrentUser;
        }

        /// <summary>
        /// Gets a texture from the <see cref="ContentProvider"/>
        /// </summary>
        /// <returns></returns>
        public static Texture2D GetTexture(string key) => ProviderManager.Root.Get<ContentProvider>().GetTexture(key);

        /// <summary>
        /// Draws a highlight over this object
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="subject"></param>
        public static void DrawMouseHighlight(SpriteBatch batch, GameObject subject)
        {
             if (subject.Texture == null) return;
            DrawMouseHighlight(batch, subject, Color.White);
        }

        /// <summary>
        /// Draws a highlight over this object
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="subject"></param>
        public static void DrawMouseHighlight(SpriteBatch batch, GameObject subject, Color color)
        {
            if (subject.Texture == null) return;
            batch.Draw(subject.Texture, subject.TextureDestination, subject.TextureSource, color * .5f);
        }
    }
}
