// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerDashAssist
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class PlayerDashAssist : Entity
    {
        public float Direction;
        public float Scale;
        public Vector2 Offset;
        private readonly List<MTexture> images;
        private EventInstance snapshot;
        private float timer;
        private bool paused;
        private int lastIndex;

        public PlayerDashAssist()
        {
            Tag = (int)Tags.Global;
            Depth = -1000000;
            Visible = false;
            images = GFX.Game.GetAtlasSubtextures("util/dasharrow/dasharrow");
        }

        public override void Update()
        {
            if (!Engine.DashAssistFreeze)
            {
                if (!paused)
                {
                    return;
                }

                if (!Scene.Paused)
                {
                    Audio.PauseGameplaySfx = false;
                }

                DisableSnapshot();
                timer = 0.0f;
                paused = false;
            }
            else
            {
                paused = true;
                Audio.PauseGameplaySfx = true;
                timer += Engine.RawDeltaTime;
                if (timer > 0.20000000298023224 && snapshot == null)
                {
                    EnableSnapshot();
                }

                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null)
                {
                    return;
                }

                float num1 = Input.GetAimVector(entity.Facing).Angle();
                if ((double)Calc.AbsAngleDiff(num1, Direction) >= 1.5807963609695435)
                {
                    Direction = num1;
                    Scale = 0.0f;
                }
                else
                {
                    Direction = Calc.AngleApproach(Direction, num1, 18.849556f * Engine.RawDeltaTime);
                }

                Scale = Calc.Approach(Scale, 1f, Engine.DeltaTime * 4f);
                int num2 = 1 + ((8 + (int)Math.Round((double)num1 / 0.78539818525314331)) % 8);
                if (lastIndex != 0 && lastIndex != num2)
                {
                    _ = Audio.Play("event:/game/general/assist_dash_aim", entity.Center, "dash_direction", num2);
                }

                lastIndex = num2;
            }
        }

        public override void Render()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !Engine.DashAssistFreeze)
            {
                return;
            }

            MTexture mtexture = null;
            float rotation = float.MaxValue;
            for (int index = 0; index < 8; ++index)
            {
                float num = Calc.AngleDiff((float)(6.2831854820251465 * (index / 8.0)), Direction);
                if ((double)Math.Abs(num) < (double)Math.Abs(rotation))
                {
                    rotation = num;
                    mtexture = images[index];
                }
            }
            if (mtexture == null)
            {
                return;
            }

            if ((double)Math.Abs(rotation) < 0.05000000074505806)
            {
                rotation = 0.0f;
            }

            mtexture.DrawOutlineCentered((entity.Center + Offset + Calc.AngleToVector(Direction, 20f)).Round(), Color.White, Ease.BounceOut(Scale), rotation);
        }

        private void EnableSnapshot()
        {
        }

        private void DisableSnapshot()
        {
            if (!(snapshot != null))
            {
                return;
            }

            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
        }

        public override void Removed(Scene scene)
        {
            DisableSnapshot();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            DisableSnapshot();
            base.SceneEnd(scene);
        }
    }
}
