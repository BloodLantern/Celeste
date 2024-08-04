using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public class Shaker : Component
    {
        public Vector2 Value;
        public float Interval = 0.05f;
        public float Timer;
        public bool RemoveOnFinish;
        public Action<Vector2> OnShake;
        private bool on;

        public Shaker(bool on = true, Action<Vector2> onShake = null)
            : base(true, false)
        {
            this.on = on;
            OnShake = onShake;
        }

        public Shaker(float time, bool removeOnFinish, Action<Vector2> onShake = null)
            : this(onShake: onShake)
        {
            Timer = time;
            RemoveOnFinish = removeOnFinish;
        }

        public bool On
        {
            get => on;
            set
            {
                on = value;
                if (on)
                    return;
                Timer = 0.0f;
                if (!(Value != Vector2.Zero))
                    return;
                Value = Vector2.Zero;
                if (OnShake == null)
                    return;
                OnShake(Vector2.Zero);
            }
        }

        public Shaker ShakeFor(float seconds, bool removeOnFinish)
        {
            on = true;
            Timer = seconds;
            RemoveOnFinish = removeOnFinish;
            return this;
        }

        public override void Update()
        {
            if (on && Timer > 0.0)
            {
                Timer -= Engine.DeltaTime;
                if (Timer <= 0.0)
                {
                    on = false;
                    Value = Vector2.Zero;
                    if (OnShake != null)
                        OnShake(Vector2.Zero);
                    if (!RemoveOnFinish)
                        return;
                    RemoveSelf();
                    return;
                }
            }
            if (!on || !Scene.OnInterval(Interval))
                return;
            Value = Calc.Random.ShakeVector();
            if (OnShake == null)
                return;
            OnShake(Value);
        }
    }
}
