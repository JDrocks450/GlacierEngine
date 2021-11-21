using Glacier.Common.Primitives;
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
    public struct TileObjectOffset
    {
        /// <summary>
        /// Returns a TileObjectOffset instance that aligns the object to the bottom of the tile
        /// </summary>
        /// <param name="HEIGHT"></param>
        /// <returns></returns>
        public static TileObjectOffset GetAlignBottom(int HEIGHT)
        {
            return new TileObjectOffset(new Vector2(0, -(HEIGHT / 2) + (WorldTile.TILE_HEIGHT / 2)));
        }
        /// <summary>
        /// Returns a TileObjectOffset instance that aligns the object to the bottom of the tile
        /// </summary>
        /// <param name="HEIGHT"></param>
        /// <returns></returns>
        public static TileObjectOffset GetAlignTop(int HEIGHT)
        {
            return new TileObjectOffset(new Vector2(0, - (WorldTile.TILE_HEIGHT / 2)));
        }
        /// <summary>
        /// Returns a TileObjectOffset instance that aligns the object to the bottom of the tile
        /// </summary>
        /// <param name="HEIGHT"></param>
        /// <returns></returns>
        public static TileObjectOffset GetAlignLeft(int WIDTH)
        {
            return new TileObjectOffset(new Vector2(-(WorldTile.TILE_WIDTH / 2),0));
        }
        /// <summary>
        /// Returns a TileObjectOffset instance that aligns the object to the bottom of the tile
        /// </summary>
        /// <param name="HEIGHT"></param>
        /// <returns></returns>
        public static TileObjectOffset GetAlignTopLeft(int WIDTH, int HEIGHT)
        {
            var left = GetAlignLeft(WIDTH);
            return new TileObjectOffset(new Vector2(left.Value.X, GetAlignTop(HEIGHT).Value.Y));
        }
        public TileObjectOffset(Vector2 Offset)
        {
            this.Value = Offset;
        }
        public Vector2 Value
        {
            get;set;
        }
    }
    public interface IWorldGenerationParams
    {

    }
    public interface ITileObject
    {
        string Title
        {
            get;
        }
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
        TileObjectOffset Offset
        {
            get;
        }
        int SquareSize
        {
            get;
        }
        Vector2 OriginRatio { get; }

        void Update(GameTime time);
    }
    public class WorldTile
    {
        public World Parent { get; internal set; }
        public const int TILE_WIDTH = 120, TILE_HEIGHT = 60;
        public enum TileTypes : short
        {
            DeepOcean,
            Ocean,            
            Dirt,
            Grass,
            DeepGrass,
            DarkStone,
            Stone,
            Snow
        }
        public TileTypes TileType;
        private Texture2D _texture;
        private Color? _highlight;

        public int Row
        {
            get => GridLocation.Row;
            set => GridLocation.Row = value;
        }
        /// <summary>
        /// Calculates the position of this tile in the world using <see cref="World.ChunkPosition"/>, <see cref="Row"/> and <see cref="Column"/>
        /// </summary>
        /// <returns></returns>
        public Point GetWorldPos()
        {
            var wCol = Parent.ChunkPosition.X;
            var wRow = Parent.ChunkPosition.Y;
            return new Point(
                    (int)(wCol * (50 * (TILE_WIDTH / 2.0f)) + wRow * (50 * (TILE_WIDTH / 2.0f))) +
                    (int)(Column * (.5f * TILE_WIDTH) + Row * (.5f * TILE_WIDTH)),
                    (int)(wRow * (50 * (TILE_HEIGHT / 2.0f)) - wCol * (50 * (TILE_HEIGHT / 2.0f))) +
                    (int)(Row * (.5f * TILE_HEIGHT) - Column * (.5f * TILE_HEIGHT)));
        }
        public int Column
        {
            get => GridLocation.Column;
            set => GridLocation.Column = value;
        }
        public GridCoordinate GridLocation
        {
            get; private set;
        } = new GridCoordinate(0,0);
        /// <summary>
        /// The world-position of this tile. See: <see cref="GetWorldPos"/>
        /// </summary>
        public Point WorldPosition => GetWorldPos();
        public double ZIndex
        {
            get; internal set;
        }              
        public bool Occupied
        {
            get; internal set;
        }
        public Texture2D Texture
        {
            get; set;
        }
        public Direction FacingDirection
        {
            get;set;
        }
        public Color Highlight
        {
            get
            {
                if (_highlight == null)
                    _highlight = Parent.GetTileColor(TileType);
                return _highlight.Value;
            }
        }
        public Rectangle Area => new Rectangle(WorldPosition, new Point(TILE_WIDTH, TILE_HEIGHT));
        public static readonly Rectangle DefaultArea = new Rectangle(0, 0, TILE_WIDTH, TILE_HEIGHT);
        public Point Center => Area.Center;
        public ITileObject PlacedObject
        {
            get;
            private set;
        }
        public WorldTile(World world, TileTypes Type, int Row, int Col)
        {
            this.Parent = world;
            TileType = Type;
            this.Row = Row;
            Column = Col;
            PlacedObject = null;
        }
        internal bool PlaceObject<T>(T Object) where T : ITileObject
        {
            if (PlacedObject != null)
                return false;
            if (Occupied) return false;
            PathfindingProvider pathfindingSystem = null; // ProviderManager.Root.Get<PathfindingProvider>();
            if (pathfindingSystem != null)            
                pathfindingSystem.TrySetTileState(WorldPosition,
                    PathfindingProvider.PathfindingTileSpaceState.Occupied, "GLACIER.WORLDTILE", out _);            
            PlacedObject = Object;
            Object.Position = (Center
                            - new Point(Object.Size.X / 2, Object.Size.Y / 2)).ToVector2()
                            + Object.Offset.Value; // position in direct center of tile, plus the Offset in ITileObject
            Object.Parent = this;
            Occupied = true;            
            return true;
        }

        internal void DeleteObject()
        {            
            PlacedObject.Parent = null;
            PlacedObject = null;
            Occupied = false;
        }
    }
    public abstract class World : Engine.IGameComponent
    {             
        public const int W_SIZE = 50;
        public string Name
        {
            get;set;
        }
        public Point ChunkPosition { get; internal set;  }
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
        public double[,] PerlinNoise { get; set; }

        /// <summary>
        /// A cache of the WorldLocation of each tile of the map with it's GridCoordinate -- avoids unnecessary mathmatical calculations with grids at angles other than 90
        /// </summary>
        protected abstract Dictionary<Point, GridCoordinate> WorldLocationCache
        {
            get; set;
        }
        
        public WorldTile[,] WorldData;
        /// <summary>
        /// The Z-Sorted WorldData array, sorted from Farthest to Closest
        /// </summary>
        public List<WorldTile> OrderedWorldData
        {
            get; protected set;
        }
        public bool Destroyed { get; set; }

        private List<Point> tileCachedLocations = new List<Point>();

        protected World(Point ChunkPosition) : this("AntWorld", ChunkPosition, false)
        {
            
        }

        protected World(string name, Point ChunkPosition, bool init) : this(name, ChunkPosition, init, W_SIZE, W_SIZE)
        {
            
        }
        protected World(string name, Point ChunkPosition, bool init, int sizeX = W_SIZE, int sizeY = W_SIZE)
        {
            Name = name;
            SizeX = sizeX;
            SizeY = sizeY; 
            this.ChunkPosition = ChunkPosition;
            if (init)
                Initialize();
        }

        public abstract string GetTileTextureName(WorldTile.TileTypes TileType);
        public abstract Color GetTileColor(WorldTile.TileTypes TileType);

        /// <summary>
        /// Gets the Tile Row/Column from the World Position using the <see cref="WorldLocationCache"/>
        /// </summary>
        /// <param name="WorldPosition">The world location of the tile</param>
        /// <returns>NULL if the exact position is not found. If precision is not important, try: </returns>
        public GridCoordinate GetTileIndexFromWorldPosition(Point WorldPosition)
        {
            if (WorldLocationCache.TryGetValue(WorldPosition, out var gridloc))
                return gridloc;
            else return null;
        }
        
        public WorldTile GetFromWorldPosition(Point WorldPosition)
        {
            var location = GetTileIndexFromWorldPosition(WorldPosition);
            if (location != null)
                return GetFromGridLocation(location);
            var tiles = new List<WorldTile>();
            foreach (var worldTile in OrderedWorldData.Reverse<WorldTile>())
            {
                if (worldTile.Area.Contains(WorldPosition))
                    tiles.Add(worldTile);
            }
            if (tiles.Count == 1)
                return tiles[0];
            foreach (var tile in tiles)
            {
                var localPosition = GameResources.MouseWorldPosition - tile.WorldPosition;
                if (PointInsideViewableArea(localPosition))
                    return tile;
            }
            if (tiles.Any())
                return tiles[0];
            else return null;
        } 

        public T PlaceObjectOnTile<T>(T Object, int Column, int Row) where T : ITileObject
        {
            var tile = WorldData[Column, Row];
            return PlaceObjectOnTile(Object, tile);
        }  

        public T PlaceObjectOnTile<T>(T Object, WorldTile tile) where T : ITileObject
        {
            try
            {
                ProviderManager.Root.Get<GameObjectManager>().Add(Object.ThisObject);
            }
            catch (Exception) { }
            tile.PlaceObject(Object);
            if (Object.SquareSize > 1)
            {
                for(int x = 0; x < ((ITileObject)Object).SquareSize; x++)
                {
                    for (int y = 0; y < ((ITileObject)Object).SquareSize; y++)
                    {
                        var multitile = GetFromGridLocation(new GridCoordinate(tile.GridLocation.Row + y,
                                                                               tile.GridLocation.Column + x));
                        multitile.Occupied = true;
                    }
                }
            }
            return Object;
        }
        
        public T PlaceObjectOnTile<T>(T Object, GridCoordinate RowColPoint) where T : ITileObject
        {
            return PlaceObjectOnTile(Object, RowColPoint.Column, RowColPoint.Row);
        }

        public void RemoveObjectOnTile(WorldTile tile)
        {
            var Object = tile.PlacedObject;
            tile.DeleteObject();
            ProviderManager.Root.Get<GameObjectManager>().Add(Object.ThisObject);
            if (Object.SquareSize > 1)
            for (int x = 0; x < Object.SquareSize; x++)
            {
                for (int y = 0; y < Object.SquareSize; y++)
                {
                    var multitile = GetFromGridLocation(new GridCoordinate(tile.GridLocation.Row + y,
                                                                           tile.GridLocation.Column + x));
                    multitile.Occupied = false;
                }
            }        
        }

        public abstract void GenerateStructures(IWorldGenerationParams Params);

        public virtual void Initialize()
        {
            OrderedWorldData = new List<WorldTile>();
            int row = 0, loops = 0;
            for(int y = 0; y < SizeY; y++)
            {
                var Y = y;
                for(int x = SizeX-1; x >= 0; x--)
                {
                    if (Y <= -1)
                        break;
                    var currentTile = WorldData[x, Y];
                    OrderedWorldData.Add(currentTile);
                    currentTile.ZIndex = (double)row / (SizeX + SizeY);
                    loops++;
                    Y--;
                }
                row++;
                
            }            
            for (int x = SizeX - 2; x >= 0; x--)
            {
                var X = x;
                for (int y = SizeY - 1; y >= 0; y--)
                {
                    if (X <= -1)
                        break;
                    var currentTile = WorldData[X, y];
                    OrderedWorldData.Add(currentTile);
                    currentTile.ZIndex = (double)row / (SizeX + SizeY);
                    loops++;
                    X--;
                }
                row++;
            }           
            IsLoaded = true;
            /// THIS IS LEGACY CODE FOR PER-PIXEL COLLISION ON TILES
            /*
            var texture = GameResources.GetTexture("Tiles/blank_reg");
            var colors = new Color[texture.Width*texture.Height];
            texture.GetData(0, null, colors, 0, colors.Length);
            for(int x = 0; x < texture.Width; x++)
            {
                for(int y = 0; y < texture.Height; y++)
                {
                    var color = colors[y * texture.Width + x];
                    if (color.A != 0)
                        tileCachedLocations.Add(new Point(x, y));
                }
            }
            */
        }
        /// <summary>
        /// Calculates whether the supplied point is within the boundaries of this tile's visible space.
        /// <para>This is done by simply calculating if the point is within a diamond of the Tile's size - it does not take into
        /// consideration an irregularly shaped tile. This is too performance intensive for almost no benefit.</para>
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public Boolean PointInsideViewableArea(Point Position)
        {
            int x = Position.X;
            float y = Position.Y;
            double width = WorldTile.TILE_WIDTH;
            double height = WorldTile.TILE_HEIGHT;
            Point center = new Point((int)(width / 2), (int)(height / 2));
            int dx = Math.Abs(x - center.X);
            double dy = Math.Abs(y - center.Y);
            double d = dx / width + dy / height;
            return d <= 0.5;
        }
        public abstract void Update(GameTime gt);
        public WorldTile GetFromGridLocation(GridCoordinate Coordinates)
        {
            if (Coordinates.Column < 0)
                return null;//Coordinates.Column = 0;
            if (Coordinates.Column >= SizeX)
                return null;//Coordinates.Column = SizeX-1;
            if (Coordinates.Row < 0)
                return null;// Coordinates.Row = 0;
            if (Coordinates.Row >= SizeY)
                return null;// Coordinates.Row = SizeY-1;                
            try
            {
                return WorldData[Coordinates.Column, Coordinates.Row];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<T> SearchFromCenter<T>(GridCoordinate Center, int SquareArea) => 
            Search<T>(new GridCoordinate(Center.Row - (SquareArea / 2), Center.Column - (SquareArea / 2)),
                new GridCoordinate(Center.Row + (SquareArea / 2), Center.Column + (SquareArea / 2)));

        public IEnumerable<T> Search<T>(GridCoordinate From, int SquareArea)=>
            Search<T>(From, new GridCoordinate(From.Row + SquareArea, From.Column + SquareArea));

        public IEnumerable<T> Search<T>(GridCoordinate From, GridCoordinate To)
        {
            List<WorldTile> list = new List<WorldTile>();
            for(int x = From.Column; x < To.Column; x++)
            {
                for (int y = From.Row; y < To.Row; y++)
                {
                    var tile = GetFromGridLocation(new GridCoordinate(y, x));
                    if (tile == null) continue;
                    if (tile.PlacedObject != null && tile.PlacedObject is T)
                    {
                        yield return (T)tile.PlacedObject;
                    }
                }
            }            
        }

        public GridCoordinate GetRandomTileCoordinate()
        {
            return GameResources.GetRandomPoint(0, SizeX, 0, SizeY);
        }

        public void Dispose()
        {
            WorldLocationCache.Clear();
            WorldData = null;
            PerlinNoise = null;
            OrderedWorldData.Clear();
            OrderedWorldData = null;
        }
    }
}
