// Decompiled with JetBrains decompiler
// Type: Celeste.Overlay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class Overlay : Entity
    {
        public float Fade;
        public bool XboxOverlay;

        public Overlay()
        {
            Tag = (int)Tags.HUD;
            Depth = -100000;
        }

        public override void Added(Scene scene)
        {
            if (scene is IOverlayHandler overlayHandler)
            {
                overlayHandler.Overlay = this;
            }

            base.Added(scene);
        }

        public override void Removed(Scene scene)
        {
            if (scene is IOverlayHandler overlayHandler && overlayHandler.Overlay == this)
            {
                overlayHandler.Overlay = null;
            }

            base.Removed(scene);
        }

        public IEnumerator FadeIn()
        {
            for (; Fade < 1.0; Fade += Engine.DeltaTime * 4f)
            {
                yield return null;
            }

            Fade = 1f;
        }

        public IEnumerator FadeOut()
        {
            for (; Fade > 0.0; Fade -= Engine.DeltaTime * 4f)
            {
                yield return null;
            }
        }

        public void RenderFade()
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeInOut(Fade) * 0.95f);
        }
    }
}
