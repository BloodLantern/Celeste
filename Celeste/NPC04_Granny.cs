using Microsoft.Xna.Framework;
using Monocle;
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
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Scale.X = -1f;
            Sprite.Play("idle");
            Add(new GrannyLaughSfx(Sprite));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
            if (Session.GetFlag("granny_1") && !Session.GetFlag("granny_2"))
                Sprite.Play("laugh");
            if (Session.GetFlag("granny_3"))
                return;
            Add(Talker = new TalkComponent(new Rectangle(-20, -16, 40, 16), new Vector2(0.0f, -24f), OnTalk));
            if (Session.GetFlag("granny_1"))
                return;
            Talker.Enabled = false;
        }

        public override void Update()
        {
            Player entity = Level.Tracker.GetEntity<Player>();
            if (entity != null && !Session.GetFlag("granny_1") && !cutscene && entity.X > X - 40.0)
            {
                cutscene = true;
                Scene.Add(new CS04_Granny(this, entity));
                if (Talker != null)
                    Talker.Enabled = true;
            }
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(Player player)
        {
            Level.StartCutscene(TalkEnd);
            Add(talkRoutine = new Coroutine(TalkRoutine(player)));
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC04_Granny npC04Granny = this;
            npC04Granny.Sprite.Play("idle");
            player.ForceCameraUpdate = true;
            yield return npC04Granny.PlayerApproachLeftSide(player, spacing: 20f);
            yield return npC04Granny.Level.ZoomTo(new Vector2((float) ((player.X + (double) npC04Granny.X) / 2.0) - npC04Granny.Level.Camera.X, 116f), 2f, 0.5f);
            if (!npC04Granny.Session.GetFlag("granny_2"))
                yield return Textbox.Say("CH4_GRANNY_2");
            else
                yield return Textbox.Say("CH4_GRANNY_3");
            yield return npC04Granny.Level.ZoomBack(0.5f);
            npC04Granny.Level.EndCutscene();
            npC04Granny.TalkEnd(npC04Granny.Level);
        }

        private void TalkEnd(Level level)
        {
            if (!Session.GetFlag("granny_2"))
                Session.SetFlag("granny_2");
            else if (!Session.GetFlag("granny_3"))
            {
                Session.SetFlag("granny_3");
                Remove(Talker);
            }
            if (talkRoutine != null)
            {
                talkRoutine.RemoveSelf();
                talkRoutine = null;
            }
            Player entity = Level.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
            entity.ForceCameraUpdate = false;
        }
    }
}
