using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class SwitchGate : Solid
    {
        public static ParticleType P_Behind;
        public static ParticleType P_Dust;
        private MTexture[,] nineSlice;
        private Sprite icon;
        private Vector2 iconOffset;
        private Wiggler wiggler;
        private Vector2 node;
        private SoundSource openSfx;
        private bool persistent;
        private Color inactiveColor = Calc.HexToColor("5fcde4");
        private Color activeColor = Color.White;
        private Color finishColor = Calc.HexToColor("f141df");

        public SwitchGate(
            Vector2 position,
            float width,
            float height,
            Vector2 node,
            bool persistent,
            string spriteName)
            : base(position, width, height, false)
        {
            this.node = node;
            this.persistent = persistent;
            this.Add((Component) (this.icon = new Sprite(GFX.Game, "objects/switchgate/icon")));
            this.icon.Add("spin", "", 0.1f, "spin");
            this.icon.Play("spin");
            this.icon.Rate = 0.0f;
            this.icon.Color = this.inactiveColor;
            this.icon.Position = this.iconOffset = new Vector2(width / 2f, height / 2f);
            this.icon.CenterOrigin();
            this.Add((Component) (this.wiggler = Wiggler.Create(0.5f, 4f, (Action<float>) (f => this.icon.Scale = Vector2.One * (1f + f)))));
            MTexture mtexture = GFX.Game["objects/switchgate/" + spriteName];
            this.nineSlice = new MTexture[3, 3];
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                    this.nineSlice[index1, index2] = mtexture.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
            }
            this.Add((Component) (this.openSfx = new SoundSource()));
            this.Add((Component) new LightOcclude(0.5f));
        }

        public SwitchGate(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height, data.Nodes[0] + offset, data.Bool(nameof (persistent)), data.Attr("sprite", "block"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Switch.CheckLevelFlag(this.SceneAs<Level>()))
            {
                this.MoveTo(this.node);
                this.icon.Rate = 0.0f;
                this.icon.SetAnimationFrame(0);
                this.icon.Color = this.finishColor;
            }
            else
                this.Add((Component) new Coroutine(this.Sequence(this.node)));
        }

        public override void Render()
        {
            float num1 = (float) ((double) this.Collider.Width / 8.0 - 1.0);
            float num2 = (float) ((double) this.Collider.Height / 8.0 - 1.0);
            for (int val1_1 = 0; (double) val1_1 <= (double) num1; ++val1_1)
            {
                for (int val1_2 = 0; (double) val1_2 <= (double) num2; ++val1_2)
                    this.nineSlice[(double) val1_1 < (double) num1 ? Math.Min(val1_1, 1) : 2, (double) val1_2 < (double) num2 ? Math.Min(val1_2, 1) : 2].Draw(this.Position + this.Shake + new Vector2((float) (val1_1 * 8), (float) (val1_2 * 8)));
            }
            this.icon.Position = this.iconOffset + this.Shake;
            this.icon.DrawOutline();
            base.Render();
        }

        private IEnumerator Sequence(Vector2 node)
        {
            SwitchGate switchGate = this;
            Vector2 start = switchGate.Position;
            while (!Switch.Check(switchGate.Scene))
                yield return (object) null;
            if (switchGate.persistent)
                Switch.SetLevelFlag(switchGate.SceneAs<Level>());
            yield return (object) 0.1f;
            switchGate.openSfx.Play("event:/game/general/touchswitch_gate_open");
            switchGate.StartShaking(0.5f);
            while ((double) switchGate.icon.Rate < 1.0)
            {
                switchGate.icon.Color = Color.Lerp(switchGate.inactiveColor, switchGate.activeColor, switchGate.icon.Rate);
                switchGate.icon.Rate += Engine.DeltaTime * 2f;
                yield return (object) null;
            }
            yield return (object) 0.1f;
            int particleAt = 0;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, true);
            tween.OnUpdate = (Action<Tween>) (t =>
            {
                this.MoveTo(Vector2.Lerp(start, node, t.Eased));
                if (!this.Scene.OnInterval(0.1f))
                    return;
                ++particleAt;
                particleAt %= 2;
                for (int index1 = 0; (double) index1 < (double) this.Width / 8.0; ++index1)
                {
                    for (int index2 = 0; (double) index2 < (double) this.Height / 8.0; ++index2)
                    {
                        if ((index1 + index2) % 2 == particleAt)
                            this.SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind, this.Position + new Vector2((float) (index1 * 8), (float) (index2 * 8)) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                    }
                }
            });
            switchGate.Add((Component) tween);
            yield return (object) 1.8f;
            bool collidable1 = switchGate.Collidable;
            switchGate.Collidable = false;
            if ((double) node.X <= (double) start.X)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; (double) index < (double) switchGate.Height / 8.0; ++index)
                {
                    Vector2 point1 = new Vector2(switchGate.Left - 1f, switchGate.Top + 4f + (float) (index * 8));
                    Vector2 point2 = point1 + Vector2.UnitX;
                    if (switchGate.Scene.CollideCheck<Solid>(point1) && !switchGate.Scene.CollideCheck<Solid>(point2))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point1 + vector2, 3.14159274f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point1 - vector2, 3.14159274f);
                    }
                }
            }
            if ((double) node.X >= (double) start.X)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; (double) index < (double) switchGate.Height / 8.0; ++index)
                {
                    Vector2 point3 = new Vector2(switchGate.Right + 1f, switchGate.Top + 4f + (float) (index * 8));
                    Vector2 point4 = point3 - Vector2.UnitX * 2f;
                    if (switchGate.Scene.CollideCheck<Solid>(point3) && !switchGate.Scene.CollideCheck<Solid>(point4))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point3 + vector2, 0.0f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point3 - vector2, 0.0f);
                    }
                }
            }
            if ((double) node.Y <= (double) start.Y)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; (double) index < (double) switchGate.Width / 8.0; ++index)
                {
                    Vector2 point5 = new Vector2(switchGate.Left + 4f + (float) (index * 8), switchGate.Top - 1f);
                    Vector2 point6 = point5 + Vector2.UnitY;
                    if (switchGate.Scene.CollideCheck<Solid>(point5) && !switchGate.Scene.CollideCheck<Solid>(point6))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point5 + vector2, -1.57079637f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point5 - vector2, -1.57079637f);
                    }
                }
            }
            if ((double) node.Y >= (double) start.Y)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; (double) index < (double) switchGate.Width / 8.0; ++index)
                {
                    Vector2 point7 = new Vector2(switchGate.Left + 4f + (float) (index * 8), switchGate.Bottom + 1f);
                    Vector2 point8 = point7 - Vector2.UnitY * 2f;
                    if (switchGate.Scene.CollideCheck<Solid>(point7) && !switchGate.Scene.CollideCheck<Solid>(point8))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point7 + vector2, 1.57079637f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point7 - vector2, 1.57079637f);
                    }
                }
            }
            switchGate.Collidable = collidable1;
            Audio.Play("event:/game/general/touchswitch_gate_finish", switchGate.Position);
            switchGate.StartShaking(0.2f);
            while ((double) switchGate.icon.Rate > 0.0)
            {
                switchGate.icon.Color = Color.Lerp(switchGate.activeColor, switchGate.finishColor, 1f - switchGate.icon.Rate);
                switchGate.icon.Rate -= Engine.DeltaTime * 4f;
                yield return (object) null;
            }
            switchGate.icon.Rate = 0.0f;
            switchGate.icon.SetAnimationFrame(0);
            switchGate.wiggler.Start();
            bool collidable2 = switchGate.Collidable;
            switchGate.Collidable = false;
            if (!switchGate.Scene.CollideCheck<Solid>(switchGate.Center))
            {
                for (int index = 0; index < 32; ++index)
                {
                    float num = Calc.Random.NextFloat(6.28318548f);
                    switchGate.SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, switchGate.Position + switchGate.iconOffset + Calc.AngleToVector(num, 4f), num);
                }
            }
            switchGate.Collidable = collidable2;
        }
    }
}
