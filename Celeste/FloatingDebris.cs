using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class FloatingDebris : Entity
    {
        private Vector2 start;
        private Image image;
        private SineWave sine;
        private float rotateSpeed;
        private Vector2 pushOut;
        private float accelMult = 1f;

        public FloatingDebris(Vector2 position)
            : base(position)
        {
            start = Position;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Depth = -5;
            MTexture parent = GFX.Game["scenery/debris"];
            image = new Image(new MTexture(parent, Calc.Random.Next(parent.Width / 8) * 8, 0, 8, 8));
            image.CenterOrigin();
            Add(image);
            rotateSpeed = Calc.Random.Choose(-2, -1, 0, 0, 0, 0, 0, 0, 0, 1, 2) * 40 * ((float) Math.PI / 180f);
            Add(sine = new SineWave(0.4f));
            sine.Randomize();
            image.Y = sine.Value * 2f;
            Add(new PlayerCollider(OnPlayer));
        }

        public FloatingDebris(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Update()
        {
            base.Update();
            if (pushOut != Vector2.Zero)
            {
                Position += pushOut * Engine.DeltaTime;
                pushOut = Calc.Approach(pushOut, Vector2.Zero, 64f * accelMult * Engine.DeltaTime);
            }
            else
            {
                accelMult = 1f;
                Position = Calc.Approach(Position, start, 6f * Engine.DeltaTime);
            }
            image.Rotation += rotateSpeed * Engine.DeltaTime;
            image.Y = sine.Value * 2f;
        }

        private void OnPlayer(Player player)
        {
            Vector2 vector2 = (Position - player.Center).SafeNormalize(player.Speed.Length() * 0.2f);
            if (vector2.LengthSquared() > (double) pushOut.LengthSquared())
                pushOut = vector2;
            accelMult = 1f;
        }

        public void OnExplode(Vector2 from)
        {
            pushOut = (Position - from).SafeNormalize(160f);
            accelMult = 4f;
        }
    }
}
