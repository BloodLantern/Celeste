// Decompiled with JetBrains decompiler
// Type: Celeste.SummitGemManager
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
  public class SummitGemManager : Entity
  {
    private List<SummitGemManager.Gem> gems = new List<SummitGemManager.Gem>();

    public SummitGemManager(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Depth = -10010;
      int index = 0;
      foreach (Vector2 position in data.NodesOffset(offset))
      {
        this.gems.Add(new SummitGemManager.Gem(index, position));
        ++index;
      }
      this.Add((Component) new Coroutine(this.Routine()));
    }

    public override void Awake(Scene scene)
    {
      foreach (SummitGemManager.Gem gem in this.gems)
        scene.Add((Entity) gem);
      base.Awake(scene);
    }

    private IEnumerator Routine()
    {
      SummitGemManager summitGemManager = this;
      Level level = summitGemManager.Scene as Level;
      if (level.Session.HeartGem)
      {
        foreach (SummitGemManager.Gem gem in summitGemManager.gems)
          gem.Sprite.RemoveSelf();
        summitGemManager.gems.Clear();
      }
      else
      {
        while (true)
        {
          Player entity = summitGemManager.Scene.Tracker.GetEntity<Player>();
          if (entity == null || (double) (entity.Position - summitGemManager.Position).Length() >= 64.0)
            yield return (object) null;
          else
            break;
        }
        yield return (object) 0.5f;
        bool alreadyHasHeart = level.Session.OldStats.Modes[0].HeartGem;
        int broken = 0;
        int index = 0;
        foreach (SummitGemManager.Gem gem in summitGemManager.gems)
        {
          bool flag = level.Session.SummitGems[index];
          if (!alreadyHasHeart)
            flag = ((flag ? 1 : 0) | (SaveData.Instance.SummitGems == null ? 0 : (SaveData.Instance.SummitGems[index] ? 1 : 0))) != 0;
          if (flag)
          {
            switch (index)
            {
              case 0:
                Audio.Play("event:/game/07_summit/gem_unlock_1", gem.Position);
                break;
              case 1:
                Audio.Play("event:/game/07_summit/gem_unlock_2", gem.Position);
                break;
              case 2:
                Audio.Play("event:/game/07_summit/gem_unlock_3", gem.Position);
                break;
              case 3:
                Audio.Play("event:/game/07_summit/gem_unlock_4", gem.Position);
                break;
              case 4:
                Audio.Play("event:/game/07_summit/gem_unlock_5", gem.Position);
                break;
              case 5:
                Audio.Play("event:/game/07_summit/gem_unlock_6", gem.Position);
                break;
            }
            gem.Sprite.Play("spin");
            while (gem.Sprite.CurrentAnimationID == "spin")
            {
              gem.Bloom.Alpha = Calc.Approach(gem.Bloom.Alpha, 1f, Engine.DeltaTime * 3f);
              if ((double) gem.Bloom.Alpha > 0.5)
                gem.Shake = Calc.Random.ShakeVector();
              gem.Sprite.Y -= Engine.DeltaTime * 8f;
              gem.Sprite.Scale = Vector2.One * (float) (1.0 + (double) gem.Bloom.Alpha * 0.10000000149011612);
              yield return (object) null;
            }
            yield return (object) 0.2f;
            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            for (int index1 = 0; index1 < 20; ++index1)
              level.ParticlesFG.Emit(SummitGem.P_Shatter, gem.Position + new Vector2((float) Calc.Random.Range(-8, 8), (float) Calc.Random.Range(-8, 8)), SummitGem.GemColors[index], Calc.Random.NextFloat(6.28318548f));
            ++broken;
            gem.Bloom.RemoveSelf();
            gem.Sprite.RemoveSelf();
            yield return (object) 0.25f;
          }
          ++index;
        }
        if (broken >= 6)
        {
          HeartGem heart = summitGemManager.Scene.Entities.FindFirst<HeartGem>();
          if (heart != null)
          {
            Audio.Play("event:/game/07_summit/gem_unlock_complete", heart.Position);
            yield return (object) 0.1f;
            Vector2 from = heart.Position;
            for (float p = 0.0f; (double) p < 1.0 && heart.Scene != null; p += Engine.DeltaTime)
            {
              heart.Position = Vector2.Lerp(from, summitGemManager.Position + new Vector2(0.0f, -16f), Ease.CubeOut(p));
              yield return (object) null;
            }
            from = new Vector2();
          }
          heart = (HeartGem) null;
        }
      }
    }

    private class Gem : Entity
    {
      public Vector2 Shake;
      public Sprite Sprite;
      public Monocle.Image Bg;
      public BloomPoint Bloom;

      public Gem(int index, Vector2 position)
        : base(position)
      {
        this.Depth = -10010;
        this.Add((Component) (this.Bg = new Monocle.Image(GFX.Game["collectables/summitgems/" + (object) index + "/bg"])));
        this.Add((Component) (this.Sprite = new Sprite(GFX.Game, "collectables/summitgems/" + (object) index + "/gem")));
        this.Add((Component) (this.Bloom = new BloomPoint(0.0f, 20f)));
        this.Sprite.AddLoop("idle", "", 0.05f, new int[1]);
        this.Sprite.Add("spin", "", 0.05f, "idle");
        this.Sprite.Play("idle");
        this.Sprite.CenterOrigin();
        this.Bg.CenterOrigin();
      }

      public override void Update()
      {
        this.Bloom.Position = this.Sprite.Position;
        base.Update();
      }

      public override void Render()
      {
        Vector2 position = this.Sprite.Position;
        Sprite sprite = this.Sprite;
        sprite.Position = sprite.Position + this.Shake;
        base.Render();
        this.Sprite.Position = position;
      }
    }
  }
}
