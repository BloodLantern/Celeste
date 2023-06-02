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
            OVR.Atlas = (Atlas) null;
            OVR.Loaded = false;
        }
    }
}
