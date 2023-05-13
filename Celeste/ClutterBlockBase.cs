// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterBlockBase
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class ClutterBlockBase : Solid
    {
        private static readonly Color enabledColor = Color.Black * 0.7f;
        private static readonly Color disabledColor = Color.Black * 0.3f;
        public ClutterBlock.Colors BlockColor;
        private Color color;
        private bool enabled;
        private LightOcclude occluder;

        public ClutterBlockBase(
            Vector2 position,
            int width,
            int height,
            bool enabled,
            ClutterBlock.Colors blockColor)
            : base(position, width, height, true)
        {
            EnableAssistModeChecks = false;
            BlockColor = blockColor;
            Depth = 8999;
            this.enabled = enabled;
            color = enabled ? enabledColor : disabledColor;
            if (enabled)
                Add(occluder = new LightOcclude());
            else
                Collidable = false;

            switch (blockColor)
            {
                case ClutterBlock.Colors.Red:
                    SurfaceSoundIndex = 17;
                    break;
                case ClutterBlock.Colors.Green:
                    SurfaceSoundIndex = 19;
                    break;
                case ClutterBlock.Colors.Yellow:
                    SurfaceSoundIndex = 18;
                    break;
            }
        }

        public void Deactivate()
        {
            Collidable = false;
            color = disabledColor;
            enabled = false;
            if (occluder == null)
                return;

            Remove(occluder);
            occluder = null;
        }

        public override void Render()
        {
            Draw.Rect(X, Y, Width, Height + (enabled ? 2f : 0f), color);
        }
    }
}
