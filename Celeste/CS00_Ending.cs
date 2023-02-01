// Decompiled with JetBrains decompiler
// Type: Celeste.CS00_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
  public class CS00_Ending : CutsceneEntity
  {
    private Player player;
    private BirdNPC bird;
    private Bridge bridge;
    private bool keyOffed;
    private PrologueEndingText endingText;

    public CS00_Ending(Player player, BirdNPC bird, Bridge bridge)
      : base(false, true)
    {
      this.player = player;
      this.bird = bird;
      this.bridge = bridge;
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS00_Ending cs00Ending = this;
      for (; (double) Engine.TimeRate > 0.0; Engine.TimeRate -= Engine.RawDeltaTime * 2f)
      {
        yield return (object) null;
        if ((double) Engine.TimeRate < 0.5 && cs00Ending.bridge != null)
          cs00Ending.bridge.StopCollapseLoop();
        level.StopShake();
        MInput.GamePads[Input.Gamepad].StopRumble();
      }
      Engine.TimeRate = 0.0f;
      cs00Ending.player.StateMachine.State = 11;
      cs00Ending.player.Facing = Facings.Right;
      yield return (object) cs00Ending.WaitFor(1f);
      EventInstance instance = Audio.Play("event:/game/general/bird_in", cs00Ending.bird.Position);
      cs00Ending.bird.Facing = Facings.Left;
      cs00Ending.bird.Sprite.Play("fall");
      float percent = 0.0f;
      Vector2 from = cs00Ending.bird.Position;
      Vector2 to = cs00Ending.bird.StartPosition;
      while ((double) percent < 1.0)
      {
        cs00Ending.bird.Position = from + (to - from) * Ease.QuadOut(percent);
        Audio.Position(instance, cs00Ending.bird.Position);
        if ((double) percent > 0.5)
          cs00Ending.bird.Sprite.Play("fly");
        percent += Engine.RawDeltaTime * 0.5f;
        yield return (object) null;
      }
      cs00Ending.bird.Position = to;
      instance = (EventInstance) null;
      from = new Vector2();
      to = new Vector2();
      Audio.Play("event:/game/general/bird_land_dirt", cs00Ending.bird.Position);
      Dust.Burst(cs00Ending.bird.Position, -1.57079637f, 12);
      cs00Ending.bird.Sprite.Play("idle");
      yield return (object) cs00Ending.WaitFor(0.5f);
      cs00Ending.bird.Sprite.Play("peck");
      yield return (object) cs00Ending.WaitFor(1.1f);
      yield return (object) cs00Ending.bird.ShowTutorial(new BirdTutorialGui((Entity) cs00Ending.bird, new Vector2(0.0f, -16f), (object) Dialog.Clean("tutorial_dash"), new object[3]
      {
        (object) new Vector2(1f, -1f),
        (object) "+",
        (object) BirdTutorialGui.ButtonPrompt.Dash
      }), true);
      while (true)
      {
        Vector2 aimVector = Input.GetAimVector();
        if ((double) aimVector.X <= 0.0 || (double) aimVector.Y >= 0.0 || !Input.Dash.Pressed)
          yield return (object) null;
        else
          break;
      }
      cs00Ending.player.StateMachine.State = 16;
      cs00Ending.player.Dashes = 0;
      level.Session.Inventory.Dashes = 1;
      Engine.TimeRate = 1f;
      cs00Ending.keyOffed = true;
      int num1 = (int) Audio.CurrentMusicEventInstance.triggerCue();
      cs00Ending.bird.Add((Component) new Coroutine(cs00Ending.bird.HideTutorial()));
      yield return (object) 0.25f;
      cs00Ending.bird.Add((Component) new Coroutine(cs00Ending.bird.StartleAndFlyAway()));
      while (!cs00Ending.player.Dead && !cs00Ending.player.OnGround())
        yield return (object) null;
      yield return (object) 2f;
      Audio.SetMusic("event:/music/lvl0/title_ping");
      yield return (object) 2f;
      cs00Ending.endingText = new PrologueEndingText(false);
      cs00Ending.Scene.Add((Entity) cs00Ending.endingText);
      Snow bgSnow = level.Background.Get<Snow>();
      Snow fgSnow = level.Foreground.Get<Snow>();
      level.Add((Monocle.Renderer) (level.HiresSnow = new HiresSnow()));
      level.HiresSnow.Alpha = 0.0f;
      float ease = 0.0f;
      while ((double) ease < 1.0)
      {
        ease += Engine.DeltaTime * 0.25f;
        float num2 = Ease.CubeInOut(ease);
        if (fgSnow != null)
          fgSnow.Alpha -= Engine.DeltaTime * 0.5f;
        if (bgSnow != null)
          bgSnow.Alpha -= Engine.DeltaTime * 0.5f;
        level.HiresSnow.Alpha = Calc.Approach(level.HiresSnow.Alpha, 1f, Engine.DeltaTime * 0.5f);
        cs00Ending.endingText.Position = new Vector2(960f, (float) (540.0 - 1080.0 * (1.0 - (double) num2)));
        level.Camera.Y = (float) level.Bounds.Top - 3900f * num2;
        yield return (object) null;
      }
      cs00Ending.EndCutscene(level);
    }

    private IEnumerator WaitFor(float time)
    {
      for (float t = 0.0f; (double) t < (double) time; t += Engine.RawDeltaTime)
        yield return (object) null;
    }

    public override void OnEnd(Level level)
    {
      if (this.WasSkipped)
      {
        if (this.bird != null)
          this.bird.Visible = false;
        if (this.player != null)
        {
          this.player.Position = new Vector2(2120f, 40f);
          this.player.StateMachine.State = 11;
          this.player.DummyAutoAnimate = false;
          this.player.Sprite.Play("tired");
          this.player.Speed = Vector2.Zero;
        }
        if (!this.keyOffed)
        {
          int num = (int) Audio.CurrentMusicEventInstance.triggerCue();
        }
        if (level.HiresSnow == null)
          level.Add((Monocle.Renderer) (level.HiresSnow = new HiresSnow()));
        level.HiresSnow.Alpha = 1f;
        Snow snow1 = level.Background.Get<Snow>();
        if (snow1 != null)
          snow1.Alpha = 0.0f;
        Snow snow2 = level.Foreground.Get<Snow>();
        if (snow2 != null)
          snow2.Alpha = 0.0f;
        if (this.endingText != null)
          level.Remove((Entity) this.endingText);
        level.Add((Entity) (this.endingText = new PrologueEndingText(true)));
        this.endingText.Position = new Vector2(960f, 540f);
        level.Camera.Y = (float) (level.Bounds.Top - 3900);
      }
      Engine.TimeRate = 1f;
      level.PauseLock = true;
      level.Entities.FindFirst<SpeedrunTimerDisplay>().CompleteTimer = 10f;
      level.Add((Entity) new CS00_Ending.EndingCutsceneDelay());
    }

    private class EndingCutsceneDelay : Entity
    {
      public EndingCutsceneDelay() => this.Add((Component) new Coroutine(this.Routine()));

      private IEnumerator Routine()
      {
        // ISSUE: reference to a compiler-generated field
        int num = this.\u003C\u003E1__state;
        CS00_Ending.EndingCutsceneDelay endingCutsceneDelay = this;
        if (num != 0)
        {
          if (num != 1)
            return false;
          // ISSUE: reference to a compiler-generated field
          this.\u003C\u003E1__state = -1;
          (endingCutsceneDelay.Scene as Level).CompleteArea(false);
          return false;
        }
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E1__state = -1;
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E2__current = (object) 3f;
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E1__state = 1;
        return true;
      }
    }
  }
}
