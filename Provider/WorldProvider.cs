using Glacier.Common.Engine;
using Glacier.Common.GlacerMath;
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
        private Dictionary<Point, T> worldGrid = new Dictionary<Point, T>();
        public T Current => GetCurrentWorld();
        public Point CurrentChunk { get; private set; }
        public int ViewDistance {  get; set; }
        private World[] surroundingWorlds = new World[0];
        public NoiseGenerator NoiseGenerator {  get; private set; }

        public void SetChunkUpdateParams(Point CurrentChunk, int UpdateDistance)
        {
            this.CurrentChunk = CurrentChunk;
            ViewDistance = UpdateDistance;
            surroundingWorlds = GetSurroundingWorlds(CurrentChunk, UpdateDistance);
        }

        public T Create(string name, int size = World.W_SIZE)
        {
            return new T()
            {
                Name = name,
                SizeX = size,
                SizeY = size,
                WorldData = new WorldTile[size, size]
            };
        }

        public T Load(string filename)
        {
            string content;
            var world = Create("GlacierChunk");
            using (var file = File.OpenText(filename))
                content = file.ReadToEnd();
            var filecontent = content.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            var byteContent = filecontent.Select(x => (WorldTile.TileTypes)(short.Parse(x) / 5) + 1).ToArray();
            world.SizeX = world.SizeY = (int)Math.Sqrt(byteContent.Length);            
            for(int row = 0; row<world.SizeY; row++)
            {
                for(int col = 0; col<world.SizeX; col++)
                {

                    world.WorldData[col, row] = new WorldTile(world, byteContent[(row * world.SizeX) + col], row, col);
                }
            }
            Add(world, new Point());
            SetChunkUpdateParams(new Point(), 3);
            world.Initialize();                  
            return world;
        }

        public void ApplyNoiseGenParams(double persistence, double frequency, double amplitude, int octaves, int seed)
        {
            NoiseGenerator = new NoiseGenerator(persistence, frequency, amplitude, octaves, seed);
        }

        public async Task<T> Generate(string worldName, double persistence, double frequency, double amplitude, int octaves, int seed)
        {            
            ApplyNoiseGenParams(persistence, frequency, amplitude, octaves, seed);
            return await Generate(worldName, new Point(), NoiseGenerator);
        }

        public async Task<T> Generate(string worldName, Point Offset, Common.GlacerMath.NoiseGenerator Generator)
        {            
            T world = Create(worldName);
            double[,] map = new double[world.SizeX, world.SizeY];
            await Generator.FillMap2D(map, Offset, world.SizeX / 5, world.SizeY / 5);
            //var values = (WorldTile.TileTypes[])Enum.GetValues(typeof(WorldTile.TileTypes));\
            var values = new WorldTile.TileTypes[]
            {
                WorldTile.TileTypes.DeepOcean,
                WorldTile.TileTypes.Ocean,
                WorldTile.TileTypes.Dirt,
                WorldTile.TileTypes.Grass,
                WorldTile.TileTypes.DeepGrass
            };
            float lower = (float)map.OfType<double>().Min(), upper = (float)map.OfType<double>().Max();
            for (int x = 0; x < world.SizeX; x++)
            {
                for(int y = 0; y < world.SizeY; y++)
                {
                    float current = (float)map[x, y];
                    current -= lower;
                    current /= (upper - lower);
                    int amount = values.Count();
                    amount = (int)((float)amount * current);
                    WorldTile.TileTypes type = (WorldTile.TileTypes)amount;
                    if (type == WorldTile.TileTypes.DeepOcean)
                        type = WorldTile.TileTypes.Ocean;
                    world.WorldData[x,y] = new WorldTile(world, type, y, x);
                }
            }
            world.PerlinNoise = map;            
            return world;
        }

        public void Reset()
        {
            worldGrid.Clear();
        }

        /// <summary>
        /// Adds this world into the registry, allowing it to be loaded by chunk position
        /// </summary>
        /// <param name="world">The world to add</param>
        /// <param name="ChunkPosition">The position of this new world in the worldgrid. e.g. (1,1)</param>
        /// <returns></returns>
        public bool Add(in T world, Point ChunkPosition)
        {
            lock (worldGrid)
            {
                if (worldGrid.ContainsKey(ChunkPosition))
                    return false;                             
            }
            world.ChunkPosition = ChunkPosition;   
            worldGrid.Add(ChunkPosition, world);
            return true;
        }

        public bool TryGetWorld(Point ChunkPosition, out T World)
        {
            return worldGrid.TryGetValue(ChunkPosition, out World);
        }

        public T[] GetSurroundingWorlds(Point Center, int Distance)
        {
            TryGetWorld(Center - new Point(0, 1), out var North);
            TryGetWorld(Center - new Point(1, 1), out var NorthWest);
            TryGetWorld(Center - new Point(-1, 1), out var NorthEast);
            TryGetWorld(Center + new Point(0, 1), out var South);
            TryGetWorld(Center + new Point(1, 1), out var SouthEast);
            TryGetWorld(Center + new Point(-1, 1), out var SouthWest);
            TryGetWorld(Center + new Point(1, 0), out var East);
            TryGetWorld(Center - new Point(1, 0), out var West);
            TryGetWorld(Center, out var CenterWorld);
            return new T[]
            {
                            NorthEast, 
                       North,        East,
                NorthWest, CenterWorld, SouthEast,
                       West,         South, 
                            SouthWest
            };
        }
        
        public async Task<T[]> GenerateSurroundingWorlds(Point Center, int Distance, IWorldGenerationParams FirstChunkParams, IWorldGenerationParams SubsequentChunkParams)
        {
            var threader = ProviderManager.Root.Get<ThreadingProvider>();
            var surroundings = GetSurroundingWorlds(Center, Distance);
            for(int i = 0; i < surroundings.Length; i++)
            {
                var surrounding = surroundings[i];
                if (surrounding != default)
                    continue;
                Point location = new Point(Center.X, Center.Y);
                bool firstChunk = false;
                switch (i)
                {
                    case 0: // NorthEast
                        location = Center - new Point(-1, 1); break;
                    case 1: // North
                        location = Center - new Point(0, 1);  break;
                    case 2: // East
                        location = Center + new Point(1, 0);  break;
                    case 3: // NorthWest
                        location = Center - new Point(1, 1);  break;
                    case 4: // CenterWorld
                        firstChunk = true;
                        location = Center;                    break;
                    case 5: // SouthEast
                        location = Center + new Point(1, 1);  break;
                    case 6: // West
                        location = Center - new Point(1, 0);  break;
                    case 7: // South
                        location = Center + new Point(0, 1);  break;
                    case 8: // SouthWest
                        location = Center + new Point(-1, 1); break;
                }
                Point offset = new Point(50 * location.X, 50 * location.Y);
                var world = await Generate($"{location.X}, {location.Y}", offset, NoiseGenerator);                
                Add(in world, location);
                world.Initialize();
                threader.EnqueueWork(delegate { world.GenerateStructures(firstChunk ? FirstChunkParams : SubsequentChunkParams); });
            }            
            SetChunkUpdateParams(Center, Distance);
            return GetSurroundingWorlds(Center, Distance);
        }

        public T GetCurrentWorld()
        {
            if (worldGrid.ContainsKey(CurrentChunk))
                return worldGrid[CurrentChunk];
            else return null;
        }

        public void Refresh(GameTime time)
        {
            foreach (var world in surroundingWorlds)
                if (world != default)
                {
                    world.Update(time);
                }
        }
    }
}
