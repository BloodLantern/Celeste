using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Plateau : Solid
    {
        private Monocle.Image sprite;
        public LightOcclude Occluder;

        public Plateau(EntityData e, Vector2 offset)
            : base(e.Position + offset, 104f, 4f, true)
        {
            this.Collider.Left += 8f;
            this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["scenery/fallplateau"])));
            this.Add((Component) (this.Occluder = new LightOcclude()));
            this.SurfaceSoundIndex = 23;
            this.EnableAssistModeChecks = false;
        }
    }
}
