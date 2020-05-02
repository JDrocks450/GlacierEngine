using Glacier.Common.Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public sealed class WorldProvider<T> : IProvider where T : World, new()
    {
        public ProviderManager Parent { get; set; }
        public T Current { get; private set; }

        public T Create(string name, int size = World.W_SIZE)
        {
            Current = new T()
            {
                Name = name,
                SizeX = size,
                SizeY = size
            };            
            return Current;
        }

        public World Get(string filename)
        {
            string content;
            var world = Create("AntWorld");
            using (var file = File.OpenText(filename))
                content = file.ReadToEnd();
            var filecontent = content.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            var byteContent = filecontent.Select(x => (WorldTile.TileTypes)short.Parse(x)).ToArray();
            world.SizeX = world.SizeY = (int)Math.Sqrt(byteContent.Length);
            world.WorldData = new WorldTile[world.SizeX, world.SizeY];
            for(int row = 0; row<world.SizeY; row++)
            {
                for(int col = 0; col<world.SizeX; col++)
                {
                    world.WorldData[col, row] = new WorldTile(world, byteContent[(row * world.SizeX) + col], row, col);
                }
            }
            world.Initialize();
            Current = world;
            return world;
        }

        public World GetLoadedWorld()
        {
            return Current;
        }

        public void Refresh(GameTime time)
        {
            Current.Update(time);
        }
    }
}
