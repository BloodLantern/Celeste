using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(true)]
    public class JumpThru : Platform
    {
        public JumpThru(Vector2 position, int width, bool safe)
            : base(position, safe)
        {
            Collider = new Hitbox(width, 5f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
            {
                if (component.IsRiding(this) && component.Platform == null)
                {
                    staticMovers.Add(component);
                    component.Platform = this;
                    if (component.OnAttach != null)
                        component.OnAttach(this);
                }
            }
        }

        public bool HasRider()
        {
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(this))
                    return true;
            }
            return false;
        }

        public bool HasPlayerRider()
        {
            foreach (Actor entity in Scene.Tracker.GetEntities<Player>())
            {
                if (entity.IsRiding(this))
                    return true;
            }
            return false;
        }

        public Player GetPlayerRider()
        {
            foreach (Player entity in Scene.Tracker.GetEntities<Player>())
            {
                if (entity.IsRiding(this))
                    return entity;
            }
            return null;
        }

        public override void MoveHExact(int move)
        {
            if (Collidable)
            {
                foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity.IsRiding(this))
                    {
                        if (entity.TreatNaive)
                            entity.NaiveMove(Vector2.UnitX * move);
                        else
                            entity.MoveHExact(move);
                    }
                }
            }
            X += move;
            MoveStaticMovers(Vector2.UnitX * move);
        }

        public override void MoveVExact(int move)
        {
            if (Collidable)
            {
                if (move < 0)
                {
                    foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
                    {
                        if (entity.IsRiding(this))
                        {
                            Collidable = false;
                            if (entity.TreatNaive)
                                entity.NaiveMove(Vector2.UnitY * move);
                            else
                                entity.MoveVExact(move);
                            entity.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                        else if (!entity.TreatNaive && CollideCheck(entity, Position + Vector2.UnitY * move) && !CollideCheck(entity))
                        {
                            Collidable = false;
                            entity.MoveVExact((int) ((double) Top + move - (double) entity.Bottom));
                            entity.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                    }
                }
                else
                {
                    foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
                    {
                        if (entity.IsRiding(this))
                        {
                            Collidable = false;
                            if (entity.TreatNaive)
                                entity.NaiveMove(Vector2.UnitY * move);
                            else
                                entity.MoveVExact(move);
                            entity.LiftSpeed = LiftSpeed;
                            Collidable = true;
                        }
                    }
                }
            }
            Y += move;
            MoveStaticMovers(Vector2.UnitY * move);
        }
    }
}
