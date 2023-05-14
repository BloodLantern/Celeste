// Decompiled with JetBrains decompiler
// Type: Celeste.OVR
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.IO;

namespace Celeste
{
    public static class OVR
    {
        public static Atlas Atlas;

        public static bool Loaded { get; private set; }

        public static void Load()
        {
            OVR.Atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Overworld"), Atlas.AtlasDataFormat.PackerNoAtlas);
            OVR.Loaded = true;
        }

        public static void Unload()
        {
            OVR.Atlas.Dispose();
            OVR.Atlas = null;
            OVR.Loaded = false;
        }
    }
}
