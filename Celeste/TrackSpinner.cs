// Decompiled with JetBrains decompiler
// Type: Celeste.TrackSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class TrackSpinner : Entity
    {
        public static readonly float[] PauseTimes = new float[3]
        {
            0.3f,
            0.2f,
            0.6f
        };
        public static readonly float[] MoveTimes = new float[3]
        {
            0.9f,
            0.4f,
            0.3f
        };
        public bool Up = true;
        public float PauseTimer;
        public TrackSpinner.Speeds Speed;
        public bool Moving = true;
        public float Angle;

        public Vector2 Start { get; private set; }

        public Vector2 End { get; private set; }

        public float Percent { get; private set; }

        public TrackSpinner(EntityData data, Vector2 offset)
        {
            Collider = new ColliderList(new Collider[2]
            {
                 new Monocle.Circle(6f),
                 new Hitbox(16f, 4f, -8f, -3f)
            });
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Start = data.Position + offset;
            End = data.Nodes[0] + offset;
            Speed = data.Enum<TrackSpinner.Speeds>("speed", TrackSpinner.Speeds.Normal);
            Angle = (Start - End).Angle();
            Percent = data.Bool("startCenter") ? 0.5f : 0.0f;
            if ((double)Percent == 1.0)
            {
                Up = false;
            }

            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Position = Vector2.Lerp(Start, End, Ease.SineInOut(Percent));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            OnTrackStart();
        }

        public override void Update()
        {
            base.Update();
            if (!Moving)
            {
                return;
            }

            if (PauseTimer > 0.0)
            {
                PauseTimer -= Engine.DeltaTime;
                if (PauseTimer > 0.0)
                {
                    return;
                }

                OnTrackStart();
            }
            else
            {
                Percent = Calc.Approach(Percent, Up ? 1f : 0.0f, Engine.DeltaTime / TrackSpinner.MoveTimes[(int)Speed]);
                UpdatePosition();
                if ((!Up || (double)Percent != 1.0) && (Up || (double)Percent != 0.0))
                {
                    return;
                }

                Up = !Up;
                PauseTimer = TrackSpinner.PauseTimes[(int)Speed];
                OnTrackEnd();
            }
        }

        public virtual void OnPlayer(Player player)
        {
            if (player.Die((player.Position - Position).SafeNormalize()) == null)
            {
                return;
            }

            Moving = false;
        }

        public virtual void OnTrackStart()
        {
        }

        public virtual void OnTrackEnd()
        {
        }

        public enum Speeds
        {
            Slow,
            Normal,
            Fast,
        }
    }
}
