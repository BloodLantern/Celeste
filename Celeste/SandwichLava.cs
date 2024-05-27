using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class SandwichLava : Entity
    {
        private const float TopOffset = -160f;
        private const float Speed = 20f;
        public bool Waiting;
        private bool iceMode;
        private float startX;
        private float lerp;
        private float transitionStartY;
        private bool leaving;
        private float delay;
        private LavaRect bottomRect;
        private LavaRect topRect;
        private bool persistent;
        private SoundSource loopSfx;

        private float centerY => SceneAs<Level>().Bounds.Bottom - 10f;

        public SandwichLava(float startX)
        {
            this.startX = startX;
            Depth = -1000000;
            Collider = new ColliderList(new Hitbox(340f, 120f), new Hitbox(340f, 120f, y: -280f));
            Visible = false;
            Add(loopSfx = new SoundSource());
            Add(new PlayerCollider(OnPlayer));
            Add(new CoreModeListener(OnChangeMode));
            Add(bottomRect = new LavaRect(400f, 200f, 4));
            bottomRect.Position = new Vector2(-40f, 0.0f);
            bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
            bottomRect.SmallWaveAmplitude = 2f;
            Add(topRect = new LavaRect(400f, 200f, 4));
            topRect.Position = new Vector2(-40f, -360f);
            topRect.OnlyMode = LavaRect.OnlyModes.OnlyBottom;
            topRect.SmallWaveAmplitude = 2f;
            topRect.BigWaveAmplitude = bottomRect.BigWaveAmplitude = 2f;
            topRect.CurveAmplitude = bottomRect.CurveAmplitude = 4f;
            Add(new TransitionListener
            {
                OnOutBegin = () =>
                {
                    transitionStartY = Y;
                    if (!persistent || Scene == null || Scene.Entities.FindAll<SandwichLava>().Count > 1)
                        return;
                    Leave();
                },
                OnOut = f =>
                {
                    if (Scene != null)
                    {
                        X = (Scene as Level).Camera.X;
                        if (!leaving)
                            Y = MathHelper.Lerp(transitionStartY, centerY, f);
                    }
                    if (!(f > 0.949999988079071 & leaving))
                        return;
                    RemoveSelf();
                }
            });
        }

        public SandwichLava(EntityData data, Vector2 offset)
            : this(data.Position.X + offset.X)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            X = SceneAs<Level>().Bounds.Left - 10;
            Y = centerY;
            iceMode = SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && (entity.JustRespawned || entity.X < (double) startX))
                Waiting = true;
            List<SandwichLava> all = Scene.Entities.FindAll<SandwichLava>();
            bool flag = false;
            if (!persistent && all.Count >= 2)
            {
                SandwichLava sandwichLava = all[0] == this ? all[1] : all[0];
                if (!sandwichLava.leaving)
                {
                    sandwichLava.startX = startX;
                    sandwichLava.Waiting = true;
                    RemoveSelf();
                    flag = true;
                }
            }
            if (flag)
                return;
            persistent = true;
            Tag = (int) Tags.Persistent;
            if ((scene as Level).LastIntroType != Player.IntroTypes.Respawn)
            {
                topRect.Position.Y -= 60f;
                bottomRect.Position.Y += 60f;
            }
            else
                Visible = true;
            loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1f : 0.0f);
            loopSfx.Position = new Vector2(Width / 2f, 0.0f);
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            iceMode = mode == Session.CoreModes.Cold;
            loopSfx.Param("room_state", iceMode ? 1f : 0.0f);
        }

        private void OnPlayer(Player player)
        {
            if (Waiting)
                return;
            if (SaveData.Instance.Assists.Invincible)
            {
                if (delay > 0.0)
                    return;
                int num = player.Y > Y + (double) bottomRect.Position.Y - 32.0 ? 1 : -1;
                float from = Y;
                float to = Y + num * 48;
                player.Speed.Y = -num * 200;
                if (num > 0)
                    player.RefillDash();
                Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, t => Y = MathHelper.Lerp(from, to, t.Eased));
                delay = 0.5f;
                loopSfx.Param("rising", 0.0f);
                Audio.Play("event:/game/general/assist_screenbottom", player.Position);
            }
            else
                player.Die(-Vector2.UnitY);
        }

        public void Leave()
        {
            AddTag((int) Tags.TransitionUpdate);
            leaving = true;
            Collidable = false;
            Alarm.Set(this, 2f, () => RemoveSelf());
        }

        public override void Update()
        {
            X = (Scene as Level).Camera.X;
            delay -= Engine.DeltaTime;
            base.Update();
            Visible = true;
            if (Waiting)
            {
                Y = Calc.Approach(Y, centerY, 128f * Engine.DeltaTime);
                loopSfx.Param("rising", 0.0f);
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.X >= (double) startX && !entity.JustRespawned && entity.StateMachine.State != 11)
                    Waiting = false;
            }
            else if (!leaving && delay <= 0.0)
            {
                loopSfx.Param("rising", 1f);
                if (iceMode)
                    Y += 20f * Engine.DeltaTime;
                else
                    Y -= 20f * Engine.DeltaTime;
            }
            topRect.Position.Y = Calc.Approach(topRect.Position.Y, (float) (-160.0 - topRect.Height + (leaving ? -512.0 : 0.0)), (leaving ? 256f : 64f) * Engine.DeltaTime);
            bottomRect.Position.Y = Calc.Approach(bottomRect.Position.Y, leaving ? 512f : 0.0f, (leaving ? 256f : 64f) * Engine.DeltaTime);
            lerp = Calc.Approach(lerp, iceMode ? 1f : 0.0f, Engine.DeltaTime * 4f);
            bottomRect.SurfaceColor = Color.Lerp(RisingLava.Hot[0], RisingLava.Cold[0], lerp);
            bottomRect.EdgeColor = Color.Lerp(RisingLava.Hot[1], RisingLava.Cold[1], lerp);
            bottomRect.CenterColor = Color.Lerp(RisingLava.Hot[2], RisingLava.Cold[2], lerp);
            bottomRect.Spikey = lerp * 5f;
            bottomRect.UpdateMultiplier = (float) ((1.0 - lerp) * 2.0);
            bottomRect.Fade = iceMode ? 128f : 32f;
            topRect.SurfaceColor = bottomRect.SurfaceColor;
            topRect.EdgeColor = bottomRect.EdgeColor;
            topRect.CenterColor = bottomRect.CenterColor;
            topRect.Spikey = bottomRect.Spikey;
            topRect.UpdateMultiplier = bottomRect.UpdateMultiplier;
            topRect.Fade = bottomRect.Fade;
        }
    }
}
