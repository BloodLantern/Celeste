// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualAsset
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
    public abstract class VirtualAsset
    {
        public string Name { get; internal set; }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        internal virtual void Unload()
        {
        }

        internal virtual void Reload()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
