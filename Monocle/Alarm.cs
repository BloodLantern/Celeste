// Decompiled with JetBrains decompiler
// Type: Monocle.Alarm
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class Alarm : Component
    {
        public Action OnComplete;
        private static readonly Stack<Alarm> cached = new();

        public Alarm.AlarmMode Mode { get; private set; }

        public float Duration { get; private set; }

        public float TimeLeft { get; private set; }

        public static Alarm Create(
            Alarm.AlarmMode mode,
            Action onComplete,
            float duration = 1f,
            bool start = false)
        {
            Alarm alarm = Alarm.cached.Count != 0 ? Alarm.cached.Pop() : new Alarm();
            alarm.Init(mode, onComplete, duration, start);
            return alarm;
        }

        public static Alarm Set(
            Entity entity,
            float duration,
            Action onComplete,
            Alarm.AlarmMode alarmMode = Alarm.AlarmMode.Oneshot)
        {
            Alarm alarm = Alarm.Create(alarmMode, onComplete, duration, true);
            entity.Add(alarm);
            return alarm;
        }

        private Alarm()
            : base(false, false)
        {
        }

        private void Init(Alarm.AlarmMode mode, Action onComplete, float duration = 1f, bool start = false)
        {
            Mode = mode;
            Duration = duration;
            OnComplete = onComplete;
            Active = false;
            TimeLeft = 0.0f;
            if (!start)
            {
                return;
            }

            Start();
        }

        public override void Update()
        {
            TimeLeft -= Engine.DeltaTime;
            if ((double)TimeLeft > 0.0)
            {
                return;
            }

            TimeLeft = 0.0f;
            OnComplete?.Invoke();
            if (Mode == Alarm.AlarmMode.Looping)
            {
                Start();
            }
            else if (Mode == Alarm.AlarmMode.Oneshot)
            {
                RemoveSelf();
            }
            else
            {
                if ((double)TimeLeft > 0.0)
                {
                    return;
                }

                Active = false;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Alarm.cached.Push(this);
        }

        public void Start()
        {
            Active = true;
            TimeLeft = Duration;
        }

        public void Start(float duration)
        {
            Duration = duration;
            Start();
        }

        public void Stop()
        {
            Active = false;
        }

        public enum AlarmMode
        {
            Persist,
            Oneshot,
            Looping,
        }
    }
}
