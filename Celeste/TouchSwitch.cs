using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class TouchSwitch : Entity
    {
        public static ParticleType P_Fire;
        public static ParticleType P_FireWhite;
        public Switch Switch;
        private SoundSource touchSfx;
        private MTexture border = GFX.Game["objects/touchswitch/container"];
        private Sprite icon = new Sprite(GFX.Game, "objects/touchswitch/icon");
        private Color inactiveColor = Calc.HexToColor("5fcde4");
        private Color activeColor = Color.White;
        private Color finishColor = Calc.HexToColor("f141df");
        private float ease;
        private Wiggler wiggler;
        private Vector2 pulse = Vector2.One;
        private float timer;
        private BloomPoint bloom;

        private Level level => (Level) Scene;

        public TouchSwitch(Vector2 position)
            : base(position)
        {
            Depth = 2000;
            Add(Switch = new Switch(false));
            Add(new PlayerCollider(OnPlayer, featherCollider: new Hitbox(30f, 30f, -15f, -15f)));
            Add(icon);
            Add(bloom = new BloomPoint(0.0f, 16f));
            bloom.Alpha = 0.0f;
            icon.Add("idle", "", 0.0f, new int[1]);
            icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
            icon.Play("spin");
            icon.Color = inactiveColor;
            icon.CenterOrigin();
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new HoldableCollider(OnHoldable, new Hitbox(20f, 20f, -10f, -10f)));
            Add(new SeekerCollider(OnSeeker, new Hitbox(24f, 24f, -12f, -12f)));
            Switch.OnActivate = () =>
            {
                wiggler.Start();
                for (int index = 0; index < 32; ++index)
                {
                    float num = Calc.Random.NextFloat(6.28318548f);
                    level.Particles.Emit(TouchSwitch.P_FireWhite, Position + Calc.AngleToVector(num, 6f), num);
                }
                icon.Rate = 4f;
            };
            Switch.OnFinish = () => ease = 0.0f;
            Switch.OnStartFinished = () =>
            {
                icon.Rate = 0.1f;
                icon.Play("idle");
                icon.Color = finishColor;
                ease = 1f;
            };
            Add(wiggler = Wiggler.Create(0.5f, 4f, v => pulse = Vector2.One * (float) (1.0 + v * 0.25)));
            Add(new VertexLight(Color.White, 0.8f, 16, 32));
            Add(touchSfx = new SoundSource());
        }

        public TouchSwitch(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public void TurnOn()
        {
            if (Switch.Activated)
                return;
            touchSfx.Play("event:/game/general/touchswitch_any");
            if (!Switch.Activate())
                return;
            SoundEmitter.Play("event:/game/general/touchswitch_last_oneshot");
            Add(new SoundSource("event:/game/general/touchswitch_last_cutoff"));
        }

        private void OnPlayer(Player player) => TurnOn();

        private void OnHoldable(Holdable h) => TurnOn();

        private void OnSeeker(Seeker seeker)
        {
            if (!SceneAs<Level>().InsideCamera(Position, 10f))
                return;
            TurnOn();
        }

        public override void Update()
        {
            timer += Engine.DeltaTime * 8f;
            ease = Calc.Approach(ease, Switch.Finished || Switch.Activated ? 1f : 0.0f, Engine.DeltaTime * 2f);
            this.icon.Color = Color.Lerp(inactiveColor, Switch.Finished ? finishColor : activeColor, ease);
            Sprite icon = this.icon;
            icon.Color *= (float) (0.5 + (Math.Sin(timer) + 1.0) / 2.0 * (1.0 - ease) * 0.5 + 0.5 * ease);
            bloom.Alpha = ease;
            if (Switch.Finished)
            {
                if (this.icon.Rate > 0.10000000149011612)
                {
                    this.icon.Rate -= 2f * Engine.DeltaTime;
                    if (this.icon.Rate <= 0.10000000149011612)
                    {
                        this.icon.Rate = 0.1f;
                        wiggler.Start();
                        this.icon.Play("idle");
                        level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                    }
                }
                else if (Scene.OnInterval(0.03f))
                {
                    Vector2 position = Position + new Vector2(0.0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
                    level.ParticlesBG.Emit(TouchSwitch.P_Fire, position);
                }
            }
            base.Update();
        }

        public override void Render()
        {
            border.DrawCentered(Position + new Vector2(0.0f, -1f), Color.Black);
            border.DrawCentered(Position, icon.Color, pulse);
            base.Render();
        }
    }
}
