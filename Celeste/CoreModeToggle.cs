using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class CoreModeToggle : Entity
    {
        private const float Cooldown = 1f;
        private bool iceMode;
        private float cooldownTimer;
        private bool onlyFire;
        private bool onlyIce;
        private bool persistent;
        private bool playSounds;
        private Sprite sprite;

        public CoreModeToggle(Vector2 position, bool onlyFire, bool onlyIce, bool persistent)
            : base(position)
        {
            this.onlyFire = onlyFire;
            this.onlyIce = onlyIce;
            this.persistent = persistent;
            Collider = new Hitbox(16f, 24f, -8f, -12f);
            Add(new CoreModeListener(OnChangeMode));
            Add(new PlayerCollider(OnPlayer));
            Add(sprite = GFX.SpriteBank.Create("coreFlipSwitch"));
            Depth = 2000;
        }

        public CoreModeToggle(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool(nameof (onlyFire)), data.Bool(nameof (onlyIce)), data.Bool(nameof (persistent)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold;
            SetSprite(false);
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            iceMode = mode == Session.CoreModes.Cold;
            SetSprite(true);
        }

        private void SetSprite(bool animate)
        {
            if (animate)
            {
                if (playSounds)
                    Audio.Play(iceMode ? "event:/game/09_core/switch_to_cold" : "event:/game/09_core/switch_to_hot", Position);
                if (Usable)
                {
                    sprite.Play(iceMode ? "ice" : "hot");
                }
                else
                {
                    if (playSounds)
                        Audio.Play("event:/game/09_core/switch_dies", Position);
                    sprite.Play(iceMode ? "iceOff" : "hotOff");
                }
            }
            else if (Usable)
                sprite.Play(iceMode ? "iceLoop" : "hotLoop");
            else
                sprite.Play(iceMode ? "iceOffLoop" : "hotOffLoop");
            playSounds = false;
        }

        private void OnPlayer(Player player)
        {
            if (!Usable || cooldownTimer > 0.0)
                return;
            playSounds = true;
            Level level = SceneAs<Level>();
            level.CoreMode = level.CoreMode != Session.CoreModes.Cold ? Session.CoreModes.Cold : Session.CoreModes.Hot;
            if (persistent)
                level.Session.CoreMode = level.CoreMode;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            level.Flash(Color.White * 0.15f, true);
            Celeste.Freeze(0.05f);
            cooldownTimer = 1f;
        }

        public override void Update()
        {
            base.Update();
            if (cooldownTimer <= 0.0)
                return;
            cooldownTimer -= Engine.DeltaTime;
        }

        private bool Usable
        {
            get
            {
                if (onlyFire && !iceMode)
                    return false;
                return !onlyIce || !iceMode;
            }
        }
    }
}
