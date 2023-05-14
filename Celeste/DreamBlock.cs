// Decompiled with JetBrains decompiler
// Type: Celeste.DreamBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked(false)]
    public class DreamBlock : Solid
    {
        private static readonly Color activeBackColor = Color.Black;
        private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");
        private static readonly Color activeLineColor = Color.White;
        private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");
        private bool playerHasDreamDash;
        private Vector2? node;
        private LightOcclude occlude;
        private readonly MTexture[] particleTextures;
        private DreamBlock.DreamParticle[] particles;
        private float whiteFill;
        private float whiteHeight = 1f;
        private Vector2 shake;
        private float animTimer;
        private Shaker shaker;
        private readonly bool fastMoving;
        private readonly bool oneUse;
        private float wobbleFrom = Calc.Random.NextFloat(6.28318548f);
        private float wobbleTo = Calc.Random.NextFloat(6.28318548f);
        private float wobbleEase;

        public DreamBlock(
            Vector2 position,
            float width,
            float height,
            Vector2? node,
            bool fastMoving,
            bool oneUse,
            bool below)
            : base(position, width, height, true)
        {
            Depth = -11000;
            this.node = node;
            this.fastMoving = fastMoving;
            this.oneUse = oneUse;
            if (below)
            {
                Depth = 5000;
            }

            SurfaceSoundIndex = 11;
            particleTextures = new MTexture[4]
            {
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
            };
        }

        public DreamBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(new Vector2?(offset)), data.Bool(nameof(fastMoving)), data.Bool(nameof(oneUse)), data.Bool("below"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;
            if (playerHasDreamDash && node.HasValue)
            {
                Vector2 start = Position;
                Vector2 end = node.Value;
                float duration = Vector2.Distance(start, end) / 12f;
                if (fastMoving)
                {
                    duration /= 3f;
                }

                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, duration, true);
                tween.OnUpdate = t =>
                {
                    if (Collidable)
                    {
                        MoveTo(Vector2.Lerp(start, end, t.Eased));
                    }
                    else
                    {
                        MoveToNaive(Vector2.Lerp(start, end, t.Eased));
                    }
                };
                Add(tween);
            }
            if (!playerHasDreamDash)
            {
                Add(occlude = new LightOcclude());
            }

            Setup();
        }

        public void Setup()
        {
            particles = new DreamBlock.DreamParticle[(int)((double)Width / 8.0 * ((double)Height / 8.0) * 0.699999988079071)];
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position = new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height));
                particles[index].Layer = Calc.Random.Choose<int>(0, 1, 1, 2, 2, 2);
                particles[index].TimeOffset = Calc.Random.NextFloat();
                particles[index].Color = Color.LightGray * (float)(0.5 + (particles[index].Layer / 2.0 * 0.5));
                if (playerHasDreamDash)
                {
                    switch (particles[index].Layer)
                    {
                        case 0:
                            particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                            continue;
                        case 1:
                            particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                            continue;
                        case 2:
                            particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        public void OnPlayerExit(Player player)
        {
            Dust.Burst(player.Position, player.Speed.Angle(), 16);
            Vector2 vector2 = Vector2.Zero;
            if (CollideCheck(player, Position + (Vector2.UnitX * 4f)))
            {
                vector2 = Vector2.UnitX;
            }
            else if (CollideCheck(player, Position - (Vector2.UnitX * 4f)))
            {
                vector2 = -Vector2.UnitX;
            }
            else if (CollideCheck(player, Position + (Vector2.UnitY * 4f)))
            {
                vector2 = Vector2.UnitY;
            }
            else if (CollideCheck(player, Position - (Vector2.UnitY * 4f)))
            {
                vector2 = -Vector2.UnitY;
            }

            _ = vector2 != Vector2.Zero ? 1 : 0;
            if (!oneUse)
            {
                return;
            }

            OneUseDestroy();
        }

        private void OneUseDestroy()
        {
            Collidable = Visible = false;
            DisableStaticMovers();
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (!playerHasDreamDash)
            {
                return;
            }

            animTimer += 6f * Engine.DeltaTime;
            wobbleEase += Engine.DeltaTime * 2f;
            if (wobbleEase > 1.0)
            {
                wobbleEase = 0.0f;
                wobbleFrom = wobbleTo;
                wobbleTo = Calc.Random.NextFloat(6.28318548f);
            }
            SurfaceSoundIndex = 12;
        }

        public bool BlockedCheck()
        {
            TheoCrystal actor1 = CollideFirst<TheoCrystal>();
            if (actor1 != null && !TryActorWiggleUp(actor1))
            {
                return true;
            }

            Player actor2 = CollideFirst<Player>();
            return actor2 != null && !TryActorWiggleUp(actor2);
        }

        private bool TryActorWiggleUp(Entity actor)
        {
            bool collidable = Collidable;
            Collidable = true;
            for (int index = 1; index <= 4; ++index)
            {
                if (!actor.CollideCheck<Solid>(actor.Position - (Vector2.UnitY * index)))
                {
                    actor.Position -= Vector2.UnitY * index;
                    Collidable = collidable;
                    return true;
                }
            }
            Collidable = collidable;
            return false;
        }

        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            if ((double)Right < (double)camera.Left || (double)Left > (double)camera.Right || (double)Bottom < (double)camera.Top || (double)Top > (double)camera.Bottom)
            {
                return;
            }

            Draw.Rect(shake.X + X, shake.Y + Y, Width, Height, playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor);
            Vector2 position = SceneAs<Level>().Camera.Position;
            for (int index = 0; index < particles.Length; ++index)
            {
                int layer = particles[index].Layer;
                Vector2 vector2 = PutInside(particles[index].Position + (position * (float)(0.30000001192092896 + (0.25 * layer))));
                Color color = particles[index].Color;
                MTexture particleTexture = layer switch
                {
                    0 => particleTextures[3 - (int)(((particles[index].TimeOffset * 4.0) + animTimer) % 4.0)],
                    1 => particleTextures[1 + (int)(((particles[index].TimeOffset * 2.0) + animTimer) % 2.0)],
                    _ => particleTextures[2],
                };
                if (vector2.X >= (double)X + 2.0 && vector2.Y >= (double)Y + 2.0 && vector2.X < (double)Right - 2.0 && vector2.Y < (double)Bottom - 2.0)
                {
                    particleTexture.DrawCentered(vector2 + shake, color);
                }
            }
            if (whiteFill > 0.0)
            {
                Draw.Rect(X + shake.X, Y + shake.Y, Width, Height * whiteHeight, Color.White * whiteFill);
            }

            WobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0.0f);
            WobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f);
            WobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f);
            WobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f);
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(shake + new Vector2((float)((double)X + (double)Width - 2.0), Y), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(shake + new Vector2(X, (float)((double)Y + (double)Height - 2.0)), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(shake + new Vector2((float)((double)X + (double)Width - 2.0), (float)((double)Y + (double)Height - 2.0)), 2f, 2f, playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        }

        private Vector2 PutInside(Vector2 pos)
        {
            while (pos.X < (double)X)
            {
                pos.X += Width;
            }

            while (pos.X > (double)X + (double)Width)
            {
                pos.X -= Width;
            }

            while (pos.Y < (double)Y)
            {
                pos.Y += Height;
            }

            while (pos.Y > (double)Y + (double)Height)
            {
                pos.Y -= Height;
            }

            return pos;
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num1 = (to - from).Length();
            Vector2 vector2_1 = Vector2.Normalize(to - from);
            Vector2 vector2_2 = new(vector2_1.Y, -vector2_1.X);
            Color color1 = playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor;
            Color color2 = playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor;
            if (whiteFill > 0.0)
            {
                color1 = Color.Lerp(color1, Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }
            float num2 = 0.0f;
            int val1 = 16;
            for (int index = 2; index < (double)num1 - 2.0; index += val1)
            {
                float num3 = Lerp(LineAmplitude(wobbleFrom + offset, index), LineAmplitude(wobbleTo + offset, index), wobbleEase);
                if (index + val1 >= (double)num1)
                {
                    num3 = 0.0f;
                }

                float num4 = Math.Min(val1, num1 - 2f - index);
                Vector2 start = from + (vector2_1 * index) + (vector2_2 * num2);
                Vector2 end = from + (vector2_1 * (index + num4)) + (vector2_2 * num3);
                Draw.Line(start - vector2_2, end - vector2_2, color2);
                Draw.Line(start - (vector2_2 * 2f), end - (vector2_2 * 2f), color2);
                Draw.Line(start, end, color1);
                num2 = num3;
            }
        }

        private float LineAmplitude(float seed, float index)
        {
            return (float)(Math.Sin((double)seed + ((double)index / 16.0) + (Math.Sin(((double)seed * 2.0) + ((double)index / 32.0)) * 6.2831854820251465)) + 1.0) * 1.5f;
        }

        private float Lerp(float a, float b, float percent)
        {
            return a + ((b - a) * percent);
        }

        public IEnumerator Activate()
        {
            DreamBlock dreamBlock = this;
            Level level = dreamBlock.SceneAs<Level>();
            yield return 1f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            // ISSUE: reference to a compiler-generated method
            dreamBlock.Add(shaker = new Shaker(true, delegate (Vector2 t)
            {
                shake = t;
            }));
            dreamBlock.shaker.Interval = 0.02f;
            dreamBlock.shaker.On = true;
            float p;
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime)
            {
                dreamBlock.whiteFill = Ease.CubeIn(p);
                yield return null;
            }
            dreamBlock.shaker.On = false;
            yield return 0.5f;
            dreamBlock.ActivateNoRoutine();
            dreamBlock.whiteHeight = 1f;
            dreamBlock.whiteFill = 1f;
            for (p = 1f; (double)p > 0.0; p -= Engine.DeltaTime * 0.5f)
            {
                dreamBlock.whiteHeight = p;
                if (level.OnInterval(0.1f))
                {
                    for (int index = 0; index < (double)dreamBlock.Width; index += 4)
                    {
                        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(dreamBlock.X + index, (float)((double)dreamBlock.Y + ((double)dreamBlock.Height * dreamBlock.whiteHeight) + 1.0)));
                    }
                }
                if (level.OnInterval(0.1f))
                {
                    level.Shake();
                }

                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                yield return null;
            }
            while (dreamBlock.whiteFill > 0.0)
            {
                dreamBlock.whiteFill -= Engine.DeltaTime * 3f;
                yield return null;
            }
        }

        public void ActivateNoRoutine()
        {
            if (!playerHasDreamDash)
            {
                playerHasDreamDash = true;
                Setup();
                Remove(occlude);
            }
            whiteHeight = 0.0f;
            whiteFill = 0.0f;
            if (shaker == null)
            {
                return;
            }

            shaker.On = false;
        }

        public void FootstepRipple(Vector2 position)
        {
            if (!playerHasDreamDash)
            {
                return;
            }

            DisplacementRenderer.Burst burst = (Scene as Level).Displacement.AddBurst(position, 0.5f, 0.0f, 40f);
            burst.WorldClipCollider = Collider;
            burst.WorldClipPadding = 1;
        }

        private struct DreamParticle
        {
            public Vector2 Position;
            public int Layer;
            public Color Color;
            public float TimeOffset;
        }
    }
}
