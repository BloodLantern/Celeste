// Decompiled with JetBrains decompiler
// Type: Celeste.PicoConsole
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class PicoConsole : Entity
    {
        private Monocle.Image sprite;
        private TalkComponent talk;
        private bool talking;
        private SoundSource sfx;

        public PicoConsole(Vector2 position)
            : base(position)
        {
            this.Depth = 1000;
            this.AddTag((int) Tags.TransitionUpdate);
            this.AddTag((int) Tags.PauseUpdate);
            this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["objects/pico8Console"])));
            this.sprite.JustifyOrigin(0.5f, 1f);
            this.Add((Component) (this.talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnInteract))));
        }

        public PicoConsole(EntityData data, Vector2 position)
            : this(data.Position + position)
        {
        }

        public override void Update()
        {
            base.Update();
            if (this.sfx != null)
                return;
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null || (double) entity.Y >= (double) this.Y + 16.0)
                return;
            this.Add((Component) (this.sfx = new SoundSource("event:/env/local/03_resort/pico8_machine")));
        }

        private void OnInteract(Player player)
        {
            if (this.talking)
                return;
            (this.Scene as Level).PauseLock = true;
            this.talking = true;
            this.Add((Component) new Coroutine(this.InteractRoutine(player)));
        }

        private IEnumerator InteractRoutine(Player player)
        {
            PicoConsole picoConsole = this;
            player.StateMachine.State = 11;
            yield return (object) player.DummyWalkToExact((int) picoConsole.X - 6);
            player.Facing = Facings.Right;
            bool wasUnlocked = Settings.Instance.Pico8OnMainMenu;
            Settings.Instance.Pico8OnMainMenu = true;
            if (!wasUnlocked)
            {
                UserIO.SaveHandler(false, true);
                while (UserIO.Saving)
                    yield return (object) null;
            }
            else
                yield return (object) 0.5f;
            bool done = false;
            SpotlightWipe.FocusPoint = player.Position - (picoConsole.Scene as Level).Camera.Position + new Vector2(0.0f, -8f);
            SpotlightWipe spotlightWipe = new SpotlightWipe(picoConsole.Scene, false, (Action) (() =>
            {
                if (!wasUnlocked)
                    this.Scene.Add((Entity) new UnlockedPico8Message((Action) (() => done = true)));
                else
                    done = true;
                Engine.Scene = (Scene) new Emulator((Scene) (this.Scene as Level));
            }));
            while (!done)
                yield return (object) null;
            yield return (object) 0.25f;
            picoConsole.talking = false;
            (picoConsole.Scene as Level).PauseLock = false;
            player.StateMachine.State = 0;
        }

        public override void SceneEnd(Scene scene)
        {
            if (this.sfx != null)
            {
                this.sfx.Stop();
                this.sfx.RemoveSelf();
                this.sfx = (SoundSource) null;
            }
            base.SceneEnd(scene);
        }
    }
}
