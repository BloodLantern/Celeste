// Decompiled with JetBrains decompiler
// Type: Celeste.NPC02_Theo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC02_Theo : NPC
    {
        private const string DoneTalking = "theoDoneTalking";
        private const string HadntMetAtStart = "hadntMetTheoAtStart";
        private Coroutine talkRoutine;
        private Selfie selfie;

        private int CurrentConversation
        {
            get => this.Session.GetCounter("theo");
            set => this.Session.SetCounter("theo", value);
        }

        public NPC02_Theo(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("theo")));
            this.Sprite.Play("idle");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (this.Session.GetFlag("theoDoneTalking"))
                return;
            this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-20, -8, 100, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
        }

        private void OnTalk(Player player)
        {
            if (!SaveData.Instance.HasFlag("MetTheo") || !SaveData.Instance.HasFlag("TheoKnowsName"))
                this.CurrentConversation = -1;
            this.Level.StartCutscene(new Action<Level>(this.OnTalkEnd));
            this.Add((Component) (this.talkRoutine = new Coroutine(this.Talk(player))));
        }

        private IEnumerator Talk(Player player)
        {
            NPC02_Theo npC02Theo = this;
            if (!SaveData.Instance.HasFlag("MetTheo"))
            {
                npC02Theo.Session.SetFlag("hadntMetTheoAtStart");
                SaveData.Instance.SetFlag("MetTheo");
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_INTRO_NEVER_MET");
            }
            else if (!SaveData.Instance.HasFlag("TheoKnowsName"))
            {
                npC02Theo.Session.SetFlag("hadntMetTheoAtStart");
                SaveData.Instance.SetFlag("TheoKnowsName");
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_INTRO_NEVER_INTRODUCED");
            }
            else if (npC02Theo.CurrentConversation <= 0)
            {
                yield return (object) npC02Theo.PlayerApproachRightSide(player);
                yield return (object) 0.2f;
                if (npC02Theo.Session.GetFlag("hadntMetTheoAtStart"))
                {
                    yield return (object) npC02Theo.PlayerApproach48px();
                    yield return (object) Textbox.Say("CH2_THEO_A", new Func<IEnumerator>(npC02Theo.ShowPhotos), new Func<IEnumerator>(npC02Theo.HidePhotos), new Func<IEnumerator>(npC02Theo.Selfie));
                }
                else
                    yield return (object) Textbox.Say("CH2_THEO_A_EXT", new Func<IEnumerator>(npC02Theo.ShowPhotos), new Func<IEnumerator>(npC02Theo.HidePhotos), new Func<IEnumerator>(npC02Theo.Selfie), new Func<IEnumerator>(((NPC) npC02Theo).PlayerApproach48px));
            }
            else if (npC02Theo.CurrentConversation == 1)
            {
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_B", new Func<IEnumerator>(npC02Theo.SelfieFiltered));
            }
            else if (npC02Theo.CurrentConversation == 2)
            {
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_C");
            }
            else if (npC02Theo.CurrentConversation == 3)
            {
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_D");
            }
            else if (npC02Theo.CurrentConversation == 4)
            {
                yield return (object) npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return (object) Textbox.Say("CH2_THEO_E");
            }
            npC02Theo.Level.EndCutscene();
            npC02Theo.OnTalkEnd(npC02Theo.Level);
        }

        private void OnTalkEnd(Level level)
        {
            if (this.CurrentConversation == 4)
            {
                this.Session.SetFlag("theoDoneTalking");
                this.Remove((Component) this.Talker);
            }
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
                if (level.SkippingCutscene)
                {
                    entity.X = (float) (int) ((double) this.X + 48.0);
                    entity.Facing = Facings.Left;
                }
            }
            this.Sprite.Scale.X = 1f;
            if (this.selfie != null)
                this.selfie.RemoveSelf();
            ++this.CurrentConversation;
            this.talkRoutine.Cancel();
            this.talkRoutine.RemoveSelf();
        }

        private IEnumerator ShowPhotos()
        {
            NPC02_Theo npC02Theo = this;
            Player entity = npC02Theo.Scene.Tracker.GetEntity<Player>();
            yield return (object) npC02Theo.PlayerApproach(entity, spacing: new float?(10f));
            npC02Theo.Sprite.Play("getPhone");
            yield return (object) 2f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator HidePhotos()
        {
                this.Sprite.Play("idle", false, false);
                yield return 0.5f;
                yield break;
        }

        private IEnumerator Selfie()
        {
            NPC02_Theo npC02Theo = this;
            yield return (object) 0.5f;
            Audio.Play("event:/game/02_old_site/theoselfie_foley", npC02Theo.Position);
            npC02Theo.Sprite.Scale.X = -npC02Theo.Sprite.Scale.X;
            npC02Theo.Sprite.Play("takeSelfie");
            yield return (object) 1f;
            npC02Theo.Scene.Add((Entity) (npC02Theo.selfie = new Selfie(npC02Theo.SceneAs<Level>())));
            yield return (object) npC02Theo.selfie.PictureRoutine();
            npC02Theo.selfie = (Selfie) null;
            npC02Theo.Sprite.Scale.X = -npC02Theo.Sprite.Scale.X;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator SelfieFiltered()
                {
                base.Scene.Add(this.selfie = new Selfie(base.SceneAs<Level>()));
                yield return this.selfie.FilterRoutine();
                this.selfie = null;
                yield break;
        }
    }
}
