namespace Monocle
{
    /// <summary>
    /// Seems to be a general-purpose map for any kind of area. Basically,
    /// it has a two-dimensional array containing the area's levels, each
    /// containing a two-dimensional array containing each tile's value.
    /// </summary>
    /// <typeparam name="T">The type to store for each tile.</typeparam>
    public class VirtualMap<T>
    {
        public const int SegmentSize = 50;

        public readonly int Columns;
        public readonly int Rows;
        public readonly int SegmentColumns;
        public readonly int SegmentRows;
        public readonly T EmptyValue;

        private readonly T[,][,] segments;

        public VirtualMap(int columns, int rows, T emptyValue = default)
        {
            Columns = columns;
            Rows = rows;
            SegmentColumns = columns / SegmentSize + 1;
            SegmentRows = rows / SegmentSize + 1;
            segments = new T[SegmentColumns, SegmentRows][,];
            EmptyValue = emptyValue;
        }

        public VirtualMap(T[,] map, T emptyValue = default)
            : this(map.GetLength(0), map.GetLength(1), emptyValue)
        {
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                    this[x, y] = map[x, y];
            }
        }

        public bool AnyInSegmentAtTile(int x, int y) => segments[x / SegmentSize, y / SegmentSize] != null;

        public bool AnyInSegment(int segmentX, int segmentY) => segments[segmentX, segmentY] != null;

        public T InSegment(int segmentX, int segmentY, int x, int y) => segments[segmentX, segmentY][x, y];

        public T[,] GetSegment(int segmentX, int segmentY) => segments[segmentX, segmentY];

        public T SafeCheck(int x, int y) => x >= 0 && y >= 0 && x < Columns && y < Rows ? this[x, y] : EmptyValue;

        /// <summary>
        /// Gets the tile value at an absolute position.
        /// </summary>
        /// <param name="x">The X position to get the tile value at.</param>
        /// <param name="y">The Y position to get the tile value at.</param>
        /// <returns>The value of the tile at (x, y).</returns>
        public T this[int x, int y]
        {
            get
            {
                int segmentX = x / SegmentSize;
                int segmentY = y / SegmentSize;
                T[,] segment = segments[segmentX, segmentY];
                return segment == null ? EmptyValue : segment[x - segmentX * SegmentSize, y - segmentY * SegmentSize];
            }
            set
            {
                int segmentX = x / SegmentSize;
                int segmentY = y / SegmentSize;
                if (segments[segmentX, segmentY] == null)
                {
                    segments[segmentX, segmentY] = new T[SegmentSize, SegmentSize];
                    if (EmptyValue != null && !EmptyValue.Equals(default(T)))
                    {
                        for (int index3 = 0; index3 < SegmentSize; ++index3)
                        {
                            for (int index4 = 0; index4 < SegmentSize; ++index4)
                                segments[segmentX, segmentY][index3, index4] = EmptyValue;
                        }
                    }
                }
                segments[segmentX, segmentY][x - segmentX * SegmentSize, y - segmentY * SegmentSize] = value;
            }
        }

        public T[,] ToArray()
        {
            T[,] array = new T[Columns, Rows];
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                    array[x, y] = this[x, y];
            }
            return array;
        }

        public VirtualMap<T> Clone()
        {
            VirtualMap<T> result = new(Columns, Rows, EmptyValue);
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                    result[x, y] = this[x, y];
            }
            return result;
        }
    }
}
