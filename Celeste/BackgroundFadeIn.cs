using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BackgroundFadeIn : Entity
    {
        public Color Color;
        public float Duration;
        public float Delay;
        public float Percent;

        public BackgroundFadeIn(Color color, float delay, float duration)
        {
            Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate;
            Depth = 10100;
            Color = color;
            Delay = delay;
            Duration = duration;
            Percent = 0.0f;
        }

        public override void Update()
        {
            if (Delay <= 0.0)
            {
                if (Percent >= 1.0)
                    RemoveSelf();
                Percent += Engine.DeltaTime / Duration;
            }
            else
                Delay -= Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color * (1f - Percent));
        }
    }
}
