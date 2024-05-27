using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class StrawberryPoints : Entity
    {
        private Sprite sprite;
        private bool ghostberry;
        private bool moonberry;
        private VertexLight light;
        private BloomPoint bloom;
        private int index;
        private DisplacementRenderer.Burst burst;

        public StrawberryPoints(Vector2 position, bool ghostberry, int index, bool moonberry)
            : base(position)
        {
            Add(sprite = GFX.SpriteBank.Create("strawberry"));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(bloom = new BloomPoint(1f, 12f));
            Depth = -2000100;
            Tag = (int) Tags.Persistent | (int) Tags.TransitionUpdate | (int) Tags.FrozenUpdate;
            this.ghostberry = ghostberry;
            this.moonberry = moonberry;
            this.index = index;
        }

        public override void Added(Scene scene)
        {
            index = Math.Min(5, index);
            if (index >= 5)
                Achievements.Register(Achievement.ONEUP);
            if (moonberry)
                sprite.Play("fade_wow");
            else
                sprite.Play("fade" + index);
            sprite.OnFinish = a => RemoveSelf();
            base.Added(scene);
            foreach (Entity entity in Scene.Tracker.GetEntities<StrawberryPoints>())
            {
                if (entity != this && Vector2.DistanceSquared(entity.Position, Position) <= 256.0)
                    entity.RemoveSelf();
            }
            burst = (scene as Level).Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f);
        }

        public override void Update()
        {
            Level scene = Scene as Level;
            if (scene.Frozen)
            {
                if (burst == null)
                    return;
                burst.AlphaFrom = burst.AlphaTo = 0.0f;
                burst.Percent = burst.Duration;
            }
            else
            {
                base.Update();
                Camera camera = scene.Camera;
                Y -= 8f * Engine.DeltaTime;
                X = Calc.Clamp(X, camera.Left + 8f, camera.Right - 8f);
                Y = Calc.Clamp(Y, camera.Top + 8f, camera.Bottom - 8f);
                light.Alpha = Calc.Approach(light.Alpha, 0.0f, Engine.DeltaTime * 4f);
                bloom.Alpha = light.Alpha;
                ParticleType type = ghostberry ? Strawberry.P_GhostGlow : Strawberry.P_Glow;
                if (moonberry && !ghostberry)
                    type = Strawberry.P_MoonGlow;
                if (Scene.OnInterval(0.05f))
                {
                    if (sprite.Color == type.Color2)
                        sprite.Color = type.Color;
                    else
                        sprite.Color = type.Color2;
                }
                if (!Scene.OnInterval(0.06f) || sprite.CurrentAnimationFrame <= 11)
                    return;
                scene.ParticlesFG.Emit(type, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
            }
        }
    }
}
