// Decompiled with JetBrains decompiler
// Type: Celeste.FireBall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  public class FireBall : Entity
  {
    public static ParticleType P_FireTrail;
    public static ParticleType P_IceTrail;
    public static ParticleType P_IceBreak;
    private const float FireSpeed = 60f;
    private const float IceSpeed = 30f;
    private const float IceSpeedMult = 0.5f;
    private Vector2[] nodes;
    private int amount;
    private int index;
    private float offset;
    private float[] lengths;
    private float speed;
    private float speedMult;
    private float percent;
    private bool iceMode;
    private bool broken;
    private float mult;
    private bool notCoreMode;
    private SoundSource trackSfx;
    private Sprite sprite;
    private Wiggler hitWiggler;
    private Vector2 hitDir;

    public FireBall(
      Vector2[] nodes,
      int amount,
      int index,
      float offset,
      float speedMult,
      bool notCoreMode)
    {
      this.Tag = (int) Tags.TransitionUpdate;
      this.Collider = (Collider) new Monocle.Circle(6f);
      this.nodes = nodes;
      this.amount = amount;
      this.index = index;
      this.offset = offset;
      this.mult = speedMult;
      this.notCoreMode = notCoreMode;
      this.lengths = new float[nodes.Length];
      for (int index1 = 1; index1 < this.lengths.Length; ++index1)
        this.lengths[index1] = this.lengths[index1 - 1] + Vector2.Distance(nodes[index1 - 1], nodes[index1]);
      this.speed = 60f / this.lengths[this.lengths.Length - 1] * this.mult;
      this.percent = index != 0 ? (float) index / (float) amount : 0.0f;
      this.percent += 1f / (float) amount * offset;
      this.percent %= 1f;
      this.Position = this.GetPercentPosition(this.percent);
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnBounce), (Collider) new Hitbox(16f, 6f, -8f, -3f)));
      this.Add((Component) new CoreModeListener(new Action<Session.CoreModes>(this.OnChangeMode)));
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("fireball")));
      this.Add((Component) (this.hitWiggler = Wiggler.Create(1.2f, 2f)));
      this.hitWiggler.StartZero = true;
      if (index != 0)
        return;
      this.Add((Component) (this.trackSfx = new SoundSource()));
    }

    public FireBall(EntityData data, Vector2 offset)
      : this(data.NodesWithPosition(offset), data.Int(nameof (amount), 1), 0, data.Float(nameof (offset)), data.Float(nameof (speed), 1f), data.Bool(nameof (notCoreMode)))
    {
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.iceMode = this.SceneAs<Level>().CoreMode == Session.CoreModes.Cold || this.notCoreMode;
      this.speedMult = this.iceMode ? 0.0f : 1f;
      this.sprite.Play(this.iceMode ? "ice" : "hot", randomizeFrame: true);
      if (this.index == 0)
      {
        for (int index = 1; index < this.amount; ++index)
          this.Scene.Add((Entity) new FireBall(this.nodes, this.amount, index, this.offset, this.mult, this.notCoreMode));
      }
      if (this.trackSfx == null || this.iceMode)
        return;
      this.PositionTrackSfx();
      this.trackSfx.Play("event:/env/local/09_core/fireballs_idle");
    }

    public override void Update()
    {
      if ((this.Scene as Level).Transitioning)
      {
        this.PositionTrackSfx();
      }
      else
      {
        base.Update();
        this.speedMult = Calc.Approach(this.speedMult, this.iceMode ? 0.5f : 1f, 2f * Engine.DeltaTime);
        this.percent += this.speed * this.speedMult * Engine.DeltaTime;
        if ((double) this.percent >= 1.0)
        {
          this.percent %= 1f;
          if (this.broken && this.nodes[this.nodes.Length - 1] != this.nodes[0])
          {
            this.broken = false;
            this.Collidable = true;
            this.sprite.Play(this.iceMode ? "ice" : "hot", randomizeFrame: true);
          }
        }
        this.Position = this.GetPercentPosition(this.percent);
        this.PositionTrackSfx();
        if (this.broken || !this.Scene.OnInterval(this.iceMode ? 0.08f : 0.05f))
          return;
        this.SceneAs<Level>().ParticlesBG.Emit(this.iceMode ? FireBall.P_IceTrail : FireBall.P_FireTrail, 1, this.Center, Vector2.One * 4f);
      }
    }

    public void PositionTrackSfx()
    {
      if (this.trackSfx == null)
        return;
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (entity == null)
        return;
      Vector2? nullable = new Vector2?();
      for (int index = 1; index < this.nodes.Length; ++index)
      {
        Vector2 vector2_1 = Calc.ClosestPointOnLine(this.nodes[index - 1], this.nodes[index], entity.Center);
        if (nullable.HasValue)
        {
          Vector2 vector2_2 = vector2_1 - entity.Center;
          double num1 = (double) vector2_2.Length();
          vector2_2 = nullable.Value - entity.Center;
          double num2 = (double) vector2_2.Length();
          if (num1 >= num2)
            continue;
        }
        nullable = new Vector2?(vector2_1);
      }
      if (!nullable.HasValue)
        return;
      this.trackSfx.Position = nullable.Value - this.Position;
      this.trackSfx.UpdateSfxPosition();
    }

    public override void Render()
    {
      this.sprite.Position = this.hitDir * this.hitWiggler.Value * 8f;
      if (!this.broken)
        this.sprite.DrawOutline(Color.Black);
      base.Render();
    }

    private void OnPlayer(Player player)
    {
      if (!this.iceMode && !this.broken)
      {
        this.KillPlayer(player);
      }
      else
      {
        if (!this.iceMode || this.broken || (double) player.Bottom <= (double) this.Y + 4.0)
          return;
        this.KillPlayer(player);
      }
    }

    private void KillPlayer(Player player)
    {
      Vector2 direction = (player.Center - this.Center).SafeNormalize();
      if (player.Die(direction) == null)
        return;
      this.hitDir = direction;
      this.hitWiggler.Start();
    }

    private void OnBounce(Player player)
    {
      if (!this.iceMode || this.broken || (double) player.Bottom > (double) this.Y + 4.0 || (double) player.Speed.Y < 0.0)
        return;
      Audio.Play("event:/game/09_core/iceball_break", this.Position);
      this.sprite.Play("shatter");
      this.broken = true;
      this.Collidable = false;
      player.Bounce((float) (int) ((double) this.Y - 2.0));
      this.SceneAs<Level>().Particles.Emit(FireBall.P_IceBreak, 18, this.Center, Vector2.One * 6f);
    }

    private void OnChangeMode(Session.CoreModes mode)
    {
      this.iceMode = mode == Session.CoreModes.Cold;
      if (!this.broken)
        this.sprite.Play(this.iceMode ? "ice" : "hot", randomizeFrame: true);
      if (this.index != 0 || this.trackSfx == null)
        return;
      if (this.iceMode)
      {
        this.trackSfx.Stop();
      }
      else
      {
        this.PositionTrackSfx();
        this.trackSfx.Play("event:/env/local/09_core/fireballs_idle");
      }
    }

    private Vector2 GetPercentPosition(float percent)
    {
      if ((double) percent <= 0.0)
        return this.nodes[0];
      if ((double) percent >= 1.0)
        return this.nodes[this.nodes.Length - 1];
      float length = this.lengths[this.lengths.Length - 1];
      float num = length * percent;
      int index = 0;
      while (index < this.lengths.Length - 1 && (double) this.lengths[index + 1] <= (double) num)
        ++index;
      float min = this.lengths[index] / length;
      float max = this.lengths[index + 1] / length;
      float amount = Calc.ClampedMap(percent, min, max);
      return Vector2.Lerp(this.nodes[index], this.nodes[index + 1], amount);
    }
  }
}
