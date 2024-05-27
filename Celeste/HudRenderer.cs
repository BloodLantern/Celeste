using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class HudRenderer : HiresRenderer
    {
        public float BackgroundFade;

        public override void RenderContent(Scene scene)
        {
            if (!scene.Entities.HasVisibleEntities((int) Tags.HUD) && BackgroundFade <= 0.0)
                return;
            HiresRenderer.BeginRender();
            if (BackgroundFade > 0.0)
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * BackgroundFade * 0.7f);
            scene.Entities.RenderOnly((int) Tags.HUD);
            HiresRenderer.EndRender();
        }
    }
}
