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
        private Patterns pattern;
        private Vector2 targetSpeed;
        private Coroutine coroutine;
        private readonly Patterns startPattern;
        private bool everSetPattern;

        public WindController(Patterns pattern)
        {
            Tag = (int) Tags.TransitionUpdate;
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
                return;
            SetPattern(startPattern);
        }

        public void SetPattern(Patterns pattern)
        {
            if (this.pattern == pattern && everSetPattern)
                return;
            everSetPattern = true;
            this.pattern = pattern;
            if (coroutine != null)
            {
                Remove(coroutine);
                coroutine = null;
            }
            switch (pattern)
            {
                case Patterns.None:
                    targetSpeed = Vector2.Zero;
                    SetAmbienceStrength(false);
                    break;
                case Patterns.Left:
                    targetSpeed.X = -Weak;
                    SetAmbienceStrength(false);
                    break;
                case Patterns.Right:
                    targetSpeed.X = Weak;
                    SetAmbienceStrength(false);
                    break;
                case Patterns.LeftStrong:
                    targetSpeed.X = -Strong;
                    SetAmbienceStrength(true);
                    break;
                case Patterns.RightStrong:
                    targetSpeed.X = Strong;
                    SetAmbienceStrength(true);
                    break;
                case Patterns.LeftOnOff:
                    Add(coroutine = new Coroutine(LeftOnOffSequence()));
                    break;
                case Patterns.RightOnOff:
                    Add(coroutine = new Coroutine(RightOnOffSequence()));
                    break;
                case Patterns.LeftOnOffFast:
                    Add(coroutine = new Coroutine(LeftOnOffFastSequence()));
                    break;
                case Patterns.RightOnOffFast:
                    Add(coroutine = new Coroutine(RightOnOffFastSequence()));
                    break;
                case Patterns.Alternating:
                    Add(coroutine = new Coroutine(AlternatingSequence()));
                    break;
                case Patterns.RightCrazy:
                    targetSpeed.X = Crazy;
                    SetAmbienceStrength(true);
                    break;
                case Patterns.Down:
                    targetSpeed.Y = Down;
                    SetAmbienceStrength(false);
                    break;
                case Patterns.Up:
                    targetSpeed.Y = Up;
                    SetAmbienceStrength(false);
                    break;
                case Patterns.Space:
                    targetSpeed.Y = Space;
                    SetAmbienceStrength(false);
                    break;
            }
        }

        private void SetAmbienceStrength(bool strong)
        {
            int num = 0;
            if (targetSpeed.X != 0)
                num = Math.Sign(targetSpeed.X);
            else if (targetSpeed.Y != 0)
                num = Math.Sign(targetSpeed.Y);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "wind_direction", num);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "strong_wind", strong ? 1f : 0f);
        }

        public void SnapWind()
        {
            if (coroutine != null && coroutine.Active)
                coroutine.Update();
            level.Wind = targetSpeed;
        }

        public override void Update()
        {
            base.Update();
            if (pattern == Patterns.LeftGemsOnly)
            {
                bool flag = false;
                foreach (StrawberrySeed entity in Scene.Tracker.GetEntities<StrawberrySeed>())
                    if (entity.Collected)
                    {
                        flag = true;
                        break;
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
                return;
            foreach (WindMover component in Scene.Tracker.GetComponents<WindMover>())
                component.Move(level.Wind * 0.1f * Engine.DeltaTime);
        }

        private IEnumerator AlternatingSequence()
        {
            while (true)
            {
                targetSpeed.X = Up;
                SetAmbienceStrength(false);
                yield return 3f;
                targetSpeed.X = 0f;
                SetAmbienceStrength(false);
                yield return 2f;
                targetSpeed.X = Down;
                SetAmbienceStrength(false);
                yield return 3f;
                targetSpeed.X = 0f;
                SetAmbienceStrength(false);
                yield return 2f;
            }
        }

        private IEnumerator RightOnOffSequence()
        {
            while (true)
            {
                targetSpeed.X = Strong;
                SetAmbienceStrength(true);
                yield return 3f;
                targetSpeed.X = 0f;
                SetAmbienceStrength(false);
                yield return 3f;
            }
        }

        private IEnumerator LeftOnOffSequence()
        {
            while (true)
            {
                targetSpeed.X = -Strong;
                SetAmbienceStrength(true);
                yield return 3f;
                targetSpeed.X = 0f;
                SetAmbienceStrength(false);
                yield return 3f;
            }
        }

        private IEnumerator RightOnOffFastSequence()
        {
            while (true)
            {
                targetSpeed.X = Strong;
                SetAmbienceStrength(true);
                yield return 2f;
                targetSpeed.X = 0f;
                SetAmbienceStrength(false);
                yield return 2f;
            }
        }

        private IEnumerator LeftOnOffFastSequence()
        {
            while (true)
            {
                targetSpeed.X = -Strong;
                SetAmbienceStrength(true);
                yield return 2f;
                targetSpeed.X = 0f;
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
