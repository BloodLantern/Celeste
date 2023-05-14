﻿// Decompiled with JetBrains decompiler
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
            get => Session.GetCounter("theo");
            set => Session.SetCounter("theo", value);
        }

        public NPC02_Theo(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Play("idle");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("theoDoneTalking"))
            {
                return;
            }

            Add(Talker = new TalkComponent(new Rectangle(-20, -8, 100, 8), new Vector2(0.0f, -24f), new Action<Player>(OnTalk)));
        }

        private void OnTalk(Player player)
        {
            if (!SaveData.Instance.HasFlag("MetTheo") || !SaveData.Instance.HasFlag("TheoKnowsName"))
            {
                CurrentConversation = -1;
            }

            Level.StartCutscene(new Action<Level>(OnTalkEnd));
            Add(talkRoutine = new Coroutine(Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC02_Theo npC02Theo = this;
            if (!SaveData.Instance.HasFlag("MetTheo"))
            {
                npC02Theo.Session.SetFlag("hadntMetTheoAtStart");
                SaveData.Instance.SetFlag("MetTheo");
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_INTRO_NEVER_MET");
            }
            else if (!SaveData.Instance.HasFlag("TheoKnowsName"))
            {
                npC02Theo.Session.SetFlag("hadntMetTheoAtStart");
                SaveData.Instance.SetFlag("TheoKnowsName");
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_INTRO_NEVER_INTRODUCED");
            }
            else if (npC02Theo.CurrentConversation <= 0)
            {
                yield return npC02Theo.PlayerApproachRightSide(player);
                yield return 0.2f;
                if (npC02Theo.Session.GetFlag("hadntMetTheoAtStart"))
                {
                    yield return npC02Theo.PlayerApproach48px();
                    yield return Textbox.Say("CH2_THEO_A", new Func<IEnumerator>(npC02Theo.ShowPhotos), new Func<IEnumerator>(npC02Theo.HidePhotos), new Func<IEnumerator>(npC02Theo.Selfie));
                }
                else
                {
                    yield return Textbox.Say("CH2_THEO_A_EXT", new Func<IEnumerator>(npC02Theo.ShowPhotos), new Func<IEnumerator>(npC02Theo.HidePhotos), new Func<IEnumerator>(npC02Theo.Selfie), new Func<IEnumerator>(npC02Theo.PlayerApproach48px));
                }
            }
            else if (npC02Theo.CurrentConversation == 1)
            {
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_B", new Func<IEnumerator>(npC02Theo.SelfieFiltered));
            }
            else if (npC02Theo.CurrentConversation == 2)
            {
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_C");
            }
            else if (npC02Theo.CurrentConversation == 3)
            {
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_D");
            }
            else if (npC02Theo.CurrentConversation == 4)
            {
                yield return npC02Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
                yield return Textbox.Say("CH2_THEO_E");
            }
            npC02Theo.Level.EndCutscene();
            npC02Theo.OnTalkEnd(npC02Theo.Level);
        }

        private void OnTalkEnd(Level level)
        {
            if (CurrentConversation == 4)
            {
                Session.SetFlag("theoDoneTalking");
                Remove(Talker);
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
                if (level.SkippingCutscene)
                {
                    entity.X = (int)((double)X + 48.0);
                    entity.Facing = Facings.Left;
                }
            }
            Sprite.Scale.X = 1f;
            selfie?.RemoveSelf();
            ++CurrentConversation;
            talkRoutine.Cancel();
            talkRoutine.RemoveSelf();
        }

        private IEnumerator ShowPhotos()
        {
            NPC02_Theo npC02Theo = this;
            Player entity = npC02Theo.Scene.Tracker.GetEntity<Player>();
            yield return npC02Theo.PlayerApproach(entity, spacing: new float?(10f));
            npC02Theo.Sprite.Play("getPhone");
            yield return 2f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator HidePhotos()
        {
            Sprite.Play("idle", false, false);
            yield return 0.5f;
            yield break;
        }

        private IEnumerator Selfie()
        {
            NPC02_Theo npC02Theo = this;
            yield return 0.5f;
            _ = Audio.Play("event:/game/02_old_site/theoselfie_foley", npC02Theo.Position);
            npC02Theo.Sprite.Scale.X = -npC02Theo.Sprite.Scale.X;
            npC02Theo.Sprite.Play("takeSelfie");
            yield return 1f;
            npC02Theo.Scene.Add(npC02Theo.selfie = new Selfie(npC02Theo.SceneAs<Level>()));
            yield return npC02Theo.selfie.PictureRoutine();
            npC02Theo.selfie = null;
            npC02Theo.Sprite.Scale.X = -npC02Theo.Sprite.Scale.X;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator SelfieFiltered()
        {
            base.Scene.Add(selfie = new Selfie(base.SceneAs<Level>()));
            yield return selfie.FilterRoutine();
            selfie = null;
            yield break;
        }
    }
}
