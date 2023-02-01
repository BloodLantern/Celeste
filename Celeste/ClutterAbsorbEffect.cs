// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterAbsorbEffect
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
  public class ClutterAbsorbEffect : Entity
  {
    private Level level;
    private List<ClutterCabinet> cabinets = new List<ClutterCabinet>();

    public ClutterAbsorbEffect()
    {
      this.Position = Vector2.Zero;
      this.Tag = (int) Tags.TransitionUpdate;
      this.Depth = -10001;
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.level = this.SceneAs<Level>();
      foreach (Entity entity in this.level.Tracker.GetEntities<ClutterCabinet>())
        this.cabinets.Add(entity as ClutterCabinet);
    }

    public void FlyClutter(Vector2 position, MTexture texture, bool shake, float delay)
    {
      Monocle.Image img = new Monocle.Image(texture);
      img.Position = position - this.Position;
      img.CenterOrigin();
      this.Add((Component) img);
      this.Add((Component) new Coroutine(this.FlyClutterRoutine(img, shake, delay))
      {
        RemoveOnComplete = true
      });
    }

    private IEnumerator FlyClutterRoutine(Monocle.Image img, bool shake, float delay)
    {
      ClutterAbsorbEffect clutterAbsorbEffect = this;
      yield return (object) delay;
      ClutterCabinet cabinet = Calc.Random.Choose<ClutterCabinet>(clutterAbsorbEffect.cabinets);
      Vector2 vector2_1 = cabinet.Position + new Vector2(8f);
      Vector2 from = img.Position;
      Vector2 vector2_2 = new Vector2((float) (Calc.Random.Next(16) - 8), (float) (Calc.Random.Next(4) - 2));
      Vector2 end = vector2_1 + vector2_2;
      Vector2 vector2_3 = (end - from).SafeNormalize();
      float num = (end - from).Length();
      Vector2 vector2_4 = new Vector2(-vector2_3.Y, vector2_3.X) * (num / 4f + Calc.Random.NextFloat(40f)) * (Calc.Random.Chance(0.5f) ? -1f : 1f);
      SimpleCurve curve = new SimpleCurve(from, end, (end + from) / 2f + vector2_4);
      float time;
      if (shake)
      {
        for (time = 0.25f; (double) time > 0.0; time -= Engine.DeltaTime)
        {
          img.X = (float) ((double) from.X + (double) Calc.Random.Next(3) - 1.0);
          img.Y = (float) ((double) from.Y + (double) Calc.Random.Next(3) - 1.0);
          yield return (object) null;
        }
      }
      for (time = 0.0f; (double) time < 1.0; time += Engine.DeltaTime)
      {
        img.Position = curve.GetPoint(Ease.CubeInOut(time));
        img.Scale = Vector2.One * Ease.CubeInOut((float) (1.0 - (double) time * 0.5));
        if ((double) time > 0.5 && !cabinet.Opened)
          cabinet.Open();
        if (clutterAbsorbEffect.level.OnInterval(0.25f))
          clutterAbsorbEffect.level.ParticlesFG.Emit(ClutterSwitch.P_ClutterFly, img.Position);
        yield return (object) null;
      }
      clutterAbsorbEffect.Remove((Component) img);
    }

    public void CloseCabinets() => this.Add((Component) new Coroutine(this.CloseCabinetsRoutine()));

    private IEnumerator CloseCabinetsRoutine()
    {
      this.cabinets.Sort((Comparison<ClutterCabinet>) ((a, b) => (double) Math.Abs(a.Y - b.Y) < 24.0 ? Math.Sign(a.X - b.X) : Math.Sign(a.Y - b.Y)));
      int i = 0;
      foreach (ClutterCabinet cabinet in this.cabinets)
      {
        cabinet.Close();
        if (i++ % 3 == 0)
          yield return (object) 0.1f;
      }
    }
  }
}
