using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class GondolaDarkness : Entity
    {
        private Sprite sprite;
        private Sprite hands;
        private Blackness blackness;
        private float anxiety;
        private float anxietyStutter;
        private WindSnowFG windSnowFG;

        public GondolaDarkness()
        {
            Add(sprite = GFX.SpriteBank.Create("gondolaDarkness"));
            sprite.Play("appear");
            Add(hands = GFX.SpriteBank.Create("gondolaHands"));
            hands.Visible = false;
            Visible = false;
            Depth = -999900;
        }

        public IEnumerator Appear(WindSnowFG windSnowFG = null)
        {
            GondolaDarkness gondolaDarkness = this;
            gondolaDarkness.windSnowFG = windSnowFG;
            gondolaDarkness.Visible = true;
            gondolaDarkness.Scene.Add(gondolaDarkness.blackness = new Blackness());
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / 2f)
            {
                yield return null;
                gondolaDarkness.blackness.Fade = t;
                gondolaDarkness.anxiety = t;
                if (windSnowFG != null)
                    windSnowFG.Alpha = 1f - t;
            }
            yield return null;
        }

        public IEnumerator Expand()
        {
            hands.Visible = true;
            hands.Play("appear");
            yield return 1f;
        }

        public IEnumerator Reach(Gondola gondola)
        {
            hands.Play("grab");
            yield return 0.4f;
            hands.Play("pull");
            gondola.PullSides();
        }

        public override void Update()
        {
            base.Update();
            if (Scene.OnInterval(0.05f))
                anxietyStutter = Calc.Random.NextFloat(0.1f);
            Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
            Distort.Anxiety = (float) (anxiety * 0.20000000298023224 + anxietyStutter * (double) anxiety);
        }

        public override void Render()
        {
            Position = (Scene as Level).Camera.Position + (Scene as Level).ZoomFocusPoint;
            base.Render();
        }

        public override void Removed(Scene scene)
        {
            anxiety = 0.0f;
            Distort.Anxiety = 0.0f;
            if (blackness != null)
                blackness.RemoveSelf();
            if (windSnowFG != null)
                windSnowFG.Alpha = 1f;
            base.Removed(scene);
        }

        private class Blackness : Entity
        {
            public float Fade;

            public Blackness() => Depth = 9001;

            public override void Render()
            {
                base.Render();
                Camera camera = (Scene as Level).Camera;
                Draw.Rect(camera.Left - 1f, camera.Top - 1f, 322f, 182f, Color.Black * Fade);
            }
        }
    }
}
