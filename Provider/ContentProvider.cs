using Glacier.Common.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Glacier.Common.Provider
{
    public sealed class ContentProvider : IProvider
    {
        private ContentManager Manager;
        private Dictionary<string, (Texture2D texture, Rectangle? safezone)> textureCache = new Dictionary<string, (Texture2D, Rectangle?)>();
        public ProviderManager Parent { get; set; }

        public ContentProvider(ContentManager manager)
        {
            Manager = manager;
        }

        public Rectangle? GetTextureSafezone(string key, bool refreshSafezone = false)
        {
            if (textureCache.TryGetValue(key, out var tuple))
            {
                if (tuple.safezone == default || refreshSafezone)
                {
                    var safezone = Crop(tuple.texture);
                    textureCache[key] = (tuple.texture, safezone);
                    return safezone;
                }
                else
                    return tuple.safezone;
            }
            else return null;
        }
        public Rectangle? GetTextureSafezone(Texture2D key, bool refreshSafezone = false)
        {
            if (key == null) return null;
            return GetTextureSafezone(key.Name, refreshSafezone);
            /*
            var dicKey = textureCache.FirstOrDefault(x => key == x.Value.texture);
            var tuple = dicKey.Value;
            if (tuple != default)
            {
                if (tuple.safezone == default || refreshSafezone)
                {
                    var safezone = Crop(tuple.texture);
                    textureCache[dicKey.Key] = (tuple.texture, safezone);
                    return safezone;
                }
                else
                    return tuple.safezone;
            }
            else return null;
            */
        }

        public void RefreshTexture(string name)
        {
            GetTextureSafezone(name, true);
        }

        private Rectangle Crop(Texture2D texture)
        {
            var foo = new Color[texture.Width * texture.Height];
            texture.GetData(0, null, foo, 0, foo.Length);
            int startLine = -1, endLine = -1, startIndex = -1, endIndex = -1;
            for (var line = 0; line < texture.Height; line++)
            {
                int colorIndex = -1;
                for (var index = 0; index < texture.Width; index++)
                {
                    var color = foo[line * texture.Width + index];
                    if (color != Color.Transparent)
                    {
                        if (startLine == -1)
                            startLine = line;
                        if (startIndex == -1 || startIndex > index)
                            startIndex = index;
                        colorIndex = index;
                    }
                }
                if (endIndex == -1 || colorIndex > endIndex)
                    endIndex = colorIndex;
                if (colorIndex != -1)
                    endLine = line;
            }
            return new Rectangle(startIndex, startLine, endIndex - startIndex, endLine - startLine);
        }

        public Texture2D Create(string name, int width, int height) => Add(name, new Texture2D(GameResources.Device, width, height));

        public Texture2D Add(string name, Texture2D texture)
        {
            texture.Name = name;
            textureCache.Add(name, (texture, Crop(texture)));
            return texture;
        }

#if false
        private unsafe int DEBUG_GetCacheSize()
        {
            int amount = 0;
            foreach(var tuple in textureCache.Values)
            {
                //amount += sizeof(tuple.texture);
            }
            return amount;
        }
#endif

        public Texture2D GetTexture(string key)
        {
            if (textureCache.TryGetValue(key, out var tuple))
                return tuple.texture;
            else
                try
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    var texture = GetContent<Texture2D>(key);
                    textureCache.Add(key, (texture, Crop(texture)));
                    watch.Stop();
                    Debug.WriteLine("[GLACIER] TextureCache: Loaded " + key + " in " + watch.ElapsedMilliseconds + "ms");
                    Debug.WriteLine("[GLACIER] TextureCache: " + textureCache.Count + " textures in memory, GC Estimated: " + GC.GetTotalMemory(false));
                    return texture;
                }
                catch (Exception)
                {
                    return null;
                }
        }

        /// <summary>
        /// For loading textures, consider <see cref="GetTexture(string)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetContent<T>(string key)
        {
            return Manager.Load<T>(key);
        }

        public void Refresh(GameTime time)
        {
            ;
        }
    }
}
