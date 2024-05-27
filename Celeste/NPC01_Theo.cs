using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class NPC01_Theo : NPC
    {
        public static ParticleType P_YOLO;
        private const string DoneTalking = "theoDoneTalking";
        private int currentConversation;
        private Coroutine talkRoutine;

        public NPC01_Theo(Vector2 position)
            : base(position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Play("idle");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            currentConversation = Session.GetCounter("theo");
            if (Session.GetFlag("theoDoneTalking"))
                return;
            Add(Talker = new TalkComponent(new Rectangle(-8, -8, 88, 8), new Vector2(0.0f, -24f), OnTalk));
        }

        private void OnTalk(Player player)
        {
            Level.StartCutscene(OnTalkEnd);
            Add(talkRoutine = new Coroutine(Talk(player)));
        }

        private IEnumerator Talk(Player player)
        {
            NPC01_Theo npC01Theo = this;
            if (npC01Theo.currentConversation == 0)
            {
                yield return npC01Theo.PlayerApproachRightSide(player);
                yield return Textbox.Say("CH1_THEO_A", npC01Theo.PlayerApproach48px);
            }
            else if (npC01Theo.currentConversation == 1)
            {
                yield return npC01Theo.PlayerApproachRightSide(player);
                yield return 0.2f;
                yield return npC01Theo.PlayerApproach(player, spacing: 48f);
                yield return Textbox.Say("CH1_THEO_B");
            }
            else if (npC01Theo.currentConversation == 2)
            {
                yield return npC01Theo.PlayerApproachRightSide(player, spacing: 48f);
                yield return Textbox.Say("CH1_THEO_C");
            }
            else if (npC01Theo.currentConversation == 3)
            {
                yield return npC01Theo.PlayerApproachRightSide(player, spacing: 48f);
                yield return Textbox.Say("CH1_THEO_D");
            }
            else if (npC01Theo.currentConversation == 4)
            {
                yield return npC01Theo.PlayerApproachRightSide(player, spacing: 48f);
                yield return Textbox.Say("CH1_THEO_E");
            }
            else if (npC01Theo.currentConversation == 5)
            {
                yield return npC01Theo.PlayerApproachRightSide(player, spacing: 48f);
                yield return Textbox.Say("CH1_THEO_F", npC01Theo.Yolo);
                npC01Theo.Sprite.Play("yoloEnd");
                npC01Theo.Remove(npC01Theo.Talker);
                yield return npC01Theo.Level.ZoomBack(0.5f);
            }
            npC01Theo.Level.EndCutscene();
            npC01Theo.OnTalkEnd(npC01Theo.Level);
        }

        private void OnTalkEnd(Level level)
        {
            if (currentConversation == 0)
                SaveData.Instance.SetFlag("MetTheo");
            else if (currentConversation == 1)
                SaveData.Instance.SetFlag("TheoKnowsName");
            else if (currentConversation == 5)
            {
                Session.SetFlag("theoDoneTalking");
                Remove(Talker);
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
            Session.IncrementCounter("theo");
            ++currentConversation;
            talkRoutine.Cancel();
            talkRoutine.RemoveSelf();
            Sprite.Play("idle");
        }

        private IEnumerator Yolo()
        {
            NPC01_Theo npC01Theo = this;
            yield return npC01Theo.Level.ZoomTo(new Vector2(128f, 128f), 2f, 0.5f);
            yield return 0.2f;
            Audio.Play("event:/char/theo/yolo_fist", npC01Theo.Position);
            npC01Theo.Sprite.Play("yolo");
            yield return 0.1f;
            npC01Theo.Level.DirectionalShake(-Vector2.UnitY);
            npC01Theo.Level.ParticlesFG.Emit(NPC01_Theo.P_YOLO, 6, npC01Theo.Position + new Vector2(-3f, -24f), Vector2.One * 4f);
            yield return 0.5f;
        }
    }
}
