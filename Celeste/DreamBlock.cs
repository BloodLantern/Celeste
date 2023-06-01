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
        private MTexture[] particleTextures;
        private DreamBlock.DreamParticle[] particles;
        private float whiteFill;
        private float whiteHeight = 1f;
        private Vector2 shake;
        private float animTimer;
        private Shaker shaker;
        private bool fastMoving;
        private bool oneUse;
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
            this.Depth = -11000;
            this.node = node;
            this.fastMoving = fastMoving;
            this.oneUse = oneUse;
            if (below)
                this.Depth = 5000;
            this.SurfaceSoundIndex = 11;
            this.particleTextures = new MTexture[4]
            {
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
                GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
            };
        }

        public DreamBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height, data.FirstNodeNullable(new Vector2?(offset)), data.Bool(nameof (fastMoving)), data.Bool(nameof (oneUse)), data.Bool("below"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.playerHasDreamDash = this.SceneAs<Level>().Session.Inventory.DreamDash;
            if (this.playerHasDreamDash && this.node.HasValue)
            {
                Vector2 start = this.Position;
                Vector2 end = this.node.Value;
                float duration = Vector2.Distance(start, end) / 12f;
                if (this.fastMoving)
                    duration /= 3f;
                Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, duration, true);
                tween.OnUpdate = (Action<Tween>) (t =>
                {
                    if (this.Collidable)
                        this.MoveTo(Vector2.Lerp(start, end, t.Eased));
                    else
                        this.MoveToNaive(Vector2.Lerp(start, end, t.Eased));
                });
                this.Add((Component) tween);
            }
            if (!this.playerHasDreamDash)
                this.Add((Component) (this.occlude = new LightOcclude()));
            this.Setup();
        }

        public void Setup()
        {
            this.particles = new DreamBlock.DreamParticle[(int) ((double) this.Width / 8.0 * ((double) this.Height / 8.0) * 0.699999988079071)];
            for (int index = 0; index < this.particles.Length; ++index)
            {
                this.particles[index].Position = new Vector2(Calc.Random.NextFloat(this.Width), Calc.Random.NextFloat(this.Height));
                this.particles[index].Layer = Calc.Random.Choose<int>(0, 1, 1, 2, 2, 2);
                this.particles[index].TimeOffset = Calc.Random.NextFloat();
                this.particles[index].Color = Color.LightGray * (float) (0.5 + (double) this.particles[index].Layer / 2.0 * 0.5);
                if (this.playerHasDreamDash)
                {
                    switch (this.particles[index].Layer)
                    {
                        case 0:
                            this.particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
                            continue;
                        case 1:
                            this.particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
                            continue;
                        case 2:
                            this.particles[index].Color = Calc.Random.Choose<Color>(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
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
            if (this.CollideCheck((Entity) player, this.Position + Vector2.UnitX * 4f))
                vector2 = Vector2.UnitX;
            else if (this.CollideCheck((Entity) player, this.Position - Vector2.UnitX * 4f))
                vector2 = -Vector2.UnitX;
            else if (this.CollideCheck((Entity) player, this.Position + Vector2.UnitY * 4f))
                vector2 = Vector2.UnitY;
            else if (this.CollideCheck((Entity) player, this.Position - Vector2.UnitY * 4f))
                vector2 = -Vector2.UnitY;
            int num = vector2 != Vector2.Zero ? 1 : 0;
            if (!this.oneUse)
                return;
            this.OneUseDestroy();
        }

        private void OneUseDestroy()
        {
            this.Collidable = this.Visible = false;
            this.DisableStaticMovers();
            this.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (!this.playerHasDreamDash)
                return;
            this.animTimer += 6f * Engine.DeltaTime;
            this.wobbleEase += Engine.DeltaTime * 2f;
            if ((double) this.wobbleEase > 1.0)
            {
                this.wobbleEase = 0.0f;
                this.wobbleFrom = this.wobbleTo;
                this.wobbleTo = Calc.Random.NextFloat(6.28318548f);
            }
            this.SurfaceSoundIndex = 12;
        }

        public bool BlockedCheck()
        {
            TheoCrystal actor1 = this.CollideFirst<TheoCrystal>();
            if (actor1 != null && !this.TryActorWiggleUp((Entity) actor1))
                return true;
            Player actor2 = this.CollideFirst<Player>();
            return actor2 != null && !this.TryActorWiggleUp((Entity) actor2);
        }

        private bool TryActorWiggleUp(Entity actor)
        {
            bool collidable = this.Collidable;
            this.Collidable = true;
            for (int index = 1; index <= 4; ++index)
            {
                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * (float) index))
                {
                    actor.Position -= Vector2.UnitY * (float) index;
                    this.Collidable = collidable;
                    return true;
                }
            }
            this.Collidable = collidable;
            return false;
        }

        public override void Render()
        {
            Camera camera = this.SceneAs<Level>().Camera;
            if ((double) this.Right < (double) camera.Left || (double) this.Left > (double) camera.Right || (double) this.Bottom < (double) camera.Top || (double) this.Top > (double) camera.Bottom)
                return;
            Draw.Rect(this.shake.X + this.X, this.shake.Y + this.Y, this.Width, this.Height, this.playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor);
            Vector2 position = this.SceneAs<Level>().Camera.Position;
            for (int index = 0; index < this.particles.Length; ++index)
            {
                int layer = this.particles[index].Layer;
                Vector2 vector2 = this.PutInside(this.particles[index].Position + position * (float) (0.30000001192092896 + 0.25 * (double) layer));
                Color color = this.particles[index].Color;
                MTexture particleTexture;
                switch (layer)
                {
                    case 0:
                        particleTexture = this.particleTextures[3 - (int) (((double) this.particles[index].TimeOffset * 4.0 + (double) this.animTimer) % 4.0)];
                        break;
                    case 1:
                        particleTexture = this.particleTextures[1 + (int) (((double) this.particles[index].TimeOffset * 2.0 + (double) this.animTimer) % 2.0)];
                        break;
                    default:
                        particleTexture = this.particleTextures[2];
                        break;
                }
                if ((double) vector2.X >= (double) this.X + 2.0 && (double) vector2.Y >= (double) this.Y + 2.0 && (double) vector2.X < (double) this.Right - 2.0 && (double) vector2.Y < (double) this.Bottom - 2.0)
                    particleTexture.DrawCentered(vector2 + this.shake, color);
            }
            if ((double) this.whiteFill > 0.0)
                Draw.Rect(this.X + this.shake.X, this.Y + this.shake.Y, this.Width, this.Height * this.whiteHeight, Color.White * this.whiteFill);
            this.WobbleLine(this.shake + new Vector2(this.X, this.Y), this.shake + new Vector2(this.X + this.Width, this.Y), 0.0f);
            this.WobbleLine(this.shake + new Vector2(this.X + this.Width, this.Y), this.shake + new Vector2(this.X + this.Width, this.Y + this.Height), 0.7f);
            this.WobbleLine(this.shake + new Vector2(this.X + this.Width, this.Y + this.Height), this.shake + new Vector2(this.X, this.Y + this.Height), 1.5f);
            this.WobbleLine(this.shake + new Vector2(this.X, this.Y + this.Height), this.shake + new Vector2(this.X, this.Y), 2.5f);
            Draw.Rect(this.shake + new Vector2(this.X, this.Y), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(this.shake + new Vector2((float) ((double) this.X + (double) this.Width - 2.0), this.Y), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(this.shake + new Vector2(this.X, (float) ((double) this.Y + (double) this.Height - 2.0)), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
            Draw.Rect(this.shake + new Vector2((float) ((double) this.X + (double) this.Width - 2.0), (float) ((double) this.Y + (double) this.Height - 2.0)), 2f, 2f, this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor);
        }

        private Vector2 PutInside(Vector2 pos)
        {
            while ((double) pos.X < (double) this.X)
                pos.X += this.Width;
            while ((double) pos.X > (double) this.X + (double) this.Width)
                pos.X -= this.Width;
            while ((double) pos.Y < (double) this.Y)
                pos.Y += this.Height;
            while ((double) pos.Y > (double) this.Y + (double) this.Height)
                pos.Y -= this.Height;
            return pos;
        }

        private void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num1 = (to - from).Length();
            Vector2 vector2_1 = Vector2.Normalize(to - from);
            Vector2 vector2_2 = new Vector2(vector2_1.Y, -vector2_1.X);
            Color color1 = this.playerHasDreamDash ? DreamBlock.activeLineColor : DreamBlock.disabledLineColor;
            Color color2 = this.playerHasDreamDash ? DreamBlock.activeBackColor : DreamBlock.disabledBackColor;
            if ((double) this.whiteFill > 0.0)
            {
                color1 = Color.Lerp(color1, Color.White, this.whiteFill);
                color2 = Color.Lerp(color2, Color.White, this.whiteFill);
            }
            float num2 = 0.0f;
            int val1 = 16;
            for (int index = 2; (double) index < (double) num1 - 2.0; index += val1)
            {
                float num3 = this.Lerp(this.LineAmplitude(this.wobbleFrom + offset, (float) index), this.LineAmplitude(this.wobbleTo + offset, (float) index), this.wobbleEase);
                if ((double) (index + val1) >= (double) num1)
                    num3 = 0.0f;
                float num4 = Math.Min((float) val1, num1 - 2f - (float) index);
                Vector2 start = from + vector2_1 * (float) index + vector2_2 * num2;
                Vector2 end = from + vector2_1 * ((float) index + num4) + vector2_2 * num3;
                Draw.Line(start - vector2_2, end - vector2_2, color2);
                Draw.Line(start - vector2_2 * 2f, end - vector2_2 * 2f, color2);
                Draw.Line(start, end, color1);
                num2 = num3;
            }
        }

        private float LineAmplitude(float seed, float index) => (float) (Math.Sin((double) seed + (double) index / 16.0 + Math.Sin((double) seed * 2.0 + (double) index / 32.0) * 6.2831854820251465) + 1.0) * 1.5f;

        private float Lerp(float a, float b, float percent) => a + (b - a) * percent;

        public IEnumerator Activate()
        {
            DreamBlock dreamBlock = this;
            Level level = dreamBlock.SceneAs<Level>();
            yield return (object) 1f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            // ISSUE: reference to a compiler-generated method
            dreamBlock.Add(this.shaker = new Shaker(true, delegate (Vector2 t)
            {
                    this.shake = t;
            }));
            dreamBlock.shaker.Interval = 0.02f;
            dreamBlock.shaker.On = true;
            float p;
            for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
            {
                dreamBlock.whiteFill = Ease.CubeIn(p);
                yield return (object) null;
            }
            dreamBlock.shaker.On = false;
            yield return (object) 0.5f;
            dreamBlock.ActivateNoRoutine();
            dreamBlock.whiteHeight = 1f;
            dreamBlock.whiteFill = 1f;
            for (p = 1f; (double) p > 0.0; p -= Engine.DeltaTime * 0.5f)
            {
                dreamBlock.whiteHeight = p;
                if (level.OnInterval(0.1f))
                {
                    for (int index = 0; (double) index < (double) dreamBlock.Width; index += 4)
                        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(dreamBlock.X + (float) index, (float) ((double) dreamBlock.Y + (double) dreamBlock.Height * (double) dreamBlock.whiteHeight + 1.0)));
                }
                if (level.OnInterval(0.1f))
                    level.Shake();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                yield return (object) null;
            }
            while ((double) dreamBlock.whiteFill > 0.0)
            {
                dreamBlock.whiteFill -= Engine.DeltaTime * 3f;
                yield return (object) null;
            }
        }

        public void ActivateNoRoutine()
        {
            if (!this.playerHasDreamDash)
            {
                this.playerHasDreamDash = true;
                this.Setup();
                this.Remove((Component) this.occlude);
            }
            this.whiteHeight = 0.0f;
            this.whiteFill = 0.0f;
            if (this.shaker == null)
                return;
            this.shaker.On = false;
        }

        public void FootstepRipple(Vector2 position)
        {
            if (!this.playerHasDreamDash)
                return;
            DisplacementRenderer.Burst burst = (this.Scene as Level).Displacement.AddBurst(position, 0.5f, 0.0f, 40f);
            burst.WorldClipCollider = this.Collider;
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
