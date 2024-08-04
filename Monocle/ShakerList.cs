using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public class ShakerList : Component
    {
        public Vector2[] Values;
        public float Interval = 0.05f;
        public float Timer;
        public bool RemoveOnFinish;
        public Action<Vector2[]> OnShake;
        private bool on;

        public ShakerList(int length, bool on = true, Action<Vector2[]> onShake = null)
            : base(true, false)
        {
            Values = new Vector2[length];
            this.on = on;
            OnShake = onShake;
        }

        public ShakerList(int length, float time, bool removeOnFinish, Action<Vector2[]> onShake = null)
            : this(length, onShake: onShake)
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
                if (!(Values[0] != Vector2.Zero))
                    return;
                for (int index = 0; index < Values.Length; ++index)
                    Values[index] = Vector2.Zero;
                if (OnShake == null)
                    return;
                OnShake(Values);
            }
        }

        public ShakerList ShakeFor(float seconds, bool removeOnFinish)
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
                    for (int index = 0; index < Values.Length; ++index)
                        Values[index] = Vector2.Zero;
                    if (OnShake != null)
                        OnShake(Values);
                    if (!RemoveOnFinish)
                        return;
                    RemoveSelf();
                    return;
                }
            }
            if (!on || !Scene.OnInterval(Interval))
                return;
            for (int index = 0; index < Values.Length; ++index)
                Values[index] = Calc.Random.ShakeVector();
            if (OnShake == null)
                return;
            OnShake(Values);
        }
    }
}
