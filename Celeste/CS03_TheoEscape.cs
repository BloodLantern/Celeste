using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS03_TheoEscape : CutsceneEntity
    {
        public const string Flag = "resort_theo";
        private NPC03_Theo_Escaping theo;
        private Player player;
        private Vector2 theoStart;

        public CS03_TheoEscape(NPC03_Theo_Escaping theo, Player player)
        {
            this.theo = theo;
            theoStart = theo.Position;
            this.player = player;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS03_TheoEscape cs03TheoEscape = this;
            cs03TheoEscape.player.StateMachine.State = 11;
            cs03TheoEscape.player.StateMachine.Locked = true;
            yield return cs03TheoEscape.player.DummyWalkTo(cs03TheoEscape.theo.X - 64f);
            cs03TheoEscape.player.Facing = Facings.Right;
            yield return cs03TheoEscape.Level.ZoomTo(new Vector2(240f, 135f), 2f, 0.5f);
            Func<IEnumerator>[] funcArray = new Func<IEnumerator>[4]
            {
                cs03TheoEscape.StopRemovingVent,
                cs03TheoEscape.StartRemoveVent,
                cs03TheoEscape.RemoveVent,
                cs03TheoEscape.GivePhone
            };
            string dialog = "CH3_THEO_INTRO";
            if (!SaveData.Instance.HasFlag("MetTheo"))
                dialog = "CH3_THEO_NEVER_MET";
            else if (!SaveData.Instance.HasFlag("TheoKnowsName"))
                dialog = "CH3_THEO_NEVER_INTRODUCED";
            yield return Textbox.Say(dialog, funcArray);
            cs03TheoEscape.theo.Sprite.Scale.X = 1f;
            yield return 0.2f;
            cs03TheoEscape.theo.Sprite.Play("walk");
            while (!cs03TheoEscape.theo.CollideCheck<Solid>(cs03TheoEscape.theo.Position + new Vector2(2f, 0.0f)))
            {
                yield return null;
                cs03TheoEscape.theo.X += 48f * Engine.DeltaTime;
            }
            cs03TheoEscape.theo.Sprite.Play("idle");
            yield return 0.2f;
            Audio.Play("event:/char/theo/resort_standtocrawl", cs03TheoEscape.theo.Position);
            cs03TheoEscape.theo.Sprite.Play("duck");
            yield return 0.5f;
            if (cs03TheoEscape.theo.Talker != null)
                cs03TheoEscape.theo.Talker.Active = false;
            level.Session.SetFlag("resort_theo");
            cs03TheoEscape.player.StateMachine.Locked = false;
            cs03TheoEscape.player.StateMachine.State = 0;
            cs03TheoEscape.theo.CrawlUntilOut();
            yield return level.ZoomBack(0.5f);
            cs03TheoEscape.EndCutscene(level);
        }

        private IEnumerator StartRemoveVent()
        {
            theo.Sprite.Scale.X = 1f;
            yield return 0.1f;
            Audio.Play("event:/char/theo/resort_vent_grab", theo.Position);
            theo.Sprite.Play("goToVent");
            yield return 0.25f;
        }

        private IEnumerator StopRemovingVent()
        {
            theo.Sprite.Play("idle");
            yield return 0.1f;
            theo.Sprite.Scale.X = -1f;
        }

        private IEnumerator RemoveVent()
        {
            yield return 0.8f;
            Audio.Play("event:/char/theo/resort_vent_rip", theo.Position);
            theo.Sprite.Play("fallVent");
            yield return 0.8f;
            theo.grate.Fall();
            yield return 0.8f;
            theo.Sprite.Scale.X = -1f;
            yield return 0.25f;
        }

        private IEnumerator GivePhone()
        {
            CS03_TheoEscape cs03TheoEscape = this;
            Player player = cs03TheoEscape.Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                cs03TheoEscape.theo.Sprite.Play("walk");
                cs03TheoEscape.theo.Sprite.Scale.X = -1f;
                while (cs03TheoEscape.theo.X > player.X + 24.0)
                {
                    cs03TheoEscape.theo.X -= 48f * Engine.DeltaTime;
                    yield return null;
                }
            }
            cs03TheoEscape.theo.Sprite.Play("idle");
            yield return 1f;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag("resort_theo");
            SaveData.Instance.SetFlag("MetTheo");
            SaveData.Instance.SetFlag("TheoKnowsName");
            if (theo == null || !WasSkipped)
                return;
            theo.Position = theoStart;
            theo.CrawlUntilOut();
            if (theo.grate == null)
                return;
            theo.grate.RemoveSelf();
        }
    }
}
