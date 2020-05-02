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
