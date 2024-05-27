using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Snowball : Entity
    {
        private const float ResetTime = 0.8f;
        private Sprite sprite;
        private float resetTimer;
        private Level level;
        private SineWave sine;
        private float atY;
        private SoundSource spawnSfx;
        private Collider bounceCollider;

        public Snowball()
        {
            Depth = -12500;
            Collider = new Hitbox(12f, 9f, -5f, -2f);
            bounceCollider = new Hitbox(16f, 6f, -6f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(new PlayerCollider(OnPlayerBounce, bounceCollider));
            Add(sine = new SineWave(0.5f));
            Add(sprite = GFX.SpriteBank.Create("snowball"));
            sprite.Play("spin");
            Add(spawnSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            ResetPosition();
        }

        private void ResetPosition()
        {
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null && entity.Right < (double) (level.Bounds.Right - 64))
            {
                spawnSfx.Play("event:/game/04_cliffside/snowball_spawn");
                Collidable = Visible = true;
                resetTimer = 0.0f;
                X = level.Camera.Right + 10f;
                atY = Y = entity.CenterY;
                sine.Reset();
                sprite.Play("spin");
            }
            else
                resetTimer = 0.05f;
        }

        private void Destroy()
        {
            Collidable = false;
            sprite.Play("break");
        }

        private void OnPlayer(Player player)
        {
            player.Die(new Vector2(-1f, 0.0f));
            Destroy();
            Audio.Play("event:/game/04_cliffside/snowball_impact", Position);
        }

        private void OnPlayerBounce(Player player)
        {
            if (CollideCheck(player))
                return;
            Celeste.Freeze(0.1f);
            player.Bounce(Top - 2f);
            Destroy();
            Audio.Play("event:/game/general/thing_booped", Position);
        }

        public override void Update()
        {
            base.Update();
            X -= 200f * Engine.DeltaTime;
            Y = atY + 4f * sine.Value;
            if (X >= level.Camera.Left - 60.0)
                return;
            resetTimer += Engine.DeltaTime;
            if (resetTimer < 0.800000011920929)
                return;
            ResetPosition();
        }

        public override void Render()
        {
            sprite.DrawOutline();
            base.Render();
        }
    }
}
