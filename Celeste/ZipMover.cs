using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class ZipMover : Solid
    {
        public static ParticleType P_Scrape;
        public static ParticleType P_Sparks;
        private Themes theme;
        private MTexture[,] edges = new MTexture[3, 3];
        private Sprite streetlight;
        private BloomPoint bloom;
        private ZipMoverPathRenderer pathRenderer;
        private List<MTexture> innerCogs;
        private MTexture temp = new MTexture();
        private bool drawBlackBorder;
        private Vector2 start;
        private Vector2 target;
        private float percent;
        private static readonly Color ropeColor = Calc.HexToColor("663931");
        private static readonly Color ropeLightColor = Calc.HexToColor("9b6157");
        private SoundSource sfx = new SoundSource();

        public ZipMover(
            Vector2 position,
            int width,
            int height,
            Vector2 target,
            Themes theme)
            : base(position, width, height, false)
        {
            Depth = -9999;
            start = Position;
            this.target = target;
            this.theme = theme;
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            string path;
            string id;
            string key;
            if (theme == Themes.Moon)
            {
                path = "objects/zipmover/moon/light";
                id = "objects/zipmover/moon/block";
                key = "objects/zipmover/moon/innercog";
                drawBlackBorder = false;
            }
            else
            {
                path = "objects/zipmover/light";
                id = "objects/zipmover/block";
                key = "objects/zipmover/innercog";
                drawBlackBorder = true;
            }
            innerCogs = GFX.Game.GetAtlasSubtextures(key);
            Add(streetlight = new Sprite(GFX.Game, path));
            streetlight.Add("frames", "", 1f);
            streetlight.Play("frames");
            streetlight.Active = false;
            streetlight.SetAnimationFrame(1);
            streetlight.Position = new Vector2((float) (Width / 2.0 - streetlight.Width / 2.0), 0.0f);
            Add(bloom = new BloomPoint(1f, 6f));
            bloom.Position = new Vector2(Width / 2f, 4f);
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                    edges[index1, index2] = GFX.Game[id].GetSubtexture(index1 * 8, index2 * 8, 8, 8);
            }
            SurfaceSoundIndex = 7;
            sfx.Position = new Vector2(Width, Height) / 2f;
            Add(sfx);
        }

        public ZipMover(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum<Themes>(nameof (theme)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(pathRenderer = new ZipMoverPathRenderer(this));
        }

        public override void Removed(Scene scene)
        {
            scene.Remove(pathRenderer);
            pathRenderer = null;
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            bloom.Y = streetlight.CurrentAnimationFrame * 3;
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            Draw.Rect(X + 1f, Y + 1f, Width - 2f, Height - 2f, Color.Black);
            int num1 = 1;
            float num2 = 0.0f;
            int count = innerCogs.Count;
            for (int y = 4; y <= Height - 4.0; y += 8)
            {
                int num3 = num1;
                for (int x = 4; x <= Width - 4.0; x += 8)
                {
                    MTexture innerCog = innerCogs[(int) (mod((float) ((num2 + num1 * (double) percent * 3.1415927410125732 * 4.0) / 1.5707963705062866), 1f) * (double) count)];
                    Rectangle rectangle = new Rectangle(0, 0, innerCog.Width, innerCog.Height);
                    Vector2 zero = Vector2.Zero;
                    if (x <= 4)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else if (x >= Width - 4.0)
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (y <= 4)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else if (y >= Height - 4.0)
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    innerCog.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp).DrawCentered(Position + new Vector2(x, y) + zero, Color.White * (num1 < 0 ? 0.5f : 1f));
                    num1 = -num1;
                    num2 += 1.04719758f;
                }
                if (num3 == num1)
                    num1 = -num1;
            }
            for (int index1 = 0; index1 < Width / 8.0; ++index1)
            {
                for (int index2 = 0; index2 < Height / 8.0; ++index2)
                {
                    int index3 = index1 == 0 ? 0 : (index1 == Width / 8.0 - 1.0 ? 2 : 1);
                    int index4 = index2 == 0 ? 0 : (index2 == Height / 8.0 - 1.0 ? 2 : 1);
                    if (index3 != 1 || index4 != 1)
                        edges[index3, index4].Draw(new Vector2(X + index1 * 8, Y + index2 * 8));
                }
            }
            base.Render();
            Position = position;
        }

        private void ScrapeParticlesCheck(Vector2 to)
        {
            if (!Scene.OnInterval(0.03f))
                return;
            bool flag1 = to.Y != (double) ExactPosition.Y;
            bool flag2 = to.X != (double) ExactPosition.X;
            if (flag1 && !flag2)
            {
                int num1 = Math.Sign(to.Y - ExactPosition.Y);
                Vector2 vector2 = num1 != 1 ? TopLeft : BottomLeft;
                int num2 = 4;
                if (num1 == 1)
                    num2 = Math.Min((int) Height - 12, 20);
                int num3 = (int) Height;
                if (num1 == -1)
                    num3 = Math.Max(16, (int) Height - 16);
                if (Scene.CollideCheck<Solid>(vector2 + new Vector2(-2f, num1 * -2)))
                {
                    for (int index = num2; index < num3; index += 8)
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2(0.0f, index + num1 * 2f), num1 == 1 ? -0.7853982f : 0.7853982f);
                }
                if (!Scene.CollideCheck<Solid>(vector2 + new Vector2(Width + 2f, num1 * -2)))
                    return;
                for (int index = num2; index < num3; index += 8)
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopRight + new Vector2(-1f, index + num1 * 2f), num1 == 1 ? -2.3561945f : 2.3561945f);
            }
            else
            {
                if (!flag2 || flag1)
                    return;
                int num4 = Math.Sign(to.X - ExactPosition.X);
                Vector2 vector2 = num4 != 1 ? TopLeft : TopRight;
                int num5 = 4;
                if (num4 == 1)
                    num5 = Math.Min((int) Width - 12, 20);
                int num6 = (int) Width;
                if (num4 == -1)
                    num6 = Math.Max(16, (int) Width - 16);
                if (Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, -2f)))
                {
                    for (int index = num5; index < num6; index += 8)
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, TopLeft + new Vector2(index + num4 * 2f, -1f), num4 == 1 ? 2.3561945f : 0.7853982f);
                }
                if (!Scene.CollideCheck<Solid>(vector2 + new Vector2(num4 * -2, Height + 2f)))
                    return;
                for (int index = num5; index < num6; index += 8)
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, BottomLeft + new Vector2(index + num4 * 2f, 0.0f), num4 == 1 ? -2.3561945f : -0.7853982f);
            }
        }

        private IEnumerator Sequence()
        {
            ZipMover zipMover = this;
            Vector2 start = zipMover.Position;
            while (true)
            {
                while (!zipMover.HasPlayerRider())
                    yield return null;
                zipMover.sfx.Play(zipMover.theme == Themes.Normal ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                zipMover.StartShaking(0.1f);
                yield return 0.1f;
                zipMover.streetlight.SetAnimationFrame(3);
                zipMover.StopPlayerRunIntoAnimation = false;
                float at = 0.0f;
                while (at < 1.0)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
                    zipMover.percent = Ease.SineIn(at);
                    Vector2 vector2 = Vector2.Lerp(start, zipMover.target, zipMover.percent);
                    zipMover.ScrapeParticlesCheck(vector2);
                    if (zipMover.Scene.OnInterval(0.1f))
                        zipMover.pathRenderer.CreateSparks();
                    zipMover.MoveTo(vector2);
                }
                zipMover.StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                zipMover.SceneAs<Level>().Shake();
                zipMover.StopPlayerRunIntoAnimation = true;
                yield return 0.5f;
                zipMover.StopPlayerRunIntoAnimation = false;
                zipMover.streetlight.SetAnimationFrame(2);
                at = 0.0f;
                while (at < 1.0)
                {
                    yield return null;
                    at = Calc.Approach(at, 1f, 0.5f * Engine.DeltaTime);
                    zipMover.percent = 1f - Ease.SineIn(at);
                    Vector2 position = Vector2.Lerp(zipMover.target, start, Ease.SineIn(at));
                    zipMover.MoveTo(position);
                }
                zipMover.StopPlayerRunIntoAnimation = true;
                zipMover.StartShaking(0.2f);
                zipMover.streetlight.SetAnimationFrame(1);
                yield return 0.5f;
            }
        }

        private float mod(float x, float m) => (x % m + m) % m;

        public enum Themes
        {
            Normal,
            Moon,
        }

        private class ZipMoverPathRenderer : Entity
        {
            public ZipMover ZipMover;
            private MTexture cog;
            private Vector2 from;
            private Vector2 to;
            private Vector2 sparkAdd;
            private float sparkDirFromA;
            private float sparkDirFromB;
            private float sparkDirToA;
            private float sparkDirToB;

            public ZipMoverPathRenderer(ZipMover zipMover)
            {
                Depth = 5000;
                ZipMover = zipMover;
                from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
                sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
                float num = (from - to).Angle();
                sparkDirFromA = num + 0.3926991f;
                sparkDirFromB = num - 0.3926991f;
                sparkDirToA = (float) (num + 3.1415927410125732 - 0.39269909262657166);
                sparkDirToB = (float) (num + 3.1415927410125732 + 0.39269909262657166);
                if (zipMover.theme == Themes.Moon)
                    cog = GFX.Game["objects/zipmover/moon/cog"];
                else
                    cog = GFX.Game["objects/zipmover/cog"];
            }

            public void CreateSparks()
            {
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
                SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
            }

            public override void Render()
            {
                DrawCogs(Vector2.UnitY, Color.Black);
                DrawCogs(Vector2.Zero);
                if (!ZipMover.drawBlackBorder)
                    return;
                Draw.Rect(new Rectangle((int) (ZipMover.X + (double) ZipMover.Shake.X - 1.0), (int) (ZipMover.Y + (double) ZipMover.Shake.Y - 1.0), (int) ZipMover.Width + 2, (int) ZipMover.Height + 2), Color.Black);
            }

            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 vector2_1 = vector.Perpendicular() * 3f;
                Vector2 vector2_2 = -vector.Perpendicular() * 4f;
                float rotation = (float) (ZipMover.percent * 3.1415927410125732 * 2.0);
                Draw.Line(from + vector2_1 + offset, to + vector2_1 + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeColor);
                Draw.Line(from + vector2_2 + offset, to + vector2_2 + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeColor);
                for (float num = (float) (4.0 - ZipMover.percent * 3.1415927410125732 * 8.0 % 4.0); num < (double) (to - from).Length(); num += 4f)
                {
                    Vector2 vector2_3 = from + vector2_1 + vector.Perpendicular() + vector * num;
                    Vector2 vector2_4 = to + vector2_2 - vector * num;
                    Draw.Line(vector2_3 + offset, vector2_3 + vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeLightColor);
                    Draw.Line(vector2_4 + offset, vector2_4 - vector * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ZipMover.ropeLightColor);
                }
                cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
                cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }
    }
}
