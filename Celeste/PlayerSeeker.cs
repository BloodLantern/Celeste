// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerSeeker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class PlayerSeeker : Actor
  {
    private Facings facing;
    private Sprite sprite;
    private Vector2 speed;
    private bool enabled;
    private float dashTimer;
    private Vector2 dashDirection;
    private float trailTimerA;
    private float trailTimerB;
    private Shaker shaker;

    public PlayerSeeker(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("seeker")));
      this.sprite.Play("statue");
      this.sprite.OnLastFrame = (Action<string>) (a =>
      {
        if (!(a == "flipMouth") && !(a == "flipEyes"))
          return;
        this.facing = (Facings) -(int) this.facing;
      });
      this.Collider = (Collider) new Hitbox(10f, 10f, -5f, -5f);
      this.Add((Component) new MirrorReflection());
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
      this.Add((Component) new VertexLight(Color.White, 1f, 32, 64));
      this.facing = Facings.Right;
      this.Add((Component) (this.shaker = new Shaker(false)));
      this.Add((Component) new Coroutine(this.IntroSequence()));
    }

    public override void Awake(Scene scene)
    {
      base.Awake(scene);
      Level level = scene as Level;
      level.Session.ColorGrade = "templevoid";
      level.ScreenPadding = 32f;
      level.CanRetry = false;
    }

    private IEnumerator IntroSequence()
    {
      PlayerSeeker playerSeeker = this;
      Level level = playerSeeker.Scene as Level;
      yield return (object) null;
      Glitch.Value = 0.05f;
      level.Tracker.GetEntity<Player>()?.StartTempleMirrorVoidSleep();
      yield return (object) 3f;
      Vector2 from = level.Camera.Position;
      Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, true);
      tween.OnUpdate = (Action<Tween>) (f => level.Camera.Position = from + (this.CameraTarget - from) * f.Eased);
      playerSeeker.Add((Component) tween);
      yield return (object) 2f;
      playerSeeker.shaker.ShakeFor(0.5f, false);
      playerSeeker.BreakOutParticles();
      Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
      yield return (object) 1f;
      playerSeeker.shaker.ShakeFor(0.5f, false);
      playerSeeker.BreakOutParticles();
      Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
      yield return (object) 1f;
      playerSeeker.BreakOutParticles();
      Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", playerSeeker.Position);
      playerSeeker.shaker.ShakeFor(1f, false);
      playerSeeker.sprite.Play("hatch");
      Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
      playerSeeker.enabled = true;
      yield return (object) 0.8f;
      playerSeeker.BreakOutParticles();
      yield return (object) 0.7f;
    }

    private void BreakOutParticles()
    {
      Level level = this.SceneAs<Level>();
      for (float direction = 0.0f; (double) direction < 6.2831854820251465; direction += 0.17453292f)
      {
        Vector2 position = this.Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f), (float) Calc.Random.Range(12, 20));
        level.Particles.Emit(Seeker.P_BreakOut, position, direction);
      }
    }

    private void OnPlayer(Player player)
    {
      if (player.Dead)
        return;
      Leader.StoreStrawberries(player.Leader);
      PlayerDeadBody playerDeadBody = player.Die((player.Position - this.Position).SafeNormalize(), true, false);
      playerDeadBody.DeathAction = new Action(this.End);
      playerDeadBody.ActionDelay = 0.3f;
      Engine.TimeRate = 0.25f;
    }

    private void End()
    {
      Level level = this.Scene as Level;
      level.OnEndOfFrame += (Action) (() =>
      {
        Glitch.Value = 0.0f;
        Distort.Anxiety = 0.0f;
        Engine.TimeRate = 1f;
        level.Session.ColorGrade = (string) null;
        level.UnloadLevel();
        level.CanRetry = true;
        level.Session.Level = "c-00";
        Session session = level.Session;
        Level level1 = level;
        Rectangle bounds = level.Bounds;
        double left = (double) bounds.Left;
        bounds = level.Bounds;
        double top = (double) bounds.Top;
        Vector2 from = new Vector2((float) left, (float) top);
        Vector2? nullable = new Vector2?(level1.GetSpawnPoint(from));
        session.RespawnPoint = nullable;
        level.LoadLevel(Player.IntroTypes.WakeUp);
        Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
      });
    }

    public override void Update()
    {
      foreach (Entity entity in this.Scene.Tracker.GetEntities<SeekerBarrier>())
        entity.Collidable = true;
      Level scene = this.Scene as Level;
      base.Update();
      this.sprite.Scale.X = Calc.Approach(this.sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
      this.sprite.Scale.Y = Calc.Approach(this.sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
      if (this.enabled && this.sprite.CurrentAnimationID != "hatch")
      {
        if ((double) this.dashTimer > 0.0)
        {
          this.speed = Calc.Approach(this.speed, Vector2.Zero, 800f * Engine.DeltaTime);
          this.dashTimer -= Engine.DeltaTime;
          if ((double) this.dashTimer <= 0.0)
            this.sprite.Play("spotted");
          if ((double) this.trailTimerA > 0.0)
          {
            this.trailTimerA -= Engine.DeltaTime;
            if ((double) this.trailTimerA <= 0.0)
              this.CreateTrail();
          }
          if ((double) this.trailTimerB > 0.0)
          {
            this.trailTimerB -= Engine.DeltaTime;
            if ((double) this.trailTimerB <= 0.0)
              this.CreateTrail();
          }
          if (this.Scene.OnInterval(0.04f))
          {
            Vector2 vector = this.speed.SafeNormalize();
            this.SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, this.Position + vector * 4f, Vector2.One * 4f, vector.Angle());
          }
        }
        else
        {
          Vector2 vector2 = Input.Aim.Value.SafeNormalize();
          this.speed += vector2 * 600f * Engine.DeltaTime;
          float val = this.speed.Length();
          if ((double) val > 120.0)
            this.speed = this.speed.SafeNormalize(Calc.Approach(val, 120f, Engine.DeltaTime * 700f));
          if ((double) vector2.Y == 0.0)
            this.speed.Y = Calc.Approach(this.speed.Y, 0.0f, 400f * Engine.DeltaTime);
          if ((double) vector2.X == 0.0)
            this.speed.X = Calc.Approach(this.speed.X, 0.0f, 400f * Engine.DeltaTime);
          if ((double) vector2.Length() > 0.0 && this.sprite.CurrentAnimationID == "idle")
          {
            scene.Displacement.AddBurst(this.Position, 0.5f, 8f, 32f);
            this.sprite.Play("spotted");
            Audio.Play("event:/game/05_mirror_temple/seeker_playercontrolstart");
          }
          int num1 = Math.Sign((int) this.facing);
          int num2 = Math.Sign(this.speed.X);
          if (num2 != 0 && num1 != num2 && Math.Sign(Input.Aim.Value.X) == Math.Sign(this.speed.X) && (double) Math.Abs(this.speed.X) > 20.0 && this.sprite.CurrentAnimationID != "flipMouth" && this.sprite.CurrentAnimationID != "flipEyes")
            this.sprite.Play("flipMouth");
          if (Input.Dash.Pressed)
            this.Dash(Input.Aim.Value.EightWayNormal());
        }
        this.MoveH(this.speed.X * Engine.DeltaTime, new Collision(this.OnCollide));
        this.MoveV(this.speed.Y * Engine.DeltaTime, new Collision(this.OnCollide));
        Vector2 position = this.Position;
        double x = (double) scene.Bounds.X;
        double y = (double) scene.Bounds.Y;
        Rectangle bounds = scene.Bounds;
        double right = (double) bounds.Right;
        bounds = scene.Bounds;
        double bottom = (double) bounds.Bottom;
        this.Position = position.Clamp((float) x, (float) y, (float) right, (float) bottom);
        Player entity = this.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
          float val = (this.Position - entity.Position).Length();
          if ((double) val < 200.0 && entity.Sprite.CurrentAnimationID == "asleep")
          {
            entity.Sprite.Rate = 2f;
            entity.Sprite.Play("wakeUp");
          }
          else if ((double) val < 100.0 && entity.Sprite.CurrentAnimationID != "wakeUp")
          {
            entity.Sprite.Rate = 1f;
            entity.Sprite.Play("runFast");
            entity.Facing = (double) this.X > (double) entity.X ? Facings.Left : Facings.Right;
          }
          if ((double) val < 50.0 && (double) this.dashTimer <= 0.0)
            this.Dash((entity.Center - this.Center).SafeNormalize());
          Engine.TimeRate = Calc.ClampedMap(val, 60f, 220f, 0.5f);
          Camera camera = scene.Camera;
          Vector2 cameraTarget = this.CameraTarget;
          camera.Position += (cameraTarget - camera.Position) * (1f - (float) Math.Pow(0.0099999997764825821, (double) Engine.DeltaTime));
          Distort.Anxiety = Calc.ClampedMap(val, 0.0f, 200f, 0.25f, 0.0f) + Calc.Random.NextFloat(0.05f);
          Distort.AnxietyOrigin = (new Vector2(entity.X, scene.Camera.Top) - scene.Camera.Position) / new Vector2(320f, 180f);
        }
        else
          Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 1f * Engine.DeltaTime);
      }
      foreach (Entity entity in this.Scene.Tracker.GetEntities<SeekerBarrier>())
        entity.Collidable = false;
    }

    private void CreateTrail()
    {
      Vector2 scale = this.sprite.Scale;
      this.sprite.Scale.X *= (float) this.facing;
      TrailManager.Add((Entity) this, Seeker.TrailColor);
      this.sprite.Scale = scale;
    }

    private void OnCollide(CollisionData data)
    {
      if ((double) this.dashTimer <= 0.0)
      {
        if ((double) data.Direction.X != 0.0)
          this.speed.X = 0.0f;
        if ((double) data.Direction.Y == 0.0)
          return;
        this.speed.Y = 0.0f;
      }
      else
      {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        if ((double) data.Direction.X > 0.0)
        {
          direction = 3.14159274f;
          position = new Vector2(this.Right, this.Y);
          positionRange = Vector2.UnitY * 4f;
        }
        else if ((double) data.Direction.X < 0.0)
        {
          direction = 0.0f;
          position = new Vector2(this.Left, this.Y);
          positionRange = Vector2.UnitY * 4f;
        }
        else if ((double) data.Direction.Y > 0.0)
        {
          direction = -1.57079637f;
          position = new Vector2(this.X, this.Bottom);
          positionRange = Vector2.UnitX * 4f;
        }
        else
        {
          direction = 1.57079637f;
          position = new Vector2(this.X, this.Top);
          positionRange = Vector2.UnitX * 4f;
        }
        this.SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, position, positionRange, direction);
        if (data.Hit is SeekerBarrier)
        {
          (data.Hit as SeekerBarrier).OnReflectSeeker();
          Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", this.Position);
        }
        else
          Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", this.Position);
        if ((double) data.Direction.X != 0.0)
        {
          this.speed.X *= -0.8f;
          this.sprite.Scale = new Vector2(0.6f, 1.4f);
        }
        else if ((double) data.Direction.Y != 0.0)
        {
          this.speed.Y *= -0.8f;
          this.sprite.Scale = new Vector2(1.4f, 0.6f);
        }
        if (!(data.Hit is TempleCrackedBlock))
          return;
        Celeste.Celeste.Freeze(0.15f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        (data.Hit as TempleCrackedBlock).Break(this.Position);
      }
    }

    private void Dash(Vector2 dir)
    {
      if ((double) this.dashTimer <= 0.0)
      {
        this.CreateTrail();
        this.trailTimerA = 0.1f;
        this.trailTimerB = 0.25f;
      }
      this.dashTimer = 0.3f;
      this.dashDirection = dir;
      if (this.dashDirection == Vector2.Zero)
        this.dashDirection.X = (float) Math.Sign((int) this.facing);
      if ((double) this.dashDirection.X != 0.0)
        this.facing = (Facings) Math.Sign(this.dashDirection.X);
      this.speed = this.dashDirection * 400f;
      this.sprite.Play("attacking");
      this.SceneAs<Level>().DirectionalShake(this.dashDirection);
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
      Audio.Play("event:/game/05_mirror_temple/seeker_dash", this.Position);
      if ((double) this.dashDirection.X == 0.0)
        this.sprite.Scale = new Vector2(0.6f, 1.4f);
      else
        this.sprite.Scale = new Vector2(1.4f, 0.6f);
    }

    public Vector2 CameraTarget
    {
      get
      {
        Rectangle bounds = (this.Scene as Level).Bounds;
        return (this.Position + new Vector2(-160f, -90f)).Clamp((float) bounds.Left, (float) bounds.Top, (float) (bounds.Right - 320), (float) (bounds.Bottom - 180));
      }
    }

    public override void Render()
    {
      if (SaveData.Instance.Assists.InvisibleMotion && this.enabled && (double) this.speed.LengthSquared() > 100.0)
        return;
      Vector2 position = this.Position;
      this.Position = this.Position + this.shaker.Value;
      Vector2 scale = this.sprite.Scale;
      this.sprite.Scale.X *= (float) this.facing;
      base.Render();
      this.Position = position;
      this.sprite.Scale = scale;
    }
  }
}
