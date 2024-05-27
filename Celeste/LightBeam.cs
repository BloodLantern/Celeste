using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class LightBeam : Entity
    {
        public static ParticleType P_Glow;
        private MTexture texture = GFX.Game["util/lightbeam"];
        private Color color = new Color(0.8f, 1f, 1f);
        private float alpha;
        public int LightWidth;
        public int LightLength;
        public float Rotation;
        public string Flag;
        private float timer = Calc.Random.NextFloat(1000f);

        public LightBeam(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = (int) Tags.TransitionUpdate;
            Depth = -9998;
            LightWidth = data.Width;
            LightLength = data.Height;
            Flag = data.Attr("flag");
            Rotation = data.Float("rotation") * ((float) Math.PI / 180f);
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            Level scene = Scene as Level;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && (string.IsNullOrEmpty(Flag) || scene.Session.GetFlag(Flag)))
            {
                Vector2 vector2_1 = Calc.ClosestPointOnLine(Position, Position + Calc.AngleToVector(Rotation + 1.57079637f, 1f) * 10000f, entity.Center);
                Vector2 vector2_2 = vector2_1 - Position;
                float target = Math.Min(1f, (float) (Math.Max(0.0f, (float) (vector2_2.Length() - 8.0)) / (double) LightLength));
                vector2_2 = vector2_1 - entity.Center;
                if (vector2_2.Length() > LightWidth / 2.0)
                    target = 1f;
                if (scene.Transitioning)
                    target = 0.0f;
                alpha = Calc.Approach(alpha, target, Engine.DeltaTime * 4f);
            }
            if (alpha >= 0.5 && scene.OnInterval(0.8f))
            {
                Vector2 vector = Calc.AngleToVector(Rotation + 1.57079637f, 1f);
                Vector2 position = Position - vector * 4f + (Calc.Random.Next(LightWidth - 4) + 2 - LightWidth / 2) * vector.Perpendicular();
                scene.Particles.Emit(LightBeam.P_Glow, position, Rotation + 1.57079637f);
            }
            base.Update();
        }

        public override void Render()
        {
            if (alpha <= 0.0)
                return;
            DrawTexture(0.0f, LightWidth, LightLength - 4 + (float) Math.Sin(timer * 2.0) * 4f, 0.4f);
            for (int index = 0; index < LightWidth; index += 4)
            {
                float num = timer + index * 0.6f;
                float width = (float) (4.0 + Math.Sin(num * 0.5 + 1.2000000476837158) * 4.0);
                float offset = (float) Math.Sin((num + (double) (index * 32)) * 0.10000000149011612 + Math.Sin(num * 0.05000000074505806 + index * 0.10000000149011612) * 0.25) * (float) (LightWidth / 2.0 - width / 2.0);
                float length = LightLength + (float) Math.Sin(num * 0.25) * 8f;
                float a = (float) (0.60000002384185791 + Math.Sin(num + 0.800000011920929) * 0.30000001192092896);
                DrawTexture(offset, width, length, a);
            }
        }

        private void DrawTexture(float offset, float width, float length, float a)
        {
            float rotation = Rotation + 1.57079637f;
            if (width < 1.0)
                return;
            texture.Draw(Position + Calc.AngleToVector(Rotation, 1f) * offset, new Vector2(0.0f, 0.5f), color * a * alpha, new Vector2(1f / texture.Width * length, width), rotation);
        }
    }
}
