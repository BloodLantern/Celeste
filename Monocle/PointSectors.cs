// Decompiled with JetBrains decompiler
// Type: Monocle.PointSectors
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
