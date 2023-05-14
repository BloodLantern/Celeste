// Decompiled with JetBrains decompiler
// Type: Celeste.WindController
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class WindController : Entity
    {
        private const float Weak = 400f;
        private const float Strong = 800f;
        private const float Crazy = 1200f;
        private const float Accel = 1000f;
        private const float Down = 300f;
        private const float Up = -400f;
        private const float Space = -600f;
        private Level level;
        private WindController.Patterns pattern;
        private Vector2 targetSpeed;
        private Coroutine coroutine;
        private readonly WindController.Patterns startPattern;
        private bool everSetPattern;

        public WindController(WindController.Patterns pattern)
        {
            Tag = (int)Tags.TransitionUpdate;
            startPattern = pattern;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public void SetStartPattern()
        {
            if (everSetPattern)
            {
                return;
            }

            SetPattern(startPattern);
        }

        public void SetPattern(WindController.Patterns pattern)
        {
            if (this.pattern == pattern && everSetPattern)
            {
                return;
            }

            everSetPattern = true;
            this.pattern = pattern;
            if (coroutine != null)
            {
                Remove(coroutine);
                coroutine = null;
            }
            switch (pattern)
            {
                case WindController.Patterns.None:
                    targetSpeed = Vector2.Zero;
                    SetAmbienceStrength(false);
                    break;
                case WindController.Patterns.Left:
                    targetSpeed.X = -400f;
                    SetAmbienceStrength(false);
                    break;
                case WindController.Patterns.Right:
                    targetSpeed.X = 400f;
                    SetAmbienceStrength(false);
                    break;
                case WindController.Patterns.LeftStrong:
                    targetSpeed.X = -800f;
                    SetAmbienceStrength(true);
                    break;
                case WindController.Patterns.RightStrong:
                    targetSpeed.X = 800f;
                    SetAmbienceStrength(true);
                    break;
                case WindController.Patterns.LeftOnOff:
                    Add(coroutine = new Coroutine(LeftOnOffSequence()));
                    break;
                case WindController.Patterns.RightOnOff:
                    Add(coroutine = new Coroutine(RightOnOffSequence()));
                    break;
                case WindController.Patterns.LeftOnOffFast:
                    Add(coroutine = new Coroutine(LeftOnOffFastSequence()));
                    break;
                case WindController.Patterns.RightOnOffFast:
                    Add(coroutine = new Coroutine(RightOnOffFastSequence()));
                    break;
                case WindController.Patterns.Alternating:
                    Add(coroutine = new Coroutine(AlternatingSequence()));
                    break;
                case WindController.Patterns.RightCrazy:
                    targetSpeed.X = 1200f;
                    SetAmbienceStrength(true);
                    break;
                case WindController.Patterns.Down:
                    targetSpeed.Y = 300f;
                    SetAmbienceStrength(false);
                    break;
                case WindController.Patterns.Up:
                    targetSpeed.Y = -400f;
                    SetAmbienceStrength(false);
                    break;
                case WindController.Patterns.Space:
                    targetSpeed.Y = -600f;
                    SetAmbienceStrength(false);
                    break;
            }
        }

        private void SetAmbienceStrength(bool strong)
        {
            int num = 0;
            if (targetSpeed.X != 0.0)
            {
                num = Math.Sign(targetSpeed.X);
            }
            else if (targetSpeed.Y != 0.0)
            {
                num = Math.Sign(targetSpeed.Y);
            }

            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", num);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", strong ? 1f : 0.0f);
        }

        public void SnapWind()
        {
            if (coroutine != null && coroutine.Active)
            {
                coroutine.Update();
            }

            level.Wind = targetSpeed;
        }

        public override void Update()
        {
            base.Update();
            if (pattern == WindController.Patterns.LeftGemsOnly)
            {
                bool flag = false;
                foreach (StrawberrySeed entity in Scene.Tracker.GetEntities<StrawberrySeed>())
                {
                    if (entity.Collected)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    targetSpeed.X = -400f;
                    SetAmbienceStrength(false);
                }
                else
                {
                    targetSpeed.X = 0.0f;
                    SetAmbienceStrength(false);
                }
            }
            level.Wind = Calc.Approach(level.Wind, targetSpeed, 1000f * Engine.DeltaTime);
            if (!(level.Wind != Vector2.Zero) || level.Transitioning)
            {
                return;
            }

            foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>())
            {
                component.Move(level.Wind * 0.1f * Engine.DeltaTime);
            }
        }

        private IEnumerator AlternatingSequence()
        {
            while (true)
            {
                targetSpeed.X = -400f;
                SetAmbienceStrength(false);
                yield return 3f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 2f;
                targetSpeed.X = 400f;
                SetAmbienceStrength(false);
                yield return 3f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 2f;
            }
        }

        private IEnumerator RightOnOffSequence()
        {
            while (true)
            {
                targetSpeed.X = 800f;
                SetAmbienceStrength(true);
                yield return 3f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 3f;
            }
        }

        private IEnumerator LeftOnOffSequence()
        {
            while (true)
            {
                targetSpeed.X = -800f;
                SetAmbienceStrength(true);
                yield return 3f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 3f;
            }
        }

        private IEnumerator RightOnOffFastSequence()
        {
            while (true)
            {
                targetSpeed.X = 800f;
                SetAmbienceStrength(true);
                yield return 2f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 2f;
            }
        }

        private IEnumerator LeftOnOffFastSequence()
        {
            while (true)
            {
                targetSpeed.X = -800f;
                SetAmbienceStrength(true);
                yield return 2f;
                targetSpeed.X = 0.0f;
                SetAmbienceStrength(false);
                yield return 2f;
            }
        }

        public enum Patterns
        {
            None,
            Left,
            Right,
            LeftStrong,
            RightStrong,
            LeftOnOff,
            RightOnOff,
            LeftOnOffFast,
            RightOnOffFast,
            Alternating,
            LeftGemsOnly,
            RightCrazy,
            Down,
            Up,
            Space,
        }
    }
}
