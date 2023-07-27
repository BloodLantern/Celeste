using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Celeste
{
    public class Decal : Entity
    {
        public const string Root = "decals";
        public const string MirrorMaskRoot = "mirrormasks";
        public string Name;
        public float AnimationSpeed = 12f;
        private Component image;
        public bool IsCrack;
        private readonly List<MTexture> textures;
        private Vector2 scale;
        private float frame;
        private readonly bool animated = true;
        private bool parallax;
        private float parallaxAmount;
        private bool scaredAnimal;
        private SineWave wave;

        public Decal(string texture, Vector2 position, Vector2 scale, int depth)
            : base(position)
        {
            Depth = depth;
            this.scale = scale;
            string extension = Path.GetExtension(texture);
            Name = Regex.Replace(Path.Combine("decals", texture.Replace(extension, "")).Replace('\\', '/'), "\\d+$", string.Empty);
            textures = GFX.Game.GetAtlasSubtextures(Name);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string path = Name.ToLower().Replace("decals/", "");
            switch (path)
            {
                case "0-prologue/house":
                    CreateSmoke(new Vector2(36f, -28f), true);
                    break;
                case "1-forsakencity/rags":
                case "1-forsakencity/ragsb":
                case "3-resort/curtain_side_a":
                case "3-resort/curtain_side_d":
                    MakeBanner(2f, 3.5f, 2, 0.05f, true);
                    break;
                case "10-farewell/bed":
                case "10-farewell/car":
                case "10-farewell/cliffside":
                case "10-farewell/floating house":
                case "10-farewell/giantcassete":
                case "10-farewell/heart_a":
                case "10-farewell/heart_b":
                case "10-farewell/reflection":
                case "10-farewell/temple":
                case "10-farewell/tower":
                    Depth = 10001;
                    MakeParallax(-0.15f);
                    MakeFloaty();
                    break;
                case "10-farewell/clouds/cloud_a":
                case "10-farewell/clouds/cloud_b":
                case "10-farewell/clouds/cloud_bb":
                case "10-farewell/clouds/cloud_bc":
                case "10-farewell/clouds/cloud_bd":
                case "10-farewell/clouds/cloud_c":
                case "10-farewell/clouds/cloud_cb":
                case "10-farewell/clouds/cloud_cc":
                case "10-farewell/clouds/cloud_cd":
                case "10-farewell/clouds/cloud_ce":
                case "10-farewell/clouds/cloud_d":
                case "10-farewell/clouds/cloud_db":
                case "10-farewell/clouds/cloud_dc":
                case "10-farewell/clouds/cloud_dd":
                case "10-farewell/clouds/cloud_e":
                case "10-farewell/clouds/cloud_f":
                case "10-farewell/clouds/cloud_g":
                case "10-farewell/clouds/cloud_h":
                case "10-farewell/clouds/cloud_i":
                case "10-farewell/clouds/cloud_j":
                    Depth = -13001;
                    MakeParallax(0.1f);
                    scale *= 1.15f;
                    break;
                case "10-farewell/coral_":
                case "10-farewell/coral_a":
                case "10-farewell/coral_b":
                case "10-farewell/coral_c":
                case "10-farewell/coral_d":
                    MakeScaredAnimation();
                    break;
                case "10-farewell/creature_a":
                case "10-farewell/creature_b":
                case "10-farewell/creature_c":
                case "10-farewell/creature_d":
                case "10-farewell/creature_e":
                case "10-farewell/creature_f":
                    Depth = 10001;
                    MakeParallax(-0.1f);
                    MakeFloaty();
                    break;
                case "10-farewell/finalflag":
                    AnimationSpeed = 6f;
                    Add(image = new FinalFlagDecalImage());
                    break;
                case "10-farewell/glitch_a_":
                case "10-farewell/glitch_b_":
                case "10-farewell/glitch_c":
                    frame = Calc.Random.NextFloat(textures.Count);
                    break;
                case "3-resort/bridgecolumn":
                    MakeSolid(-5f, -8f, 10f, 16f, 8);
                    break;
                case "3-resort/bridgecolumntop":
                    MakeSolid(-8f, -8f, 16f, 8f, 8);
                    MakeSolid(-5f, 0.0f, 10f, 8f, 8);
                    break;
                case "3-resort/brokenelevator":
                    MakeSolid(-16f, -20f, 32f, 48f, 22);
                    break;
                case "3-resort/roofcenter":
                case "3-resort/roofcenter_b":
                case "3-resort/roofcenter_c":
                case "3-resort/roofcenter_d":
                    MakeSolid(-8f, -4f, 16f, 8f, 14);
                    break;
                case "3-resort/roofedge":
                case "3-resort/roofedge_b":
                case "3-resort/roofedge_c":
                case "3-resort/roofedge_d":
                    MakeSolid(scale.X < 0.0 ? 0.0f : -8f, -4f, 8f, 8f, 14);
                    break;
                case "3-resort/vent":
                    CreateSmoke(Vector2.Zero, false);
                    break;
                case "4-cliffside/bridge_a":
                    MakeSolid(-24f, 0.0f, 48f, 8f, 8, Depth != 9000);
                    break;
                case "4-cliffside/flower_a":
                case "4-cliffside/flower_b":
                case "4-cliffside/flower_c":
                case "4-cliffside/flower_d":
                    MakeBanner(2f, 2f, 1, 0.05f, false, 2f, true);
                    break;
                case "5-temple-dark/mosaic_b":
                    Add(new BloomPoint(new Vector2(0.0f, 5f), 0.75f, 16f));
                    break;
                case "5-temple/bg_mirror_a":
                case "5-temple/bg_mirror_b":
                case "5-temple/bg_mirror_shard_a":
                case "5-temple/bg_mirror_shard_b":
                case "5-temple/bg_mirror_shard_c":
                case "5-temple/bg_mirror_shard_d":
                case "5-temple/bg_mirror_shard_e":
                case "5-temple/bg_mirror_shard_f":
                case "5-temple/bg_mirror_shard_g":
                case "5-temple/bg_mirror_shard_group_a":
                case "5-temple/bg_mirror_shard_group_a_b":
                case "5-temple/bg_mirror_shard_group_a_c":
                case "5-temple/bg_mirror_shard_group_b":
                case "5-temple/bg_mirror_shard_group_c":
                case "5-temple/bg_mirror_shard_group_d":
                case "5-temple/bg_mirror_shard_group_e":
                case "5-temple/bg_mirror_shard_h":
                case "5-temple/bg_mirror_shard_i":
                case "5-temple/bg_mirror_shard_j":
                case "5-temple/bg_mirror_shard_k":
                    scale.Y = 1f;
                    MakeMirror(path, false);
                    break;
                case "5-temple/bg_mirror_c":
                case "5-temple/statue_d":
                    MakeMirror(path, true);
                    break;
                case "6-reflection/crystal_reflection":
                    MakeMirrorSpecialCase(path, new Vector2(-12f, 2f));
                    break;
                case "7-summit/cloud_a":
                case "7-summit/cloud_b":
                case "7-summit/cloud_bb":
                case "7-summit/cloud_bc":
                case "7-summit/cloud_bd":
                case "7-summit/cloud_c":
                case "7-summit/cloud_cb":
                case "7-summit/cloud_cc":
                case "7-summit/cloud_cd":
                case "7-summit/cloud_ce":
                case "7-summit/cloud_d":
                case "7-summit/cloud_db":
                case "7-summit/cloud_dc":
                case "7-summit/cloud_dd":
                case "7-summit/cloud_e":
                case "7-summit/cloud_f":
                case "7-summit/cloud_g":
                case "7-summit/cloud_h":
                case "7-summit/cloud_i":
                case "7-summit/cloud_j":
                    Depth = -13001;
                    MakeParallax(0.1f);
                    scale *= 1.15f;
                    break;
                case "7-summit/summitflag":
                    Add(new SoundSource("event:/env/local/07_summit/flag_flap"));
                    break;
                case "9-core/ball_a":
                    Add(image = new CoreSwapImage(textures[0], GFX.Game["decals/9-core/ball_a_ice"]));
                    break;
                case "9-core/ball_a_ice":
                    Add(image = new CoreSwapImage(GFX.Game["decals/9-core/ball_a"], textures[0]));
                    break;
                case "9-core/heart_bevel_a":
                case "9-core/heart_bevel_b":
                case "9-core/heart_bevel_c":
                case "9-core/heart_bevel_d":
                    scale.Y = 1f;
                    scale.X = 1f;
                    break;
                case "9-core/rock_e":
                    Add(image = new CoreSwapImage(textures[0], GFX.Game["decals/9-core/rock_e_ice"]));
                    break;
                case "9-core/rock_e_ice":
                    Add(image = new CoreSwapImage(GFX.Game["decals/9-core/rock_e"], textures[0]));
                    break;
                case "generic/grass_a":
                case "generic/grass_b":
                case "generic/grass_c":
                case "generic/grass_d":
                    MakeBanner(2f, 2f, 1, 0.05f, false, -2f);
                    break;
            }
            if (Name.Contains("crack"))
                IsCrack = true;
            if (image != null)
                return;
            Add(image = new DecalImage());
        }

        private void MakeBanner(
            float speed,
            float amplitude,
            int sliceSize,
            float sliceSinIncrement,
            bool easeDown,
            float offset = 0.0f,
            bool onlyIfWindy = false)
        {
            Banner banner = new()
            {
                WaveSpeed = speed,
                WaveAmplitude = amplitude,
                SliceSize = sliceSize,
                SliceSinIncrement = sliceSinIncrement,
                Segments = new List<List<MTexture>>(),
                EaseDown = easeDown,
                Offset = offset,
                OnlyIfWindy = onlyIfWindy
            };
            foreach (MTexture texture in textures)
            {
                List<MTexture> mtextureList = new();
                for (int y = 0; y < texture.Height; y += sliceSize)
                    mtextureList.Add(texture.GetSubtexture(0, y, texture.Width, sliceSize));
                banner.Segments.Add(mtextureList);
            }
            Add(image = banner);
        }

        private void MakeFloaty() => Add(wave = new SineWave(Calc.Random.Range(0.1f, 0.4f), Calc.Random.NextFloat() * 6.28318548f));

        private void MakeSolid(
            float x,
            float y,
            float w,
            float h,
            int surfaceSoundIndex,
            bool blockWaterfalls = true)
        {
            Solid solid = new(Position + new Vector2(x, y), w, h, true);
            solid.BlockWaterfalls = blockWaterfalls;
            solid.SurfaceSoundIndex = surfaceSoundIndex;
            Scene.Add(solid);
        }

        private void CreateSmoke(Vector2 offset, bool inbg)
        {
            Level scene = Scene as Level;
            ParticleEmitter particleEmitter = new(inbg ? scene.ParticlesBG : scene.ParticlesFG, ParticleTypes.Chimney, offset, new Vector2(4f, 1f), -1.57079637f, 1, 0.2f);
            Add(particleEmitter);
            particleEmitter.SimulateCycle();
        }

        private void MakeMirror(string path, bool keepOffsetsClose)
        {
            Depth = 9500;
            if (keepOffsetsClose)
            {
                MakeMirror(path, GetMirrorOffset());
            }
            else
            {
                foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("mirrormasks/" + path))
                {
                    MTexture mask = atlasSubtexture;
                    MirrorSurface surface = new()
                    {
                        ReflectionOffset = GetMirrorOffset()
                    };
                    surface.OnRender = () => mask.DrawCentered(Position, surface.ReflectionColor, scale);
                    Add(surface);
                }
            }
        }

        private void MakeMirror(string path, Vector2 offset)
        {
            Depth = 9500;
            foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("mirrormasks/" + path))
            {
                MTexture mask = atlasSubtexture;
                MirrorSurface surface = new()
                {
                    ReflectionOffset = offset + new Vector2(Calc.Random.NextFloat(4f) - 2f, Calc.Random.NextFloat(4f) - 2f)
                };
                surface.OnRender = () => mask.DrawCentered(Position, surface.ReflectionColor, scale);
                Add(surface);
            }
        }

        private void MakeMirrorSpecialCase(string path, Vector2 offset)
        {
            Depth = 9500;
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("mirrormasks/" + path);
            for (int index = 0; index < atlasSubtextures.Count; ++index)
            {
                Vector2 vector2 = new(Calc.Random.NextFloat(4f) - 2f, Calc.Random.NextFloat(4f) - 2f);
                switch (index)
                {
                    case 2:
                        vector2 = new Vector2(4f, 2f);
                        break;
                    case 6:
                        vector2 = new Vector2(-2f, 0.0f);
                        break;
                }
                MTexture mask = atlasSubtextures[index];
                MirrorSurface surface = new()
                {
                    ReflectionOffset = offset + vector2
                };
                surface.OnRender = () => mask.DrawCentered(Position, surface.ReflectionColor, scale);
                Add(surface);
            }
        }

        private Vector2 GetMirrorOffset() => new(Calc.Random.Range(5, 14) * Calc.Random.Choose<int>(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose<int>(1, -1));

        private void MakeParallax(float amount)
        {
            parallax = true;
            parallaxAmount = amount;
        }

        private void MakeScaredAnimation()
        {
            Sprite sprite = new(null, null);
            image = sprite;
            sprite.AddLoop("hidden", 0.1f, textures[0]);
            sprite.Add("return", 0.1f, "idle", textures[1]);
            sprite.AddLoop("idle", 0.1f, textures[2], textures[3], textures[4], textures[5], textures[6], textures[7]);
            sprite.Add("hide", 0.1f, "hidden", textures[8], textures[9], textures[10], textures[11], textures[12]);
            sprite.Play("idle", true);
            sprite.Scale = scale;
            sprite.CenterOrigin();
            Add(sprite);
            scaredAnimal = true;
        }

        public override void Update()
        {
            if (animated && textures.Count > 1)
            {
                frame += AnimationSpeed * Engine.DeltaTime;
                frame %= textures.Count;
            }
            if (scaredAnimal)
            {
                Sprite image = this.image as Sprite;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    if (image.CurrentAnimationID == "idle" && (double) (entity.Position - Position).Length() < 32.0)
                        image.Play("hide");
                    else if (image.CurrentAnimationID == "hidden" && (double) (entity.Position - Position).Length() > 48.0)
                        image.Play("return");
                }
            }
            base.Update();
        }

        public override void Render()
        {
            Vector2 oldPosition = Position;
            if (parallax)
                Position += (Position - ((Scene as Level).Camera.Position + new Vector2(160f, 90f))) * parallaxAmount;
            if (wave != null)
                Position.Y += wave.Value * 4f;
            base.Render();
            Position = oldPosition;
        }

        public void FinalFlagTrigger()
        {
            Wiggler wiggler = Wiggler.Create(1f, 4f, v => (image as FinalFlagDecalImage).Rotation = 0.209439516f * v, true);
            Vector2 position = Position;
            position.X = Calc.Snap(position.X, 8f) - 8f;
            position.Y += 6f;
            Scene.Add(new SummitCheckpoint.ConfettiRenderer(position));
            Add(wiggler);
        }

        private class Banner : Component
        {
            public float WaveSpeed;
            public float WaveAmplitude;
            public int SliceSize;
            public float SliceSinIncrement;
            public bool EaseDown;
            public float Offset;
            public bool OnlyIfWindy;
            public float WindMultiplier = 1f;
            private float sineTimer = Calc.Random.NextFloat();
            public List<List<MTexture>> Segments;

            public Decal Decal => (Decal) Entity;

            public Banner()
                : base(true, true)
            {
            }

            public override void Update()
            {
                if (OnlyIfWindy)
                {
                    float x = (Scene as Level).Wind.X;
                    WindMultiplier = Calc.Approach(WindMultiplier, Math.Min(3f, Math.Abs(x) * 0.004f), Engine.DeltaTime * 4f);
                    if ((double) x != 0.0)
                        Offset = Math.Sign(x) * Math.Abs(Offset);
                }
                sineTimer += Engine.DeltaTime * WindMultiplier;
                base.Update();
            }

            public override void Render()
            {
                MTexture texture = Decal.textures[(int) Decal.frame];
                List<MTexture> segment = Segments[(int) Decal.frame];
                for (int index = 0; index < segment.Count; ++index)
                {
                    float num = (EaseDown ? index / (float) segment.Count : (float) (1.0 - index / (double) segment.Count)) * WindMultiplier;
                    float x = (float) (Math.Sin(sineTimer * (double) WaveSpeed + index * (double) SliceSinIncrement) * (double) num * WaveAmplitude + (double) num * Offset);
                    segment[index].Draw(Decal.Position + new Vector2(x, 0.0f), new Vector2(texture.Width / 2, texture.Height / 2 - index * SliceSize), Color.White, Decal.scale);
                }
            }
        }

        private class DecalImage : Component
        {
            public Decal Decal => (Decal) Entity;

            public DecalImage()
                : base(true, true)
            {
            }

            public override void Render() => Decal.textures[(int) Decal.frame].DrawCentered(Decal.Position, Color.White, Decal.scale);
        }

        private class FinalFlagDecalImage : Component
        {
            public float Rotation;

            public Decal Decal => (Decal) Entity;

            public FinalFlagDecalImage()
                : base(true, true)
            {
            }

            public override void Render()
            {
                MTexture texture = Decal.textures[(int) Decal.frame];
                texture.DrawJustified(Decal.Position + Vector2.UnitY * (texture.Height / 2), new Vector2(0.5f, 1f), Color.White, Decal.scale, Rotation);
            }
        }

        private class CoreSwapImage : Component
        {
            private readonly MTexture hot;
            private readonly MTexture cold;

            public Decal Decal => (Decal) Entity;

            public CoreSwapImage(MTexture hot, MTexture cold)
                : base(false, true)
            {
                this.hot = hot;
                this.cold = cold;
            }

            public override void Render() => ((Scene as Level).CoreMode == Session.CoreModes.Cold ? cold : hot).DrawCentered(Decal.Position, Color.White, Decal.scale);
        }
    }
}
