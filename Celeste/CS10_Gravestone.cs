using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS10_Gravestone : CutsceneEntity
    {
        private Player player;
        private NPC10_Gravestone gravestone;
        private BadelineDummy badeline;
        private BirdNPC bird;
        private Vector2 boostTarget;
        private bool addedBooster;

        public CS10_Gravestone(Player player, NPC10_Gravestone gravestone, Vector2 boostTarget)
        {
            this.player = player;
            this.gravestone = gravestone;
            this.boostTarget = boostTarget;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene()));

        private IEnumerator Cutscene()
        {
            CS10_Gravestone cs10Gravestone = this;
            cs10Gravestone.player.StateMachine.State = 11;
            cs10Gravestone.player.ForceCameraUpdate = true;
            cs10Gravestone.player.DummyGravity = false;
            cs10Gravestone.player.Speed.Y = 0.0f;
            yield return 0.1f;
            yield return cs10Gravestone.player.DummyWalkToExact((int) cs10Gravestone.gravestone.X - 30);
            yield return 0.1f;
            cs10Gravestone.player.Facing = Facings.Right;
            yield return 0.2f;
            yield return cs10Gravestone.Level.ZoomTo(new Vector2(160f, 90f), 2f, 3f);
            cs10Gravestone.player.ForceCameraUpdate = false;
            yield return 0.5f;
            yield return Textbox.Say("CH9_GRAVESTONE", cs10Gravestone.StepForward, cs10Gravestone.BadelineAppears, cs10Gravestone.SitDown);
            yield return 1f;
            yield return cs10Gravestone.BirdStuff();
            yield return cs10Gravestone.BadelineRejoin();
            yield return 0.1f;
            yield return cs10Gravestone.Level.ZoomBack(0.5f);
            yield return 0.3f;
            cs10Gravestone.addedBooster = true;
            cs10Gravestone.Level.Displacement.AddBurst(cs10Gravestone.boostTarget, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("event:/new_content/char/badeline/booster_first_appear", cs10Gravestone.boostTarget);
            cs10Gravestone.Level.Add(new BadelineBoost(new Vector2[1]
            {
                cs10Gravestone.boostTarget
            }, false));
            yield return 0.2f;
            cs10Gravestone.EndCutscene(cs10Gravestone.Level);
        }

        private IEnumerator StepForward()
        {
            yield return player.DummyWalkTo(player.X + 8f);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator BadelineAppears()
        {
                Level.Session.Inventory.Dashes = 1;
                player.Dashes = 1;
                Vector2 vector = player.Position + new Vector2(-12f, -10f);
                Level.Displacement.AddBurst(vector, 0.5f, 8f, 32f, 0.5f);
                Level.Add(badeline = new BadelineDummy(vector));
                Audio.Play("event:/char/badeline/maddy_split", vector);
                badeline.Sprite.Scale.X = 1f;
                yield return badeline.FloatTo(vector + new Vector2(0f, -6f), 1, false);
        }

        private IEnumerator SitDown()
        {
            yield return 0.2f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("sitDown");
            yield return 0.3f;
        }

        private IEnumerator BirdStuff()
        {
            CS10_Gravestone cs10Gravestone = this;
            cs10Gravestone.bird = new BirdNPC(cs10Gravestone.player.Position + new Vector2(88f, -200f), BirdNPC.Modes.None);
            cs10Gravestone.bird.DisableFlapSfx = true;
            cs10Gravestone.Scene.Add(cs10Gravestone.bird);
            EventInstance instance = Audio.Play("event:/game/general/bird_in", cs10Gravestone.bird.Position);
            cs10Gravestone.bird.Facing = Facings.Left;
            cs10Gravestone.bird.Sprite.Play("fall");
            Vector2 from = cs10Gravestone.bird.Position;
            Vector2 to = cs10Gravestone.gravestone.Position + new Vector2(1f, -16f);
            float percent = 0.0f;
            while (percent < 1.0)
            {
                cs10Gravestone.bird.Position = from + (to - from) * Ease.QuadOut(percent);
                Audio.Position(instance, cs10Gravestone.bird.Position);
                if (percent > 0.5)
                    cs10Gravestone.bird.Sprite.Play("fly");
                percent += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            cs10Gravestone.bird.Position = to;
            cs10Gravestone.bird.Sprite.Play("idle");
            yield return 0.5f;
            cs10Gravestone.bird.Sprite.Play("croak");
            yield return 0.6f;
            Audio.Play("event:/game/general/bird_squawk", cs10Gravestone.bird.Position);
            yield return 0.9f;
            Audio.Play("event:/char/madeline/stand", cs10Gravestone.player.Position);
            cs10Gravestone.player.Sprite.Play("idle");
            yield return 1f;
            yield return cs10Gravestone.bird.StartleAndFlyAway();
        }

        private IEnumerator BadelineRejoin()
        {
            CS10_Gravestone cs10Gravestone = this;
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", cs10Gravestone.badeline.Position);
            Vector2 from = cs10Gravestone.badeline.Position;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.25f)
            {
                cs10Gravestone.badeline.Position = Vector2.Lerp(from, cs10Gravestone.player.Position, Ease.CubeIn(p));
                yield return null;
            }
            cs10Gravestone.Level.Displacement.AddBurst(cs10Gravestone.player.Center, 0.5f, 8f, 32f, 0.5f);
            cs10Gravestone.Level.Session.Inventory.Dashes = 2;
            cs10Gravestone.player.Dashes = 2;
            cs10Gravestone.badeline.RemoveSelf();
        }

        public override void OnEnd(Level level)
        {
            player.Facing = Facings.Right;
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.StateMachine.State = 0;
            Level.Session.Inventory.Dashes = 2;
            player.Dashes = 2;
            if (badeline != null)
                badeline.RemoveSelf();
            if (bird != null)
                bird.RemoveSelf();
            if (!addedBooster)
                level.Add(new BadelineBoost(new Vector2[1]
                {
                    boostTarget
                }, false));
            level.ResetZoom();
        }
    }
}
