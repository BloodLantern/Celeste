// Decompiled with JetBrains decompiler
// Type: Celeste.Solid
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(true)]
    public class Solid : Platform
    {
        public Vector2 Speed;
        public bool AllowStaticMovers = true;
        public bool EnableAssistModeChecks = true;
        public bool DisableLightsInside = true;
        public bool StopPlayerRunIntoAnimation = true;
        public bool SquishEvenInAssistMode;
        private static readonly HashSet<Actor> riders = new();

        public Solid(Vector2 position, float width, float height, bool safe)
            : base(position, safe)
        {
            Collider = new Hitbox(width, height);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!AllowStaticMovers)
            {
                return;
            }

            bool collidable = Collidable;
            Collidable = true;
            foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
            {
                if (component.IsRiding(this) && component.Platform == null)
                {
                    staticMovers.Add(component);
                    component.Platform = this;
                    component.OnAttach?.Invoke(this);
                }
            }
            Collidable = collidable;
        }

        public override void Update()
        {
            base.Update();
            MoveH(Speed.X * Engine.DeltaTime);
            MoveV(Speed.Y * Engine.DeltaTime);
            if (!EnableAssistModeChecks || SaveData.Instance == null || !SaveData.Instance.Assists.Invincible || Components.Get<SolidOnInvinciblePlayer>() != null || !Collidable)
            {
                return;
            }

            Player player = CollideFirst<Player>();
            Level scene = Scene as Level;
            if (player == null && (double)Bottom > scene.Bounds.Bottom)
            {
                player = CollideFirst<Player>(Position + Vector2.UnitY);
            }

            if (player != null && player.StateMachine.State != 9 && player.StateMachine.State != 21)
            {
                Add(new SolidOnInvinciblePlayer());
            }
            else
            {
                TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
                if (theoCrystal == null || theoCrystal.Hold.IsHeld)
                {
                    return;
                }

                Add(new SolidOnInvinciblePlayer());
            }
        }

        public bool HasRider()
        {
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(this))
                {
                    return true;
                }
            }
            return false;
        }

        public Player GetPlayerRider()
        {
            foreach (Player entity in Scene.Tracker.GetEntities<Player>())
            {
                if (entity.IsRiding(this))
                {
                    return entity;
                }
            }
            return null;
        }

        public bool HasPlayerRider()
        {
            return GetPlayerRider() != null;
        }

        public bool HasPlayerOnTop()
        {
            return GetPlayerOnTop() != null;
        }

        public Player GetPlayerOnTop()
        {
            return CollideFirst<Player>(Position - Vector2.UnitY);
        }

        public bool HasPlayerClimbing()
        {
            return GetPlayerClimbing() != null;
        }

        public Player GetPlayerClimbing()
        {
            foreach (Player entity in Scene.Tracker.GetEntities<Player>())
            {
                if (entity.StateMachine.State == 1 && ((entity.Facing == Facings.Left && CollideCheck(entity, Position + Vector2.UnitX)) || (entity.Facing == Facings.Right && CollideCheck(entity, Position - Vector2.UnitX))))
                {
                    return entity;
                }
            }
            return null;
        }

        public void GetRiders()
        {
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(this))
                {
                    _ = Solid.riders.Add(entity);
                }
            }
        }

        public override void MoveHExact(int move)
        {
            GetRiders();
            float right = Right;
            float left = Left;
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            if (entity1 != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(entity1.Speed.X) == Math.Sign(move) && !Solid.riders.Contains(entity1) && CollideCheck(entity1, Position + (Vector2.UnitX * move) - Vector2.UnitY))
            {
                _ = entity1.MoveV(1f);
            }

            X += move;
            MoveStaticMovers(Vector2.UnitX * move);
            if (Collidable)
            {
                foreach (Actor entity2 in Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity2.AllowPushing)
                    {
                        bool collidable = entity2.Collidable;
                        entity2.Collidable = true;
                        if (!entity2.TreatNaive && CollideCheck(entity2, Position))
                        {
                            int moveH = move <= 0 ? move - (int)((double)entity2.Right - (double)left) : move - (int)((double)entity2.Left - (double)right);
                            Collidable = false;
                            _ = entity2.MoveHExact(moveH, entity2.SquishCallback, this);
                            entity2.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        else if (Solid.riders.Contains(entity2))
                        {
                            Collidable = false;
                            if (entity2.TreatNaive)
                            {
                                entity2.NaiveMove(Vector2.UnitX * move);
                            }
                            else
                            {
                                _ = entity2.MoveHExact(move);
                            }

                            entity2.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        entity2.Collidable = collidable;
                    }
                }
            }
            Solid.riders.Clear();
        }

        public override void MoveVExact(int move)
        {
            GetRiders();
            float bottom = Bottom;
            float top = Top;
            Y += move;
            MoveStaticMovers(Vector2.UnitY * move);
            if (Collidable)
            {
                foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity.AllowPushing)
                    {
                        bool collidable = entity.Collidable;
                        entity.Collidable = true;
                        if (!entity.TreatNaive && CollideCheck(entity, Position))
                        {
                            int moveV = move <= 0 ? move - (int)((double)entity.Bottom - (double)top) : move - (int)((double)entity.Top - (double)bottom);
                            Collidable = false;
                            _ = entity.MoveVExact(moveV, entity.SquishCallback, this);
                            entity.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        else if (Solid.riders.Contains(entity))
                        {
                            Collidable = false;
                            if (entity.TreatNaive)
                            {
                                entity.NaiveMove(Vector2.UnitY * move);
                            }
                            else
                            {
                                _ = entity.MoveVExact(move);
                            }

                            entity.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        entity.Collidable = collidable;
                    }
                }
            }
            Solid.riders.Clear();
        }
    }
}
