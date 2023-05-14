// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterAbsorbEffect
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class ClutterAbsorbEffect : Entity
    {
        private Level level;
        private readonly List<ClutterCabinet> cabinets = new();

        public ClutterAbsorbEffect()
        {
            Position = Vector2.Zero;
            Tag = (int)Tags.TransitionUpdate;
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            foreach (Entity entity in level.Tracker.GetEntities<ClutterCabinet>())
            {
                cabinets.Add(entity as ClutterCabinet);
            }
        }

        public void FlyClutter(Vector2 position, MTexture texture, bool shake, float delay)
        {
            Image img = new(texture)
            {
                Position = position - Position
            };
            _ = img.CenterOrigin();
            Add(img);
            Add(new Coroutine(FlyClutterRoutine(img, shake, delay))
            {
                RemoveOnComplete = true
            });
        }

        private IEnumerator FlyClutterRoutine(Image img, bool shake, float delay)
        {
            ClutterAbsorbEffect clutterAbsorbEffect = this;
            yield return delay;
            ClutterCabinet cabinet = Calc.Random.Choose(clutterAbsorbEffect.cabinets);
            Vector2 vector2_1 = cabinet.Position + new Vector2(8f);
            Vector2 from = img.Position;
            Vector2 vector2_2 = new(Calc.Random.Next(16) - 8, Calc.Random.Next(4) - 2);
            Vector2 end = vector2_1 + vector2_2;
            Vector2 vector2_3 = (end - from).SafeNormalize();
            float num = (end - from).Length();
            Vector2 vector2_4 = new Vector2(-vector2_3.Y, vector2_3.X) * ((num / 4f) + Calc.Random.NextFloat(40f)) * (Calc.Random.Chance(0.5f) ? -1f : 1f);
            SimpleCurve curve = new(from, end, ((end + from) / 2f) + vector2_4);
            float time;
            if (shake)
            {
                for (time = 0.25f; time > 0; time -= Engine.DeltaTime)
                {
                    img.X = from.X + Calc.Random.Next(3) - 1;
                    img.Y = from.Y + Calc.Random.Next(3) - 1;
                    yield return null;
                }
            }
            for (time = 0f; time < 1; time += Engine.DeltaTime)
            {
                img.Position = curve.GetPoint(Ease.CubeInOut(time));
                img.Scale = Vector2.One * Ease.CubeInOut(1 - (time * 0.5f));
                if (time > 0.5f && !cabinet.Opened)
                {
                    cabinet.Open();
                }

                if (clutterAbsorbEffect.level.OnInterval(0.25f))
                {
                    clutterAbsorbEffect.level.ParticlesFG.Emit(ClutterSwitch.P_ClutterFly, img.Position);
                }

                yield return null;
            }
            clutterAbsorbEffect.Remove(img);
        }

        public void CloseCabinets()
        {
            Add(new Coroutine(CloseCabinetsRoutine()));
        }

        private IEnumerator CloseCabinetsRoutine()
        {
            cabinets.Sort((a, b) => Math.Abs(a.Y - b.Y) < 24 ? Math.Sign(a.X - b.X) : Math.Sign(a.Y - b.Y));
            int i = 0;
            foreach (ClutterCabinet cabinet in cabinets)
            {
                cabinet.Close();
                if (i++ % 3 == 0)
                {
                    yield return 0.1f;
                }
            }
        }
    }
}
