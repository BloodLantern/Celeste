using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MountainState
    {
        public Skybox Skybox;
        public VirtualTexture TerrainTexture;
        public VirtualTexture BuildingsTexture;
        public Color FogColor;

        public MountainState(
            VirtualTexture terrainTexture,
            VirtualTexture buildingsTexture,
            VirtualTexture skyboxTexture,
            Color fogColor)
        {
            this.TerrainTexture = terrainTexture;
            this.BuildingsTexture = buildingsTexture;
            this.Skybox = new Skybox(skyboxTexture);
            this.FogColor = fogColor;
        }
    }
}
