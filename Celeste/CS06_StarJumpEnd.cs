// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_StarJumpEnd
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
  public class CS06_StarJumpEnd : CutsceneEntity
  {
    public const string Flag = "plateau_2";
    private bool waiting = true;
    private bool shaking;
    private NPC theo;
    private Player player;
    private Bonfire bonfire;
    private BadelineDummy badeline;
    private Plateau plateau;
    private BreathingMinigame breathing;
    private List<ReflectionTentacles> tentacles = new List<ReflectionTentacles>();
    private Vector2 playerStart;
    private Vector2 cameraStart;
    private float anxietyFade;
    private SineWave anxietySine;
    private float anxietyJitter;
    private bool hidingNorthingLights;
    private bool charactersSpinning;
    private float maddySine;
    private float maddySineTarget;
    private float maddySineAnchorY;
    private SoundSource shakingLoopSfx;
    private bool baddyCircling;
    private BreathingRumbler rumbler;
    private int tentacleIndex;

    public CS06_StarJumpEnd(NPC theo, Player player, Vector2 playerStart, Vector2 cameraStart)
      : base()
    {
      this.Depth = 10100;
      this.theo = theo;
      this.player = player;
      this.playerStart = playerStart;
      this.cameraStart = cameraStart;
      this.Add((Component) (this.anxietySine = new SineWave(0.3f)));
    }

    public override void Added(Scene scene)
    {
      this.Level = scene as Level;
      this.bonfire = scene.Entities.FindFirst<Bonfire>();
      this.plateau = scene.Entities.FindFirst<Plateau>();
    }

    public override void Update()
    {
      base.Update();
      if (this.waiting && (double) this.player.Y <= (double) (this.Level.Bounds.Top + 160))
      {
        this.waiting = false;
        this.Start();
      }
      if (this.shaking)
        this.Level.Shake(0.2f);
      if (this.Level != null && this.Level.OnInterval(0.1f))
        this.anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
      Distort.Anxiety = this.anxietyFade * Math.Max(0.0f, (float) (0.0 + (double) this.anxietyJitter + (double) this.anxietySine.Value * 0.60000002384185791));
      this.maddySine = Calc.Approach(this.maddySine, this.maddySineTarget, 12f * Engine.DeltaTime);
      if ((double) this.maddySine <= 0.0)
        return;
      this.player.Y = this.maddySineAnchorY + (float) (Math.Sin((double) this.Level.TimeActive * 2.0) * 3.0) * this.maddySine;
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS06_StarJumpEnd cs06StarJumpEnd = this;
      level.Entities.FindFirst<StarJumpController>()?.RemoveSelf();
      foreach (Entity entity in level.Entities.FindAll<StarJumpBlock>())
        entity.Collidable = false;
      int center = level.Bounds.X + 160;
      Vector2 cutsceneCenter = new Vector2((float) center, (float) (level.Bounds.Top + 150));
      NorthernLights bg = level.Background.Get<NorthernLights>();
      level.CameraOffset.Y = -30f;
      cs06StarJumpEnd.Add((Component) new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -70f), 1.5f, Ease.CubeOut)));
      cs06StarJumpEnd.Add((Component) new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -120f), 2f, Ease.CubeInOut, 1.5f)));
      Tween.Set((Entity) cs06StarJumpEnd, Tween.TweenMode.Oneshot, 3f, Ease.CubeInOut, (Action<Tween>) (t => bg.OffsetY = t.Eased * 32f));
      if (cs06StarJumpEnd.player.StateMachine.State == 19)
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
      cs06StarJumpEnd.player.Dashes = 0;
      cs06StarJumpEnd.player.StateMachine.State = 11;
      cs06StarJumpEnd.player.DummyGravity = false;
      cs06StarJumpEnd.player.DummyAutoAnimate = false;
      cs06StarJumpEnd.player.Sprite.Play("fallSlow");
      cs06StarJumpEnd.player.Dashes = 1;
      cs06StarJumpEnd.player.Speed = new Vector2(0.0f, -80f);
      cs06StarJumpEnd.player.Facing = Facings.Right;
      cs06StarJumpEnd.player.ForceCameraUpdate = false;
      while ((double) cs06StarJumpEnd.player.Speed.Length() > 0.0 || cs06StarJumpEnd.player.Position != cutsceneCenter)
      {
        cs06StarJumpEnd.player.Speed = Calc.Approach(cs06StarJumpEnd.player.Speed, Vector2.Zero, 200f * Engine.DeltaTime);
        cs06StarJumpEnd.player.Position = Calc.Approach(cs06StarJumpEnd.player.Position, cutsceneCenter, 64f * Engine.DeltaTime);
        yield return (object) null;
      }
      cs06StarJumpEnd.player.Sprite.Play("spin");
      yield return (object) 3.5f;
      cs06StarJumpEnd.player.Facing = Facings.Right;
      level.Add((Entity) (cs06StarJumpEnd.badeline = new BadelineDummy(cs06StarJumpEnd.player.Position)));
      level.Displacement.AddBurst(cs06StarJumpEnd.player.Position, 0.5f, 8f, 48f, 0.5f);
      Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
      cs06StarJumpEnd.player.CreateSplitParticles();
      Audio.Play("event:/char/badeline/maddy_split");
      cs06StarJumpEnd.badeline.Sprite.Scale.X = -1f;
      Vector2 start = cs06StarJumpEnd.player.Position;
      Vector2 target = cutsceneCenter + new Vector2(-30f, 0.0f);
      cs06StarJumpEnd.maddySineAnchorY = cutsceneCenter.Y;
      float p;
      for (p = 0.0f; (double) p <= 1.0; p += 2f * Engine.DeltaTime)
      {
        yield return (object) null;
        if ((double) p > 1.0)
          p = 1f;
        cs06StarJumpEnd.player.Position = Vector2.Lerp(start, target, Ease.CubeOut(p));
        cs06StarJumpEnd.badeline.Position = new Vector2((float) center + ((float) center - cs06StarJumpEnd.player.X), cs06StarJumpEnd.player.Y);
      }
      start = new Vector2();
      target = new Vector2();
      cs06StarJumpEnd.charactersSpinning = true;
      cs06StarJumpEnd.Add((Component) new Coroutine(cs06StarJumpEnd.SpinCharacters()));
      cs06StarJumpEnd.SetMusicLayer(2);
      yield return (object) 1f;
      yield return (object) Textbox.Say("ch6_dreaming", new Func<IEnumerator>(cs06StarJumpEnd.TentaclesAppear), new Func<IEnumerator>(cs06StarJumpEnd.TentaclesGrab), new Func<IEnumerator>(cs06StarJumpEnd.FeatherMinigame), new Func<IEnumerator>(cs06StarJumpEnd.EndFeatherMinigame), new Func<IEnumerator>(cs06StarJumpEnd.StartCirclingPlayer));
      Audio.Play("event:/game/06_reflection/badeline_pull_whooshdown");
      cs06StarJumpEnd.Add((Component) new Coroutine(cs06StarJumpEnd.BadelineFlyDown()));
      yield return (object) 0.7f;
      foreach (Entity entity in level.Entities.FindAll<FlyFeather>())
        entity.RemoveSelf();
      foreach (Entity entity in level.Entities.FindAll<StarJumpBlock>())
        entity.RemoveSelf();
      foreach (Entity entity in level.Entities.FindAll<JumpThru>())
        entity.RemoveSelf();
      level.Shake();
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
      level.CameraOffset.Y = 0.0f;
      cs06StarJumpEnd.player.Sprite.Play("tentacle_pull");
      cs06StarJumpEnd.player.Speed.Y = 160f;
      FallEffects.Show(true);
      for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 3f)
      {
        cs06StarJumpEnd.player.Speed.Y += Engine.DeltaTime * 100f;
        if ((double) cs06StarJumpEnd.player.X < (double) (level.Bounds.X + 32))
          cs06StarJumpEnd.player.X = (float) (level.Bounds.X + 32);
        Rectangle bounds;
        if ((double) cs06StarJumpEnd.player.X > (double) (level.Bounds.Right - 32))
        {
          Player player = cs06StarJumpEnd.player;
          bounds = level.Bounds;
          double num = (double) (bounds.Right - 32);
          player.X = (float) num;
        }
        if ((double) p > 0.699999988079071)
          level.CameraOffset.Y -= 100f * Engine.DeltaTime;
        foreach (ReflectionTentacles tentacle in cs06StarJumpEnd.tentacles)
        {
          List<Vector2> nodes1 = tentacle.Nodes;
          bounds = level.Bounds;
          Vector2 vector2_1 = new Vector2((float) bounds.Center.X, cs06StarJumpEnd.player.Y + 300f);
          nodes1[0] = vector2_1;
          List<Vector2> nodes2 = tentacle.Nodes;
          bounds = level.Bounds;
          Vector2 vector2_2 = new Vector2((float) bounds.Center.X, cs06StarJumpEnd.player.Y + 600f);
          nodes2[1] = vector2_2;
        }
        FallEffects.SpeedMultiplier += Engine.DeltaTime * 0.75f;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
        yield return (object) null;
      }
      Audio.Play("event:/game/06_reflection/badeline_pull_impact");
      FallEffects.Show(false);
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
      level.Flash(Color.White);
      level.Session.Dreaming = false;
      level.CameraOffset.Y = 0.0f;
      level.Camera.Position = cs06StarJumpEnd.cameraStart;
      cs06StarJumpEnd.SetBloom(0.0f);
      cs06StarJumpEnd.bonfire.SetMode(Bonfire.Mode.Smoking);
      cs06StarJumpEnd.plateau.Depth = cs06StarJumpEnd.player.Depth + 10;
      cs06StarJumpEnd.plateau.Remove((Component) cs06StarJumpEnd.plateau.Occluder);
      cs06StarJumpEnd.player.Position = cs06StarJumpEnd.playerStart + new Vector2(0.0f, 8f);
      cs06StarJumpEnd.player.Speed = Vector2.Zero;
      cs06StarJumpEnd.player.Sprite.Play("tentacle_dangling");
      cs06StarJumpEnd.player.Facing = Facings.Left;
      cs06StarJumpEnd.theo.Position.X -= 24f;
      cs06StarJumpEnd.theo.Sprite.Play("alert");
      foreach (ReflectionTentacles tentacle in cs06StarJumpEnd.tentacles)
      {
        tentacle.Index = 0;
        tentacle.Nodes[0] = new Vector2((float) level.Bounds.Center.X, cs06StarJumpEnd.player.Y + 32f);
        tentacle.Nodes[1] = new Vector2((float) level.Bounds.Center.X, cs06StarJumpEnd.player.Y + 400f);
        tentacle.SnapTentacles();
      }
      cs06StarJumpEnd.shaking = true;
      cs06StarJumpEnd.Add((Component) (cs06StarJumpEnd.shakingLoopSfx = new SoundSource()));
      cs06StarJumpEnd.shakingLoopSfx.Play("event:/game/06_reflection/badeline_pull_rumble_loop");
      yield return (object) Textbox.Say("ch6_theo_watchout");
      Audio.Play("event:/game/06_reflection/badeline_pull_cliffbreak");
      Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
      cs06StarJumpEnd.shakingLoopSfx.Stop();
      cs06StarJumpEnd.shaking = false;
      for (int index = 0; (double) index < (double) cs06StarJumpEnd.plateau.Width; index += 8)
      {
        level.Add((Entity) Engine.Pooler.Create<Debris>().Init(cs06StarJumpEnd.plateau.Position + new Vector2((float) index + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3').BlastFrom(cs06StarJumpEnd.plateau.Center + new Vector2(0.0f, 8f)));
        level.Add((Entity) Engine.Pooler.Create<Debris>().Init(cs06StarJumpEnd.plateau.Position + new Vector2((float) index + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3').BlastFrom(cs06StarJumpEnd.plateau.Center + new Vector2(0.0f, 8f)));
      }
      cs06StarJumpEnd.plateau.RemoveSelf();
      cs06StarJumpEnd.bonfire.RemoveSelf();
      level.Shake();
      cs06StarJumpEnd.player.Speed.Y = 160f;
      cs06StarJumpEnd.player.Sprite.Play("tentacle_pull");
      cs06StarJumpEnd.player.ForceCameraUpdate = false;
      FadeWipe wipe = new FadeWipe((Scene) level, false, (Action) (() => this.EndCutscene(level)));
      wipe.Duration = 3f;
      target = level.Camera.Position;
      start = level.Camera.Position + new Vector2(0.0f, 400f);
      while ((double) wipe.Percent < 1.0)
      {
        level.Camera.Position = Vector2.Lerp(target, start, Ease.CubeIn(wipe.Percent));
        cs06StarJumpEnd.player.Speed.Y += 400f * Engine.DeltaTime;
        foreach (ReflectionTentacles tentacle in cs06StarJumpEnd.tentacles)
        {
          tentacle.Nodes[0] = new Vector2((float) level.Bounds.Center.X, cs06StarJumpEnd.player.Y + 300f);
          tentacle.Nodes[1] = new Vector2((float) level.Bounds.Center.X, cs06StarJumpEnd.player.Y + 600f);
        }
        yield return (object) null;
      }
      wipe = (FadeWipe) null;
      target = new Vector2();
      start = new Vector2();
    }

    private void SetMusicLayer(int index)
    {
      for (int index1 = 1; index1 <= 3; ++index1)
        this.Level.Session.Audio.Music.Layer(index1, index == index1);
      this.Level.Session.Audio.Apply();
    }

    private IEnumerator TentaclesAppear()
    {
      CS06_StarJumpEnd cs06StarJumpEnd = this;
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
      if (cs06StarJumpEnd.tentacleIndex == 0)
        Audio.Play("event:/game/06_reflection/badeline_freakout_1");
      else if (cs06StarJumpEnd.tentacleIndex == 1)
        Audio.Play("event:/game/06_reflection/badeline_freakout_2");
      else if (cs06StarJumpEnd.tentacleIndex == 2)
        Audio.Play("event:/game/06_reflection/badeline_freakout_3");
      else
        Audio.Play("event:/game/06_reflection/badeline_freakout_4");
      if (!cs06StarJumpEnd.hidingNorthingLights)
      {
        cs06StarJumpEnd.Add((Component) new Coroutine(cs06StarJumpEnd.NothernLightsDown()));
        cs06StarJumpEnd.hidingNorthingLights = true;
      }
      cs06StarJumpEnd.Level.Shake();
      cs06StarJumpEnd.anxietyFade += 0.1f;
      if (cs06StarJumpEnd.tentacleIndex == 0)
        cs06StarJumpEnd.SetMusicLayer(3);
      int num1 = 400;
      int num2 = 140;
      List<Vector2> startNodes = new List<Vector2>();
      startNodes.Add(new Vector2(cs06StarJumpEnd.Level.Camera.X + 160f, cs06StarJumpEnd.Level.Camera.Y + (float) num1));
      startNodes.Add(new Vector2(cs06StarJumpEnd.Level.Camera.X + 160f, (float) ((double) cs06StarJumpEnd.Level.Camera.Y + (double) num1 + 200.0)));
      ReflectionTentacles reflectionTentacles = new ReflectionTentacles();
      reflectionTentacles.Create(0.0f, 0, cs06StarJumpEnd.tentacles.Count, startNodes);
      reflectionTentacles.Nodes[0] = new Vector2(reflectionTentacles.Nodes[0].X, cs06StarJumpEnd.Level.Camera.Y + (float) num2);
      cs06StarJumpEnd.Level.Add((Entity) reflectionTentacles);
      cs06StarJumpEnd.tentacles.Add(reflectionTentacles);
      cs06StarJumpEnd.charactersSpinning = false;
      ++cs06StarJumpEnd.tentacleIndex;
      cs06StarJumpEnd.badeline.Sprite.Play("angry");
      cs06StarJumpEnd.maddySineTarget = 1f;
      yield return (object) null;
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator TentaclesGrab()
    {
        this.maddySineTarget = 0f;
        Audio.Play("event:/game/06_reflection/badeline_freakout_5");
        this.player.Sprite.Play("tentacle_grab", false, false);
        yield return 0.1f;
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        this.Level.Shake(0.3f);
        this.rumbler = new BreathingRumbler();
        this.Level.Add(this.rumbler);
        yield break;
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator StartCirclingPlayer()
    {
        base.Add(new Coroutine(this.BadelineCirclePlayer(), true));
        Vector2 from = this.player.Position;
        Vector2 to = new Vector2((float)this.Level.Bounds.Center.X, this.player.Y);
        Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.CubeOut, delegate (Tween t)
        {
            this.player.Position = Vector2.Lerp(from, to, t.Eased);
        }, null);
        yield return null;
        yield break;
    }

    private IEnumerator EndCirclingPlayer()
    {
      this.baddyCircling = false;
      yield return (object) null;
    }

    private IEnumerator BadelineCirclePlayer()
    {
      CS06_StarJumpEnd cs06StarJumpEnd = this;
      float offset = 0.0f;
      float dist = (cs06StarJumpEnd.badeline.Position - cs06StarJumpEnd.player.Position).Length();
      cs06StarJumpEnd.baddyCircling = true;
      while (cs06StarJumpEnd.baddyCircling)
      {
        offset -= Engine.DeltaTime * 4f;
        dist = Calc.Approach(dist, 24f, Engine.DeltaTime * 32f);
        cs06StarJumpEnd.badeline.Position = cs06StarJumpEnd.player.Position + Calc.AngleToVector(offset, dist);
        int num = Math.Sign(cs06StarJumpEnd.player.X - cs06StarJumpEnd.badeline.X);
        if (num != 0)
          cs06StarJumpEnd.badeline.Sprite.Scale.X = (float) num;
        if (cs06StarJumpEnd.Level.OnInterval(0.1f))
          TrailManager.Add((Entity) cs06StarJumpEnd.badeline, Player.NormalHairColor);
        yield return (object) null;
      }
      cs06StarJumpEnd.badeline.Sprite.Scale.X = -1f;
      yield return (object) cs06StarJumpEnd.badeline.FloatTo(cs06StarJumpEnd.player.Position + new Vector2(40f, -16f), new int?(-1), false);
    }

    private IEnumerator FeatherMinigame()
    {
      CS06_StarJumpEnd cs06StarJumpEnd = this;
      cs06StarJumpEnd.breathing = new BreathingMinigame(false, cs06StarJumpEnd.rumbler);
      cs06StarJumpEnd.Level.Add((Entity) cs06StarJumpEnd.breathing);
      while (!cs06StarJumpEnd.breathing.Pausing)
        yield return (object) null;
    }

    private IEnumerator EndFeatherMinigame()
    {
      this.baddyCircling = false;
      this.breathing.Pausing = false;
      while (!this.breathing.Completed)
        yield return (object) null;
      this.breathing = (BreathingMinigame) null;
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator BadelineFlyDown()
    {
        this.badeline.Sprite.Play("fallFast", false, false);
        this.badeline.FloatSpeed = 600f;
        this.badeline.FloatAccel = 1200f;
        yield return this.badeline.FloatTo(new Vector2(this.badeline.X, this.Level.Camera.Y + 200f), null, true, true, false);
        this.badeline.RemoveSelf();
        yield break;
    }

    private IEnumerator NothernLightsDown()
    {
      NorthernLights bg = this.Level.Background.Get<NorthernLights>();
      if (bg != null)
      {
        while ((double) bg.NorthernLightsAlpha > 0.0)
        {
          bg.NorthernLightsAlpha -= Engine.DeltaTime * 0.5f;
          yield return (object) null;
        }
      }
    }

    private IEnumerator SpinCharacters()
    {
      Vector2 maddyStart = this.player.Position;
      Vector2 baddyStart = this.badeline.Position;
      Vector2 center = (maddyStart + baddyStart) / 2f;
      float dist = Math.Abs(maddyStart.X - center.X);
      float timer = 1.57079637f;
      this.player.Sprite.Play("spin");
      this.badeline.Sprite.Play("spin");
      this.badeline.Sprite.Scale.X = 1f;
      while (this.charactersSpinning)
      {
        int frame = (int) ((double) timer / 6.2831854820251465 * 14.0 + 10.0);
        this.player.Sprite.SetAnimationFrame(frame);
        this.badeline.Sprite.SetAnimationFrame(frame + 7);
        float num1 = (float) Math.Sin((double) timer);
        float num2 = (float) Math.Cos((double) timer);
        this.player.Position = center - new Vector2(num1 * dist, num2 * 8f);
        this.badeline.Position = center + new Vector2(num1 * dist, num2 * 8f);
        timer += Engine.DeltaTime * 2f;
        yield return (object) null;
      }
      this.player.Facing = Facings.Right;
      this.player.Sprite.Play("fallSlow");
      this.badeline.Sprite.Scale.X = -1f;
      this.badeline.Sprite.Play("angry");
      this.badeline.AutoAnimator.Enabled = false;
      Vector2 maddyFrom = this.player.Position;
      Vector2 baddyFrom = this.badeline.Position;
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 3f)
      {
        this.player.Position = Vector2.Lerp(maddyFrom, maddyStart, Ease.CubeOut(p));
        this.badeline.Position = Vector2.Lerp(baddyFrom, baddyStart, Ease.CubeOut(p));
        yield return (object) null;
      }
    }

    public override void OnEnd(Level level)
    {
      if (this.rumbler != null)
      {
        this.rumbler.RemoveSelf();
        this.rumbler = (BreathingRumbler) null;
      }
      if (this.breathing != null)
        this.breathing.RemoveSelf();
      this.SetBloom(0.0f);
      level.Session.Audio.Music.Event = (string) null;
      level.Session.Audio.Apply();
      level.Remove((Entity) this.player);
      level.UnloadLevel();
      level.EndCutscene();
      level.Session.SetFlag("plateau_2");
      level.SnapColorGrade(AreaData.Get((Scene) level).ColorGrade);
      level.Session.Dreaming = false;
      level.Session.FirstLevel = false;
      if (this.WasSkipped)
        level.OnEndOfFrame += (Action) (() =>
        {
          level.Session.Level = "00";
          Session session = level.Session;
          Level level1 = level;
          Rectangle bounds = level.Bounds;
          double left = (double) bounds.Left;
          bounds = level.Bounds;
          double bottom = (double) bounds.Bottom;
          Vector2 from = new Vector2((float) left, (float) bottom);
          Vector2? nullable = new Vector2?(level1.GetSpawnPoint(from));
          session.RespawnPoint = nullable;
          level.LoadLevel(Player.IntroTypes.None);
          FallEffects.Show(false);
          level.Session.Audio.Music.Event = "event:/music/lvl6/main";
          level.Session.Audio.Apply();
        });
      else
        Engine.Scene = (Scene) new OverworldReflectionsFall(level, (Action) (() =>
        {
          Audio.SetAmbience((string) null);
          level.Session.Level = "04";
          Session session = level.Session;
          Level level2 = level;
          Rectangle bounds = level.Bounds;
          double x = (double) bounds.Center.X;
          bounds = level.Bounds;
          double top = (double) bounds.Top;
          Vector2 from = new Vector2((float) x, (float) top);
          Vector2? nullable = new Vector2?(level2.GetSpawnPoint(from));
          session.RespawnPoint = nullable;
          level.LoadLevel(Player.IntroTypes.Fall);
          level.Add((Entity) new BackgroundFadeIn(Color.Black, 2f, 30f));
          level.Entities.UpdateLists();
          foreach (CrystalStaticSpinner entity in level.Tracker.GetEntities<CrystalStaticSpinner>())
            entity.ForceInstantiate();
        }));
    }

    private void SetBloom(float add)
    {
      this.Level.Session.BloomBaseAdd = add;
      this.Level.Bloom.Base = AreaData.Get((Scene) this.Level).BloomBase + add;
    }
  }
}
