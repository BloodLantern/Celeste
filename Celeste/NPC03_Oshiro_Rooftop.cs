using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC03_Oshiro_Rooftop : NPC
    {
        public NPC03_Oshiro_Rooftop(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(1));
            (Sprite as OshiroSprite).AllowTurnInvisible = false;
            MoveAnim = "move";
            IdleAnim = "idle";
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("oshiro_resort_roof"))
            {
                RemoveSelf();
            }
            else
            {
                Visible = false;
                Scene.Add(new CS03_OshiroRooftop(this));
            }
        }
    }
}
