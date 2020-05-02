using Glacier.Common.Primitives;
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
        void Update(GameTime time);
    }
    public class WorldTile
    {
        public World Parent { get; internal set; }
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
        private Texture2D _texture;
        private Color? _highlight;

        public int Row
        {
            get => GridLocation.Row;
            set
            {
                GridLocation.Row = value;
                WorldPosition = new Point((int)(Column * (.5f * TILE_WIDTH) + Row * (.5f * TILE_WIDTH)),
                        (int)(Row * (.5f * TILE_HEIGHT) - Column * (.5f * TILE_HEIGHT)));  
            }
        }
        public int Column
        {            
            get => GridLocation.Column;
            set
            {
                GridLocation.Column = value;
                WorldPosition = new Point((int)(Column * (.5f * TILE_WIDTH) + Row * (.5f * TILE_WIDTH)),
                        (int)(Row * (.5f * TILE_HEIGHT) - Column * (.5f * TILE_HEIGHT)));   
            }     
        }
        public GridCoordinate GridLocation
        {
            get; private set;
        } = new GridCoordinate(0,0);
        public Point WorldPosition
        {
            get; private set;
        }
        public double ZIndex
        {
            get; internal set;
        }              
        public Texture2D Texture
        {
            get
            {
                if (_texture == null)
                    _texture = GameResources.GetTexture("Tiles/" + Parent.GetTileTextureName(TileType));
                return _texture;
            }
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
            PlacedObject = Object;
            Object.Position = (Center
                            - new Point(Object.Size.X / 2, Object.Size.Y / 2)).ToVector2()
                            + Object.Offset.Value; // position in direct center of tile, plus the Offset in ITileObject
            Object.Parent = this;
            return true;
        }

        internal void DeleteObject()
        {            
            PlacedObject.Parent = null;
            PlacedObject = null;
        }
    }
    public abstract class World : Engine.IGameComponent
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
        public HashSet<WorldTile> WorldTilesWithObjects = new HashSet<WorldTile>();
        private List<Point> tileCachedLocations = new List<Point>();

        protected World() : this("AntWorld", false)
        {

        }

        protected World(string name, bool init) : this(name, init, W_SIZE, W_SIZE)
        {
            
        }
        protected World(string name, bool init, int sizeX = W_SIZE, int sizeY = W_SIZE)
        {
            Name = name;
            SizeX = sizeX;
            SizeY = sizeY; 
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
                if (tileCachedLocations.Contains(localPosition))
                    return tile;
            }
            return null;
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
                WorldTilesWithObjects.Add(tile);
            }
            catch (Exception) { }
            tile.PlaceObject(Object);
            return Object;
        }
        
        public T PlaceObjectOnTile<T>(T Object, GridCoordinate RowColPoint) where T : ITileObject
        {
            return PlaceObjectOnTile(Object, RowColPoint.Column, RowColPoint.Row);
        } 

        public void RemoveObjectOnTile(WorldTile tile)
        {
            tile.DeleteObject();
            WorldTilesWithObjects.Remove(tile);
        }
        
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
        }
        public abstract void Update(GameTime gt);
        public WorldTile GetFromGridLocation(GridCoordinate Coordinates)
        {
            return WorldData[Coordinates.Column, Coordinates.Row];
        }

        public GridCoordinate GetRandomTileCoordinate()
        {
            return GameResources.GetRandomPoint(0, SizeX, 0, SizeY);
        }
    }
}
