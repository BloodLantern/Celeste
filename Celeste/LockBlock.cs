using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class LockBlock : Solid
    {
        public static ParticleType P_Appear;
        public EntityID ID;
        public bool UnlockingRegistered;
        private Sprite sprite;
        private bool opening;
        private bool stepMusicProgress;
        private string unlockSfxName;

        public LockBlock(
            Vector2 position,
            EntityID id,
            bool stepMusicProgress,
            string spriteName,
            string unlock_sfx)
            : base(position, 32f, 32f, false)
        {
            ID = id;
            DisableLightsInside = false;
            this.stepMusicProgress = stepMusicProgress;
            Add(new PlayerCollider(OnPlayer, new Circle(60f, 16f, 16f)));
            Add(sprite = GFX.SpriteBank.Create("lockdoor_" + spriteName));
            sprite.Play("idle");
            sprite.Position = new Vector2(Width / 2f, Height / 2f);
            if (string.IsNullOrWhiteSpace(unlock_sfx))
            {
                unlockSfxName = "event:/game/03_resort/key_unlock";
                if (spriteName == "temple_a")
                {
                    unlockSfxName = "event:/game/05_mirror_temple/key_unlock_light";
                }
                else
                {
                    if (!(spriteName == "temple_b"))
                        return;
                    unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
                }
            }
            else
                unlockSfxName = SFX.EventnameByHandle(unlock_sfx);
        }

        public LockBlock(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Bool(nameof (stepMusicProgress)), data.Attr(nameof (sprite), "wood"), data.Attr("unlock_sfx", null))
        {
        }

        public void Appear()
        {
            Visible = true;
            sprite.Play("appear");
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
            {
                Level scene = Scene as Level;
                if (!CollideCheck<Solid>(Position - Vector2.UnitX))
                {
                    scene.Particles.Emit(LockBlock.P_Appear, 16, Position + new Vector2(3f, 16f), new Vector2(2f, 10f), 3.14159274f);
                    scene.Particles.Emit(LockBlock.P_Appear, 16, Position + new Vector2(29f, 16f), new Vector2(2f, 10f), 0.0f);
                }
                scene.Shake();
            }, 0.25f, true));
        }

        private void OnPlayer(Player player)
        {
            if (opening)
                return;
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Key && !(follower.Entity as Key).StartedUsing)
                {
                    TryOpen(player, follower);
                    break;
                }
            }
        }

        private void TryOpen(Player player, Follower fol)
        {
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(player.Center, Center))
            {
                opening = true;
                (fol.Entity as Key).StartedUsing = true;
                Add(new Coroutine(UnlockRoutine(fol)));
            }
            Collidable = true;
        }

        private IEnumerator UnlockRoutine(Follower fol)
        {
            LockBlock follow = this;
            SoundEmitter emitter = SoundEmitter.Play(follow.unlockSfxName, follow);
            emitter.Source.DisposeOnTransition = true;
            Level level = follow.SceneAs<Level>();
            Key key = fol.Entity as Key;
            follow.Add(new Coroutine(key.UseRoutine(follow.Center + new Vector2(0.0f, 2f))));
            yield return 1.2f;
            follow.UnlockingRegistered = true;
            if (follow.stepMusicProgress)
            {
                ++level.Session.Audio.Music.Progress;
                level.Session.Audio.Apply();
            }
            level.Session.DoNotLoad.Add(follow.ID);
            key.RegisterUsed();
            while (key.Turning)
                yield return null;
            follow.Tag |= (int) Tags.TransitionUpdate;
            follow.Collidable = false;
            emitter.Source.DisposeOnTransition = false;
            yield return follow.sprite.PlayRoutine("open");
            level.Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return follow.sprite.PlayRoutine("burst");
            follow.RemoveSelf();
        }
    }
}
