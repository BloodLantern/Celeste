// Decompiled with JetBrains decompiler
// Type: Celeste.StarsBG
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class StarsBG : Backdrop
    {
        private const int StarCount = 100;
        private readonly StarsBG.Star[] stars;
        private readonly Color[] colors;
        private readonly List<List<MTexture>> textures;
        private float falling;
        private Vector2 center;

        public StarsBG()
        {
            textures = new List<List<MTexture>>
            {
                GFX.Game.GetAtlasSubtextures("bgs/02/stars/a"),
                GFX.Game.GetAtlasSubtextures("bgs/02/stars/b"),
                GFX.Game.GetAtlasSubtextures("bgs/02/stars/c")
            };
            center = new Vector2(textures[0][0].Width, textures[0][0].Height) / 2f;
            stars = new StarsBG.Star[100];
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index] = new StarsBG.Star()
                {
                    Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f)),
                    Timer = Calc.Random.NextFloat(6.28318548f),
                    Rate = 2f + Calc.Random.NextFloat(2f),
                    TextureSet = Calc.Random.Next(textures.Count)
                };
            }

            colors = new Color[8];
            for (int index = 0; index < colors.Length; ++index)
            {
                colors[index] = Color.Teal * 0.7f * (float)(1.0 - (index / (double)colors.Length));
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (!Visible)
            {
                return;
            }

            Level level = scene as Level;
            for (int index = 0; index < stars.Length; ++index)
            {
                stars[index].Timer += Engine.DeltaTime * stars[index].Rate;
            }

            if (!level.Session.Dreaming)
            {
                return;
            }

            falling += Engine.DeltaTime * 12f;
        }

        public override void Render(Scene scene)
        {
            Draw.Rect(0.0f, 0.0f, 320f, 180f, Color.Black);
            Level level = scene as Level;
            Color color = Color.White;
            int num = 100;
            if (level.Session.Dreaming)
            {
                color = Color.Teal * 0.7f;
            }
            else
            {
                num /= 2;
            }

            for (int index1 = 0; index1 < num; ++index1)
            {
                List<MTexture> texture = textures[stars[index1].TextureSet];
                int index2 = (int)((Math.Sin(stars[index1].Timer) + 1.0) / 2.0 * texture.Count) % texture.Count;
                Vector2 position = stars[index1].Position;
                MTexture mtexture = texture[index2];
                if (level.Session.Dreaming)
                {
                    position.Y -= level.Camera.Y;
                    position.Y += falling * stars[index1].Rate;
                    position.Y %= 180f;
                    if (position.Y < 0.0)
                    {
                        position.Y += 180f;
                    }

                    for (int index3 = 0; index3 < colors.Length; ++index3)
                    {
                        mtexture.Draw(position - (Vector2.UnitY * index3), center, colors[index3]);
                    }
                }
                mtexture.Draw(position, center, color);
            }
        }

        private struct Star
        {
            public Vector2 Position;
            public int TextureSet;
            public float Timer;
            public float Rate;
        }
    }
}
