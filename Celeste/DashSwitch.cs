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
        private Sides side;
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
            Sides side,
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
            if (side == Sides.Up || side == Sides.Down)
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
                case Sides.Up:
                    sprite.Position = new Vector2(8f, 0.0f);
                    sprite.Rotation = -1.57079637f;
                    pressedTarget = Position + Vector2.UnitY * -8f;
                    pressDirection = -Vector2.UnitY;
                    break;
                case Sides.Down:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 1.57079637f;
                    pressedTarget = Position + Vector2.UnitY * 8f;
                    pressDirection = Vector2.UnitY;
                    startY = Y;
                    break;
                case Sides.Left:
                    sprite.Position = new Vector2(0.0f, 8f);
                    sprite.Rotation = 3.14159274f;
                    pressedTarget = Position + Vector2.UnitX * -8f;
                    pressDirection = -Vector2.UnitX;
                    break;
                case Sides.Right:
                    sprite.Position = new Vector2(8f, 8f);
                    sprite.Rotation = 0.0f;
                    pressedTarget = Position + Vector2.UnitX * 8f;
                    pressDirection = Vector2.UnitX;
                    break;
            }
            OnDashCollide = OnDashed;
        }

        public static DashSwitch Create(EntityData data, Vector2 offset, EntityID id)
        {
            Vector2 position = data.Position + offset;
            bool persistent = data.Bool("persistent");
            bool allGates = data.Bool("allGates");
            string spriteName = data.Attr("sprite", "default");
            return data.Name.Equals("dashSwitchH") ? (data.Bool("leftSide") ? new DashSwitch(position, Sides.Left, persistent, allGates, id, spriteName) : new DashSwitch(position, Sides.Right, persistent, allGates, id, spriteName)) : (data.Bool("ceiling") ? new DashSwitch(position, Sides.Up, persistent, allGates, id, spriteName) : new DashSwitch(position, Sides.Down, persistent, allGates, id, spriteName));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!persistent || !SceneAs<Level>().Session.GetFlag(FlagName))
                return;
            sprite.Play("pushed");
            Position = pressedTarget - pressDirection * 2f;
            pressed = true;
            Collidable = false;
            if (allGates)
            {
                foreach (TempleGate entity in Scene.Tracker.GetEntities<TempleGate>())
                {
                    if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
                        entity.StartOpen();
                }
            }
            else
                GetGate()?.StartOpen();
        }

        public override void Update()
        {
            base.Update();
            if (pressed || side != Sides.Down)
                return;
            Player playerOnTop = GetPlayerOnTop();
            if (playerOnTop != null)
            {
                if (playerOnTop.Holding != null)
                {
                    int num = (int) OnDashed(playerOnTop, Vector2.UnitY);
                }
                else
                {
                    if (speedY < 0.0)
                        speedY = 0.0f;
                    speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                    MoveTowardsY(startY + 2f, speedY * Engine.DeltaTime);
                    if (!playerWasOn)
                        Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
                }
            }
            else
            {
                if (speedY > 0.0)
                    speedY = 0.0f;
                speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
                MoveTowardsY(startY, -speedY * Engine.DeltaTime);
                if (playerWasOn)
                    Audio.Play("event:/game/05_mirror_temple/button_return", Position);
            }
            playerWasOn = playerOnTop != null;
        }

        public DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!pressed && direction == pressDirection)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
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
                            entity.SwitchOpen();
                    }
                }
                else
                    GetGate()?.SwitchOpen();
                Scene.Entities.FindFirst<TempleMirrorPortal>()?.OnSwitchHit(Math.Sign(X - (Scene as Level).Bounds.Center.X));
                if (persistent)
                    SceneAs<Level>().Session.SetFlag(FlagName);
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
                    if (gate == null || num2 < (double) num1)
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

        private string FlagName => DashSwitch.GetFlagName(id);

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
