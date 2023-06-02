using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC03_Oshiro_Suite : NPC
    {
        private const string ConversationCounter = "oshiroSuiteSadConversation";
        private bool finishedTalking;

        public NPC03_Oshiro_Suite(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = (Sprite) new OshiroSprite(1)));
            this.Add((Component) (this.Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64)));
            this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-16, -8, 32, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
            this.Talker.Enabled = false;
            this.MoveAnim = "move";
            this.IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!this.Session.GetFlag("oshiro_resort_suite"))
            {
                this.Scene.Add((Entity) new CS03_OshiroMasterSuite((NPC) this));
            }
            else
            {
                this.Sprite.Play("idle_ground");
                this.Talker.Enabled = true;
            }
        }

        private void OnTalk(Player player)
        {
            this.finishedTalking = false;
            this.Level.StartCutscene(new Action<Level>(this.EndTalking));
            this.Add((Component) new Coroutine(this.Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC03_Oshiro_Suite npC03OshiroSuite = this;
            int conversation = npC03OshiroSuite.Session.GetCounter("oshiroSuiteSadConversation");
            yield return (object) npC03OshiroSuite.PlayerApproach(player, false, new float?(12f));
            yield return (object) Textbox.Say("CH3_OSHIRO_SUITE_SAD" + (object) conversation);
            yield return (object) npC03OshiroSuite.PlayerLeave(player);
            npC03OshiroSuite.EndTalking(npC03OshiroSuite.SceneAs<Level>());
        }

        private void EndTalking(Level level)
        {
            Player first = this.Scene.Entities.FindFirst<Player>();
            if (first != null)
            {
                first.StateMachine.Locked = false;
                first.StateMachine.State = 0;
            }
            if (this.finishedTalking)
                return;
            int num = (this.Session.GetCounter("oshiroSuiteSadConversation") + 1) % 7;
            if (num == 0)
                ++num;
            this.Session.SetCounter("oshiroSuiteSadConversation", num);
            this.finishedTalking = true;
        }
    }
}
