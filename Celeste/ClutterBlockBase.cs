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
            color = enabled ? ClutterBlockBase.enabledColor : ClutterBlockBase.disabledColor;
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
            color = ClutterBlockBase.disabledColor;
            enabled = false;
            if (occluder == null)
                return;
            Remove(occluder);
            occluder = null;
        }

        public override void Render() => Draw.Rect(X, Y, Width, Height + (enabled ? 2f : 0.0f), color);
    }
}
