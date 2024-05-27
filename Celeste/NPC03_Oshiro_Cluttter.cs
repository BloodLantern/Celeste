using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class NPC03_Oshiro_Cluttter : NPC
    {
        public const string TalkFlagsA = "oshiro_clutter_";
        public const string TalkFlagsB = "oshiro_clutter_optional_";
        public const string ClearedFlags = "oshiro_clutter_cleared_";
        public const string FinishedFlag = "oshiro_clutter_finished";
        public const string DoorOpenFlag = "oshiro_clutter_door_open";
        public Vector2 HomePosition;
        private int sectionsComplete;
        private bool talked;
        private bool inRoutine;
        private List<Vector2> nodes = new List<Vector2>();
        private Coroutine paceRoutine;
        private Coroutine talkRoutine;
        private SoundSource paceSfx;
        private float paceTimer;

        public NPC03_Oshiro_Cluttter(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = new OshiroSprite(-1));
            Add(Talker = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(0.0f, -24f), OnTalk));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
            foreach (Vector2 node in data.Nodes)
                nodes.Add(node + offset);
            Add(paceSfx = new SoundSource());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Session.GetFlag("oshiro_clutter_finished"))
            {
                RemoveSelf();
            }
            else
            {
                if (Session.GetFlag("oshiro_clutter_cleared_0"))
                    ++sectionsComplete;
                if (Session.GetFlag("oshiro_clutter_cleared_1"))
                    ++sectionsComplete;
                if (Session.GetFlag("oshiro_clutter_cleared_2"))
                    ++sectionsComplete;
                if (sectionsComplete == 0 || sectionsComplete == 3)
                    Sprite.Scale.X = 1f;
                if (sectionsComplete > 0)
                    Position = nodes[sectionsComplete - 1];
                else if (!Session.GetFlag("oshiro_clutter_0"))
                    Add(paceRoutine = new Coroutine(Pace()));
                if (sectionsComplete == 0 && Session.GetFlag("oshiro_clutter_0") && !Session.GetFlag("oshiro_clutter_optional_0"))
                    Sprite.Play("idle_ground");
                if (sectionsComplete == 3 || Session.GetFlag("oshiro_clutter_optional_" + sectionsComplete))
                    Remove(Talker);
            }
            HomePosition = Position;
        }

        public Vector2 ZoomPoint => sectionsComplete < 2 ? Position + new Vector2(0.0f, -30f) - Level.Camera.Position : Position + new Vector2(0.0f, -15f) - Level.Camera.Position;

        private void OnTalk(Player player)
        {
            talked = true;
            if (paceRoutine != null)
                paceRoutine.RemoveSelf();
            paceRoutine = null;
            if (!Session.GetFlag("oshiro_clutter_" + sectionsComplete))
            {
                Scene.Add(new CS03_OshiroClutter(player, this, sectionsComplete));
            }
            else
            {
                Level.StartCutscene(EndTalkRoutine);
                Session.SetFlag("oshiro_clutter_optional_" + sectionsComplete);
                Add(talkRoutine = new Coroutine(TalkRoutine(player)));
                if (Talker == null)
                    return;
                Talker.Enabled = false;
            }
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
            yield return c03OshiroCluttter.PlayerApproach(player, spacing: 24f, side: c03OshiroCluttter.sectionsComplete == 1 || c03OshiroCluttter.sectionsComplete == 2 ? -1 : 1);
            yield return c03OshiroCluttter.Level.ZoomTo(c03OshiroCluttter.ZoomPoint, 2f, 0.5f);
            yield return Textbox.Say("CH3_OSHIRO_CLUTTER" + c03OshiroCluttter.sectionsComplete + "_B", c03OshiroCluttter.StandUp);
            yield return c03OshiroCluttter.Level.ZoomBack(0.5f);
            c03OshiroCluttter.Level.EndCutscene();
            c03OshiroCluttter.EndTalkRoutine(c03OshiroCluttter.Level);
        }

        private void EndTalkRoutine(Level level)
        {
            if (talkRoutine != null)
                talkRoutine.RemoveSelf();
            talkRoutine = null;
            (Sprite as OshiroSprite).Pop("idle", false);
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator StandUp()
        {
                Audio.Play("event:/char/oshiro/chat_get_up", Position);
                (Sprite as OshiroSprite).Pop("idle", false);
                yield return 0.25f;
        }

        private IEnumerator Pace()
        {
            NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
            while (true)
            {
                (c03OshiroCluttter.Sprite as OshiroSprite).Wiggle();
                yield return c03OshiroCluttter.PaceLeft();
                while (c03OshiroCluttter.paceTimer < 2.2660000324249268)
                    yield return null;
                c03OshiroCluttter.paceTimer = 0.0f;
                (c03OshiroCluttter.Sprite as OshiroSprite).Wiggle();
                yield return c03OshiroCluttter.PaceRight();
                while (c03OshiroCluttter.paceTimer < 2.2660000324249268)
                    yield return null;
                c03OshiroCluttter.paceTimer = 0.0f;
            }
        }

        public IEnumerator PaceRight()
        {
            NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
            Vector2 homePosition = c03OshiroCluttter.HomePosition;
            if ((c03OshiroCluttter.Position - homePosition).Length() > 8.0)
                c03OshiroCluttter.paceSfx.Play("event:/char/oshiro/move_04_pace_right");
            yield return c03OshiroCluttter.MoveTo(homePosition);
        }

        public IEnumerator PaceLeft()
        {
            NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
            Vector2 target = c03OshiroCluttter.HomePosition + new Vector2(-20f, 0.0f);
            if ((c03OshiroCluttter.Position - target).Length() > 8.0)
                c03OshiroCluttter.paceSfx.Play("event:/char/oshiro/move_04_pace_left");
            yield return c03OshiroCluttter.MoveTo(target);
        }

        public override void Update()
        {
            base.Update();
            paceTimer += Engine.DeltaTime;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (sectionsComplete == 3 && !inRoutine && entity != null && entity.X < X + 32.0 && entity.Y <= (double) Y)
            {
                OnTalk(entity);
                inRoutine = true;
            }
            if (sectionsComplete != 0 || talked)
                return;
            Level scene = Scene as Level;
            if (entity != null && !entity.Dead)
            {
                float num = Calc.ClampedMap(Vector2.Distance(Center, entity.Center), 40f, 128f);
                scene.Session.Audio.Music.Layer(1, num);
                scene.Session.Audio.Music.Layer(2, 1f - num);
                scene.Session.Audio.Apply();
            }
            else
            {
                scene.Session.Audio.Music.Layer(1, true);
                scene.Session.Audio.Music.Layer(2, false);
                scene.Session.Audio.Apply();
            }
        }
    }
}
