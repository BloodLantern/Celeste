// Decompiled with JetBrains decompiler
// Type: Monocle.Tween
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
  public class Tween : Component
  {
    public Ease.Easer Easer;
    public Action<Tween> OnUpdate;
    public Action<Tween> OnComplete;
    public Action<Tween> OnStart;
    public bool UseRawDeltaTime;
    private bool startedReversed;
    private ulong cachedFrame;
    private static List<Tween> cached = new List<Tween>();

    public Tween.TweenMode Mode { get; private set; }

    public float Duration { get; private set; }

    public float TimeLeft { get; private set; }

    public float Percent { get; private set; }

    public float Eased { get; private set; }

    public bool Reverse { get; private set; }

    public static Tween Create(
      Tween.TweenMode mode,
      Ease.Easer easer = null,
      float duration = 1f,
      bool start = false)
    {
      Tween tween1 = (Tween) null;
      foreach (Tween tween2 in Tween.cached)
      {
        if (Engine.FrameCounter > tween2.cachedFrame + 3UL)
        {
          tween1 = tween2;
          Tween.cached.Remove(tween2);
          break;
        }
      }
      if (tween1 == null)
        tween1 = new Tween();
      tween1.OnUpdate = tween1.OnComplete = tween1.OnStart = (Action<Tween>) null;
      tween1.Init(mode, easer, duration, start);
      return tween1;
    }

    public static Tween Set(
      Entity entity,
      Tween.TweenMode tweenMode,
      float duration,
      Ease.Easer easer,
      Action<Tween> onUpdate,
      Action<Tween> onComplete = null)
    {
      Tween tween = Tween.Create(tweenMode, easer, duration, true);
      tween.OnUpdate += onUpdate;
      tween.OnComplete += onComplete;
      entity.Add((Component) tween);
      return tween;
    }

    public static Tween Position(
      Entity entity,
      Vector2 targetPosition,
      float duration,
      Ease.Easer easer,
      Tween.TweenMode tweenMode = Tween.TweenMode.Oneshot)
    {
      Vector2 startPosition = entity.Position;
      Tween tween = Tween.Create(tweenMode, easer, duration, true);
      tween.OnUpdate = (Action<Tween>) (t => entity.Position = Vector2.Lerp(startPosition, targetPosition, t.Eased));
      entity.Add((Component) tween);
      return tween;
    }

    private Tween()
      : base(false, false)
    {
    }

    private void Init(Tween.TweenMode mode, Ease.Easer easer, float duration, bool start)
    {
      if ((double) duration <= 0.0)
        duration = 1E-06f;
      this.UseRawDeltaTime = false;
      this.Mode = mode;
      this.Easer = easer;
      this.Duration = duration;
      this.TimeLeft = 0.0f;
      this.Percent = 0.0f;
      this.Active = false;
      if (!start)
        return;
      this.Start();
    }

    public override void Removed(Entity entity)
    {
      base.Removed(entity);
      Tween.cached.Add(this);
      this.cachedFrame = Engine.FrameCounter;
    }

    public override void Update()
    {
      this.TimeLeft -= this.UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime;
      this.Percent = Math.Max(0.0f, this.TimeLeft) / this.Duration;
      if (!this.Reverse)
        this.Percent = 1f - this.Percent;
      this.Eased = this.Easer == null ? this.Percent : this.Easer(this.Percent);
      if (this.OnUpdate != null)
        this.OnUpdate(this);
      if ((double) this.TimeLeft > 0.0)
        return;
      this.TimeLeft = 0.0f;
      if (this.OnComplete != null)
        this.OnComplete(this);
      switch (this.Mode)
      {
        case Tween.TweenMode.Persist:
          this.Active = false;
          break;
        case Tween.TweenMode.Oneshot:
          this.Active = false;
          this.RemoveSelf();
          break;
        case Tween.TweenMode.Looping:
          this.Start(this.Reverse);
          break;
        case Tween.TweenMode.YoyoOneshot:
          if (this.Reverse == this.startedReversed)
          {
            this.Start(!this.Reverse);
            this.startedReversed = !this.Reverse;
            break;
          }
          this.Active = false;
          this.RemoveSelf();
          break;
        case Tween.TweenMode.YoyoLooping:
          this.Start(!this.Reverse);
          break;
      }
    }

    public void Start() => this.Start(false);

    public void Start(bool reverse)
    {
      this.startedReversed = this.Reverse = reverse;
      this.TimeLeft = this.Duration;
      this.Eased = this.Percent = this.Reverse ? 1f : 0.0f;
      this.Active = true;
      if (this.OnStart == null)
        return;
      this.OnStart(this);
    }

    public void Start(float duration, bool reverse = false)
    {
      this.Duration = duration;
      this.Start(reverse);
    }

    public void Stop() => this.Active = false;

    public void Reset()
    {
      this.TimeLeft = this.Duration;
      this.Eased = this.Percent = this.Reverse ? 1f : 0.0f;
    }

    public IEnumerator Wait()
    {
      Tween tween = this;
      while (tween.Active)
        yield return (object) null;
    }

    public float Inverted => 1f - this.Eased;

    public enum TweenMode
    {
      Persist,
      Oneshot,
      Looping,
      YoyoOneshot,
      YoyoLooping,
    }
  }
}
