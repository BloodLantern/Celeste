using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Celeste
{
    public static class GFX
    {
        public static Atlas Game;
        public static Atlas Gui;
        public static Atlas Opening;
        public static Atlas Misc;
        public static Atlas Portraits;
        public static Atlas ColorGrades;
        public static VirtualTexture SplashScreen;
        public static VirtualTexture MagicGlowNoise;
        public static Effect FxMountain;
        public static Effect FxDistort;
        public static Effect FxGlitch;
        public static Effect FxGaussianBlur;
        public static Effect FxPrimitive;
        public static Effect FxDust;
        public static Effect FxDither;
        public static Effect FxMagicGlow;
        public static Effect FxMirrors;
        public static Effect FxColorGrading;
        public static BasicEffect FxDebug;
        public static Effect FxTexture;
        public static Effect FxLighting;
        public static SpriteBank SpriteBank;
        public static SpriteBank GuiSpriteBank;
        public static SpriteBank PortraitsSpriteBank;
        public static XmlDocument CompleteScreensXml;
        public static AnimatedTilesBank AnimatedTilesBank;
        public static Tileset SceneryTiles;
        public static Autotiler BGAutotiler;
        public static Autotiler FGAutotiler;
        public const float PortraitSize = 240f;
        public static readonly BlendState Subtract = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };
        public static readonly BlendState DestinationTransparencySubtract = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        public static bool Loaded { get; private set; }

        public static bool DataLoaded { get; private set; }

        public static void Load()
        {
            if (!Loaded)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                Game = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Gameplay"), Atlas.AtlasDataFormat.Packer);
                Opening = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Opening"), Atlas.AtlasDataFormat.PackerNoAtlas);
                Gui = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Gui"), Atlas.AtlasDataFormat.Packer);
                GuiSpriteBank = new SpriteBank(Gui, Path.Combine("Graphics", "SpritesGui.xml"));
                Misc = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Misc"), Atlas.AtlasDataFormat.PackerNoAtlas);
                Portraits = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Portraits"), Atlas.AtlasDataFormat.PackerNoAtlas);

                Draw.Particle = Game["util/particle"];
                Draw.Pixel = new MTexture(Game["util/pixel"], 1, 1, 1, 1);
                ParticleTypes.Load();

                ColorGrades = Atlas.FromDirectory(Path.Combine("Graphics", "ColorGrading"));

                MagicGlowNoise = VirtualContent.CreateTexture("glow-noise", 128, 128, Color.White);
                Color[] data = new Color[MagicGlowNoise.Width * MagicGlowNoise.Height];
                for (int index = 0; index < data.Length; ++index)
                    data[index] = new Color(Calc.Random.NextFloat(), Calc.Random.NextFloat(), Calc.Random.NextFloat(), 0.0f);
                MagicGlowNoise.Texture.SetData(data);

                Console.WriteLine(" - GFX LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            Loaded = true;
        }

        public static void LoadData()
        {
            if (!DataLoaded)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                PortraitsSpriteBank = new SpriteBank(Portraits, Path.Combine("Graphics", "Portraits.xml"));
                SpriteBank = new SpriteBank(Game, Path.Combine("Graphics", "Sprites.xml"));
                BGAutotiler = new Autotiler(Path.Combine("Graphics", "BackgroundTiles.xml"));
                FGAutotiler = new Autotiler(Path.Combine("Graphics", "ForegroundTiles.xml"));
                SceneryTiles = new Tileset(Game["tilesets/scenery"], 8, 8);
                PlayerSprite.ClearFramesMetadata();
                PlayerSprite.CreateFramesMetadata("player");
                PlayerSprite.CreateFramesMetadata("player_no_backpack");
                PlayerSprite.CreateFramesMetadata("badeline");
                PlayerSprite.CreateFramesMetadata("player_badeline");
                PlayerSprite.CreateFramesMetadata("player_playback");
                CompleteScreensXml = Calc.LoadContentXML(Path.Combine("Graphics", "CompleteScreens.xml"));
                AnimatedTilesBank = new AnimatedTilesBank();
                foreach (XmlElement xml in (XmlNode) Calc.LoadContentXML(Path.Combine("Graphics", "AnimatedTiles.xml"))["Data"])
                {
                    if (xml != null)
                        AnimatedTilesBank.Add(xml.Attr("name"), xml.AttrFloat("delay", 0.0f), xml.AttrVector2("posX", "posY", Vector2.Zero), xml.AttrVector2("origX", "origY", Vector2.Zero), Game.GetAtlasSubtextures(xml.Attr("path")));
                }
                Console.WriteLine(" - GFX DATA LOAD: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            DataLoaded = true;
        }

        public static void Unload()
        {
            if (Loaded)
            {
                Game.Dispose();
                Game = null;
                Gui.Dispose();
                Gui = null;
                Opening.Dispose();
                Opening = null;
                Misc.Dispose();
                Misc = null;
                ColorGrades.Dispose();
                ColorGrades = null;
                MagicGlowNoise.Dispose();
                MagicGlowNoise = null;
                Portraits.Dispose();
                Portraits = null;
            }
            Loaded = false;
        }

        public static void UnloadData()
        {
            if (DataLoaded)
            {
                GuiSpriteBank = null;
                PortraitsSpriteBank = null;
                SpriteBank = null;
                CompleteScreensXml = null;
                SceneryTiles = null;
                BGAutotiler = null;
                FGAutotiler = null;
            }
            DataLoaded = false;
        }

        public static void LoadEffects()
        {
            FxMountain = LoadFx("MountainRender");
            FxGaussianBlur = LoadFx("GaussianBlur");
            FxDistort = LoadFx("Distort");
            FxDust = LoadFx("Dust");
            FxPrimitive = LoadFx("Primitive");
            FxDither = LoadFx("Dither");
            FxMagicGlow = LoadFx("MagicGlow");
            FxMirrors = LoadFx("Mirrors");
            FxColorGrading = LoadFx("ColorGrade");
            FxGlitch = LoadFx("Glitch");
            FxTexture = LoadFx("Texture");
            FxLighting = LoadFx("Lighting");
            FxDebug = new BasicEffect(Engine.Graphics.GraphicsDevice);
        }

        public static Effect LoadFx(string name) => Engine.Instance.Content.Load<Effect>(Path.Combine("Effects", name));

        public static void DrawVertices<T>(
            Matrix matrix,
            T[] vertices,
            int vertexCount,
            Effect effect = null,
            BlendState blendState = null)
            where T : struct, IVertexType
        {
            Effect effect1 = effect != null ? effect : FxPrimitive;
            BlendState blendState1 = blendState != null ? blendState : BlendState.AlphaBlend;
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            double width = viewport.Width;
            viewport = Engine.Graphics.GraphicsDevice.Viewport;
            double height = viewport.Height;
            Vector2 local = new((float) width, (float) height);
            matrix *= Matrix.CreateScale((float) (1.0 / local.X * 2.0), (float) (-(1.0 / local.Y) * 2.0), 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0.0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState1;
            effect1.Parameters["World"].SetValue(matrix);
            foreach (EffectPass pass in effect1.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserPrimitives<T>(PrimitiveType.TriangleList, vertices, 0, vertexCount / 3);
            }
        }

        public static void DrawIndexedVertices<T>(
            Matrix matrix,
            T[] vertices,
            int vertexCount,
            int[] indices,
            int primitiveCount,
            Effect effect = null,
            BlendState blendState = null)
            where T : struct, IVertexType
        {
            Effect effect1 = effect != null ? effect : FxPrimitive;
            BlendState blendState1 = blendState != null ? blendState : BlendState.AlphaBlend;
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            double width = viewport.Width;
            viewport = Engine.Graphics.GraphicsDevice.Viewport;
            double height = viewport.Height;
            Vector2 local = new((float) width, (float) height);
            matrix *= Matrix.CreateScale((float) (1.0 / local.X * 2.0), (float) (-(1.0 / local.Y) * 2.0), 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0.0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState1;
            effect1.Parameters["World"].SetValue(matrix);
            foreach (EffectPass pass in effect1.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives<T>(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
    }
}
