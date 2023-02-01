// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_BossIntro
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class CS06_BossIntro : CutsceneEntity
  {
    public const string Flag = "boss_intro";
    private Player player;
    private FinalBoss boss;
    private Vector2 bossEndPosition;
    private BadelineAutoAnimator animator;
    private float playerTargetX;

    public CS06_BossIntro(float playerTargetX, Player player, FinalBoss boss)
      : base()
    {
      this.player = player;
      this.boss = boss;
      this.playerTargetX = playerTargetX;
      this.bossEndPosition = boss.Position + new Vector2(0.0f, -16f);
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS06_BossIntro cs06BossIntro = this;
      cs06BossIntro.player.StateMachine.State = 11;
      cs06BossIntro.player.StateMachine.Locked = true;
      while (!cs06BossIntro.player.Dead && !cs06BossIntro.player.OnGround())
        yield return (object) null;
      while (cs06BossIntro.player.Dead)
        yield return (object) null;
      cs06BossIntro.player.Facing = Facings.Right;
      cs06BossIntro.Add((Component) new Coroutine(CutsceneEntity.CameraTo(new Vector2((float) (((double) cs06BossIntro.player.X + (double) cs06BossIntro.boss.X) / 2.0 - 160.0), (float) (level.Bounds.Bottom - 180)), 1f)));
      yield return (object) 0.5f;
      if (!cs06BossIntro.player.Dead)
        yield return (object) cs06BossIntro.player.DummyWalkToExact((int) ((double) cs06BossIntro.playerTargetX - 8.0));
      cs06BossIntro.player.Facing = Facings.Right;
      yield return (object) Textbox.Say("ch6_boss_start", new Func<IEnumerator>(cs06BossIntro.BadelineFloat), new Func<IEnumerator>(cs06BossIntro.PlayerStepForward));
      yield return (object) level.ZoomBack(0.5f);
      cs06BossIntro.EndCutscene(level);
    }

    private IEnumerator BadelineFloat()
    {
      CS06_BossIntro cs06BossIntro = this;
      cs06BossIntro.Add((Component) new Coroutine(cs06BossIntro.Level.ZoomTo(new Vector2(170f, 110f), 2f, 1f)));
      Audio.Play("event:/char/badeline/boss_prefight_getup", cs06BossIntro.boss.Position);
      cs06BossIntro.boss.Sitting = false;
      cs06BossIntro.boss.NormalSprite.Play("fallSlow");
      cs06BossIntro.boss.NormalSprite.Scale.X = -1f;
      cs06BossIntro.boss.Add((Component) (cs06BossIntro.animator = new BadelineAutoAnimator()));
      float fromY = cs06BossIntro.boss.Y;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
      {
        cs06BossIntro.boss.Position.Y = MathHelper.Lerp(fromY, cs06BossIntro.bossEndPosition.Y, Ease.CubeInOut(p));
        yield return (object) null;
      }
    }

    private IEnumerator PlayerStepForward()
    {
      yield return (object) this.player.DummyWalkToExact((int) this.player.X + 8);
    }

    public override void OnEnd(Level level)
    {
      if (this.WasSkipped && this.player != null)
      {
        this.player.X = this.playerTargetX;
        Player player;
        for (; !this.player.OnGround() && (double) this.player.Y < (double) level.Bounds.Bottom; ++player.Y)
          player = this.player;
      }
      this.player.StateMachine.Locked = false;
      this.player.StateMachine.State = 0;
      this.boss.Position = this.bossEndPosition;
      if (this.boss.NormalSprite != null)
      {
        this.boss.NormalSprite.Scale.X = -1f;
        this.boss.NormalSprite.Play("laugh");
      }
      this.boss.Sitting = false;
      if (this.animator != null)
        this.boss.Remove((Component) this.animator);
      level.Session.SetFlag("boss_intro");
    }
  }
}
