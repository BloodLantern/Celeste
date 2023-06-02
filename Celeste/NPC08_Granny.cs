using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC08_Granny : NPC
    {
        public NPC08_Granny(EntityData data, Vector2 position)
            : base(data.Position + position)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("granny")));
            this.Sprite.Scale.X = -1f;
            this.Sprite.Play("idle");
            this.IdleAnim = "idle";
            this.MoveAnim = "walk";
            this.Maxspeed = 30f;
            this.Depth = -10;
        }
    }
}
