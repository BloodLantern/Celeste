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
            Add(icon = new Sprite(GFX.Game, "objects/switchgate/icon"));
            icon.Add("spin", "", 0.1f, "spin");
            icon.Play("spin");
            icon.Rate = 0.0f;
            icon.Color = inactiveColor;
            icon.Position = iconOffset = new Vector2(width / 2f, height / 2f);
            icon.CenterOrigin();
            Add(wiggler = Wiggler.Create(0.5f, 4f, f => icon.Scale = Vector2.One * (1f + f)));
            MTexture mtexture = GFX.Game["objects/switchgate/" + spriteName];
            nineSlice = new MTexture[3, 3];
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                    nineSlice[index1, index2] = mtexture.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
            }
            Add(openSfx = new SoundSource());
            Add(new LightOcclude(0.5f));
        }

        public SwitchGate(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Bool(nameof (persistent)), data.Attr("sprite", "block"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Switch.CheckLevelFlag(SceneAs<Level>()))
            {
                MoveTo(node);
                icon.Rate = 0.0f;
                icon.SetAnimationFrame(0);
                icon.Color = finishColor;
            }
            else
                Add(new Coroutine(Sequence(node)));
        }

        public override void Render()
        {
            float num1 = (float) (Collider.Width / 8.0 - 1.0);
            float num2 = (float) (Collider.Height / 8.0 - 1.0);
            for (int val1_1 = 0; val1_1 <= (double) num1; ++val1_1)
            {
                for (int val1_2 = 0; val1_2 <= (double) num2; ++val1_2)
                    nineSlice[val1_1 < (double) num1 ? Math.Min(val1_1, 1) : 2, val1_2 < (double) num2 ? Math.Min(val1_2, 1) : 2].Draw(Position + Shake + new Vector2(val1_1 * 8, val1_2 * 8));
            }
            icon.Position = iconOffset + Shake;
            icon.DrawOutline();
            base.Render();
        }

        private IEnumerator Sequence(Vector2 node)
        {
            SwitchGate switchGate = this;
            Vector2 start = switchGate.Position;
            while (!Switch.Check(switchGate.Scene))
                yield return null;
            if (switchGate.persistent)
                Switch.SetLevelFlag(switchGate.SceneAs<Level>());
            yield return 0.1f;
            switchGate.openSfx.Play("event:/game/general/touchswitch_gate_open");
            switchGate.StartShaking(0.5f);
            while (switchGate.icon.Rate < 1.0)
            {
                switchGate.icon.Color = Color.Lerp(switchGate.inactiveColor, switchGate.activeColor, switchGate.icon.Rate);
                switchGate.icon.Rate += Engine.DeltaTime * 2f;
                yield return null;
            }
            yield return 0.1f;
            int particleAt = 0;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, true);
            tween.OnUpdate = t =>
            {
                MoveTo(Vector2.Lerp(start, node, t.Eased));
                if (!Scene.OnInterval(0.1f))
                    return;
                ++particleAt;
                particleAt %= 2;
                for (int index1 = 0; index1 < Width / 8.0; ++index1)
                {
                    for (int index2 = 0; index2 < Height / 8.0; ++index2)
                    {
                        if ((index1 + index2) % 2 == particleAt)
                            SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind, Position + new Vector2(index1 * 8, index2 * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                    }
                }
            };
            switchGate.Add(tween);
            yield return 1.8f;
            bool collidable1 = switchGate.Collidable;
            switchGate.Collidable = false;
            if (node.X <= (double) start.X)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; index < switchGate.Height / 8.0; ++index)
                {
                    Vector2 point1 = new Vector2(switchGate.Left - 1f, switchGate.Top + 4f + index * 8);
                    Vector2 point2 = point1 + Vector2.UnitX;
                    if (switchGate.Scene.CollideCheck<Solid>(point1) && !switchGate.Scene.CollideCheck<Solid>(point2))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point1 + vector2, 3.14159274f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point1 - vector2, 3.14159274f);
                    }
                }
            }
            if (node.X >= (double) start.X)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; index < switchGate.Height / 8.0; ++index)
                {
                    Vector2 point3 = new Vector2(switchGate.Right + 1f, switchGate.Top + 4f + index * 8);
                    Vector2 point4 = point3 - Vector2.UnitX * 2f;
                    if (switchGate.Scene.CollideCheck<Solid>(point3) && !switchGate.Scene.CollideCheck<Solid>(point4))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point3 + vector2, 0.0f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point3 - vector2, 0.0f);
                    }
                }
            }
            if (node.Y <= (double) start.Y)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; index < switchGate.Width / 8.0; ++index)
                {
                    Vector2 point5 = new Vector2(switchGate.Left + 4f + index * 8, switchGate.Top - 1f);
                    Vector2 point6 = point5 + Vector2.UnitY;
                    if (switchGate.Scene.CollideCheck<Solid>(point5) && !switchGate.Scene.CollideCheck<Solid>(point6))
                    {
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point5 + vector2, -1.57079637f);
                        switchGate.SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, point5 - vector2, -1.57079637f);
                    }
                }
            }
            if (node.Y >= (double) start.Y)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; index < switchGate.Width / 8.0; ++index)
                {
                    Vector2 point7 = new Vector2(switchGate.Left + 4f + index * 8, switchGate.Bottom + 1f);
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
            while (switchGate.icon.Rate > 0.0)
            {
                switchGate.icon.Color = Color.Lerp(switchGate.activeColor, switchGate.finishColor, 1f - switchGate.icon.Rate);
                switchGate.icon.Rate -= Engine.DeltaTime * 4f;
                yield return null;
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
