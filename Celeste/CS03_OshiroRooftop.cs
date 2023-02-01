// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_OshiroRooftop
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class CS03_OshiroRooftop : CutsceneEntity
  {
    public const string Flag = "oshiro_resort_roof";
    private const float playerEndPosition = 170f;
    private Player player;
    private NPC oshiro;
    private BadelineDummy evil;
    private Vector2 bossSpawnPosition;
    private float anxiety;
    private float anxietyFlicker;
    private Sprite bossSprite = GFX.SpriteBank.Create("oshiro_boss");
    private float bossSpriteOffset;
    private bool oshiroRumble;

    public CS03_OshiroRooftop(NPC oshiro)
      : base()
    {
      this.oshiro = oshiro;
    }

    public override void OnBegin(Level level)
    {
      this.bossSpawnPosition = new Vector2(this.oshiro.X, (float) (level.Bounds.Bottom - 40));
      this.Add((Component) new Coroutine(this.Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      while (cs03OshiroRooftop.player == null)
      {
        cs03OshiroRooftop.player = cs03OshiroRooftop.Scene.Tracker.GetEntity<Player>();
        if (cs03OshiroRooftop.player == null)
          yield return (object) null;
        else
          break;
      }
      cs03OshiroRooftop.player.StateMachine.State = 11;
      cs03OshiroRooftop.player.StateMachine.Locked = true;
      while (!cs03OshiroRooftop.player.OnGround() || (double) cs03OshiroRooftop.player.Speed.Y < 0.0)
        yield return (object) null;
      yield return (object) 0.6f;
      cs03OshiroRooftop.evil = new BadelineDummy(new Vector2(cs03OshiroRooftop.oshiro.X - 40f, (float) (level.Bounds.Bottom - 60)));
      cs03OshiroRooftop.evil.Sprite.Scale.X = 1f;
      cs03OshiroRooftop.evil.Appear(level);
      level.Add((Entity) cs03OshiroRooftop.evil);
      yield return (object) 0.1f;
      cs03OshiroRooftop.player.Facing = Facings.Left;
      yield return (object) Textbox.Say("CH3_OSHIRO_START_CHASE", new Func<IEnumerator>(cs03OshiroRooftop.MaddyWalkAway), new Func<IEnumerator>(cs03OshiroRooftop.MaddyTurnAround), new Func<IEnumerator>(cs03OshiroRooftop.EnterOshiro), new Func<IEnumerator>(cs03OshiroRooftop.OshiroGetsAngry));
      yield return (object) cs03OshiroRooftop.OshiroTransform();
      cs03OshiroRooftop.Add((Component) new Coroutine(cs03OshiroRooftop.AnxietyAndCameraOut()));
      yield return (object) level.ZoomBack(0.5f);
      yield return (object) 0.25f;
      cs03OshiroRooftop.EndCutscene(level);
    }

    private IEnumerator MaddyWalkAway()
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      Level scene = cs03OshiroRooftop.Scene as Level;
      cs03OshiroRooftop.Add((Component) new Coroutine(cs03OshiroRooftop.player.DummyWalkTo((float) scene.Bounds.Left + 170f)));
      yield return (object) 0.2f;
      Audio.Play("event:/game/03_resort/suite_bad_moveroof", cs03OshiroRooftop.evil.Position);
      cs03OshiroRooftop.Add((Component) new Coroutine(cs03OshiroRooftop.evil.FloatTo(cs03OshiroRooftop.evil.Position + new Vector2(80f, 30f))));
      yield return (object) null;
    }

    private IEnumerator MaddyTurnAround()
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      yield return (object) 0.25f;
      cs03OshiroRooftop.player.Facing = Facings.Left;
      yield return (object) 0.1f;
      Level level = cs03OshiroRooftop.SceneAs<Level>();
      yield return (object) level.ZoomTo(new Vector2(150f, (float) ((double) cs03OshiroRooftop.bossSpawnPosition.Y - (double) level.Bounds.Y - 8.0)), 2f, 0.5f);
    }

    private IEnumerator EnterOshiro()
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      yield return (object) 0.3f;
      cs03OshiroRooftop.bossSpriteOffset = (cs03OshiroRooftop.bossSprite.Justify.Value.Y - cs03OshiroRooftop.oshiro.Sprite.Justify.Value.Y) * cs03OshiroRooftop.bossSprite.Height;
      cs03OshiroRooftop.oshiro.Visible = true;
      cs03OshiroRooftop.oshiro.Sprite.Scale.X = 1f;
      cs03OshiroRooftop.Add((Component) new Coroutine(cs03OshiroRooftop.oshiro.MoveTo(cs03OshiroRooftop.bossSpawnPosition - new Vector2(0.0f, cs03OshiroRooftop.bossSpriteOffset))));
      cs03OshiroRooftop.oshiro.Add((Component) new SoundSource("event:/char/oshiro/move_07_roof00_enter"));
      float from = cs03OshiroRooftop.Level.ZoomFocusPoint.X;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.7f)
      {
        cs03OshiroRooftop.Level.ZoomFocusPoint.X = from + (126f - from) * Ease.CubeInOut(p);
        yield return (object) null;
      }
      yield return (object) 0.3f;
      cs03OshiroRooftop.player.Facing = Facings.Left;
      yield return (object) 0.1f;
      cs03OshiroRooftop.evil.Sprite.Scale.X = -1f;
    }

    private IEnumerator OshiroGetsAngry()
    {
      yield return (object) 0.1f;
      this.evil.Vanish();
      Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
      this.evil = (BadelineDummy) null;
      yield return (object) 0.8f;
      Audio.Play("event:/char/oshiro/boss_transform_begin", this.oshiro.Position);
      this.oshiro.Remove((Component) this.oshiro.Sprite);
      this.oshiro.Sprite = this.bossSprite;
      this.oshiro.Sprite.Play("transformStart");
      this.oshiro.Y += this.bossSpriteOffset;
      this.oshiro.Add((Component) this.oshiro.Sprite);
      this.oshiro.Depth = -12500;
      this.oshiroRumble = true;
      yield return (object) 1f;
    }

    private IEnumerator OshiroTransform()
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      yield return (object) 0.2f;
      Audio.Play("event:/char/oshiro/boss_transform_burst", cs03OshiroRooftop.oshiro.Position);
      cs03OshiroRooftop.oshiro.Sprite.Play("transformFinish");
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
      cs03OshiroRooftop.SceneAs<Level>().Shake(0.5f);
      cs03OshiroRooftop.SetChaseMusic();
      while ((double) cs03OshiroRooftop.anxiety < 0.5)
      {
        cs03OshiroRooftop.anxiety = Calc.Approach(cs03OshiroRooftop.anxiety, 0.5f, Engine.DeltaTime * 0.5f);
        yield return (object) null;
      }
      yield return (object) 0.25f;
    }

    private IEnumerator AnxietyAndCameraOut()
    {
      CS03_OshiroRooftop cs03OshiroRooftop = this;
      Level level = cs03OshiroRooftop.Scene as Level;
      Vector2 from = level.Camera.Position;
      Vector2 to = cs03OshiroRooftop.player.CameraTarget;
      for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime * 2f)
      {
        cs03OshiroRooftop.anxiety = Calc.Approach(cs03OshiroRooftop.anxiety, 0.0f, Engine.DeltaTime * 4f);
        level.Camera.Position = from + (to - from) * Ease.CubeInOut(t);
        yield return (object) null;
      }
    }

    private void SetChaseMusic()
    {
      Level scene = this.Scene as Level;
      scene.Session.Audio.Music.Event = "event:/music/lvl3/oshiro_chase";
      scene.Session.Audio.Apply();
    }

    public override void OnEnd(Level level)
    {
      Distort.Anxiety = this.anxiety = this.anxietyFlicker = 0.0f;
      if (this.evil != null)
        level.Remove((Entity) this.evil);
      this.player = this.Scene.Tracker.GetEntity<Player>();
      if (this.player != null)
      {
        this.player.StateMachine.Locked = false;
        this.player.StateMachine.State = 0;
        this.player.X = (float) level.Bounds.Left + 170f;
        this.player.Speed.Y = 0.0f;
        while (this.player.CollideCheck<Solid>())
          --this.player.Y;
        level.Camera.Position = this.player.CameraTarget;
      }
      if (this.WasSkipped)
        this.SetChaseMusic();
      this.oshiro.RemoveSelf();
      this.Scene.Add((Entity) new AngryOshiro(this.bossSpawnPosition, true));
      level.Session.RespawnPoint = new Vector2?(new Vector2((float) level.Bounds.Left + 170f, (float) (level.Bounds.Top + 160)));
      level.Session.SetFlag("oshiro_resort_roof");
    }

    public override void Update()
    {
      Distort.Anxiety = this.anxiety + this.anxiety * this.anxietyFlicker;
      if (this.Scene.OnInterval(0.05f))
        this.anxietyFlicker = Calc.Random.NextFloat(0.4f) - 0.2f;
      base.Update();
      if (!this.oshiroRumble)
        return;
      Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
    }
  }
}
