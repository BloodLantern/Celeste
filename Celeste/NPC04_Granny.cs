// Decompiled with JetBrains decompiler
// Type: Celeste.NPC04_Granny
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC04_Granny : NPC
    {
        public Hahaha Hahaha;
        private bool cutscene;
        private Coroutine talkRoutine;
        private const string talkedFlagA = "granny_2";
        private const string talkedFlagB = "granny_3";

        public NPC04_Granny(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("granny")));
            this.Sprite.Scale.X = -1f;
            this.Sprite.Play("idle");
            this.Add((Component) new GrannyLaughSfx(this.Sprite));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add((Entity) (this.Hahaha = new Hahaha(this.Position + new Vector2(8f, -4f))));
            this.Hahaha.Enabled = false;
            if (this.Session.GetFlag("granny_1") && !this.Session.GetFlag("granny_2"))
                this.Sprite.Play("laugh");
            if (this.Session.GetFlag("granny_3"))
                return;
            this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-20, -16, 40, 16), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
            if (this.Session.GetFlag("granny_1"))
                return;
            this.Talker.Enabled = false;
        }

        public override void Update()
        {
            Player entity = this.Level.Tracker.GetEntity<Player>();
            if (entity != null && !this.Session.GetFlag("granny_1") && !this.cutscene && (double) entity.X > (double) this.X - 40.0)
            {
                this.cutscene = true;
                this.Scene.Add((Entity) new CS04_Granny(this, entity));
                if (this.Talker != null)
                    this.Talker.Enabled = true;
            }
            this.Hahaha.Enabled = this.Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(Player player)
        {
            this.Level.StartCutscene(new Action<Level>(this.TalkEnd));
            this.Add((Component) (this.talkRoutine = new Coroutine(this.TalkRoutine(player))));
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC04_Granny npC04Granny = this;
            npC04Granny.Sprite.Play("idle");
            player.ForceCameraUpdate = true;
            yield return (object) npC04Granny.PlayerApproachLeftSide(player, spacing: new float?(20f));
            yield return (object) npC04Granny.Level.ZoomTo(new Vector2((float) (((double) player.X + (double) npC04Granny.X) / 2.0) - npC04Granny.Level.Camera.X, 116f), 2f, 0.5f);
            if (!npC04Granny.Session.GetFlag("granny_2"))
                yield return (object) Textbox.Say("CH4_GRANNY_2");
            else
                yield return (object) Textbox.Say("CH4_GRANNY_3");
            yield return (object) npC04Granny.Level.ZoomBack(0.5f);
            npC04Granny.Level.EndCutscene();
            npC04Granny.TalkEnd(npC04Granny.Level);
        }

        private void TalkEnd(Level level)
        {
            if (!this.Session.GetFlag("granny_2"))
                this.Session.SetFlag("granny_2");
            else if (!this.Session.GetFlag("granny_3"))
            {
                this.Session.SetFlag("granny_3");
                this.Remove((Component) this.Talker);
            }
            if (this.talkRoutine != null)
            {
                this.talkRoutine.RemoveSelf();
                this.talkRoutine = (Coroutine) null;
            }
            Player entity = this.Level.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
            entity.ForceCameraUpdate = false;
        }
    }
}
