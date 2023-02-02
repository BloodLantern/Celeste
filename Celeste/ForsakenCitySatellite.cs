// Decompiled with JetBrains decompiler
// Type: Celeste.ForsakenCitySatellite
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
  public class ForsakenCitySatellite : Entity
  {
    private const string UnlockedFlag = "unlocked_satellite";
    public static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>()
    {
      {
        "U",
        Calc.HexToColor("f0f0f0")
      },
      {
        "L",
        Calc.HexToColor("9171f2")
      },
      {
        "DR",
        Calc.HexToColor("0a44e0")
      },
      {
        "UR",
        Calc.HexToColor("b32d00")
      },
      {
        "UL",
        Calc.HexToColor("ffcd37")
      }
    };
    public static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>()
    {
      {
        "U",
        "event:/game/01_forsaken_city/console_white"
      },
      {
        "L",
        "event:/game/01_forsaken_city/console_purple"
      },
      {
        "DR",
        "event:/game/01_forsaken_city/console_blue"
      },
      {
        "UR",
        "event:/game/01_forsaken_city/console_red"
      },
      {
        "UL",
        "event:/game/01_forsaken_city/console_yellow"
      }
    };
    public static readonly Dictionary<string, ParticleType> Particles = new Dictionary<string, ParticleType>();
    private static readonly string[] Code = new string[6]
    {
      "U",
      "L",
      "DR",
      "UR",
      "L",
      "UL"
    };
    private static List<string> uniqueCodes = new List<string>();
    private bool enabled;
    private List<string> currentInputs = new List<string>();
    private List<ForsakenCitySatellite.CodeBird> birds = new List<ForsakenCitySatellite.CodeBird>();
    private Vector2 gemSpawnPosition;
    private Vector2 birdFlyPosition;
    private Monocle.Image sprite;
    private Monocle.Image pulse;
    private Monocle.Image computer;
    private Monocle.Image computerScreen;
    private Sprite computerScreenNoise;
    private Monocle.Image computerScreenShine;
    private BloomPoint pulseBloom;
    private BloomPoint screenBloom;
    private Level level;
    private DashListener dashListener;
    private SoundSource birdFlyingSfx;
    private SoundSource birdThrustSfx;
    private SoundSource birdFinishSfx;
    private SoundSource staticLoopSfx;

    public ForsakenCitySatellite(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["objects/citysatellite/dish"])));
      this.Add((Component) (this.pulse = new Monocle.Image(GFX.Game["objects/citysatellite/light"])));
      this.Add((Component) (this.computer = new Monocle.Image(GFX.Game["objects/citysatellite/computer"])));
      this.Add((Component) (this.computerScreen = new Monocle.Image(GFX.Game["objects/citysatellite/computerscreen"])));
      this.Add((Component) (this.computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise")));
      this.Add((Component) (this.computerScreenShine = new Monocle.Image(GFX.Game["objects/citysatellite/computerscreenShine"])));
      this.sprite.JustifyOrigin(0.5f, 1f);
      this.pulse.JustifyOrigin(0.5f, 1f);
      this.Add((Component) new Coroutine(this.PulseRoutine()));
      this.Add((Component) (this.pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f)));
      this.Add((Component) (this.screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f)));
      this.computerScreenNoise.AddLoop("static", "", 0.05f);
      this.computerScreenNoise.Play("static");
      this.computer.Position = this.computerScreen.Position = this.computerScreenShine.Position = this.computerScreenNoise.Position = new Vector2(8f, 8f);
      this.birdFlyPosition = offset + data.Nodes[0];
      this.gemSpawnPosition = offset + data.Nodes[1];
      this.Add((Component) (this.dashListener = new DashListener()));
      this.dashListener.OnDash = (Action<Vector2>) (dir =>
      {
        string str = "";
        if ((double) dir.Y < 0.0)
          str = "U";
        else if ((double) dir.Y > 0.0)
          str = "D";
        if ((double) dir.X < 0.0)
          str += "L";
        else if ((double) dir.X > 0.0)
          str += "R";
        this.currentInputs.Add(str);
        if (this.currentInputs.Count > ForsakenCitySatellite.Code.Length)
          this.currentInputs.RemoveAt(0);
        if (this.currentInputs.Count != ForsakenCitySatellite.Code.Length)
          return;
        bool flag = true;
        for (int index = 0; index < ForsakenCitySatellite.Code.Length; ++index)
        {
          if (!this.currentInputs[index].Equals(ForsakenCitySatellite.Code[index]))
            flag = false;
        }
        if (!flag || (double) this.level.Camera.Left + 32.0 >= (double) this.gemSpawnPosition.X || !this.enabled)
          return;
        this.Add((Component) new Coroutine(this.UnlockGem()));
      });
      foreach (string str in ForsakenCitySatellite.Code)
      {
        if (!ForsakenCitySatellite.uniqueCodes.Contains(str))
          ForsakenCitySatellite.uniqueCodes.Add(str);
      }
      this.Depth = 8999;
      this.Add((Component) (this.staticLoopSfx = new SoundSource()));
      this.staticLoopSfx.Position = this.computer.Position;
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.level = scene as Level;
      this.enabled = !this.level.Session.HeartGem && !this.level.Session.GetFlag("unlocked_satellite");
      if (this.enabled)
      {
        foreach (string uniqueCode in ForsakenCitySatellite.uniqueCodes)
        {
          ForsakenCitySatellite.CodeBird codeBird = new ForsakenCitySatellite.CodeBird(this.birdFlyPosition, uniqueCode);
          this.birds.Add(codeBird);
          this.level.Add((Entity) codeBird);
        }
        this.Add((Component) (this.birdFlyingSfx = new SoundSource()));
        this.Add((Component) (this.birdFinishSfx = new SoundSource()));
        this.Add((Component) (this.birdThrustSfx = new SoundSource()));
        this.birdFlyingSfx.Position = this.birdFlyPosition - this.Position;
        this.birdFlyingSfx.Play("event:/game/01_forsaken_city/birdbros_fly_loop");
      }
      else
        this.staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
      if (this.level.Session.HeartGem || !this.level.Session.GetFlag("unlocked_satellite"))
        return;
      this.level.Add((Entity) new HeartGem(this.gemSpawnPosition));
    }

    public override void Update()
    {
      base.Update();
      this.computerScreenNoise.Visible = !this.pulse.Visible;
      this.computerScreen.Visible = this.pulse.Visible;
      this.screenBloom.Visible = this.pulseBloom.Visible;
    }

    private IEnumerator PulseRoutine()
    {
      ForsakenCitySatellite forsakenCitySatellite = this;
      forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
      while (forsakenCitySatellite.enabled)
      {
        yield return (object) 2f;
        for (int i = 0; i < ForsakenCitySatellite.Code.Length && forsakenCitySatellite.enabled; ++i)
        {
          forsakenCitySatellite.pulse.Color = forsakenCitySatellite.computerScreen.Color = ForsakenCitySatellite.Colors[ForsakenCitySatellite.Code[i]];
          forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = true;
          Audio.Play(ForsakenCitySatellite.Sounds[ForsakenCitySatellite.Code[i]], forsakenCitySatellite.Position + forsakenCitySatellite.computer.Position);
          yield return (object) 0.5f;
          forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
          Audio.Play(i < ForsakenCitySatellite.Code.Length - 1 ? "event:/game/01_forsaken_city/console_static_short" : "event:/game/01_forsaken_city/console_static_long", forsakenCitySatellite.Position + forsakenCitySatellite.computer.Position);
          yield return (object) 0.2f;
        }
        // ISSUE: reference to a compiler-generated method
        forsakenCitySatellite.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
        {
            if (this.enabled)
            {
                this.birdThrustSfx.Position = this.birdFlyPosition - this.Position;
                this.birdThrustSfx.Play("event:/game/01_forsaken_city/birdbros_thrust", null, 0f);
            }
        }, 1.1f, true));
        forsakenCitySatellite.birds.Shuffle<ForsakenCitySatellite.CodeBird>();
        foreach (ForsakenCitySatellite.CodeBird bird in forsakenCitySatellite.birds)
        {
          if (forsakenCitySatellite.enabled)
          {
            bird.Dash();
            yield return (object) 0.02f;
          }
        }
      }
      forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
    }

    private IEnumerator UnlockGem()
    {
      ForsakenCitySatellite forsakenCitySatellite = this;
      forsakenCitySatellite.level.Session.SetFlag("unlocked_satellite");
      forsakenCitySatellite.birdFinishSfx.Position = forsakenCitySatellite.birdFlyPosition - forsakenCitySatellite.Position;
      forsakenCitySatellite.birdFinishSfx.Play("event:/game/01_forsaken_city/birdbros_finish");
      forsakenCitySatellite.staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
      forsakenCitySatellite.enabled = false;
      yield return (object) 0.25f;
      forsakenCitySatellite.level.Displacement.Clear();
      yield return (object) null;
      forsakenCitySatellite.birdFlyingSfx.Stop();
      forsakenCitySatellite.level.Frozen = true;
      forsakenCitySatellite.Tag = (int) Tags.FrozenUpdate;
      BloomPoint bloom = new BloomPoint(forsakenCitySatellite.birdFlyPosition - forsakenCitySatellite.Position, 0.0f, 32f);
      forsakenCitySatellite.Add((Component) bloom);
      foreach (ForsakenCitySatellite.CodeBird bird in forsakenCitySatellite.birds)
        bird.Transform(3f);
      while ((double) bloom.Alpha < 1.0)
      {
        bloom.Alpha += Engine.DeltaTime / 3f;
        yield return (object) null;
      }
      yield return (object) 0.25f;
      foreach (Entity bird in forsakenCitySatellite.birds)
        bird.RemoveSelf();
      ParticleSystem particles = new ParticleSystem(-10000, 100);
      particles.Tag = (int) Tags.FrozenUpdate;
      particles.Emit(BirdNPC.P_Feather, 24, forsakenCitySatellite.birdFlyPosition, new Vector2(4f, 4f));
      forsakenCitySatellite.level.Add((Entity) particles);
      HeartGem gem = new HeartGem(forsakenCitySatellite.birdFlyPosition);
      gem.Tag = (int) Tags.FrozenUpdate;
      forsakenCitySatellite.level.Add((Entity) gem);
      yield return (object) null;
      gem.ScaleWiggler.Start();
      yield return (object) 0.85f;
      SimpleCurve curve = new SimpleCurve(gem.Position, forsakenCitySatellite.gemSpawnPosition, (gem.Position + forsakenCitySatellite.gemSpawnPosition) / 2f + new Vector2(0.0f, -64f));
      for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime)
      {
        yield return (object) null;
        gem.Position = curve.GetPoint(Ease.CubeInOut(t));
      }
      yield return (object) 0.5f;
      particles.RemoveSelf();
      forsakenCitySatellite.Remove((Component) bloom);
      forsakenCitySatellite.level.Frozen = false;
    }

    private class CodeBird : Entity
    {
      private Sprite sprite;
      private Coroutine routine;
      private float timer = Calc.Random.NextFloat();
      private Vector2 speed;
      private Monocle.Image heartImage;
      private readonly string code;
      private readonly Vector2 origin;
      private readonly Vector2 dash;

      public CodeBird(Vector2 origin, string code)
        : base(origin)
      {
        this.code = code;
        this.origin = origin;
        this.Add((Component) (this.sprite = new Sprite(GFX.Game, "scenery/flutterbird/")));
        this.sprite.AddLoop("fly", "flap", 0.08f);
        this.sprite.Play("fly");
        this.sprite.CenterOrigin();
        this.sprite.Color = ForsakenCitySatellite.Colors[code];
        this.dash = (Vector2.Zero with
        {
          X = (code.Contains<char>('L') ? -1f : (code.Contains<char>('R') ? 1f : 0.0f)),
          Y = (code.Contains<char>('U') ? -1f : (code.Contains<char>('D') ? 1f : 0.0f))
        }).SafeNormalize();
        this.Add((Component) (this.routine = new Coroutine(this.AimlessFlightRoutine())));
      }

      public override void Update()
      {
        this.timer += Engine.DeltaTime;
        this.sprite.Y = (float) Math.Sin((double) this.timer * 2.0);
        base.Update();
      }

      public void Dash() => this.routine.Replace(this.DashRoutine());

      public void Transform(float duration)
      {
        this.Tag = (int) Tags.FrozenUpdate;
        this.routine.Replace(this.TransformRoutine(duration));
      }

      private IEnumerator AimlessFlightRoutine()
      {
        ForsakenCitySatellite.CodeBird codeBird = this;
        codeBird.speed = Vector2.Zero;
        while (true)
        {
          Vector2 target = codeBird.origin + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 16f + Calc.Random.NextFloat(40f));
          float reset = 0.0f;
          while ((double) reset < 1.0 && (double) (target - codeBird.Position).Length() > 8.0)
          {
            Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
            codeBird.speed += vector2 * 420f * Engine.DeltaTime;
            if ((double) codeBird.speed.Length() > 90.0)
              codeBird.speed = codeBird.speed.SafeNormalize(90f);
            codeBird.Position = codeBird.Position + codeBird.speed * Engine.DeltaTime;
            reset += Engine.DeltaTime;
            if (Math.Sign(vector2.X) != 0)
              codeBird.sprite.Scale.X = (float) Math.Sign(vector2.X);
            yield return (object) null;
          }
          target = new Vector2();
        }
      }

      private IEnumerator DashRoutine()
      {
        ForsakenCitySatellite.CodeBird codeBird = this;
        float t;
        for (t = 0.25f; (double) t > 0.0; t -= Engine.DeltaTime)
        {
          codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 200f * Engine.DeltaTime);
          codeBird.Position = codeBird.Position + codeBird.speed * Engine.DeltaTime;
          yield return (object) null;
        }
        Vector2 from = codeBird.Position;
        Vector2 to = codeBird.origin + codeBird.dash * 8f;
        if (Math.Sign(to.X - from.X) != 0)
          codeBird.sprite.Scale.X = (float) Math.Sign(to.X - from.X);
        for (t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime * 1.5f)
        {
          codeBird.Position = from + (to - from) * Ease.CubeInOut(t);
          yield return (object) null;
        }
        codeBird.Position = to;
        yield return (object) 0.2f;
        from = new Vector2();
        to = new Vector2();
        if ((double) codeBird.dash.X != 0.0)
          codeBird.sprite.Scale.X = (float) Math.Sign(codeBird.dash.X);
        (codeBird.Scene as Level).Displacement.AddBurst(codeBird.Position, 0.25f, 4f, 24f, 0.4f);
        codeBird.speed = codeBird.dash * 300f;
        for (t = 0.4f; (double) t > 0.0; t -= Engine.DeltaTime)
        {
          if ((double) t > 0.10000000149011612 && codeBird.Scene.OnInterval(0.02f))
            codeBird.SceneAs<Level>().ParticlesBG.Emit(ForsakenCitySatellite.Particles[codeBird.code], 1, codeBird.Position, Vector2.One * 2f, codeBird.dash.Angle());
          codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 800f * Engine.DeltaTime);
          codeBird.Position = codeBird.Position + codeBird.speed * Engine.DeltaTime;
          yield return (object) null;
        }
        yield return (object) 0.4f;
        codeBird.routine.Replace(codeBird.AimlessFlightRoutine());
      }

      private IEnumerator TransformRoutine(float duration)
      {
        ForsakenCitySatellite.CodeBird codeBird = this;
        Color colorFrom = codeBird.sprite.Color;
        Color colorTo = Color.White;
        Vector2 target = codeBird.origin;
        codeBird.Add((Component) (codeBird.heartImage = new Monocle.Image(GFX.Game["collectables/heartGem/shape"])));
        codeBird.heartImage.CenterOrigin();
        codeBird.heartImage.Scale = Vector2.Zero;
        for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime / duration)
        {
          Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
          codeBird.speed += 400f * vector2 * Engine.DeltaTime;
          float length = Math.Max(20f, (float) ((1.0 - (double) t) * 200.0));
          if ((double) codeBird.speed.Length() > (double) length)
            codeBird.speed = codeBird.speed.SafeNormalize(length);
          codeBird.Position = codeBird.Position + codeBird.speed * Engine.DeltaTime;
          codeBird.sprite.Color = Color.Lerp(colorFrom, colorTo, t);
          codeBird.heartImage.Scale = Vector2.One * Math.Max(0.0f, (float) (((double) t - 0.75) * 4.0));
          if ((double) vector2.X != 0.0)
            codeBird.sprite.Scale.X = Math.Abs(codeBird.sprite.Scale.X) * (float) Math.Sign(vector2.X);
          codeBird.sprite.Scale.X = (float) Math.Sign(codeBird.sprite.Scale.X) * (1f - codeBird.heartImage.Scale.X);
          codeBird.sprite.Scale.Y = 1f - codeBird.heartImage.Scale.X;
          yield return (object) null;
        }
      }
    }
  }
}
