// Decompiled with JetBrains decompiler
// Type: Celeste.NPC07X_Granny_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC07X_Granny_Ending : NPC
    {
        public Hahaha Hahaha;
        public GrannyLaughSfx LaughSfx;
        private Player player;
        private readonly TalkComponent talker;
        private Coroutine talkRoutine;
        private int conversation;
        private readonly bool ch9EasterEgg;

        public NPC07X_Granny_Ending(EntityData data, Vector2 offset, bool ch9EasterEgg = false)
            : base(data.Position + offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Play("idle");
            Sprite.Scale.X = -1f;
            Add(LaughSfx = new GrannyLaughSfx(Sprite));
            Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0.0f, -24f), new Action<Player>(OnTalk)));
            MoveAnim = "walk";
            Maxspeed = 40f;
            this.ch9EasterEgg = ch9EasterEgg;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
        }

        public override void Update()
        {
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(Player player)
        {
            this.player = player;
            (Scene as Level).StartCutscene(new Action<Level>(EndTalking));
            Add(talkRoutine = new Coroutine(TalkRoutine(player)));
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC07X_Granny_Ending c07XGrannyEnding = this;
            player.StateMachine.State = 11;
            player.ForceCameraUpdate = true;
            while (!player.OnGround())
            {
                yield return null;
            }

            yield return player.DummyWalkToExact((int)c07XGrannyEnding.X - 16);
            player.Facing = Facings.Right;
            if (c07XGrannyEnding.ch9EasterEgg)
            {
                yield return 0.5f;
                yield return c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
                Dialog.Language.Dialog["CH10_GRANNY_EASTEREGG"] = "{portrait GRANNY right mock} I see you have discovered Debug Mode.";
                yield return Textbox.Say("CH10_GRANNY_EASTEREGG");
                c07XGrannyEnding.talker.Enabled = false;
            }
            else if (c07XGrannyEnding.conversation == 0)
            {
                yield return 0.5f;
                yield return c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
                yield return Textbox.Say("CH7_CSIDE_OLDLADY", new Func<IEnumerator>(c07XGrannyEnding.StartLaughing), new Func<IEnumerator>(c07XGrannyEnding.StopLaughing));
            }
            else if (c07XGrannyEnding.conversation == 1)
            {
                yield return 0.5f;
                yield return c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
                yield return Textbox.Say("CH7_CSIDE_OLDLADY_B", new Func<IEnumerator>(c07XGrannyEnding.StartLaughing), new Func<IEnumerator>(c07XGrannyEnding.StopLaughing));
                c07XGrannyEnding.talker.Enabled = false;
            }
            yield return c07XGrannyEnding.Level.ZoomBack(0.5f);
            c07XGrannyEnding.Level.EndCutscene();
            c07XGrannyEnding.EndTalking(c07XGrannyEnding.Level);
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
            ++conversation;
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
                talkRoutine = null;
            }
            Sprite.Play("idle");
        }
    }
}
