using Microsoft.Xna.Framework;
using Monocle;
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
            Add(Sprite = new OshiroSprite(1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            Add(Talker = new TalkComponent(new Rectangle(-16, -8, 32, 8), new Vector2(0.0f, -24f), OnTalk));
            Talker.Enabled = false;
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Session.GetFlag("oshiro_resort_suite"))
            {
                Scene.Add(new CS03_OshiroMasterSuite(this));
            }
            else
            {
                Sprite.Play("idle_ground");
                Talker.Enabled = true;
            }
        }

        private void OnTalk(Player player)
        {
            finishedTalking = false;
            Level.StartCutscene(EndTalking);
            Add(new Coroutine(Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC03_Oshiro_Suite npC03OshiroSuite = this;
            int conversation = npC03OshiroSuite.Session.GetCounter("oshiroSuiteSadConversation");
            yield return npC03OshiroSuite.PlayerApproach(player, false, 12f);
            yield return Textbox.Say("CH3_OSHIRO_SUITE_SAD" + conversation);
            yield return npC03OshiroSuite.PlayerLeave(player);
            npC03OshiroSuite.EndTalking(npC03OshiroSuite.SceneAs<Level>());
        }

        private void EndTalking(Level level)
        {
            Player first = Scene.Entities.FindFirst<Player>();
            if (first != null)
            {
                first.StateMachine.Locked = false;
                first.StateMachine.State = 0;
            }
            if (finishedTalking)
                return;
            int num = (Session.GetCounter("oshiroSuiteSadConversation") + 1) % 7;
            if (num == 0)
                ++num;
            Session.SetCounter("oshiroSuiteSadConversation", num);
            finishedTalking = true;
        }
    }
}
