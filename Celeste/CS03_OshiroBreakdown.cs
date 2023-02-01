// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_OshiroBreakdown
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
  public class CS03_OshiroBreakdown : CutsceneEntity
  {
    public const string Flag = "oshiro_breakdown";
    private const int PlayerWalkTo = 200;
    private List<DustStaticSpinner> creatures = new List<DustStaticSpinner>();
    private List<Vector2> creatureHomes = new List<Vector2>();
    private NPC oshiro;
    private Player player;
    private Vector2 origin;
    private const int DustAmountA = 4;

    public CS03_OshiroBreakdown(Player player, NPC oshiro)
      : base()
    {
      this.oshiro = oshiro;
      this.player = player;
      this.origin = oshiro.Position;
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS03_OshiroBreakdown cs03OshiroBreakdown = this;
      cs03OshiroBreakdown.player.StateMachine.State = 11;
      cs03OshiroBreakdown.player.StateMachine.Locked = true;
      cs03OshiroBreakdown.Add((Component) new Coroutine(cs03OshiroBreakdown.player.DummyWalkTo(cs03OshiroBreakdown.player.X - 64f)));
      List<DustStaticSpinner> all = level.Entities.FindAll<DustStaticSpinner>();
      all.Shuffle<DustStaticSpinner>();
      foreach (DustStaticSpinner dustStaticSpinner in all)
      {
        if ((double) (dustStaticSpinner.Position - cs03OshiroBreakdown.oshiro.Position).Length() < 128.0)
        {
          cs03OshiroBreakdown.creatures.Add(dustStaticSpinner);
          cs03OshiroBreakdown.creatureHomes.Add(dustStaticSpinner.Position);
          dustStaticSpinner.Visible = false;
        }
      }
      yield return (object) cs03OshiroBreakdown.PanCamera((float) level.Bounds.Left);
      yield return (object) 0.2f;
      yield return (object) cs03OshiroBreakdown.Level.ZoomTo(new Vector2(100f, 120f), 2f, 0.5f);
      yield return (object) Textbox.Say("CH3_OSHIRO_BREAKDOWN", new Func<IEnumerator>(cs03OshiroBreakdown.WalkLeft), new Func<IEnumerator>(cs03OshiroBreakdown.WalkRight), new Func<IEnumerator>(cs03OshiroBreakdown.CreateDustA), new Func<IEnumerator>(cs03OshiroBreakdown.CreateDustB));
      cs03OshiroBreakdown.Add((Component) new Coroutine(cs03OshiroBreakdown.oshiro.MoveTo(new Vector2((float) (level.Bounds.Left - 64), cs03OshiroBreakdown.oshiro.Y))));
      cs03OshiroBreakdown.oshiro.Add((Component) new SoundSource("event:/char/oshiro/move_06_04d_exit"));
      yield return (object) 0.25f;
      yield return (object) cs03OshiroBreakdown.PanCamera(cs03OshiroBreakdown.player.CameraTarget.X);
      cs03OshiroBreakdown.EndCutscene(level);
    }

    private IEnumerator PanCamera(float to)
    {
      CS03_OshiroBreakdown cs03OshiroBreakdown = this;
      float from = cs03OshiroBreakdown.Level.Camera.X;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        cs03OshiroBreakdown.Level.Camera.X = from + (to - from) * Ease.CubeInOut(p);
        yield return (object) null;
      }
    }

    private IEnumerator WalkLeft()
    {
      (this.oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
      yield return (object) this.oshiro.MoveTo(this.origin + new Vector2(-24f, 0.0f));
      (this.oshiro.Sprite as OshiroSprite).AllowSpriteChanges = true;
    }

    private IEnumerator WalkRight()
    {
      (this.oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
      yield return (object) this.oshiro.MoveTo(this.origin + new Vector2(0.0f, 0.0f));
      (this.oshiro.Sprite as OshiroSprite).AllowSpriteChanges = true;
    }

    private IEnumerator CreateDustA()
    {
      CS03_OshiroBreakdown cs03OshiroBreakdown = this;
      cs03OshiroBreakdown.Add((Component) new SoundSource(cs03OshiroBreakdown.oshiro.Position, "event:/game/03_resort/sequence_oshirofluff_pt1"));
      (cs03OshiroBreakdown.oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
      cs03OshiroBreakdown.oshiro.Sprite.Play("fall");
      Audio.Play("event:/char/oshiro/chat_collapse", cs03OshiroBreakdown.oshiro.Position);
      Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
      for (int i = 0; i < 4; ++i)
      {
        cs03OshiroBreakdown.Add((Component) new Coroutine(cs03OshiroBreakdown.MoveDust(cs03OshiroBreakdown.creatures[i], cs03OshiroBreakdown.creatureHomes[i])));
        Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
        if (i % 4 == 0)
        {
          Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
          cs03OshiroBreakdown.Level.Shake();
          Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
          yield return (object) 0.4f;
        }
        else
          yield return (object) 0.1f;
      }
      yield return (object) 0.5f;
    }

    private IEnumerator CreateDustB()
    {
      CS03_OshiroBreakdown cs03OshiroBreakdown = this;
      cs03OshiroBreakdown.Add((Component) new SoundSource(cs03OshiroBreakdown.oshiro.Position, "event:/game/03_resort/sequence_oshirofluff_pt2"));
      for (int i = 4; i < cs03OshiroBreakdown.creatures.Count; ++i)
      {
        cs03OshiroBreakdown.Add((Component) new Coroutine(cs03OshiroBreakdown.MoveDust(cs03OshiroBreakdown.creatures[i], cs03OshiroBreakdown.creatureHomes[i])));
        Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
        cs03OshiroBreakdown.Level.Shake();
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        if ((i - 4) % 4 == 0)
        {
          Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
          yield return (object) 0.4f;
        }
        else
          yield return (object) 0.1f;
      }
      yield return (object) 1f;
      while ((double) Distort.Anxiety > 0.0)
      {
        Distort.Anxiety -= Engine.DeltaTime;
        yield return (object) null;
      }
      yield return (object) cs03OshiroBreakdown.Level.ZoomBack(0.5f);
      yield return (object) cs03OshiroBreakdown.player.DummyWalkToExact(cs03OshiroBreakdown.Level.Bounds.Left + 200);
      yield return (object) 1f;
      Audio.Play("event:/char/oshiro/chat_get_up", cs03OshiroBreakdown.oshiro.Position);
      cs03OshiroBreakdown.oshiro.Sprite.Play("recover");
      yield return (object) 0.7f;
      cs03OshiroBreakdown.oshiro.Sprite.Scale.X = 1f;
      yield return (object) 0.5f;
    }

    private IEnumerator MoveDust(DustStaticSpinner creature, Vector2 to)
    {
      CS03_OshiroBreakdown cs03OshiroBreakdown = this;
      Vector2 begin = cs03OshiroBreakdown.oshiro.Position + new Vector2(0.0f, -12f);
      SimpleCurve curve = new SimpleCurve(begin, to, (to + begin) / 2f + Vector2.UnitY * (Calc.Random.NextFloat(60f) - 30f));
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        yield return (object) null;
        creature.Sprite.Scale = (float) (0.5 + (double) p * 0.5);
        creature.Position = curve.GetPoint(Ease.CubeOut(p));
        creature.Visible = true;
        if (cs03OshiroBreakdown.Scene.OnInterval(0.02f))
          cs03OshiroBreakdown.SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, creature.Position, Vector2.One * 4f);
      }
    }

    public override void OnEnd(Level level)
    {
      this.player.StateMachine.Locked = false;
      this.player.StateMachine.State = 0;
      if (this.WasSkipped)
      {
        this.player.X = (float) (level.Bounds.Left + 200);
        while (!this.player.OnGround())
          ++this.player.Y;
        for (int index = 0; index < this.creatures.Count; ++index)
        {
          this.creatures[index].ForceInstantiate();
          this.creatures[index].Visible = true;
          this.creatures[index].Position = this.creatureHomes[index];
        }
      }
      level.Camera.Position = this.player.CameraTarget;
      level.Remove((Entity) this.oshiro);
      level.Session.SetFlag("oshiro_breakdown");
    }
  }
}
