using System;

namespace Monocle
{
    [Flags]
    public enum PointSectors
    {
        Center = 0,
        Top = 1,
        Bottom = 2,
        TopLeft = 9,
        TopRight = 5,
        Left = 8,
        Right = 4,
        BottomLeft = Left | Bottom, // 0x0000000A
        BottomRight = Right | Bottom, // 0x00000006
    }
}
