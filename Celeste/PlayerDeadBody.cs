using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class PlayerDeadBody : Entity
    {
        public Action DeathAction;
        public float ActionDelay;
        public bool HasGolden;
        private Color initialHairColor;
        private Vector2 bounce = Vector2.Zero;
        private Player player;
        private PlayerHair hair;
        private PlayerSprite sprite;
        private VertexLight light;
        private DeathEffect deathEffect;
        private Facings facing;
        private float scale = 1f;
        private bool finished;

        public PlayerDeadBody(Player player, Vector2 direction)
        {
            Depth = -1000000;
            this.player = player;
            facing = player.Facing;
            Position = player.Position;
            Add(hair = player.Hair);
            Add(sprite = player.Sprite);
            Add(light = player.Light);
            sprite.Color = Color.White;
            initialHairColor = hair.Color;
            bounce = direction;
            Add(new Coroutine(DeathRoutine()));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!(bounce != Vector2.Zero))
                return;
            if (Math.Abs(bounce.X) > (double) Math.Abs(bounce.Y))
            {
                sprite.Play("deadside");
                facing = (Facings) (-Math.Sign(bounce.X));
            }
            else
            {
                bounce = Calc.AngleToVector(Calc.AngleApproach(bounce.Angle(), new Vector2(-(int) player.Facing, 0.0f).Angle(), 0.5f), 1f);
                if (bounce.Y < 0.0)
                    sprite.Play("deadup");
                else
                    sprite.Play("deaddown");
            }
        }

        private IEnumerator DeathRoutine()
        {
            PlayerDeadBody playerDeadBody1 = this;
            Level level = playerDeadBody1.SceneAs<Level>();
            if (playerDeadBody1.bounce != Vector2.Zero)
            {
                PlayerDeadBody playerDeadBody = playerDeadBody1;
                Audio.Play("event:/char/madeline/predeath", playerDeadBody1.Position);
                playerDeadBody1.scale = 1.5f;
                Celeste.Freeze(0.05f);
                yield return null;
                Vector2 from = playerDeadBody1.Position;
                Vector2 to = from + playerDeadBody1.bounce * 24f;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, true);
                playerDeadBody1.Add(tween);
                tween.OnUpdate = t =>
                {
                    playerDeadBody.Position = from + (to - from) * t.Eased;
                    playerDeadBody.scale = (float) (1.5 - t.Eased * 0.5);
                    playerDeadBody.sprite.Rotation = (float) (Math.Floor(t.Eased * 4.0) * 6.2831854820251465);
                };
                yield return (float) (tween.Duration * 0.75);
                tween.Stop();
                tween = null;
            }
            playerDeadBody1.Position += Vector2.UnitY * -5f;
            level.Displacement.AddBurst(playerDeadBody1.Position, 0.3f, 0.0f, 80f);
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            Audio.Play(playerDeadBody1.HasGolden ? "event:/new_content/char/madeline/death_golden" : "event:/char/madeline/death", playerDeadBody1.Position);
            playerDeadBody1.deathEffect = new DeathEffect(playerDeadBody1.initialHairColor, playerDeadBody1.Center - playerDeadBody1.Position);
            // ISSUE: reference to a compiler-generated method
            playerDeadBody1.deathEffect.OnUpdate = delegate (float f)
            {
                    light.Alpha = 1f - f;
            };
            playerDeadBody1.Add(playerDeadBody1.deathEffect);
            yield return (float) (playerDeadBody1.deathEffect.Duration * 0.64999997615814209);
            if (playerDeadBody1.ActionDelay > 0.0)
                yield return playerDeadBody1.ActionDelay;
            playerDeadBody1.End();
        }

        private void End()
        {
            if (finished)
                return;
            finished = true;
            Level level = SceneAs<Level>();
            if (DeathAction == null)
                DeathAction = level.Reload;
            level.DoScreenWipe(false, DeathAction);
        }

        public override void Update()
        {
            base.Update();
            if (Input.MenuConfirm.Pressed && !finished)
                End();
            hair.Color = sprite.CurrentAnimationFrame == 0 ? Color.White : initialHairColor;
        }

        public override void Render()
        {
            if (deathEffect == null)
            {
                sprite.Scale.X = (float) facing * scale;
                sprite.Scale.Y = scale;
                hair.Facing = facing;
                base.Render();
            }
            else
                deathEffect.Render();
        }
    }
}
