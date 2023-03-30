// Decompiled with JetBrains decompiler
// Type: Monocle.Renderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
    public abstract class Renderer
    {
        public bool Visible = true;

        public virtual void Update(Scene scene)
        {
        }

        public virtual void BeforeRender(Scene scene)
        {
        }

        public virtual void Render(Scene scene)
        {
        }

        public virtual void AfterRender(Scene scene)
        {
        }
    }
}
