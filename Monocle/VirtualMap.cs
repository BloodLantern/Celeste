// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualMap`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
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
            SegmentColumns = (columns / 50) + 1;
            SegmentRows = (rows / 50) + 1;
            segments = new T[SegmentColumns, SegmentRows][,];
            EmptyValue = emptyValue;
        }

        public VirtualMap(T[,] map, T emptyValue = default)
            : this(map.GetLength(0), map.GetLength(1), emptyValue)
        {
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                {
                    this[x, y] = map[x, y];
                }
            }
        }

        public bool AnyInSegmentAtTile(int x, int y)
        {
            return segments[x / 50, y / 50] != null;
        }

        public bool AnyInSegment(int segmentX, int segmentY)
        {
            return segments[segmentX, segmentY] != null;
        }

        public T InSegment(int segmentX, int segmentY, int x, int y)
        {
            return segments[segmentX, segmentY][x, y];
        }

        public T[,] GetSegment(int segmentX, int segmentY)
        {
            return segments[segmentX, segmentY];
        }

        public T SafeCheck(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Columns && y < Rows ? this[x, y] : EmptyValue;
        }

        public T this[int x, int y]
        {
            get
            {
                int index1 = x / 50;
                int index2 = y / 50;
                T[,] segment = segments[index1, index2];
                return segment == null ? EmptyValue : segment[x - (index1 * 50), y - (index2 * 50)];
            }
            set
            {
                int index1 = x / 50;
                int index2 = y / 50;
                if (segments[index1, index2] == null)
                {
                    segments[index1, index2] = new T[50, 50];
                    if (EmptyValue != null && !EmptyValue.Equals(default(T)))
                    {
                        for (int index3 = 0; index3 < 50; ++index3)
                        {
                            for (int index4 = 0; index4 < 50; ++index4)
                            {
                                segments[index1, index2][index3, index4] = EmptyValue;
                            }
                        }
                    }
                }
                segments[index1, index2][x - (index1 * 50), y - (index2 * 50)] = value;
            }
        }

        public T[,] ToArray()
        {
            T[,] array = new T[Columns, Rows];
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                {
                    array[x, y] = this[x, y];
                }
            }
            return array;
        }

        public VirtualMap<T> Clone()
        {
            VirtualMap<T> virtualMap = new(Columns, Rows, EmptyValue);
            for (int x = 0; x < Columns; ++x)
            {
                for (int y = 0; y < Rows; ++y)
                {
                    virtualMap[x, y] = this[x, y];
                }
            }
            return virtualMap;
        }
    }
}
