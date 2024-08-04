using System.Xml;

namespace Monocle
{
    public class AutotileData
    {
        public int[] Center;
        public int[] Single;
        public int[] SingleHorizontalLeft;
        public int[] SingleHorizontalCenter;
        public int[] SingleHorizontalRight;
        public int[] SingleVerticalTop;
        public int[] SingleVerticalCenter;
        public int[] SingleVerticalBottom;
        public int[] Top;
        public int[] Bottom;
        public int[] Left;
        public int[] Right;
        public int[] TopLeft;
        public int[] TopRight;
        public int[] BottomLeft;
        public int[] BottomRight;
        public int[] InsideTopLeft;
        public int[] InsideTopRight;
        public int[] InsideBottomLeft;
        public int[] InsideBottomRight;

        public AutotileData(XmlElement xml)
        {
            Center = Calc.ReadCSVInt(xml.ChildText(nameof (Center), ""));
            Single = Calc.ReadCSVInt(xml.ChildText(nameof (Single), ""));
            SingleHorizontalLeft = Calc.ReadCSVInt(xml.ChildText(nameof (SingleHorizontalLeft), ""));
            SingleHorizontalCenter = Calc.ReadCSVInt(xml.ChildText(nameof (SingleHorizontalCenter), ""));
            SingleHorizontalRight = Calc.ReadCSVInt(xml.ChildText(nameof (SingleHorizontalRight), ""));
            SingleVerticalTop = Calc.ReadCSVInt(xml.ChildText(nameof (SingleVerticalTop), ""));
            SingleVerticalCenter = Calc.ReadCSVInt(xml.ChildText(nameof (SingleVerticalCenter), ""));
            SingleVerticalBottom = Calc.ReadCSVInt(xml.ChildText(nameof (SingleVerticalBottom), ""));
            Top = Calc.ReadCSVInt(xml.ChildText(nameof (Top), ""));
            Bottom = Calc.ReadCSVInt(xml.ChildText(nameof (Bottom), ""));
            Left = Calc.ReadCSVInt(xml.ChildText(nameof (Left), ""));
            Right = Calc.ReadCSVInt(xml.ChildText(nameof (Right), ""));
            TopLeft = Calc.ReadCSVInt(xml.ChildText(nameof (TopLeft), ""));
            TopRight = Calc.ReadCSVInt(xml.ChildText(nameof (TopRight), ""));
            BottomLeft = Calc.ReadCSVInt(xml.ChildText(nameof (BottomLeft), ""));
            BottomRight = Calc.ReadCSVInt(xml.ChildText(nameof (BottomRight), ""));
            InsideTopLeft = Calc.ReadCSVInt(xml.ChildText(nameof (InsideTopLeft), ""));
            InsideTopRight = Calc.ReadCSVInt(xml.ChildText(nameof (InsideTopRight), ""));
            InsideBottomLeft = Calc.ReadCSVInt(xml.ChildText(nameof (InsideBottomLeft), ""));
            InsideBottomRight = Calc.ReadCSVInt(xml.ChildText(nameof (InsideBottomRight), ""));
        }

        public int TileHandler()
        {
            if (Tiler.Left && Tiler.Right && Tiler.Up && Tiler.Down && Tiler.UpLeft && Tiler.UpRight && Tiler.DownLeft && Tiler.DownRight)
                return GetTileID(Center);
            if (!Tiler.Up && !Tiler.Down)
            {
                if (Tiler.Left && Tiler.Right)
                    return GetTileID(SingleHorizontalCenter);
                if (!Tiler.Left && !Tiler.Right)
                    return GetTileID(Single);
                return Tiler.Left ? GetTileID(SingleHorizontalRight) : GetTileID(SingleHorizontalLeft);
            }
            if (!Tiler.Left && !Tiler.Right)
            {
                if (Tiler.Up && Tiler.Down)
                    return GetTileID(SingleVerticalCenter);
                return Tiler.Down ? GetTileID(SingleVerticalTop) : GetTileID(SingleVerticalBottom);
            }
            if (Tiler.Up && Tiler.Down && Tiler.Left && !Tiler.Right)
                return GetTileID(Right);
            if (Tiler.Up && Tiler.Down && !Tiler.Left && Tiler.Right)
                return GetTileID(Left);
            if (Tiler.Up && !Tiler.Left && Tiler.Right && !Tiler.Down)
                return GetTileID(BottomLeft);
            if (Tiler.Up && Tiler.Left && !Tiler.Right && !Tiler.Down)
                return GetTileID(BottomRight);
            if (Tiler.Down && Tiler.Right && !Tiler.Left && !Tiler.Up)
                return GetTileID(TopLeft);
            if (Tiler.Down && !Tiler.Right && Tiler.Left && !Tiler.Up)
                return GetTileID(TopRight);
            if (Tiler.Up && Tiler.Down && !Tiler.DownRight && Tiler.DownLeft)
                return GetTileID(InsideTopLeft);
            if (Tiler.Up && Tiler.Down && Tiler.DownRight && !Tiler.DownLeft)
                return GetTileID(InsideTopRight);
            if (Tiler.Up && Tiler.Down && Tiler.UpLeft && !Tiler.UpRight)
                return GetTileID(InsideBottomLeft);
            if (Tiler.Up && Tiler.Down && !Tiler.UpLeft && Tiler.UpRight)
                return GetTileID(InsideBottomRight);
            return !Tiler.Down ? GetTileID(Bottom) : GetTileID(Top);
        }

        private int GetTileID(int[] choices)
        {
            if (choices.Length == 0)
                return -1;
            return choices.Length == 1 ? choices[0] : Calc.Random.Choose(choices);
        }
    }
}
