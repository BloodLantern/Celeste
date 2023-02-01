// Decompiled with JetBrains decompiler
// Type: Celeste.StatsForStadia
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Celeste
{
  public static class StatsForStadia
  {
    private static Dictionary<StadiaStat, string> statToString = new Dictionary<StadiaStat, string>();
    private static bool ready;

    public static void MakeRequest()
    {
    }

    public static void Increment(StadiaStat stat, int increment = 1)
    {
    }

    public static void SetIfLarger(StadiaStat stat, int value)
    {
    }

    public static void BeginFrame(IntPtr handle)
    {
    }
  }
}
