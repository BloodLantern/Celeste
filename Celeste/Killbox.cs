using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class Killbox : Entity
    {
        public Killbox(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, 32f);
            Collidable = false;
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                player.Play("event:/game/general/assist_screenbottom");
                player.Bounce(Top);
            }
            else
                player.Die(Vector2.Zero);
        }

        public override void Update()
        {
            if (!Collidable)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Bottom < Top - 32.0)
                    Collidable = true;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Top > Bottom + 32.0)
                    Collidable = false;
            }
            base.Update();
        }
    }
}
