// Decompiled with JetBrains decompiler
// Type: Celeste.DashSwitch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
  public class DashSwitch : Solid
  {
    public static ParticleType P_PressA;
    public static ParticleType P_PressB;
    public static ParticleType P_PressAMirror;
    public static ParticleType P_PressBMirror;
    private DashSwitch.Sides side;
    private Vector2 pressedTarget;
    private bool pressed;
    private Vector2 pressDirection;
    private float speedY;
    private float startY;
    private bool persistent;
    private EntityID id;
    private bool mirrorMode;
    private bool playerWasOn;
    private bool allGates;
    private Sprite sprite;

    public DashSwitch(
      Vector2 position,
      DashSwitch.Sides side,
      bool persistent,
      bool allGates,
      EntityID id,
      string spriteName)
      : base(position, 0.0f, 0.0f, true)
    {
      this.side = side;
      this.persistent = persistent;
      this.allGates = allGates;
      this.id = id;
      this.mirrorMode = spriteName != "default";
      this.Add((Component) (this.sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName)));
      this.sprite.Play("idle");
      if (side == DashSwitch.Sides.Up || side == DashSwitch.Sides.Down)
      {
        this.Collider.Width = 16f;
        this.Collider.Height = 8f;
      }
      else
      {
        this.Collider.Width = 8f;
        this.Collider.Height = 16f;
      }
      switch (side)
      {
        case DashSwitch.Sides.Up:
          this.sprite.Position = new Vector2(8f, 0.0f);
          this.sprite.Rotation = -1.57079637f;
          this.pressedTarget = this.Position + Vector2.UnitY * -8f;
          this.pressDirection = -Vector2.UnitY;
          break;
        case DashSwitch.Sides.Down:
          this.sprite.Position = new Vector2(8f, 8f);
          this.sprite.Rotation = 1.57079637f;
          this.pressedTarget = this.Position + Vector2.UnitY * 8f;
          this.pressDirection = Vector2.UnitY;
          this.startY = this.Y;
          break;
        case DashSwitch.Sides.Left:
          this.sprite.Position = new Vector2(0.0f, 8f);
          this.sprite.Rotation = 3.14159274f;
          this.pressedTarget = this.Position + Vector2.UnitX * -8f;
          this.pressDirection = -Vector2.UnitX;
          break;
        case DashSwitch.Sides.Right:
          this.sprite.Position = new Vector2(8f, 8f);
          this.sprite.Rotation = 0.0f;
          this.pressedTarget = this.Position + Vector2.UnitX * 8f;
          this.pressDirection = Vector2.UnitX;
          break;
      }
      this.OnDashCollide = new DashCollision(this.OnDashed);
    }

    public static DashSwitch Create(EntityData data, Vector2 offset, EntityID id)
    {
      Vector2 position = data.Position + offset;
      bool persistent = data.Bool("persistent");
      bool allGates = data.Bool("allGates");
      string spriteName = data.Attr("sprite", "default");
      return data.Name.Equals("dashSwitchH") ? (data.Bool("leftSide") ? new DashSwitch(position, DashSwitch.Sides.Left, persistent, allGates, id, spriteName) : new DashSwitch(position, DashSwitch.Sides.Right, persistent, allGates, id, spriteName)) : (data.Bool("ceiling") ? new DashSwitch(position, DashSwitch.Sides.Up, persistent, allGates, id, spriteName) : new DashSwitch(position, DashSwitch.Sides.Down, persistent, allGates, id, spriteName));
    }

    public override void Awake(Scene scene)
    {
      base.Awake(scene);
      if (!this.persistent || !this.SceneAs<Level>().Session.GetFlag(this.FlagName))
        return;
      this.sprite.Play("pushed");
      this.Position = this.pressedTarget - this.pressDirection * 2f;
      this.pressed = true;
      this.Collidable = false;
      if (this.allGates)
      {
        foreach (TempleGate entity in this.Scene.Tracker.GetEntities<TempleGate>())
        {
          if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == this.id.Level)
            entity.StartOpen();
        }
      }
      else
        this.GetGate()?.StartOpen();
    }

    public override void Update()
    {
      base.Update();
      if (this.pressed || this.side != DashSwitch.Sides.Down)
        return;
      Player playerOnTop = this.GetPlayerOnTop();
      if (playerOnTop != null)
      {
        if (playerOnTop.Holding != null)
        {
          int num = (int) this.OnDashed(playerOnTop, Vector2.UnitY);
        }
        else
        {
          if ((double) this.speedY < 0.0)
            this.speedY = 0.0f;
          this.speedY = Calc.Approach(this.speedY, 70f, 200f * Engine.DeltaTime);
          this.MoveTowardsY(this.startY + 2f, this.speedY * Engine.DeltaTime);
          if (!this.playerWasOn)
            Audio.Play("event:/game/05_mirror_temple/button_depress", this.Position);
        }
      }
      else
      {
        if ((double) this.speedY > 0.0)
          this.speedY = 0.0f;
        this.speedY = Calc.Approach(this.speedY, -150f, 200f * Engine.DeltaTime);
        this.MoveTowardsY(this.startY, -this.speedY * Engine.DeltaTime);
        if (this.playerWasOn)
          Audio.Play("event:/game/05_mirror_temple/button_return", this.Position);
      }
      this.playerWasOn = playerOnTop != null;
    }

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
      if (!this.pressed && direction == this.pressDirection)
      {
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Audio.Play("event:/game/05_mirror_temple/button_activate", this.Position);
        this.sprite.Play("push");
        this.pressed = true;
        this.MoveTo(this.pressedTarget);
        this.Collidable = false;
        this.Position = this.Position - this.pressDirection * 2f;
        this.SceneAs<Level>().ParticlesFG.Emit(this.mirrorMode ? DashSwitch.P_PressAMirror : DashSwitch.P_PressA, 10, this.Position + this.sprite.Position, direction.Perpendicular() * 6f, this.sprite.Rotation - 3.14159274f);
        this.SceneAs<Level>().ParticlesFG.Emit(this.mirrorMode ? DashSwitch.P_PressBMirror : DashSwitch.P_PressB, 4, this.Position + this.sprite.Position, direction.Perpendicular() * 6f, this.sprite.Rotation - 3.14159274f);
        if (this.allGates)
        {
          foreach (TempleGate entity in this.Scene.Tracker.GetEntities<TempleGate>())
          {
            if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == this.id.Level)
              entity.SwitchOpen();
          }
        }
        else
          this.GetGate()?.SwitchOpen();
        this.Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(this.X - (float) (this.Scene as Level).Bounds.Center.X));
        if (this.persistent)
          this.SceneAs<Level>().Session.SetFlag(this.FlagName);
      }
      return DashCollisionResults.NormalCollision;
    }

    private TempleGate GetGate()
    {
      List<Entity> entities = this.Scene.Tracker.GetEntities<TempleGate>();
      TempleGate gate = (TempleGate) null;
      float num1 = 0.0f;
      foreach (TempleGate templeGate in entities)
      {
        if (templeGate.Type == TempleGate.Types.NearestSwitch && !templeGate.ClaimedByASwitch && templeGate.LevelID == this.id.Level)
        {
          float num2 = Vector2.DistanceSquared(this.Position, templeGate.Position);
          if (gate == null || (double) num2 < (double) num1)
          {
            gate = templeGate;
            num1 = num2;
          }
        }
      }
      if (gate != null)
        gate.ClaimedByASwitch = true;
      return gate;
    }

    private string FlagName => DashSwitch.GetFlagName(this.id);

    public static string GetFlagName(EntityID id) => "dashSwitch_" + id.Key;

    public enum Sides
    {
      Up,
      Down,
      Left,
      Right,
    }
  }
}
