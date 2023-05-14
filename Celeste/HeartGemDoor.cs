// Decompiled with JetBrains decompiler
// Type: Celeste.HeartGemDoor
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class HeartGemDoor : Entity
    {
        private const string OpenedFlag = "opened_heartgem_door_";
        public static ParticleType P_Shimmer;
        public static ParticleType P_Slice;
        public readonly int Requires;
        public int Size;
        private readonly float openDistance;
        private float openPercent;
        private Solid TopSolid;
        private Solid BotSolid;
        private float offset;
        private Vector2 mist;
        private readonly MTexture temp = new();
        private readonly List<MTexture> icon;
        private readonly HeartGemDoor.Particle[] particles = new HeartGemDoor.Particle[50];
        private bool startHidden;
        private float heartAlpha = 1f;

        public int HeartGems => SaveData.Instance.CheatMode ? Requires : SaveData.Instance.TotalHeartGems;

        public float Counter { get; private set; }

        public bool Opened { get; private set; }

        private float openAmount => openPercent * openDistance;

        public HeartGemDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Requires = data.Int("requires");
            Add(new CustomBloom(new Action(RenderBloom)));
            Size = data.Width;
            openDistance = 32f;
            Vector2? nullable = data.FirstNodeNullable(new Vector2?(offset));
            if (nullable.HasValue)
            {
                openDistance = Math.Abs(nullable.Value.Y - Y);
            }

            icon = GFX.Game.GetAtlasSubtextures("objects/heartdoor/icon");
            startHidden = data.Bool(nameof(startHidden));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level1 = scene as Level;
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position = new Vector2(Calc.Random.NextFloat(Size), Calc.Random.NextFloat(level1.Bounds.Height));
                particles[index].Speed = Calc.Random.Range(4, 12);
                particles[index].Color = Color.White * Calc.Random.Range(0.2f, 0.6f);
            }
            Level level2 = level1;
            double x = (double)X;
            Rectangle bounds = level1.Bounds;
            double y1 = bounds.Top - 32;
            Vector2 position1 = new((float)x, (float)y1);
            double size1 = Size;
            double y2 = (double)Y;
            bounds = level1.Bounds;
            double top = bounds.Top;
            double height1 = y2 - top + 32.0;
            Solid solid1 = TopSolid = new Solid(position1, (float)size1, (float)height1, true);
            level2.Add(solid1);
            TopSolid.SurfaceSoundIndex = 32;
            TopSolid.SquishEvenInAssistMode = true;
            TopSolid.EnableAssistModeChecks = false;
            Level level3 = level1;
            Vector2 position2 = new(X, Y);
            double size2 = Size;
            bounds = level1.Bounds;
            double height2 = bounds.Bottom - (double)Y + 32.0;
            Solid solid2 = BotSolid = new Solid(position2, (float)size2, (float)height2, true);
            level3.Add(solid2);
            BotSolid.SurfaceSoundIndex = 32;
            BotSolid.SquishEvenInAssistMode = true;
            BotSolid.EnableAssistModeChecks = false;
            if ((Scene as Level).Session.GetFlag("opened_heartgem_door_" + Requires))
            {
                Opened = true;
                Visible = true;
                openPercent = 1f;
                Counter = Requires;
                TopSolid.Y -= openDistance;
                BotSolid.Y += openDistance;
            }
            else
            {
                Add(new Coroutine(Routine()));
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Opened)
            {
                Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
            }
            else
            {
                if (!startHidden)
                {
                    return;
                }

                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && (double)entity.X > (double)X)
                {
                    startHidden = false;
                    Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
                }
                else
                {
                    Visible = false;
                }
            }
        }

        private IEnumerator Routine()
        {
            HeartGemDoor heartGemDoor = this;
            Level level = heartGemDoor.Scene as Level;
            float topTo;
            float botTo;
            float topFrom;
            float botFrom;
            float p;
            if (heartGemDoor.startHidden)
            {
                Player entity1;
                do
                {
                    yield return null;
                    entity1 = heartGemDoor.Scene.Tracker.GetEntity<Player>();
                }
                while (entity1 == null || (double)Math.Abs(entity1.X - heartGemDoor.Center.X) >= 100.0);
                _ = Audio.Play("event:/new_content/game/10_farewell/heart_door", heartGemDoor.Position);
                heartGemDoor.Visible = true;
                heartGemDoor.heartAlpha = 0.0f;
                topTo = heartGemDoor.TopSolid.Y;
                botTo = heartGemDoor.BotSolid.Y;
                topFrom = heartGemDoor.TopSolid.Y -= 240f;
                botFrom = heartGemDoor.BotSolid.Y -= 240f;
                for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 1.2f)
                {
                    float num = Ease.CubeIn(p);
                    heartGemDoor.TopSolid.MoveToY(topFrom + ((topTo - topFrom) * num));
                    heartGemDoor.BotSolid.MoveToY(botFrom + ((botTo - botFrom) * num));
                    DashBlock dashBlock = heartGemDoor.Scene.CollideFirst<DashBlock>(heartGemDoor.BotSolid.Collider.Bounds);
                    if (dashBlock != null)
                    {
                        level.Shake(0.5f);
                        Celeste.Freeze(0.1f);
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        dashBlock.Break(heartGemDoor.BotSolid.BottomCenter, new Vector2(0.0f, 1f), playDebrisSound: false);
                        Player entity2 = heartGemDoor.Scene.Tracker.GetEntity<Player>();
                        if (entity2 != null && (double)Math.Abs(entity2.X - heartGemDoor.Center.X) < 40.0)
                        {
                            entity2.PointBounce(entity2.Position + (Vector2.UnitX * 8f));
                        }
                    }
                    yield return null;
                }
                level.Shake(0.5f);
                Celeste.Freeze(0.1f);
                heartGemDoor.TopSolid.Y = topTo;
                heartGemDoor.BotSolid.Y = botTo;
                while (heartGemDoor.heartAlpha < 1.0)
                {
                    heartGemDoor.heartAlpha = Calc.Approach(heartGemDoor.heartAlpha, 1f, Engine.DeltaTime * 2f);
                    yield return null;
                }
                yield return 0.6f;
            }
            while (!heartGemDoor.Opened && (double)heartGemDoor.Counter < heartGemDoor.Requires)
            {
                Player entity = heartGemDoor.Scene.Tracker.GetEntity<Player>();
                if (entity != null && (double)Math.Abs(entity.X - heartGemDoor.Center.X) < 80.0 && (double)entity.X < (double)heartGemDoor.X)
                {
                    if ((double)heartGemDoor.Counter == 0.0 && heartGemDoor.HeartGems > 0)
                    {
                        _ = Audio.Play("event:/game/09_core/frontdoor_heartfill", heartGemDoor.Position);
                    }

                    if (heartGemDoor.HeartGems < heartGemDoor.Requires)
                    {
                        level.Session.SetFlag("granny_door");
                    }

                    int counter1 = (int)heartGemDoor.Counter;
                    int target = Math.Min(heartGemDoor.HeartGems, heartGemDoor.Requires);
                    heartGemDoor.Counter = Calc.Approach(heartGemDoor.Counter, target, (float)((double)Engine.DeltaTime * heartGemDoor.Requires * 0.800000011920929));
                    int counter2 = (int)heartGemDoor.Counter;
                    if (counter1 != counter2)
                    {
                        yield return 0.1f;
                        if ((double)heartGemDoor.Counter < target)
                        {
                            _ = Audio.Play("event:/game/09_core/frontdoor_heartfill", heartGemDoor.Position);
                        }
                    }
                }
                else
                {
                    heartGemDoor.Counter = Calc.Approach(heartGemDoor.Counter, 0.0f, (float)((double)Engine.DeltaTime * heartGemDoor.Requires * 4.0));
                }

                yield return null;
            }
            yield return 0.5f;
            heartGemDoor.Scene.Add(new HeartGemDoor.WhiteLine(heartGemDoor.Position, heartGemDoor.Size));
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White * 0.5f);
            _ = Audio.Play("event:/game/09_core/frontdoor_unlock", heartGemDoor.Position);
            heartGemDoor.Opened = true;
            level.Session.SetFlag("opened_heartgem_door_" + heartGemDoor.Requires);
            heartGemDoor.offset = 0.0f;
            yield return 0.6f;
            botFrom = heartGemDoor.TopSolid.Y;
            topFrom = heartGemDoor.TopSolid.Y - heartGemDoor.openDistance;
            botTo = heartGemDoor.BotSolid.Y;
            topTo = heartGemDoor.BotSolid.Y + heartGemDoor.openDistance;
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime)
            {
                level.Shake();
                heartGemDoor.openPercent = Ease.CubeIn(p);
                heartGemDoor.TopSolid.MoveToY(MathHelper.Lerp(botFrom, topFrom, heartGemDoor.openPercent));
                heartGemDoor.BotSolid.MoveToY(MathHelper.Lerp(botTo, topTo, heartGemDoor.openPercent));
                if ((double)p >= 0.40000000596046448 && level.OnInterval(0.1f))
                {
                    for (int index = 4; index < heartGemDoor.Size; index += 4)
                    {
                        level.ParticlesBG.Emit(HeartGemDoor.P_Shimmer, 1, new Vector2((float)((double)heartGemDoor.TopSolid.Left + index + 1.0), heartGemDoor.TopSolid.Bottom - 2f), new Vector2(2f, 2f), -1.57079637f);
                        level.ParticlesBG.Emit(HeartGemDoor.P_Shimmer, 1, new Vector2((float)((double)heartGemDoor.BotSolid.Left + index + 1.0), heartGemDoor.BotSolid.Top + 2f), new Vector2(2f, 2f), 1.57079637f);
                    }
                }
                yield return null;
            }
            heartGemDoor.TopSolid.MoveToY(topFrom);
            heartGemDoor.BotSolid.MoveToY(topTo);
            heartGemDoor.openPercent = 1f;
        }

        public override void Update()
        {
            base.Update();
            if (Opened)
            {
                return;
            }

            offset += 12f * Engine.DeltaTime;
            mist.X -= 4f * Engine.DeltaTime;
            mist.Y -= 24f * Engine.DeltaTime;
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position.Y += particles[index].Speed * Engine.DeltaTime;
            }
        }

        public void RenderBloom()
        {
            if (Opened || !Visible)
            {
                return;
            }

            DrawBloom(new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)((double)TopSolid.Height + (double)BotSolid.Height)));
        }

        private void DrawBloom(Rectangle bounds)
        {
            Draw.Rect(bounds.Left - 4, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
            Draw.Rect(bounds.Left - 2, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
            Draw.Rect(bounds, Color.White * 0.75f);
            Draw.Rect(bounds.Right, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
            Draw.Rect(bounds.Right + 2, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
        }

        private void DrawMist(Rectangle bounds, Vector2 mist)
        {
            Color color = Color.White * 0.6f;
            MTexture mtexture = GFX.Game["objects/heartdoor/mist"];
            int num1 = mtexture.Width / 2;
            int num2 = mtexture.Height / 2;
            for (int index1 = 0; index1 < bounds.Width; index1 += num1)
            {
                for (int index2 = 0; index2 < bounds.Height; index2 += num2)
                {
                    _ = mtexture.GetSubtexture((int)Mod(mist.X, num1), (int)Mod(mist.Y, num2), Math.Min(num1, bounds.Width - index1), Math.Min(num2, bounds.Height - index2), temp);
                    temp.Draw(new Vector2(bounds.X + index1, bounds.Y + index2), Vector2.Zero, color);
                }
            }
        }

        private void DrawInterior(Rectangle bounds)
        {
            Draw.Rect(bounds, Calc.HexToColor("18668f"));
            DrawMist(bounds, mist);
            DrawMist(bounds, new Vector2(mist.Y, mist.X) * 1.5f);
            Vector2 vector2_1 = (Scene as Level).Camera.Position;
            if (Opened)
            {
                vector2_1 = Vector2.Zero;
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 vector2_2 = particles[index].Position + (vector2_1 * 0.2f);
                vector2_2.X = Mod(vector2_2.X, bounds.Width);
                vector2_2.Y = Mod(vector2_2.Y, bounds.Height);
                Draw.Pixel.Draw(new Vector2(bounds.X, bounds.Y) + vector2_2, Vector2.Zero, particles[index].Color);
            }
        }

        private void DrawEdges(Rectangle bounds, Color color)
        {
            MTexture mtexture1 = GFX.Game["objects/heartdoor/edge"];
            MTexture mtexture2 = GFX.Game["objects/heartdoor/top"];
            int height = (int)(offset % 8.0);
            if (height > 0)
            {
                _ = mtexture1.GetSubtexture(0, 8 - height, 7, height, temp);
                temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
                temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
            }
            for (int index = height; index < bounds.Height; index += 8)
            {
                _ = mtexture1.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - index), temp);
                temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top + index), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
                temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top + index), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
            }
            for (int index = 0; index < bounds.Width; index += 8)
            {
                mtexture2.DrawCentered(new Vector2(bounds.Left + 4 + index, bounds.Top + 4), color);
                mtexture2.DrawCentered(new Vector2(bounds.Left + 4 + index, bounds.Bottom - 4), color, new Vector2(1f, -1f));
            }
        }

        public override void Render()
        {
            Color color1 = Opened ? Color.White * 0.25f : Color.White;
            if (!Opened && TopSolid.Visible && BotSolid.Visible)
            {
                Rectangle bounds = new((int)TopSolid.X, (int)TopSolid.Y, Size, (int)((double)TopSolid.Height + (double)BotSolid.Height));
                DrawInterior(bounds);
                DrawEdges(bounds, color1);
            }
            else
            {
                if (TopSolid.Visible)
                {
                    Rectangle bounds = new((int)TopSolid.X, (int)TopSolid.Y, Size, (int)TopSolid.Height);
                    DrawInterior(bounds);
                    DrawEdges(bounds, color1);
                }
                if (BotSolid.Visible)
                {
                    Rectangle bounds = new((int)BotSolid.X, (int)BotSolid.Y, Size, (int)BotSolid.Height);
                    DrawInterior(bounds);
                    DrawEdges(bounds, color1);
                }
            }
            if (heartAlpha <= 0.0)
            {
                return;
            }

            float num1 = 12f;
            int num2 = (int)((Size - 8) / (double)num1);
            int num3 = (int)Math.Ceiling(Requires / (double)num2);
            Color color2 = color1 * heartAlpha;
            for (int index1 = 0; index1 < num3; ++index1)
            {
                int num4 = (index1 + 1) * num2 < Requires ? num2 : Requires - (index1 * num2);
                Vector2 vector2 = new Vector2(X + (Size * 0.5f), Y) + (new Vector2((float)((-num4 / 2.0) + 0.5), (float)((-num3 / 2.0) + index1 + 0.5)) * num1);
                if (Opened)
                {
                    if (index1 < num3 / 2)
                    {
                        vector2.Y -= openAmount + 8f;
                    }
                    else
                    {
                        vector2.Y += openAmount + 8f;
                    }
                }
                for (int index2 = 0; index2 < num4; ++index2)
                {
                    int min = (index1 * num2) + index2;
                    icon[(int)((double)Ease.CubeIn(Calc.ClampedMap(Counter, min, min + 1f)) * (icon.Count - 1))].DrawCentered(vector2 + new Vector2(index2 * num1, 0.0f), color2);
                }
            }
        }

        private float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public Color Color;
        }

        private class WhiteLine : Entity
        {
            private float fade = 1f;
            private readonly int blockSize;

            public WhiteLine(Vector2 origin, int blockSize)
                : base(origin)
            {
                Depth = -1000000;
                this.blockSize = blockSize;
            }

            public override void Update()
            {
                base.Update();
                fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime);
                if (fade > 0.0)
                {
                    return;
                }

                RemoveSelf();
                Level level = SceneAs<Level>();
                for (float left = (int)level.Camera.Left; (double)left < (double)level.Camera.Right; ++left)
                {
                    if ((double)left < (double)X || (double)left >= (double)X + blockSize)
                    {
                        level.Particles.Emit(HeartGemDoor.P_Slice, new Vector2(left, Y));
                    }
                }
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                float height = Math.Max(1f, 4f * fade);
                Draw.Rect(position.X - 10f, Y - (height / 2f), 340f, height, Color.White);
            }
        }
    }
}
