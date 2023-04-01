// Decompiled with JetBrains decompiler
// Type: Celeste.BackdropRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class BackdropRenderer : Monocle.Renderer
    {
        public Matrix Matrix = Matrix.Identity;
        public List<Backdrop> Backdrops = new();
        public float Fade;
        public Color FadeColor = Color.Black;
        private bool usingSpritebatch;

        public override void BeforeRender(Scene scene)
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                backdrop.BeforeRender(scene);
            }
        }

        public override void Update(Scene scene)
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                backdrop.Update(scene);
            }
        }

        public void Ended(Scene scene)
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                backdrop.Ended(scene);
            }
        }

        public T Get<T>() where T : class
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                if (backdrop is T)
                {
                    return backdrop as T;
                }
            }
            return default;
        }

        public IEnumerable<T> GetEach<T>() where T : class
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                if (backdrop is T)
                {
                    yield return backdrop as T;
                }
            }
        }

        public IEnumerable<T> GetEach<T>(string tag) where T : class
        {
            foreach (Backdrop backdrop in Backdrops)
            {
                if (backdrop is T && backdrop.Tags.Contains(tag))
                {
                    yield return backdrop as T;
                }
            }
        }

        public void StartSpritebatch(BlendState blendState)
        {
            if (!usingSpritebatch)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix);
            }

            usingSpritebatch = true;
        }

        public void EndSpritebatch()
        {
            if (usingSpritebatch)
            {
                Draw.SpriteBatch.End();
            }

            usingSpritebatch = false;
        }

        public override void Render(Scene scene)
        {
            BlendState blendState = BlendState.AlphaBlend;
            foreach (Backdrop backdrop in Backdrops)
            {
                if (backdrop.Visible)
                {
                    if (backdrop is Parallax && (backdrop as Parallax).BlendState != blendState)
                    {
                        EndSpritebatch();
                        blendState = (backdrop as Parallax).BlendState;
                    }
                    if (backdrop.UseSpritebatch && !usingSpritebatch)
                    {
                        StartSpritebatch(blendState);
                    }

                    if (!backdrop.UseSpritebatch && usingSpritebatch)
                    {
                        EndSpritebatch();
                    }

                    backdrop.Render(scene);
                }
            }
            if (Fade > 0.0)
            {
                Draw.Rect(-10f, -10f, 340f, 200f, FadeColor * Fade);
            }

            EndSpritebatch();
        }

        public void Remove<T>() where T : Backdrop
        {
            for (int index = Backdrops.Count - 1; index >= 0; --index)
            {
                if (Backdrops[index].GetType() == typeof(T))
                {
                    Backdrops.RemoveAt(index);
                }
            }
        }
    }
}
