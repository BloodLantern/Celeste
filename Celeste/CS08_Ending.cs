// Decompiled with JetBrains decompiler
// Type: Celeste.CS08_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class CS08_Ending : CutsceneEntity
  {
    private Player player;
    private NPC08_Granny granny;
    private NPC08_Theo theo;
    private BadelineDummy badeline;
    private Entity oshiro;
    private Monocle.Image vignette;
    private Monocle.Image vignettebg;
    private string endingDialog;
    private float fade;
    private bool showVersion;
    private float versionAlpha;
    private Coroutine cutscene;
    private string version = Celeste.Instance.Version.ToString();

    public CS08_Ending()
      : base(false, true)
    {
      this.Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
      this.RemoveOnSkipped = false;
    }

    public override void OnBegin(Level level)
    {
      level.SaveQuitDisabled = true;
      int totalStrawberries = SaveData.Instance.TotalStrawberries;
      string id;
      if (totalStrawberries < 20)
      {
        id = "final1";
        this.endingDialog = "EP_PIE_DISAPPOINTED";
      }
      else if (totalStrawberries < 50)
      {
        id = "final2";
        this.endingDialog = "EP_PIE_GROSSED_OUT";
      }
      else if (totalStrawberries < 90)
      {
        id = "final3";
        this.endingDialog = "EP_PIE_OKAY";
      }
      else if (totalStrawberries < 150)
      {
        id = "final4";
        this.endingDialog = "EP_PIE_REALLY_GOOD";
      }
      else
      {
        id = "final5";
        this.endingDialog = "EP_PIE_AMAZING";
      }
      this.Add((Component) (this.vignettebg = new Monocle.Image(GFX.Portraits["finalbg"])));
      this.vignettebg.Visible = false;
      this.Add((Component) (this.vignette = new Monocle.Image(GFX.Portraits[id])));
      this.vignette.Visible = false;
      this.vignette.CenterOrigin();
      this.vignette.Position = Celeste.TargetCenter;
      this.Add((Component) (this.cutscene = new Coroutine(this.Cutscene(level))));
    }

    private IEnumerator Cutscene(Level level)
    {
      CS08_Ending cs08Ending = this;
      level.ZoomSnap(new Vector2(164f, 120f), 2f);
      level.Wipe.Cancel();
      FadeWipe fadeWipe1 = new FadeWipe((Scene) level, true);
      while (cs08Ending.player == null)
      {
        cs08Ending.granny = level.Entities.FindFirst<NPC08_Granny>();
        cs08Ending.theo = level.Entities.FindFirst<NPC08_Theo>();
        cs08Ending.player = level.Tracker.GetEntity<Player>();
        yield return (object) null;
      }
      cs08Ending.player.StateMachine.State = 11;
      yield return (object) 1f;
      yield return (object) cs08Ending.player.DummyWalkToExact((int) cs08Ending.player.X + 16);
      yield return (object) 0.25f;
      yield return (object) Textbox.Say("EP_CABIN", new Func<IEnumerator>(cs08Ending.BadelineEmerges), new Func<IEnumerator>(cs08Ending.OshiroEnters), new Func<IEnumerator>(cs08Ending.OshiroSettles), new Func<IEnumerator>(cs08Ending.MaddyTurns));
      FadeWipe fadeWipe2 = new FadeWipe((Scene) cs08Ending.Level, false);
      fadeWipe2.Duration = 1.5f;
      yield return (object) fadeWipe2.Wait();
      cs08Ending.fade = 1f;
      yield return (object) Textbox.Say("EP_PIE_START");
      yield return (object) 0.5f;
      cs08Ending.vignettebg.Visible = true;
      cs08Ending.vignette.Visible = true;
      cs08Ending.vignettebg.Color = Color.Black;
      cs08Ending.vignette.Color = Color.White * 0.0f;
      cs08Ending.Add((Component) cs08Ending.vignette);
      float p1;
      for (p1 = 0.0f; (double) p1 < 1.0; p1 += Engine.DeltaTime)
      {
        cs08Ending.vignette.Color = Color.White * Ease.CubeIn(p1);
        cs08Ending.vignette.Scale = Vector2.One * (float) (1.0 + 0.25 * (1.0 - (double) p1));
        cs08Ending.vignette.Rotation = (float) (0.05000000074505806 * (1.0 - (double) p1));
        yield return (object) null;
      }
      cs08Ending.vignette.Color = Color.White;
      cs08Ending.vignettebg.Color = Color.White;
      yield return (object) 2f;
      p1 = 1f;
      float p2;
      for (p2 = 0.0f; (double) p2 < 1.0; p2 += Engine.DeltaTime / p1)
      {
        float amount = Ease.CubeOut(p2);
        cs08Ending.vignette.Position = Vector2.Lerp(Celeste.TargetCenter, Celeste.TargetCenter + new Vector2(0.0f, 140f), amount);
        cs08Ending.vignette.Scale = Vector2.One * (float) (0.64999997615814209 + 0.34999999403953552 * (1.0 - (double) amount));
        cs08Ending.vignette.Rotation = -0.025f * amount;
        yield return (object) null;
      }
      yield return (object) Textbox.Say(cs08Ending.endingDialog);
      yield return (object) 0.25f;
      p1 = 2f;
      Vector2 posFrom = cs08Ending.vignette.Position;
      p2 = cs08Ending.vignette.Rotation;
      float scaleFrom = cs08Ending.vignette.Scale.X;
      for (float p3 = 0.0f; (double) p3 < 1.0; p3 += Engine.DeltaTime / p1)
      {
        float amount = Ease.CubeOut(p3);
        cs08Ending.vignette.Position = Vector2.Lerp(posFrom, Celeste.TargetCenter, amount);
        cs08Ending.vignette.Scale = Vector2.One * MathHelper.Lerp(scaleFrom, 1f, amount);
        cs08Ending.vignette.Rotation = MathHelper.Lerp(p2, 0.0f, amount);
        yield return (object) null;
      }
      posFrom = new Vector2();
      cs08Ending.EndCutscene(level, false);
    }

    public override void OnEnd(Level level)
    {
      this.vignette.Visible = true;
      this.vignette.Color = Color.White;
      this.vignette.Position = Celeste.TargetCenter;
      this.vignette.Scale = Vector2.One;
      this.vignette.Rotation = 0.0f;
      if (this.player != null)
        this.player.Speed = Vector2.Zero;
      this.Scene.Entities.FindFirst<Textbox>()?.RemoveSelf();
      this.cutscene.RemoveSelf();
      this.Add((Component) new Coroutine(this.EndingRoutine()));
    }

    private IEnumerator EndingRoutine()
    {
      CS08_Ending cs08Ending = this;
      cs08Ending.Level.InCutscene = true;
      cs08Ending.Level.PauseLock = true;
      yield return (object) 0.5f;
      TimeSpan timeSpan = TimeSpan.FromTicks(SaveData.Instance.Time);
      string str = ((int) timeSpan.TotalHours).ToString() + timeSpan.ToString("\\:mm\\:ss\\.fff");
      StrawberriesCounter strawbs = new StrawberriesCounter(true, SaveData.Instance.TotalStrawberries, 175, true);
      DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, true, SaveData.Instance.TotalDeaths);
      CS08_Ending.TimeDisplay time = new CS08_Ending.TimeDisplay(str);
      float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(str);
      cs08Ending.Add((Component) strawbs);
      cs08Ending.Add((Component) deaths);
      cs08Ending.Add((Component) time);
      Vector2 from = new Vector2(960f, 1180f);
      Vector2 to = new Vector2(960f, 940f);
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / 0.5f)
      {
        Vector2 vector2 = Vector2.Lerp(from, to, Ease.CubeOut(p));
        strawbs.Position = vector2 + new Vector2(-170f, 0.0f);
        deaths.Position = vector2 + new Vector2(170f, 0.0f);
        time.Position = vector2 + new Vector2((float) (-(double) timeWidth / 2.0), 100f);
        yield return (object) null;
      }
      strawbs = (StrawberriesCounter) null;
      deaths = (DeathsCounter) null;
      time = (CS08_Ending.TimeDisplay) null;
      from = new Vector2();
      to = new Vector2();
      cs08Ending.showVersion = true;
      yield return (object) 0.25f;
      while (!Input.MenuConfirm.Pressed)
        yield return (object) null;
      cs08Ending.showVersion = false;
      yield return (object) 0.25f;
      cs08Ending.Level.CompleteArea(false);
    }

    private IEnumerator MaddyTurns()
    {
      yield return (object) 0.1f;
      this.player.Facing = (Facings) (-(int) this.player.Facing);
      yield return (object) 0.1f;
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator BadelineEmerges()
    {
        this.Level.Displacement.AddBurst(this.player.Center, 0.5f, 8f, 32f, 0.5f, null, null);
        this.Level.Session.Inventory.Dashes = 1;
        this.player.Dashes = 1;
        this.Level.Add(this.badeline = new BadelineDummy(this.player.Position));
        Audio.Play("event:/char/badeline/maddy_split", this.player.Position);
        this.badeline.Sprite.Scale.X = 1f;
        yield return this.badeline.FloatTo(this.player.Position + new Vector2(-12f, -16f), new int?(1), false, false, false);
        yield break;
    }

    private IEnumerator OshiroEnters()
    {
      CS08_Ending cs08Ending = this;
      FadeWipe fadeWipe = new FadeWipe((Scene) cs08Ending.Level, false);
      fadeWipe.Duration = 1.5f;
      yield return (object) fadeWipe.Wait();
      cs08Ending.fade = 1f;
      yield return (object) 0.25f;
      float x = cs08Ending.player.X;
      cs08Ending.player.X = cs08Ending.granny.X + 8f;
      cs08Ending.badeline.X = cs08Ending.player.X + 12f;
      cs08Ending.player.Facing = Facings.Left;
      cs08Ending.badeline.Sprite.Scale.X = -1f;
      cs08Ending.granny.X = x + 8f;
      cs08Ending.theo.X += 16f;
      cs08Ending.Level.Add(cs08Ending.oshiro = new Entity(new Vector2(cs08Ending.granny.X - 24f, cs08Ending.granny.Y + 4f)));
      OshiroSprite oshiroSprite = new OshiroSprite(1);
      cs08Ending.oshiro.Add((Component) oshiroSprite);
      cs08Ending.fade = 0.0f;
      new FadeWipe((Scene) cs08Ending.Level, true).Duration = 1f;
      yield return (object) 0.25f;
      while ((double) cs08Ending.oshiro.Y > (double) cs08Ending.granny.Y - 4.0)
      {
        cs08Ending.oshiro.Y -= Engine.DeltaTime * 32f;
        yield return (object) null;
      }
    }

    private IEnumerator OshiroSettles()
    {
      Vector2 from = this.oshiro.Position;
      Vector2 to = this.oshiro.Position + new Vector2(40f, 8f);
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        this.oshiro.Position = Vector2.Lerp(from, to, p);
        yield return (object) null;
      }
      this.granny.Sprite.Scale.X = 1f;
      yield return (object) null;
    }

    public override void Update()
    {
      this.versionAlpha = Calc.Approach(this.versionAlpha, this.showVersion ? 1f : 0.0f, Engine.DeltaTime * 5f);
      base.Update();
    }

    public override void Render()
    {
      if ((double) this.fade > 0.0)
        Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * this.fade);
      base.Render();
      if (Settings.Instance.SpeedrunClock == SpeedrunType.Off || (double) this.versionAlpha <= 0.0)
        return;
      AreaComplete.VersionNumberAndVariants(this.version, this.versionAlpha, 1f);
    }

    public class TimeDisplay : Component
    {
      public Vector2 Position;
      public string Time;

      public TimeDisplay(string time)
        : base(true, true)
      {
        this.Time = time;
      }

      public override void Render() => SpeedrunTimerDisplay.DrawTime(this.RenderPosition, this.Time);

      public Vector2 RenderPosition => ((this.Entity != null ? this.Entity.Position : Vector2.Zero) + this.Position).Round();
    }
  }
}
