// Decompiled with JetBrains decompiler
// Type: Celeste.StarfieldWipe
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class StarfieldWipe : ScreenWipe
    {
        public static readonly BlendState SubtractBlendmode = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };
        private readonly StarfieldWipe.Star[] stars = new StarfieldWipe.Star[64];
        private readonly VertexPositionColor[] verts = new VertexPositionColor[1536];
        private readonly Vector2[] starShape = new Vector2[5];
        private bool hasDrawn;

        public StarfieldWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int index = 0; index < 5; ++index)
            {
                starShape[index] = Calc.AngleToVector((float)(index / 5.0 * 6.2831854820251465), 1f);
            }

            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index] = new StarfieldWipe.Star((float)Math.Pow(index / (double)stars.Length, 5.0));
            }

            for (int index = 0; index < verts.Length; ++index)
            {
                verts[index].Color = WipeIn ? Color.Black : Color.White;
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index].Update();
            }
        }

        public override void BeforeRender(Scene scene)
        {
            hasDrawn = true;
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)Celeste.WipeTarget);
            Engine.Graphics.GraphicsDevice.Clear(WipeIn ? Color.White : Color.Black);
            if (Percent > 0.800000011920929)
            {
                float height = Calc.Map(Percent, 0.8f, 1f) * 1082f;
                Draw.SpriteBatch.Begin();
                Draw.Rect(-1f, (float)((1080.0 - (double)height) * 0.5), 1922f, height, !WipeIn ? Color.White : Color.Black);
                Draw.SpriteBatch.End();
            }
            int index1 = 0;
            for (int index2 = 0; index2 < stars.Length; ++index2)
            {
                float xPosition = (float)((stars[index2].X % 2920.0) - 500.0);
                float yPosition = stars[index2].Y + ((float)Math.Sin(stars[index2].Sine) * stars[index2].SineDistance);
                float scale = (float)((0.10000000149011612 + (stars[index2].Scale * 0.89999997615814209)) * 1080.0 * 0.800000011920929) * Ease.CubeIn(Percent);
                DrawStar(ref index1, Matrix.CreateRotationZ(stars[index2].Rotation) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(xPosition, yPosition, 0.0f));
            }
            GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, verts, verts.Length);
        }

        private void DrawStar(ref int index, Matrix matrix)
        {
            int num = index;
            for (int index1 = 1; index1 < starShape.Length - 1; ++index1)
            {
                verts[index++].Position = new Vector3(starShape[0], 0.0f);
                verts[index++].Position = new Vector3(starShape[index1], 0.0f);
                verts[index++].Position = new Vector3(starShape[index1 + 1], 0.0f);
            }
            for (int index2 = 0; index2 < 5; ++index2)
            {
                Vector2 vector2_1 = starShape[index2];
                Vector2 vector2_2 = starShape[(index2 + 1) % 5];
                Vector2 vector2_3 = ((vector2_1 + vector2_2) * 0.5f) + (vector2_1 - vector2_2).SafeNormalize().TurnRight();
                verts[index++].Position = new Vector3(vector2_1, 0.0f);
                verts[index++].Position = new Vector3(vector2_3, 0.0f);
                verts[index++].Position = new Vector3(vector2_2, 0.0f);
            }
            for (int index3 = num; index3 < num + 24; ++index3)
            {
                verts[index3].Position = Vector3.Transform(verts[index3].Position, matrix);
            }
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, StarfieldWipe.SubtractBlendmode, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            if ((WipeIn && Percent <= 0.0099999997764825821) || (!WipeIn && Percent >= 0.99000000953674316))
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.White);
            }
            else if (hasDrawn)
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)Celeste.WipeTarget, new Vector2(-1f, -1f), Color.White);
            }

            Draw.SpriteBatch.End();
        }

        private struct Star
        {
            public float X;
            public float Y;
            public float Sine;
            public float SineDistance;
            public float Speed;
            public float Scale;
            public float Rotation;

            public Star(float scale)
            {
                Scale = scale;
                float num = 1f - scale;
                X = Calc.Random.Range(0, 2920);
                Y = (float)(1080.0 * (0.5 + (Calc.Random.Choose<int>(-1, 1) * (double)num * (double)Calc.Random.Range(0.25f, 0.5f))));
                Sine = Calc.Random.NextFloat(6.28318548f);
                SineDistance = (float)((double)scale * 1080.0 * 0.05000000074505806);
                Speed = (float)((0.5 + ((1.0 - Scale) * 0.5)) * 1920.0 * 0.05000000074505806);
                Rotation = Calc.Random.NextFloat(6.28318548f);
            }

            public void Update()
            {
                X += Speed * Engine.DeltaTime;
                Sine += (float)((1.0 - Scale) * 8.0) * Engine.DeltaTime;
                Rotation += (1f - Scale) * Engine.DeltaTime;
            }
        }
    }
}
