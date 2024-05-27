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
        private List<ClutterCabinet> cabinets = new List<ClutterCabinet>();

        public ClutterAbsorbEffect()
        {
            Position = Vector2.Zero;
            Tag = (int) Tags.TransitionUpdate;
            Depth = -10001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            foreach (Entity entity in level.Tracker.GetEntities<ClutterCabinet>())
                cabinets.Add(entity as ClutterCabinet);
        }

        public void FlyClutter(Vector2 position, MTexture texture, bool shake, float delay)
        {
            Image img = new Image(texture);
            img.Position = position - Position;
            img.CenterOrigin();
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
            Vector2 vector2_2 = new Vector2(Calc.Random.Next(16) - 8, Calc.Random.Next(4) - 2);
            Vector2 end = vector2_1 + vector2_2;
            Vector2 vector2_3 = (end - from).SafeNormalize();
            float num = (end - from).Length();
            Vector2 vector2_4 = new Vector2(-vector2_3.Y, vector2_3.X) * (num / 4f + Calc.Random.NextFloat(40f)) * (Calc.Random.Chance(0.5f) ? -1f : 1f);
            SimpleCurve curve = new SimpleCurve(from, end, (end + from) / 2f + vector2_4);
            float time;
            if (shake)
            {
                for (time = 0.25f; time > 0.0; time -= Engine.DeltaTime)
                {
                    img.X = (float) (from.X + (double) Calc.Random.Next(3) - 1.0);
                    img.Y = (float) (from.Y + (double) Calc.Random.Next(3) - 1.0);
                    yield return null;
                }
            }
            for (time = 0.0f; time < 1.0; time += Engine.DeltaTime)
            {
                img.Position = curve.GetPoint(Ease.CubeInOut(time));
                img.Scale = Vector2.One * Ease.CubeInOut((float) (1.0 - time * 0.5));
                if (time > 0.5 && !cabinet.Opened)
                    cabinet.Open();
                if (clutterAbsorbEffect.level.OnInterval(0.25f))
                    clutterAbsorbEffect.level.ParticlesFG.Emit(ClutterSwitch.P_ClutterFly, img.Position);
                yield return null;
            }
            clutterAbsorbEffect.Remove(img);
        }

        public void CloseCabinets() => Add(new Coroutine(CloseCabinetsRoutine()));

        private IEnumerator CloseCabinetsRoutine()
        {
            cabinets.Sort((a, b) => Math.Abs(a.Y - b.Y) < 24.0 ? Math.Sign(a.X - b.X) : Math.Sign(a.Y - b.Y));
            int i = 0;
            foreach (ClutterCabinet cabinet in cabinets)
            {
                cabinet.Close();
                if (i++ % 3 == 0)
                    yield return 0.1f;
            }
        }
    }
}
