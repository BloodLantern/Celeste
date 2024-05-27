using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS07_Ascend : CutsceneEntity
    {
        private int index;
        private string cutscene;
        private BadelineDummy badeline;
        private Player player;
        private Vector2 origin;
        private bool spinning;
        private bool dark;

        public CS07_Ascend(int index, string cutscene, bool dark)
        {
            this.index = index;
            this.cutscene = cutscene;
            this.dark = dark;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene()));

        private IEnumerator Cutscene()
        {
            CS07_Ascend cs07Ascend = this;
            while ((cs07Ascend.player = cs07Ascend.Scene.Tracker.GetEntity<Player>()) == null)
                yield return null;
            cs07Ascend.origin = cs07Ascend.player.Position;
            Audio.Play("event:/char/badeline/maddy_split");
            cs07Ascend.player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            cs07Ascend.Level.Displacement.AddBurst(cs07Ascend.player.Position, 0.4f, 8f, 32f, 0.5f);
            cs07Ascend.player.Dashes = 1;
            cs07Ascend.player.Facing = Facings.Right;
            cs07Ascend.Scene.Add(cs07Ascend.badeline = new BadelineDummy(cs07Ascend.player.Position));
            cs07Ascend.badeline.AutoAnimator.Enabled = false;
            cs07Ascend.spinning = true;
            cs07Ascend.Add(new Coroutine(cs07Ascend.SpinCharacters()));
            yield return Textbox.Say(cs07Ascend.cutscene);
            Audio.Play("event:/char/badeline/maddy_join");
            cs07Ascend.spinning = false;
            yield return 0.25f;
            cs07Ascend.badeline.RemoveSelf();
            cs07Ascend.player.Dashes = 2;
            cs07Ascend.player.CreateSplitParticles();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            cs07Ascend.Level.Displacement.AddBurst(cs07Ascend.player.Position, 0.4f, 8f, 32f, 0.5f);
            cs07Ascend.EndCutscene(cs07Ascend.Level);
        }

        private IEnumerator SpinCharacters()
        {
            float dist = 0.0f;
            Vector2 center = player.Position;
            float timer = 1.57079637f;
            player.Sprite.Play("spin");
            badeline.Sprite.Play("spin");
            badeline.Sprite.Scale.X = 1f;
            while (spinning || dist > 0.0)
            {
                dist = Calc.Approach(dist, spinning ? 1f : 0.0f, Engine.DeltaTime * 4f);
                int frame = (int) (timer / 6.2831854820251465 * 14.0 + 10.0);
                float num1 = (float) Math.Sin(timer);
                float num2 = (float) Math.Cos(timer);
                float num3 = Ease.CubeOut(dist) * 32f;
                player.Sprite.SetAnimationFrame(frame);
                badeline.Sprite.SetAnimationFrame(frame + 7);
                player.Position = center - new Vector2(num1 * num3, (float) (num2 * (double) dist * 8.0));
                badeline.Position = center + new Vector2(num1 * num3, (float) (num2 * (double) dist * 8.0));
                timer -= Engine.DeltaTime * 2f;
                if (timer <= 0.0)
                    timer += 6.28318548f;
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            if (badeline != null)
                badeline.RemoveSelf();
            if (player != null)
            {
                player.Dashes = 2;
                player.Position = origin;
            }
            if (dark)
                return;
            level.Add(new HeightDisplay(index));
        }
    }
}
