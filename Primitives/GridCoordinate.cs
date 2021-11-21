using Microsoft.Xna.Framework;

namespace Glacier.Common.Primitives
{
    public sealed class GridCoordinate
    {
        private Point point;
        public int Row
        {
            get => point.Y;
            set => point.Y = value;
        }
        public int Column
        {
            get => point.X;
            set => point.X = value;
        }

        public override string ToString()
        {
            return $"{{Row: {Row}, Column: {Column}}}";
        }

        public GridCoordinate(int Row, int Column)
        {
            point = new Point(0);
            this.Row = Row;
            this.Column = Column;
        }

        public static implicit operator Point(GridCoordinate coord)
        {
            return coord.point;
        }
        public static implicit operator GridCoordinate(Point coord)
        {
            return new GridCoordinate(coord.Y, coord.X);
        }
        public override bool Equals(object obj)
        {
            if (obj is GridCoordinate coord)
                return point == coord.point;
            else return false;
        }
        public static GridCoordinate operator +(GridCoordinate left, GridCoordinate right)
        {            
            return new GridCoordinate(left.Row + right.Row, left.Column + right.Column);
        }
        public static GridCoordinate operator -(GridCoordinate left, GridCoordinate right)
        {
            return new GridCoordinate(left.Row - right.Row, left.Column - right.Column);
        }
        public static GridCoordinate operator /(GridCoordinate left, GridCoordinate right)
        {
            return new GridCoordinate(left.Row / right.Row, left.Column / right.Column);
        }
        public static GridCoordinate operator *(GridCoordinate left, GridCoordinate right)
        {
            return new GridCoordinate(left.Row * right.Row, left.Column * right.Column);
        }
    }
}
