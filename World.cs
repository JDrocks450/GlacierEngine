using AntFarm.Common.Actions;
using AntFarm.Common.Engine;
using AntFarm.Common.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntFarm.Common
{
    public interface ITileObject
    {
        WorldTile Parent
        {
            get;set;
        }
        GameObject ThisObject
        {
            get;
        }
        Vector2 Position { get; set; }
        Point Size { get; set; }
        Vector2 Offset
        {
            get;set;
        }
    }
    public class WorldTile
    {
        public const int TILE_WIDTH = 120, TILE_HEIGHT = 60;
        public enum TileTypes : short
        {
            DeepOcean = -5,
            Ocean = 0,
            Dirt = 5,
            Grass = 10,
            DeepGrass = 15,
            DarkStone = 20,
            Stone = 25,
            Snow = 30
        }
        public TileTypes TileType;
        public int Y_Row, X_Col;
        public Point WorldPosition => new Point(
                        (int)(X_Col * (.5f * TILE_WIDTH) + Y_Row * (.5f * TILE_WIDTH)),
                        (int)(Y_Row * (.5f * TILE_HEIGHT) - X_Col * (.5f * TILE_HEIGHT)));
        public Point Center => new Rectangle(WorldPosition, new Point(TILE_WIDTH, TILE_HEIGHT)).Center;
        public ITileObject[] PlacedObjects
        {
            get;
            private set;
        }
        public WorldTile(TileTypes Type, int Row, int Col)
        {
            TileType = Type;
            Y_Row = Row;
            X_Col = Col;
            PlacedObjects = new ITileObject[3];
        }
        public T PlaceObject<T>(T Object, bool IsRandomizedLocation = false) where T : ITileObject
        {
            int index = 0;
            foreach(var obj in PlacedObjects)
            {
                if (obj == null)
                    break;
                index++;
            }
            if (index == PlacedObjects.Length)
                return Object;
            PlacedObjects[index] = Object;
            Object.Position = (Center
                            - new Point(Object.Size.X / 2, Object.Size.Y / 2)).ToVector2()
                            + Object.Offset; // position in direct center of tile, plus the Offset in ITileObject
            Object.Parent = this;
            return Object;
        }

        public void DeleteObject<T>(T Object) where T : ITileObject
        {
            int index = 0;
            foreach(var obj in PlacedObjects)
            {
                if (obj.Equals(Object))
                    break;
                index++;
            }
            if (index == PlacedObjects.Length)
                return;
            PlacedObjects[index] = Object;
            Object.Parent = null;
        }
    }
    public class World : Engine.IGameComponent, IAntSpace
    {
        internal const int W_SIZE = 50;
        public string Name
        {
            get;set;
        }
        public int SizeX
        {
            get;
            internal set;
        }
        public int SizeY
        {
            get;
            internal set;
        }
        public bool IsLoaded { get; set; }

        private List<AntHill> hills;
        private List<Ant> roamingAnts;

        public IEnumerable<Ant> Ants => roamingAnts;
        public WorldTile[,] WorldData;

        internal World(string name, bool init) : this(name, init, W_SIZE, W_SIZE)
        {
            
        }
        internal World(string name, bool init, int sizeX = W_SIZE, int sizeY = W_SIZE)
        {
            Name = name;
            SizeX = sizeX;
            SizeY = sizeY; 
            if (init)
                Initialize();
        }      

        public Point GetRandomTileCoordinate()
        {
            return GameResources.GetRandomPoint(0, SizeX, 0, SizeY);
        }

        public void Initialize()
        {
            hills = new List<AntHill>();
            roamingAnts = new List<Ant>();
            if (WorldData == null)
            {
                WorldData = new WorldTile[SizeX, SizeY];
                FillWorldTiles(WorldTile.TileTypes.Dirt);
            }           
            var firstHill = PlaceObjectOnTile(new AntHill(this, new Vector2()), GetRandomTileCoordinate()); // place the first ant hill
            var queen = new Ant(new Vector2(), firstHill, new Chromosome(Chromosome.PersonalityTraits.Peaceful, Chromosome.PersonalityTraits.Peaceful));
            hills.Add(firstHill);
            for(int i = 0; i < 150; i++)
            {
                firstHill.Enter(new Ant(new Vector2(0), firstHill, new Chromosome(Chromosome.PersonalityTraits.Aggressive, Chromosome.PersonalityTraits.Aggressive)));
            }
            for(int i = 0; i < 10; i++)
            {
                PlaceObjectOnTile(new Food(new Vector2()), GetRandomTileCoordinate()); // place the first ant hill
            }
        }

        public void FillWorldTiles(WorldTile.TileTypes type)
        {
            for(int row = 0; row<SizeY; row++)
            {
                for(int col = 0; col<SizeY; col++)
                {
                    WorldData[col, row] = new WorldTile(WorldTile.TileTypes.Dirt, row, col);
                }
            }
        }

        public void Enter(Ant ant)
        {
            if (ant.CurrentSpace != this)
            {
                roamingAnts.Add(ant);
                ant.CurrentSpace = this;
                ant.Visible = true;
            }
        }

        public ActionObjectGroup<Ant> GetAllAvailableAnts()
        {
            return ActionObjectGroup<Ant>.Provider.CreateGroup(Ants.Where(x => !x.ActionQueue.Any()).ToArray());
        }

        public ActionObjectGroup<Ant> GetAllRoamingAnts()
        {
            return ActionObjectGroup<Ant>.Provider.CreateGroup(Ants.ToArray());
        }

        public T PlaceObjectOnTile<T>(T Object, int Column, int Row) where T : ITileObject
        {
            var tile = WorldData[Column, Row];
            return tile.PlaceObject(Object);
        }  
        
        public T PlaceObjectOnTile<T>(T Object, Point RowColPoint) where T : ITileObject
        {
            return PlaceObjectOnTile(Object, RowColPoint.X, RowColPoint.Y);
        } 

        public void Update(GameTime gt)
        {
            foreach (var anthill in hills)
                anthill.Update(gt);
            foreach (var ant in roamingAnts)
                ant.Update(gt);
        }        
    }
}
