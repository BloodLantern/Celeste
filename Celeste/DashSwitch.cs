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
        private readonly DashSwitch.Sides side;
        private Vector2 pressedTarget;
        private bool pressed;
        private Vector2 pressDirection;
        private float speedY;
        private readonly float startY;
        private readonly bool persistent;
        private EntityID id;
        private readonly bool mirrorMode;
        private bool playerWasOn;
        private readonly bool allGates;
        private readonly Sprite sprite;

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
            mirrorMode = spriteName != "default";
            Add(sprite = GFX.SpriteBank.Create("dashSwitch_" + spriteName));
            sprite.Play("idle");
            if (side is DashSwitch.Sides.Up or DashSwitch.Sides.Down)
            {
                Collider.Width = 16f;
                Collider.Height = 8f;
            }
            else
            {
                Collider.Width = 8f;
                Collider.Height = 16f;
            }
            switch (side)
            {
                case DashSwitch.Sides.Up:
                    sprite.Position = new Vector2(8f, 0.0f);
                    sprite.Rotation = -1.57079637f;
                    pressedTarget = Position + (Vector2.UnitY * -8f);
                    pressDirection = -Vector2.UnitY;
                    break;
                case DashSwitch.Sides.Down:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 1.57079637f;
                    pressedTarget = Position + (Vector2.UnitY * 8f);
                    pressDirection = Vector2.UnitY;
                    startY = Y;
                    break;
                case DashSwitch.Sides.Left:
                    sprite.Position = new Vector2(0.0f, 8f);
                    sprite.Rotation = 3.14159274f;
                    pressedTarget = Position + (Vector2.UnitX * -8f);
                    pressDirection = -Vector2.UnitX;
                    break;
                case DashSwitch.Sides.Right:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 0.0f;
                    pressedTarget = Position + (Vector2.UnitX * 8f);
                    pressDirection = Vector2.UnitX;
                    break;
            }
            OnDashCollide = new DashCollision(OnDashed);
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
            if (!persistent || !SceneAs<Level>().Session.GetFlag(FlagName))
            {
                return;
            }

            sprite.Play("pushed");
            Position = pressedTarget - (pressDirection * 2f);
            pressed = true;
            Collidable = false;
            if (allGates)
            {
                foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>())
                {
                    if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
                    {
                        entity.StartOpen();
                    }
                }
            }
            else
            {
                GetGate()?.StartOpen();
            }
        }

        public override void Update()
        {
            base.Update();
            if (pressed || side != DashSwitch.Sides.Down)
            {
                return;
            }

            Player playerOnTop = GetPlayerOnTop();
            if (playerOnTop != null)
            {
                if (playerOnTop.Holding != null)
                {
                    _ = (int)OnDashed(playerOnTop, Vector2.UnitY);
                }
                else
                {
                    if (speedY < 0.0)
                    {
                        speedY = 0.0f;
                    }

                    speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                    MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
                    if (!playerWasOn)
                    {
                        _ = Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
                    }
                }
            }
            else
            {
                if (speedY > 0.0)
                {
                    speedY = 0.0f;
                }

                speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
                MoveTowardsY(startY, -speedY * Engine.DeltaTime);
                if (playerWasOn)
                {
                    _ = Audio.Play("event:/game/05_mirror_temple/button_return", Position);
                }
            }
            playerWasOn = playerOnTop != null;
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!pressed && direction == pressDirection)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                _ = Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
                sprite.Play("push");
                pressed = true;
                MoveTo(pressedTarget);
                Collidable = false;
                Position -= pressDirection * 2f;
                SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? DashSwitch.P_PressAMirror : DashSwitch.P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - 3.14159274f);
                SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? DashSwitch.P_PressBMirror : DashSwitch.P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - 3.14159274f);
                if (allGates)
                {
                    foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>())
                    {
                        if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
                        {
                            entity.SwitchOpen();
                        }
                    }
                }
                else
                {
                    GetGate()?.SwitchOpen();
                }

                Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(X - (Scene as Level).Bounds.Center.X));
                if (persistent)
                {
                    SceneAs<Level>().Session.SetFlag(FlagName);
                }
            }
            return DashCollisionResults.NormalCollision;
        }

        private TempleGate GetGate()
        {
            List<Entity> entities = Scene.Tracker.GetEntities<TempleGate>();
            TempleGate gate = null;
            float num1 = 0.0f;
            foreach (TempleGate templeGate in entities)
            {
                if (templeGate.Type == TempleGate.Types.NearestSwitch && !templeGate.ClaimedByASwitch && templeGate.LevelID == id.Level)
                {
                    float num2 = Vector2.DistanceSquared(Position, templeGate.Position);
                    if (gate == null || (double)num2 < (double)num1)
                    {
                        gate = templeGate;
                        num1 = num2;
                    }
                }
            }
            if (gate != null)
            {
                gate.ClaimedByASwitch = true;
            }

            return gate;
        }

        private string FlagName => DashSwitch.GetFlagName(id);

        public static string GetFlagName(EntityID id)
        {
            return "dashSwitch_" + id.Key;
        }

        public enum Sides
        {
            Up,
            Down,
            Left,
            Right,
        }
    }
}
