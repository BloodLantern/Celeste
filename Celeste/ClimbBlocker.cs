﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
    public class ClimbBlocker : Component
    {
        public bool Blocking = true;
        public bool Edge;

        public ClimbBlocker(bool edge)
            : base(false, false)
        {
            Edge = edge;
        }

        public static bool Check(Scene scene, Entity entity, Vector2 at)
        {
            Vector2 position = entity.Position;
            entity.Position = at;
            int num = ClimbBlocker.Check(scene, entity) ? 1 : 0;
            entity.Position = position;
            return num != 0;
        }

        public static bool Check(Scene scene, Entity entity)
        {
            foreach (ClimbBlocker component in scene.Tracker.GetComponents<ClimbBlocker>())
            {
                if (component.Blocking && entity.CollideCheck(component.Entity))
                    return true;
            }
            return false;
        }

        public static bool EdgeCheck(Scene scene, Entity entity, int dir)
        {
            foreach (ClimbBlocker component in scene.Tracker.GetComponents<ClimbBlocker>())
            {
                if (component.Blocking && component.Edge && entity.CollideCheck(component.Entity, entity.Position + Vector2.UnitX * dir))
                    return true;
            }
            return false;
        }
    }
}
