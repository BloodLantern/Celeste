using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC06_Granny_Ending : NPC
    {
        private bool talked;

        public NPC06_Granny_Ending(EntityData data, Vector2 position)
            : base(data.Position + position)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("granny")));
            this.Sprite.Scale.X = -1f;
            this.Sprite.Play("idle");
            this.IdleAnim = "idle";
            this.MoveAnim = "walk";
            this.Maxspeed = 30f;
            this.Visible = false;
            this.Add((Component) (this.Light = new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 16, 32)));
            this.SetupGrannySpriteSounds();
        }

        public override void Update()
        {
            base.Update();
            if (this.talked)
                return;
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null || !entity.OnGround())
                return;
            this.talked = true;
            this.Scene.Add((Entity) new CS06_Ending(entity, (NPC) this));
        }
    }
}
