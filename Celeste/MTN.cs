﻿using Monocle;
using System;
using System.Diagnostics;
using System.IO;

namespace Celeste
{
    public static class MTN
    {
        public static Atlas FileSelect;
        public static Atlas Journal;
        public static Atlas Mountain;
        public static Atlas Checkpoints;
        public static ObjModel MountainTerrain;
        public static ObjModel MountainBuildings;
        public static ObjModel MountainCoreWall;
        public static ObjModel MountainMoon;
        public static ObjModel MountainBird;
        public static VirtualTexture[] MountainTerrainTextures;
        public static VirtualTexture[] MountainBuildingTextures;
        public static VirtualTexture[] MountainSkyboxTextures;
        public static VirtualTexture MountainFogTexture;
        public static VirtualTexture MountainMoonTexture;
        public static VirtualTexture MountainStarSky;
        public static VirtualTexture MountainStars;
        public static VirtualTexture MountainStarStream;

        public static bool Loaded { get; private set; }

        public static bool DataLoaded { get; private set; }

        public static void Load()
        {
            if (!MTN.Loaded)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                MTN.FileSelect = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "FileSelect"), Atlas.AtlasDataFormat.Packer);
                MTN.Journal = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Journal"), Atlas.AtlasDataFormat.Packer);
                MTN.Mountain = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Mountain"), Atlas.AtlasDataFormat.PackerNoAtlas);
                MTN.Checkpoints = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Checkpoints"), Atlas.AtlasDataFormat.PackerNoAtlas);
                MTN.MountainTerrainTextures = new VirtualTexture[3];
                MTN.MountainBuildingTextures = new VirtualTexture[3];
                MTN.MountainSkyboxTextures = new VirtualTexture[3];
                for (int index = 0; index < 3; ++index)
                {
                    MTN.MountainSkyboxTextures[index] = MTN.Mountain["skybox_" + index].Texture;
                    MTN.MountainTerrainTextures[index] = MTN.Mountain["mountain_" + index].Texture;
                    MTN.MountainBuildingTextures[index] = MTN.Mountain["buildings_" + index].Texture;
                }
                MTN.MountainMoonTexture = MTN.Mountain["moon"].Texture;
                MTN.MountainFogTexture = MTN.Mountain["fog"].Texture;
                MTN.MountainStarSky = MTN.Mountain["space"].Texture;
                MTN.MountainStars = MTN.Mountain["spacestars"].Texture;
                MTN.MountainStarStream = MTN.Mountain["starstream"].Texture;
                Console.WriteLine(" - MTN LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            MTN.Loaded = true;
        }

        public static void LoadData()
        {
            if (!MTN.DataLoaded)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                string str = ".obj";
                MTN.MountainTerrain = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "mountain" + str));
                MTN.MountainBuildings = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "buildings" + str));
                MTN.MountainCoreWall = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "mountain_wall" + str));
                MTN.MountainMoon = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "moon" + str));
                MTN.MountainBird = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "bird" + str));
                Console.WriteLine(" - MTN DATA LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            MTN.DataLoaded = true;
        }

        public static void Unload()
        {
            if (MTN.Loaded)
            {
                MTN.Journal.Dispose();
                MTN.Journal = null;
                MTN.Mountain.Dispose();
                MTN.Mountain = null;
                MTN.Checkpoints.Dispose();
                MTN.Checkpoints = null;
                MTN.FileSelect.Dispose();
                MTN.FileSelect = null;
            }
            MTN.Loaded = false;
        }

        public static void UnloadData()
        {
            if (MTN.DataLoaded)
            {
                MTN.MountainTerrain.Dispose();
                MTN.MountainTerrain = null;
                MTN.MountainBuildings.Dispose();
                MTN.MountainBuildings = null;
                MTN.MountainCoreWall.Dispose();
                MTN.MountainCoreWall = null;
                MTN.MountainMoon.Dispose();
                MTN.MountainMoon = null;
                MTN.MountainBird.Dispose();
                MTN.MountainBird = null;
            }
            MTN.DataLoaded = false;
        }
    }
}
