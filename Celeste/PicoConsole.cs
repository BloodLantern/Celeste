using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class PicoConsole : Entity
    {
        private Image sprite;
        private TalkComponent talk;
        private bool talking;
        private SoundSource sfx;

        public PicoConsole(Vector2 position)
            : base(position)
        {
            Depth = 1000;
            AddTag((int) Tags.TransitionUpdate);
            AddTag((int) Tags.PauseUpdate);
            Add(sprite = new Image(GFX.Game["objects/pico8Console"]));
            sprite.JustifyOrigin(0.5f, 1f);
            Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0.0f, -24f), OnInteract));
        }

        public PicoConsole(EntityData data, Vector2 position)
            : this(data.Position + position)
        {
        }

        public override void Update()
        {
            base.Update();
            if (sfx != null)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || entity.Y >= Y + 16.0)
                return;
            Add(sfx = new SoundSource("event:/env/local/03_resort/pico8_machine"));
        }

        private void OnInteract(Player player)
        {
            if (talking)
                return;
            (Scene as Level).PauseLock = true;
            talking = true;
            Add(new Coroutine(InteractRoutine(player)));
        }

        private IEnumerator InteractRoutine(Player player)
        {
            PicoConsole picoConsole = this;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int) picoConsole.X - 6);
            player.Facing = Facings.Right;
            bool wasUnlocked = Settings.Instance.Pico8OnMainMenu;
            Settings.Instance.Pico8OnMainMenu = true;
            if (!wasUnlocked)
            {
                UserIO.SaveHandler(false, true);
                while (UserIO.Saving)
                    yield return null;
            }
            else
                yield return 0.5f;
            bool done = false;
            SpotlightWipe.FocusPoint = player.Position - (picoConsole.Scene as Level).Camera.Position + new Vector2(0.0f, -8f);
            SpotlightWipe spotlightWipe = new SpotlightWipe(picoConsole.Scene, false, () =>
            {
                if (!wasUnlocked)
                    Scene.Add(new UnlockedPico8Message(() => done = true));
                else
                    done = true;
                Engine.Scene = new Emulator(Scene as Level);
            });
            while (!done)
                yield return null;
            yield return 0.25f;
            picoConsole.talking = false;
            (picoConsole.Scene as Level).PauseLock = false;
            player.StateMachine.State = 0;
        }

        public override void SceneEnd(Scene scene)
        {
            if (sfx != null)
            {
                sfx.Stop();
                sfx.RemoveSelf();
                sfx = null;
            }
            base.SceneEnd(scene);
        }
    }
}
