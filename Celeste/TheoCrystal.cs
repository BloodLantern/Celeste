// Decompiled with JetBrains decompiler
// Type: Celeste.TheoCrystal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  [Tracked(false)]
  public class TheoCrystal : Actor
  {
    public static ParticleType P_Impact;
    public Vector2 Speed;
    public bool OnPedestal;
    public Holdable Hold;
    private Sprite sprite;
    private bool dead;
    private Level Level;
    private Collision onCollideH;
    private Collision onCollideV;
    private float noGravityTimer;
    private Vector2 prevLiftSpeed;
    private Vector2 previousPosition;
    private HoldableCollider hitSeeker;
    private float swatTimer;
    private bool shattering;
    private float hardVerticalHitSoundCooldown;
    private BirdTutorialGui tutorialGui;
    private float tutorialTimer;

    public TheoCrystal(Vector2 position)
      : base(position)
    {
      this.previousPosition = position;
      this.Depth = 100;
      this.Collider = (Collider) new Hitbox(8f, 10f, -4f, -10f);
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("theo_crystal")));
      this.sprite.Scale.X = -1f;
      this.Add((Component) (this.Hold = new Holdable()));
      this.Hold.PickupCollider = (Collider) new Hitbox(16f, 22f, -8f, -16f);
      this.Hold.SlowFall = false;
      this.Hold.SlowRun = true;
      this.Hold.OnPickup = new Action(this.OnPickup);
      this.Hold.OnRelease = new Action<Vector2>(this.OnRelease);
      this.Hold.DangerousCheck = new Func<HoldableCollider, bool>(this.Dangerous);
      this.Hold.OnHitSeeker = new Action<Seeker>(this.HitSeeker);
      this.Hold.OnSwat = new Action<HoldableCollider, int>(this.Swat);
      this.Hold.OnHitSpring = new Func<Spring, bool>(this.HitSpring);
      this.Hold.OnHitSpinner = new Action<Entity>(this.HitSpinner);
      this.Hold.SpeedGetter = (Func<Vector2>) (() => this.Speed);
      this.onCollideH = new Collision(this.OnCollideH);
      this.onCollideV = new Collision(this.OnCollideV);
      this.LiftSpeedGraceTime = 0.1f;
      this.Add((Component) new VertexLight(this.Collider.Center, Color.White, 1f, 32, 64));
      this.Tag = (int) Tags.TransitionUpdate;
      this.Add((Component) new MirrorReflection());
    }

    public TheoCrystal(EntityData e, Vector2 offset)
      : this(e.Position + offset)
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.Level = this.SceneAs<Level>();
      foreach (TheoCrystal entity in this.Level.Tracker.GetEntities<TheoCrystal>())
      {
        if (entity != this && entity.Hold.IsHeld)
          this.RemoveSelf();
      }
      if (!(this.Level.Session.Level == "e-00"))
        return;
      this.tutorialGui = new BirdTutorialGui((Entity) this, new Vector2(0.0f, -24f), (object) Dialog.Clean("tutorial_carry"), new object[2]
      {
        (object) Dialog.Clean("tutorial_hold"),
        (object) BirdTutorialGui.ButtonPrompt.Grab
      });
      this.tutorialGui.Open = false;
      this.Scene.Add((Entity) this.tutorialGui);
    }

    public override void Update()
    {
      base.Update();
      if (this.shattering || this.dead)
        return;
      if ((double) this.swatTimer > 0.0)
        this.swatTimer -= Engine.DeltaTime;
      this.hardVerticalHitSoundCooldown -= Engine.DeltaTime;
      if (this.OnPedestal)
      {
        this.Depth = 8999;
      }
      else
      {
        this.Depth = 100;
        if (this.Hold.IsHeld)
        {
          this.prevLiftSpeed = Vector2.Zero;
        }
        else
        {
          if (this.OnGround())
          {
            this.Speed.X = Calc.Approach(this.Speed.X, this.OnGround(this.Position + Vector2.UnitX * 3f) ? (this.OnGround(this.Position - Vector2.UnitX * 3f) ? 0.0f : -20f) : 20f, 800f * Engine.DeltaTime);
            Vector2 liftSpeed = this.LiftSpeed;
            if (liftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero)
            {
              this.Speed = this.prevLiftSpeed;
              this.prevLiftSpeed = Vector2.Zero;
              this.Speed.Y = Math.Min(this.Speed.Y * 0.6f, 0.0f);
              if ((double) this.Speed.X != 0.0 && (double) this.Speed.Y == 0.0)
                this.Speed.Y = -60f;
              if ((double) this.Speed.Y < 0.0)
                this.noGravityTimer = 0.15f;
            }
            else
            {
              this.prevLiftSpeed = liftSpeed;
              if ((double) liftSpeed.Y < 0.0 && (double) this.Speed.Y < 0.0)
                this.Speed.Y = 0.0f;
            }
          }
          else if (this.Hold.ShouldHaveGravity)
          {
            float num1 = 800f;
            if ((double) Math.Abs(this.Speed.Y) <= 30.0)
              num1 *= 0.5f;
            float num2 = 350f;
            if ((double) this.Speed.Y < 0.0)
              num2 *= 0.5f;
            this.Speed.X = Calc.Approach(this.Speed.X, 0.0f, num2 * Engine.DeltaTime);
            if ((double) this.noGravityTimer > 0.0)
              this.noGravityTimer -= Engine.DeltaTime;
            else
              this.Speed.Y = Calc.Approach(this.Speed.Y, 200f, num1 * Engine.DeltaTime);
          }
          this.previousPosition = this.ExactPosition;
          this.MoveH(this.Speed.X * Engine.DeltaTime, this.onCollideH);
          this.MoveV(this.Speed.Y * Engine.DeltaTime, this.onCollideV);
          if ((double) this.Center.X > (double) this.Level.Bounds.Right)
          {
            this.MoveH(32f * Engine.DeltaTime);
            if ((double) this.Left - 8.0 > (double) this.Level.Bounds.Right)
              this.RemoveSelf();
          }
          else if ((double) this.Left < (double) this.Level.Bounds.Left)
          {
            this.Left = (float) this.Level.Bounds.Left;
            this.Speed.X *= -0.4f;
          }
          else if ((double) this.Top < (double) (this.Level.Bounds.Top - 4))
          {
            this.Top = (float) (this.Level.Bounds.Top + 4);
            this.Speed.Y = 0.0f;
          }
          else if ((double) this.Bottom > (double) this.Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
          {
            this.Bottom = (float) this.Level.Bounds.Bottom;
            this.Speed.Y = -300f;
            Audio.Play("event:/game/general/assist_screenbottom", this.Position);
          }
          else if ((double) this.Top > (double) this.Level.Bounds.Bottom)
            this.Die();
          if ((double) this.X < (double) (this.Level.Bounds.Left + 10))
            this.MoveH(32f * Engine.DeltaTime);
          Player entity = this.Scene.Tracker.GetEntity<Player>();
          TempleGate templeGate = this.CollideFirst<TempleGate>();
          if (templeGate != null && entity != null)
          {
            templeGate.Collidable = false;
            this.MoveH((float) (Math.Sign(entity.X - this.X) * 32) * Engine.DeltaTime);
            templeGate.Collidable = true;
          }
        }
        if (!this.dead)
          this.Hold.CheckAgainstColliders();
        if (this.hitSeeker != null && (double) this.swatTimer <= 0.0 && !this.hitSeeker.Check(this.Hold))
          this.hitSeeker = (HoldableCollider) null;
        if (this.tutorialGui == null)
          return;
        if (!this.OnPedestal && !this.Hold.IsHeld && this.OnGround() && this.Level.Session.GetFlag("foundTheoInCrystal"))
          this.tutorialTimer += Engine.DeltaTime;
        else
          this.tutorialTimer = 0.0f;
        this.tutorialGui.Open = (double) this.tutorialTimer > 0.25;
      }
    }

    public IEnumerator Shatter()
    {
      TheoCrystal theoCrystal = this;
      theoCrystal.shattering = true;
      BloomPoint bloom = new BloomPoint(0.0f, 32f);
      VertexLight light = new VertexLight(Color.AliceBlue, 0.0f, 64, 200);
      theoCrystal.Add((Component) bloom);
      theoCrystal.Add((Component) light);
      for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        theoCrystal.Position = theoCrystal.Position + theoCrystal.Speed * (1f - p) * Engine.DeltaTime;
        theoCrystal.Level.ZoomFocusPoint = theoCrystal.TopCenter - theoCrystal.Level.Camera.Position;
        light.Alpha = p;
        bloom.Alpha = p;
        yield return (object) null;
      }
      yield return (object) 0.5f;
      theoCrystal.Level.Shake();
      theoCrystal.sprite.Play("shatter");
      yield return (object) 1f;
      theoCrystal.Level.Shake();
    }

    public void ExplodeLaunch(Vector2 from)
    {
      if (this.Hold.IsHeld)
        return;
      this.Speed = (this.Center - from).SafeNormalize(120f);
      SlashFx.Burst(this.Center, this.Speed.Angle());
    }

    public void Swat(HoldableCollider hc, int dir)
    {
      if (!this.Hold.IsHeld || this.hitSeeker != null)
        return;
      this.swatTimer = 0.1f;
      this.hitSeeker = hc;
      this.Hold.Holder.Swat(dir);
    }

    public bool Dangerous(HoldableCollider holdableCollider) => !this.Hold.IsHeld && this.Speed != Vector2.Zero && this.hitSeeker != holdableCollider;

    public void HitSeeker(Seeker seeker)
    {
      if (!this.Hold.IsHeld)
        this.Speed = (this.Center - seeker.Center).SafeNormalize(120f);
      Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
    }

    public void HitSpinner(Entity spinner)
    {
      if (this.Hold.IsHeld || (double) this.Speed.Length() >= 0.0099999997764825821)
        return;
      Vector2 vector2 = this.LiftSpeed;
      if ((double) vector2.Length() >= 0.0099999997764825821)
        return;
      vector2 = this.previousPosition - this.ExactPosition;
      if ((double) vector2.Length() >= 0.0099999997764825821 || !this.OnGround())
        return;
      int num = Math.Sign(this.X - spinner.X);
      if (num == 0)
        num = 1;
      this.Speed.X = (float) num * 120f;
      this.Speed.Y = -30f;
    }

    public bool HitSpring(Spring spring)
    {
      if (!this.Hold.IsHeld)
      {
        if (spring.Orientation == Spring.Orientations.Floor && (double) this.Speed.Y >= 0.0)
        {
          this.Speed.X *= 0.5f;
          this.Speed.Y = -160f;
          this.noGravityTimer = 0.15f;
          return true;
        }
        if (spring.Orientation == Spring.Orientations.WallLeft && (double) this.Speed.X <= 0.0)
        {
          this.MoveTowardsY(spring.CenterY + 5f, 4f);
          this.Speed.X = 220f;
          this.Speed.Y = -80f;
          this.noGravityTimer = 0.1f;
          return true;
        }
        if (spring.Orientation == Spring.Orientations.WallRight && (double) this.Speed.X >= 0.0)
        {
          this.MoveTowardsY(spring.CenterY + 5f, 4f);
          this.Speed.X = -220f;
          this.Speed.Y = -80f;
          this.noGravityTimer = 0.1f;
          return true;
        }
      }
      return false;
    }

    private void OnCollideH(CollisionData data)
    {
      if (data.Hit is DashSwitch)
      {
        int num = (int) (data.Hit as DashSwitch).OnDashCollide((Player) null, Vector2.UnitX * (float) Math.Sign(this.Speed.X));
      }
      Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
      if ((double) Math.Abs(this.Speed.X) > 100.0)
        this.ImpactParticles(data.Direction);
      this.Speed.X *= -0.4f;
    }

    private void OnCollideV(CollisionData data)
    {
      if (data.Hit is DashSwitch)
      {
        int num = (int) (data.Hit as DashSwitch).OnDashCollide((Player) null, Vector2.UnitY * (float) Math.Sign(this.Speed.Y));
      }
      if ((double) this.Speed.Y > 0.0)
      {
        if ((double) this.hardVerticalHitSoundCooldown <= 0.0)
        {
          Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", Calc.ClampedMap(this.Speed.Y, 0.0f, 200f));
          this.hardVerticalHitSoundCooldown = 0.5f;
        }
        else
          Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", 0.0f);
      }
      if ((double) this.Speed.Y > 160.0)
        this.ImpactParticles(data.Direction);
      if ((double) this.Speed.Y > 140.0 && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
        this.Speed.Y *= -0.6f;
      else
        this.Speed.Y = 0.0f;
    }

    private void ImpactParticles(Vector2 dir)
    {
      float direction;
      Vector2 position;
      Vector2 positionRange;
      if ((double) dir.X > 0.0)
      {
        direction = 3.14159274f;
        position = new Vector2(this.Right, this.Y - 4f);
        positionRange = Vector2.UnitY * 6f;
      }
      else if ((double) dir.X < 0.0)
      {
        direction = 0.0f;
        position = new Vector2(this.Left, this.Y - 4f);
        positionRange = Vector2.UnitY * 6f;
      }
      else if ((double) dir.Y > 0.0)
      {
        direction = -1.57079637f;
        position = new Vector2(this.X, this.Bottom);
        positionRange = Vector2.UnitX * 6f;
      }
      else
      {
        direction = 1.57079637f;
        position = new Vector2(this.X, this.Top);
        positionRange = Vector2.UnitX * 6f;
      }
      this.Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
    }

    public override bool IsRiding(Solid solid) => (double) this.Speed.Y == 0.0 && base.IsRiding(solid);

    protected override void OnSquish(CollisionData data)
    {
      if (this.TrySquishWiggle(data) || SaveData.Instance.Assists.Invincible)
        return;
      this.Die();
    }

    private void OnPickup()
    {
      this.Speed = Vector2.Zero;
      this.AddTag((int) Tags.Persistent);
    }

    private void OnRelease(Vector2 force)
    {
      this.RemoveTag((int) Tags.Persistent);
      if ((double) force.X != 0.0 && (double) force.Y == 0.0)
        force.Y = -0.4f;
      this.Speed = force * 200f;
      if (!(this.Speed != Vector2.Zero))
        return;
      this.noGravityTimer = 0.1f;
    }

    public void Die()
    {
      if (this.dead)
        return;
      this.dead = true;
      Player entity = this.Level.Tracker.GetEntity<Player>();
      entity?.Die(-Vector2.UnitX * (float) entity.Facing);
      Audio.Play("event:/char/madeline/death", this.Position);
      this.Add((Component) new DeathEffect(Color.ForestGreen, new Vector2?(this.Center - this.Position)));
      this.sprite.Visible = false;
      this.Depth = -1000000;
      this.AllowPushing = false;
    }
  }
}
