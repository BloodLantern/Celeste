﻿using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class SeekerEffectsController : Entity
    {
        private float randomAnxietyOffset;
        public bool enabled = true;

        public SeekerEffectsController() => Tag = (int) Tags.Global;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            level.Session.Audio.Music.Layer(3, 0.0f);
            level.Session.Audio.Apply();
        }

        public override void Update()
        {
            base.Update();
            if (enabled)
            {
                if (Scene.OnInterval(0.05f))
                    randomAnxietyOffset = Calc.Random.Range(-0.2f, 0.2f);
                Vector2 position = (Scene as Level).Camera.Position;
                Player entity1 = Scene.Tracker.GetEntity<Player>();
                float target;
                float num1;
                if (entity1 != null && !entity1.Dead)
                {
                    float num2 = -1f;
                    float num3 = -1f;
                    foreach (Seeker entity2 in Scene.Tracker.GetEntities<Seeker>())
                    {
                        float val2 = Vector2.DistanceSquared(entity1.Center, entity2.Center);
                        if (!entity2.Regenerating)
                            num2 = num2 >= 0.0 ? Math.Min(num2, val2) : val2;
                        if (entity2.Attacking)
                            num3 = num3 >= 0.0 ? Math.Min(num3, val2) : val2;
                    }
                    target = num3 < 0.0 ? 1f : Calc.ClampedMap(num3, 256f, 4096f, 0.5f);
                    Distort.AnxietyOrigin = new Vector2((float) ((entity1.Center.X - (double) position.X) / 320.0), (float) ((entity1.Center.Y - (double) position.Y) / 180.0));
                    num1 = num2 < 0.0 ? 0.0f : Calc.ClampedMap(num2, 256f, 16384f, 1f, 0.0f);
                }
                else
                {
                    target = 1f;
                    num1 = 0.0f;
                }
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, target, 4f * Engine.DeltaTime);
                Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f), Engine.DeltaTime * 2f);
                Distort.Anxiety = Calc.Approach(Distort.Anxiety, (0.5f + randomAnxietyOffset) * num1, 8f * Engine.DeltaTime);
                if (Engine.TimeRate != 1.0 || Distort.GameRate != 1.0 || Distort.Anxiety != 0.0 || Scene.Tracker.CountEntities<Seeker>() != 0)
                    return;
                enabled = false;
            }
            else
            {
                if (Scene.Tracker.CountEntities<Seeker>() <= 0)
                    return;
                enabled = true;
            }
        }
    }
}
