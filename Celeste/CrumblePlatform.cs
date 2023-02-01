// Decompiled with JetBrains decompiler
// Type: Celeste.CrumblePlatform
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
  public class CrumblePlatform : Solid
  {
    public static ParticleType P_Crumble;
    private List<Monocle.Image> images;
    private List<Monocle.Image> outline;
    private List<Coroutine> falls;
    private List<int> fallOrder;
    private ShakerList shaker;
    private LightOcclude occluder;
    private Coroutine outlineFader;

    public CrumblePlatform(Vector2 position, float width)
      : base(position, width, 8f, false)
    {
      this.EnableAssistModeChecks = false;
    }

    public CrumblePlatform(EntityData data, Vector2 offset)
      : this(data.Position + offset, (float) data.Width)
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      MTexture mtexture1 = GFX.Game["objects/crumbleBlock/outline"];
      this.outline = new List<Monocle.Image>();
      if ((double) this.Width <= 8.0)
      {
        Monocle.Image image = new Monocle.Image(mtexture1.GetSubtexture(24, 0, 8, 8));
        image.Color = Color.White * 0.0f;
        this.Add((Component) image);
        this.outline.Add(image);
      }
      else
      {
        for (int x = 0; (double) x < (double) this.Width; x += 8)
        {
          int num = x != 0 ? (x <= 0 || (double) x >= (double) this.Width - 8.0 ? 2 : 1) : 0;
          Monocle.Image image = new Monocle.Image(mtexture1.GetSubtexture(num * 8, 0, 8, 8));
          image.Position = new Vector2((float) x, 0.0f);
          image.Color = Color.White * 0.0f;
          this.Add((Component) image);
          this.outline.Add(image);
        }
      }
      this.Add((Component) (this.outlineFader = new Coroutine()));
      this.outlineFader.RemoveOnComplete = false;
      this.images = new List<Monocle.Image>();
      this.falls = new List<Coroutine>();
      this.fallOrder = new List<int>();
      MTexture mtexture2 = GFX.Game["objects/crumbleBlock/" + AreaData.Get(scene).CrumbleBlock];
      for (int index = 0; (double) index < (double) this.Width; index += 8)
      {
        int num = (int) (((double) Math.Abs(this.X) + (double) index) / 8.0) % 4;
        Monocle.Image image = new Monocle.Image(mtexture2.GetSubtexture(num * 8, 0, 8, 8));
        image.Position = new Vector2((float) (4 + index), 4f);
        image.CenterOrigin();
        this.Add((Component) image);
        this.images.Add(image);
        Coroutine coroutine = new Coroutine();
        coroutine.RemoveOnComplete = false;
        this.falls.Add(coroutine);
        this.Add((Component) coroutine);
        this.fallOrder.Add(index / 8);
      }
      this.fallOrder.Shuffle<int>();
      this.Add((Component) new Coroutine(this.Sequence()));
      this.Add((Component) (this.shaker = new ShakerList(this.images.Count, false, (Action<Vector2[]>) (v =>
      {
        for (int index = 0; index < this.images.Count; ++index)
          this.images[index].Position = new Vector2((float) (4 + index * 8), 4f) + v[index];
      }))));
      this.Add((Component) (this.occluder = new LightOcclude(0.2f)));
    }

    private IEnumerator Sequence()
    {
      CrumblePlatform crumblePlatform = this;
label_1:
      bool onTop = false;
      while (crumblePlatform.GetPlayerOnTop() == null)
      {
        if (crumblePlatform.GetPlayerClimbing() != null)
        {
          onTop = false;
          Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
          goto label_7;
        }
        else
          yield return (object) null;
      }
      onTop = true;
      Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
label_7:
      Audio.Play("event:/game/general/platform_disintegrate", crumblePlatform.Center);
      crumblePlatform.shaker.ShakeFor(onTop ? 0.6f : 1f, false);
      foreach (Monocle.Image image in crumblePlatform.images)
        crumblePlatform.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, crumblePlatform.Position + image.Position + new Vector2(0.0f, 2f), Vector2.One * 3f);
      for (int i = 0; i < (onTop ? 1 : 3); ++i)
      {
        yield return (object) 0.2f;
        foreach (Monocle.Image image in crumblePlatform.images)
          crumblePlatform.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, crumblePlatform.Position + image.Position + new Vector2(0.0f, 2f), Vector2.One * 3f);
      }
      float timer = 0.4f;
      if (onTop)
      {
        for (; (double) timer > 0.0 && crumblePlatform.GetPlayerOnTop() != null; timer -= Engine.DeltaTime)
          yield return (object) null;
      }
      else
      {
        for (; (double) timer > 0.0; timer -= Engine.DeltaTime)
          yield return (object) null;
      }
      crumblePlatform.outlineFader.Replace(crumblePlatform.OutlineFade(1f));
      crumblePlatform.occluder.Visible = false;
      crumblePlatform.Collidable = false;
      float num = 0.05f;
      for (int index1 = 0; index1 < 4; ++index1)
      {
        for (int index2 = 0; index2 < crumblePlatform.images.Count; ++index2)
        {
          if (index2 % 4 - index1 == 0)
            crumblePlatform.falls[index2].Replace(crumblePlatform.TileOut(crumblePlatform.images[crumblePlatform.fallOrder[index2]], num * (float) index1));
        }
      }
      yield return (object) 2f;
      while (crumblePlatform.CollideCheck<Actor>() || crumblePlatform.CollideCheck<Solid>())
        yield return (object) null;
      crumblePlatform.outlineFader.Replace(crumblePlatform.OutlineFade(0.0f));
      crumblePlatform.occluder.Visible = true;
      crumblePlatform.Collidable = true;
      for (int index3 = 0; index3 < 4; ++index3)
      {
        for (int index4 = 0; index4 < crumblePlatform.images.Count; ++index4)
        {
          if (index4 % 4 - index3 == 0)
            crumblePlatform.falls[index4].Replace(crumblePlatform.TileIn(index4, crumblePlatform.images[crumblePlatform.fallOrder[index4]], 0.05f * (float) index3));
        }
      }
      goto label_1;
    }

    private IEnumerator OutlineFade(float to)
    {
      float from = 1f - to;
      for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime * 2f)
      {
        Color color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
        foreach (GraphicsComponent graphicsComponent in this.outline)
          graphicsComponent.Color = color;
        yield return (object) null;
      }
    }

    private IEnumerator TileOut(Monocle.Image img, float delay)
    {
      img.Color = Color.Gray;
      yield return (object) delay;
      float distance = (float) (((double) img.X * 7.0 % 3.0 + 1.0) * 12.0);
      Vector2 from = img.Position;
      for (float time = 0.0f; (double) time < 1.0; time += Engine.DeltaTime / 0.4f)
      {
        yield return (object) null;
        img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
        img.Color = Color.Gray * (1f - time);
        img.Scale = Vector2.One * (float) (1.0 - (double) time * 0.5);
      }
      img.Visible = false;
    }

    private IEnumerator TileIn(int index, Monocle.Image img, float delay)
    {
      CrumblePlatform crumblePlatform = this;
      yield return (object) delay;
      Audio.Play("event:/game/general/platform_return", crumblePlatform.Center);
      img.Visible = true;
      img.Color = Color.White;
      img.Position = new Vector2((float) (index * 8 + 4), 4f);
      for (float time = 0.0f; (double) time < 1.0; time += Engine.DeltaTime / 0.25f)
      {
        yield return (object) null;
        img.Scale = Vector2.One * (float) (1.0 + (double) Ease.BounceOut(1f - time) * 0.20000000298023224);
      }
      img.Scale = Vector2.One;
    }
  }
}
