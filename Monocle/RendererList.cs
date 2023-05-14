// Decompiled with JetBrains decompiler
// Type: Monocle.RendererList
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections.Generic;

namespace Monocle
{
    public class RendererList
    {
        public List<Renderer> Renderers;
        private readonly List<Renderer> adding;
        private readonly List<Renderer> removing;
        private readonly Scene scene;

        internal RendererList(Scene scene)
        {
            this.scene = scene;
            Renderers = new List<Renderer>();
            adding = new List<Renderer>();
            removing = new List<Renderer>();
        }

        internal void UpdateLists()
        {
            if (adding.Count > 0)
            {
                foreach (Renderer renderer in adding)
                {
                    Renderers.Add(renderer);
                }
            }
            adding.Clear();
            if (removing.Count > 0)
            {
                foreach (Renderer renderer in removing)
                {
                    _ = Renderers.Remove(renderer);
                }
            }
            removing.Clear();
        }

        internal void Update()
        {
            foreach (Renderer renderer in Renderers)
            {
                renderer.Update(scene);
            }
        }

        internal void BeforeRender()
        {
            for (int index = 0; index < Renderers.Count; ++index)
            {
                if (Renderers[index].Visible)
                {
                    Draw.Renderer = Renderers[index];
                    Renderers[index].BeforeRender(scene);
                }
            }
        }

        internal void Render()
        {
            for (int index = 0; index < Renderers.Count; ++index)
            {
                if (Renderers[index].Visible)
                {
                    Draw.Renderer = Renderers[index];
                    Renderers[index].Render(scene);
                }
            }
        }

        internal void AfterRender()
        {
            for (int index = 0; index < Renderers.Count; ++index)
            {
                if (Renderers[index].Visible)
                {
                    Draw.Renderer = Renderers[index];
                    Renderers[index].AfterRender(scene);
                }
            }
        }

        public void MoveToFront(Renderer renderer)
        {
            _ = Renderers.Remove(renderer);
            Renderers.Add(renderer);
        }

        public void Add(Renderer renderer)
        {
            adding.Add(renderer);
        }

        public void Remove(Renderer renderer)
        {
            removing.Add(renderer);
        }
    }
}
