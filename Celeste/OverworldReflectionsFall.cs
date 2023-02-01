// Decompiled with JetBrains decompiler
// Type: Celeste.OverworldReflectionsFall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class OverworldReflectionsFall : Scene
  {
    private Level returnTo;
    private Action returnCallback;
    private Maddy3D maddy;
    private MountainRenderer mountain;
    private MountainCamera startCamera = new MountainCamera(new Vector3(-8f, 12f, -0.4f), new Vector3(-2f, 9f, -0.5f));
    private MountainCamera fallCamera = new MountainCamera(new Vector3(-10f, 6f, -0.4f), new Vector3(-4.25f, 1.5f, -1.25f));

    public OverworldReflectionsFall(Level returnTo, Action returnCallback)
    {
      this.returnTo = returnTo;
      this.returnCallback = returnCallback;
      this.Add((Monocle.Renderer) (this.mountain = new MountainRenderer()));
      this.mountain.SnapCamera(-1, new MountainCamera(this.startCamera.Position + (this.startCamera.Target - this.startCamera.Position).SafeNormalize() * 2f, this.startCamera.Target));
      this.Add((Monocle.Renderer) new HiresSnow()
      {
        ParticleAlpha = 0.0f
      });
      this.Add((Entity) new Snow3D(this.mountain.Model));
      this.Add((Entity) (this.maddy = new Maddy3D(this.mountain)));
      this.maddy.Falling();
      this.Add(new Entity()
      {
        (Component) new Coroutine(this.Routine())
      });
    }

    private IEnumerator Routine()
    {
      double num1 = (double) this.mountain.EaseCamera(-1, this.startCamera, new float?(0.4f));
      float duration = 4f;
      this.maddy.Position = this.startCamera.Target;
      for (int i = 0; i < 30; ++i)
      {
        this.maddy.Position = this.startCamera.Target + new Vector3(Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f));
        yield return (object) 0.01f;
      }
      yield return (object) 0.1f;
      this.maddy.Add((Component) new Coroutine(this.MaddyFall(duration + 0.1f)));
      yield return (object) 0.1f;
      double num2 = (double) this.mountain.EaseCamera(-1, this.fallCamera, new float?(duration));
      this.mountain.ForceNearFog = true;
      yield return (object) duration;
      yield return (object) 0.25f;
      double num3 = (double) this.mountain.EaseCamera(-1, new MountainCamera(this.fallCamera.Position + this.mountain.Model.Forward * 3f, this.fallCamera.Target), new float?(0.5f));
      this.Return();
    }

    private IEnumerator MaddyFall(float duration)
    {
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / duration)
      {
        this.maddy.Position = Vector3.Lerp(this.startCamera.Target, this.fallCamera.Target, p);
        yield return (object) null;
      }
    }

    private void Return()
    {
      FadeWipe fadeWipe = new FadeWipe((Scene) this, false, (Action) (() =>
      {
        this.mountain.Dispose();
        if (this.returnTo != null)
          Engine.Scene = (Scene) this.returnTo;
        this.returnCallback();
      }));
    }
  }
}
