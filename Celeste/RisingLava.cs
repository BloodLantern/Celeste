// Decompiled with JetBrains decompiler
// Type: Celeste.RisingLava
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class RisingLava : Entity
    {
        private const float Speed = -30f;
        private readonly bool intro;
        private bool iceMode;
        private bool waiting;
        private float lerp;
        public static Color[] Hot = new Color[3]
        {
            Calc.HexToColor("ff8933"),
            Calc.HexToColor("f25e29"),
            Calc.HexToColor("d01c01")
        };
        public static Color[] Cold = new Color[3]
        {
            Calc.HexToColor("33ffe7"),
            Calc.HexToColor("4ca2eb"),
            Calc.HexToColor("0151d0")
        };
        private readonly LavaRect bottomRect;
        private float delay;
        private readonly SoundSource loopSfx;

        public RisingLava(bool intro)
        {
            this.intro = intro;
            Depth = -1000000;
            Collider = new Hitbox(340f, 120f);
            Visible = false;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(new CoreModeListener(new Action<Session.CoreModes>(OnChangeMode)));
            Add(loopSfx = new SoundSource());
            Add(bottomRect = new LavaRect(400f, 200f, 4));
            bottomRect.Position = new Vector2(-40f, 0.0f);
            bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
            bottomRect.SmallWaveAmplitude = 2f;
        }

        public RisingLava(EntityData data, Vector2 offset)
            : this(data.Bool(nameof(intro)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            X = SceneAs<Level>().Bounds.Left - 10;
            Y = SceneAs<Level>().Bounds.Bottom + 16;
            iceMode = SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold;
            _ = loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1f : 0.0f);
            loopSfx.Position = new Vector2(Width / 2f, 0.0f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (intro)
            {
                waiting = true;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.JustRespawned)
                {
                    waiting = true;
                }
            }
            if (!intro)
            {
                return;
            }

            Visible = true;
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            iceMode = mode == Session.CoreModes.Cold;
            _ = loopSfx.Param("room_state", iceMode ? 1f : 0.0f);
        }

        private void OnPlayer(Player player)
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                if (delay > 0.0)
                {
                    return;
                }

                float from = Y;
                float to = Y + 48f;
                player.Speed.Y = -200f;
                _ = player.RefillDash();
                _ = Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, t => Y = MathHelper.Lerp(from, to, t.Eased));
                delay = 0.5f;
                _ = loopSfx.Param("rising", 0.0f);
                _ = Audio.Play("event:/game/general/assist_screenbottom", player.Position);
            }
            else
            {
                _ = player.Die(-Vector2.UnitY);
            }
        }

        public override void Update()
        {
            delay -= Engine.DeltaTime;
            X = SceneAs<Level>().Camera.X;
            Player entity = Scene.Tracker.GetEntity<Player>();
            base.Update();
            Visible = true;
            if (waiting)
            {
                _ = loopSfx.Param("rising", 0.0f);
                if (!intro && entity != null && entity.JustRespawned)
                {
                    Y = Calc.Approach(Y, entity.Y + 32f, 32f * Engine.DeltaTime);
                }

                if ((!iceMode || !intro) && (entity == null || !entity.JustRespawned))
                {
                    waiting = false;
                }
            }
            else
            {
                float num1 = SceneAs<Level>().Camera.Bottom - 12f;
                if ((double)Top > (double)num1 + 96.0)
                {
                    Top = num1 + 96f;
                }

                float num2 = (double)Top <= (double)num1 ? Calc.ClampedMap(num1 - Top, 0.0f, 32f, 1f, 0.5f) : Calc.ClampedMap(Top - num1, 0.0f, 96f, 1f, 2f);
                if (delay <= 0.0)
                {
                    _ = loopSfx.Param("rising", 1f);
                    Y += -30f * num2 * Engine.DeltaTime;
                }
            }
            lerp = Calc.Approach(lerp, iceMode ? 1f : 0.0f, Engine.DeltaTime * 4f);
            bottomRect.SurfaceColor = Color.Lerp(RisingLava.Hot[0], RisingLava.Cold[0], lerp);
            bottomRect.EdgeColor = Color.Lerp(RisingLava.Hot[1], RisingLava.Cold[1], lerp);
            bottomRect.CenterColor = Color.Lerp(RisingLava.Hot[2], RisingLava.Cold[2], lerp);
            bottomRect.Spikey = lerp * 5f;
            bottomRect.UpdateMultiplier = (float)((1.0 - lerp) * 2.0);
            bottomRect.Fade = iceMode ? 128f : 32f;
        }
    }
}
