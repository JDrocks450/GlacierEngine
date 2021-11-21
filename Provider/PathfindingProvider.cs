using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{    
    public class PathfindingProvider : IProvider
    {
        private struct PATHCELL
        {
            public PATHCELL(Point point, PathfindingTileSpaceState tileSpaceState, string sender)
            {
                Point = point;
                TileSpaceState = tileSpaceState;
                Sender = sender;
            }

            /// <summary>
            /// The world position of this tile.
            /// </summary>
            public Point Point { get; }
            /// <summary>
            /// The tile's current state
            /// </summary>
            public PathfindingTileSpaceState TileSpaceState { get; }
            /// <summary>
            /// The object that requested this change
            /// </summary>
            public string Sender {  get; }
        }
        /// <summary>
        /// Represents the various states a pathfinding cell can have.
        /// </summary>
        public enum PathfindingTileSpaceState
        {
            /// <summary>
            /// This space in the pathfinding grid can be used in a path to a destination.
            /// </summary>
            Available,
            /// <summary>
            /// This space is occupied and cannot be pathed to.
            /// </summary>
            Occupied,
            /// <summary>
            /// This grid requires special calculation or flags to be set before being pathable.
            /// </summary>
            Special,
            /// <summary>
            /// This cell is never intended to be used in pathing and will not be considered.
            /// </summary>
            Unavailable,
            /// <summary>
            /// This position is out of bounds.
            /// </summary>
            OutOfBoundary,
        }
        public ProviderManager Parent { get; set; }
        public int WorldSizeX { get; }
        public int WorldSizeY { get; }
        private PATHCELL[,] map;

        public PathfindingProvider(int WorldSizeX, int WorldSizeY)
        {
            map = new PATHCELL[WorldSizeX, WorldSizeY];
            for (int x = 0; x < WorldSizeX; x++)
            {
                for(int y = 0; y < WorldSizeY; y++)
                {
                    TrySetTileState(new Point(x, y), PathfindingTileSpaceState.Available, "GLACIER", out _);
                }
            }

            this.WorldSizeX = WorldSizeX;
            this.WorldSizeY = WorldSizeY;
        }

        /// <summary>
        /// Attempts to set the state of the tile. Will fail if the requested state or position is <see cref="PathfindingTileSpaceState.OutOfBoundary"/>
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="State"></param>
        /// <param name="Sender"></param>
        /// <param name="NewState"></param>
        /// <returns></returns>
        public bool TrySetTileState(Point Position, PathfindingTileSpaceState State, string Sender, out PathfindingTileSpaceState NewState)
        {
            if (State != PathfindingTileSpaceState.OutOfBoundary)
            {
                if (Position.X > 0 && Position.Y > 0)
                {
                    if (Position.X < WorldSizeX && Position.Y < WorldSizeY)
                    {
                        map[Position.X, Position.Y] = new PATHCELL(Position, State, Sender);
                        NewState = State;
                        return true;
                    }
                }
            }
            NewState = PathfindingTileSpaceState.OutOfBoundary;
            return false;
        }

        /// <summary>
        /// Attempts to read the tile type at the given position, 
        /// will fail if the position is out of bounds, returning <see cref="PathfindingTileSpaceState.OutOfBoundary"/>.
        /// </summary>
        /// <param name="Position">The position to read</param>
        /// <param name="State">The state of the tile.</param>
        /// <returns></returns>
        public bool TryGetTileState(Point Position, out PathfindingTileSpaceState State)
        {
            if (Position.X > 0 && Position.Y > 0)
            {
                if (Position.X < WorldSizeX && Position.Y < WorldSizeY)
                {
                    State = map[Position.X, Position.Y].TileSpaceState;
                    return true;
                }
            }
            State = PathfindingTileSpaceState.OutOfBoundary;
            return false;
        }

        public void Refresh(GameTime time)
        {
            
        }
    }
}
