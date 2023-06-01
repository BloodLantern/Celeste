// Decompiled with JetBrains decompiler
// Type: Celeste.HudRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class HudRenderer : HiresRenderer
    {
        public float BackgroundFade;

        public override void RenderContent(Scene scene)
        {
            if (!scene.Entities.HasVisibleEntities((int) Tags.HUD) && (double) this.BackgroundFade <= 0.0)
                return;
            HiresRenderer.BeginRender();
            if ((double) this.BackgroundFade > 0.0)
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * this.BackgroundFade * 0.7f);
            scene.Entities.RenderOnly((int) Tags.HUD);
            HiresRenderer.EndRender();
        }
    }
}
