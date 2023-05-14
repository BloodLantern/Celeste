// Decompiled with JetBrains decompiler
// Type: Celeste.NPC09_Granny_Inside
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC09_Granny_Inside : NPC
    {
        public const string DoorConversationAvailable = "granny_door";
        private const string DoorConversationDone = "granny_door_done";
        private const string CounterFlag = "granny";
        private int conversation;
        private const int MaxConversation = 4;
        public Hahaha Hahaha;
        public GrannyLaughSfx LaughSfx;
        private Player player;
        private readonly TalkComponent talker;
        private bool talking;
        private Coroutine talkRoutine;

        private bool HasDoorConversation => Level.Session.GetFlag("granny_door") && !Level.Session.GetFlag("granny_door_done");

        private bool talkerEnabled => (conversation > 0 && conversation < 4) || HasDoorConversation;

        public NPC09_Granny_Inside(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Play("idle");
            Add(LaughSfx = new GrannyLaughSfx(Sprite));
            MoveAnim = "walk";
            Maxspeed = 40f;
            Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0.0f, -24f), new Action<Player>(OnTalk)));
            talker.Enabled = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            conversation = Level.Session.GetCounter("granny");
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
        }

        public override void Update()
        {
            if (!talking && conversation == 0)
            {
                player = Level.Tracker.GetEntity<Player>();
                if (player != null && (double)Math.Abs(player.X - X) < 48.0)
                {
                    OnTalk(player);
                }
            }
            talker.Enabled = talkerEnabled;
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(Player player)
        {
            this.player = player;
            (Scene as Level).StartCutscene(new Action<Level>(EndTalking));
            Add(talkRoutine = new Coroutine(TalkRoutine(player)));
            talking = true;
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC09_Granny_Inside npC09GrannyInside = this;
            player.StateMachine.State = 11;
            player.Dashes = 1;
            player.ForceCameraUpdate = true;
            while (!player.OnGround())
            {
                yield return null;
            }

            yield return player.DummyWalkToExact((int)npC09GrannyInside.X - 16);
            player.Facing = Facings.Right;
            player.ForceCameraUpdate = false;
            Vector2 zoomPoint = new(npC09GrannyInside.X - 8f - npC09GrannyInside.Level.Camera.X, 110f);
            if (npC09GrannyInside.HasDoorConversation)
            {
                npC09GrannyInside.Sprite.Scale.X = -1f;
                yield return npC09GrannyInside.Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("APP_OLDLADY_LOCKED");
            }
            else if (npC09GrannyInside.conversation == 0)
            {
                yield return 0.5f;
                npC09GrannyInside.Sprite.Scale.X = -1f;
                yield return 0.25f;
                yield return npC09GrannyInside.Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("APP_OLDLADY_B", new Func<IEnumerator>(npC09GrannyInside.StartLaughing), new Func<IEnumerator>(npC09GrannyInside.StopLaughing));
            }
            else if (npC09GrannyInside.conversation == 1)
            {
                npC09GrannyInside.Sprite.Scale.X = -1f;
                yield return npC09GrannyInside.Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("APP_OLDLADY_C", new Func<IEnumerator>(npC09GrannyInside.StartLaughing), new Func<IEnumerator>(npC09GrannyInside.StopLaughing));
            }
            else if (npC09GrannyInside.conversation == 2)
            {
                npC09GrannyInside.Sprite.Scale.X = -1f;
                yield return npC09GrannyInside.Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("APP_OLDLADY_D", new Func<IEnumerator>(npC09GrannyInside.StartLaughing), new Func<IEnumerator>(npC09GrannyInside.StopLaughing));
            }
            else if (npC09GrannyInside.conversation == 3)
            {
                npC09GrannyInside.Sprite.Scale.X = -1f;
                yield return npC09GrannyInside.Level.ZoomTo(zoomPoint, 2f, 0.5f);
                yield return Textbox.Say("APP_OLDLADY_E", new Func<IEnumerator>(npC09GrannyInside.StartLaughing), new Func<IEnumerator>(npC09GrannyInside.StopLaughing));
            }
            npC09GrannyInside.talker.Enabled = npC09GrannyInside.talkerEnabled;
            yield return npC09GrannyInside.Level.ZoomBack(0.5f);
            npC09GrannyInside.Level.EndCutscene();
            npC09GrannyInside.EndTalking(npC09GrannyInside.Level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator StartLaughing()
        {
            Sprite.Play("laugh", false, false);
            yield return null;
            yield break;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator StopLaughing()
        {
            Sprite.Play("idle", false, false);
            yield return null;
            yield break;
        }

        private void EndTalking(Level level)
        {
            if (player != null)
            {
                player.StateMachine.State = 0;
                player.ForceCameraUpdate = false;
            }
            if (HasDoorConversation)
            {
                Level.Session.SetFlag("granny_door_done");
            }
            else
            {
                Level.Session.IncrementCounter("granny");
                ++conversation;
            }
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
                talkRoutine = null;
            }
            Sprite.Play("idle");
            talking = false;
        }
    }
}
