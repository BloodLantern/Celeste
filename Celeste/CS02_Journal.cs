using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS02_Journal : CutsceneEntity
    {
        private const string ReadOnceFlag = "poem_read";
        private Player player;
        private PoemPage poem;

        public CS02_Journal(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Routine()));

        private IEnumerator Routine()
        {
            CS02_Journal cs02Journal = this;
            cs02Journal.player.StateMachine.State = 11;
            cs02Journal.player.StateMachine.Locked = true;
            if (!cs02Journal.Level.Session.GetFlag("poem_read"))
            {
                yield return Textbox.Say("ch2_journal");
                yield return 0.1f;
            }
            cs02Journal.poem = new PoemPage();
            cs02Journal.Scene.Add(cs02Journal.poem);
            yield return cs02Journal.poem.EaseIn();
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            Audio.Play("event:/ui/main/button_lowkey");
            yield return cs02Journal.poem.EaseOut();
            cs02Journal.poem = null;
            cs02Journal.EndCutscene(cs02Journal.Level);
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag("poem_read");
            if (poem == null)
                return;
            poem.RemoveSelf();
        }

        private class PoemPage : Entity
        {
            private const float TextScale = 0.7f;
            private MTexture paper;
            private VirtualRenderTarget target;
            private FancyText.Text text;
            private float alpha = 1f;
            private float scale = 1f;
            private float rotation;
            private float timer;
            private bool easingOut;

            public PoemPage()
            {
                Tag = (int) Tags.HUD;
                paper = GFX.Gui["poempage"];
                text = FancyText.Parse(Dialog.Get("CH2_POEM"), (int) ((paper.Width - 120) / 0.699999988079071), -1, defaultColor: Color.Black * 0.6f);
                Add(new BeforeRenderHook(BeforeRender));
            }

            public IEnumerator EaseIn()
            {
                PoemPage poemPage = this;
                Audio.Play("event:/game/03_resort/memo_in");
                Vector2 vector2 = new Vector2(Engine.Width, Engine.Height) / 2f;
                Vector2 from = vector2 + new Vector2(0.0f, 200f);
                Vector2 to = vector2;
                float rFrom = -0.1f;
                float rTo = 0.05f;
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
                {
                    poemPage.Position = from + (to - from) * Ease.CubeOut(p);
                    poemPage.alpha = Ease.CubeOut(p);
                    poemPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                    yield return null;
                }
            }

            public IEnumerator EaseOut()
            {
                PoemPage poemPage = this;
                Audio.Play("event:/game/03_resort/memo_out");
                poemPage.easingOut = true;
                Vector2 from = poemPage.Position;
                Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0.0f, -200f);
                float rFrom = poemPage.rotation;
                float rTo = poemPage.rotation + 0.1f;
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 1.5f)
                {
                    poemPage.Position = from + (to - from) * Ease.CubeIn(p);
                    poemPage.alpha = 1f - Ease.CubeIn(p);
                    poemPage.rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                    yield return null;
                }
                poemPage.RemoveSelf();
            }

            public void BeforeRender()
            {
                if (target == null)
                    target = VirtualContent.CreateRenderTarget("journal-poem", paper.Width, paper.Height);
                Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                paper.Draw(Vector2.Zero);
                text.DrawJustifyPerLine(new Vector2(paper.Width, paper.Height) / 2f, new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
                Draw.SpriteBatch.End();
            }

            public override void Removed(Scene scene)
            {
                if (target != null)
                    target.Dispose();
                target = null;
                base.Removed(scene);
            }

            public override void SceneEnd(Scene scene)
            {
                if (target != null)
                    target.Dispose();
                target = null;
                base.SceneEnd(scene);
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            public override void Render()
            {
                if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene) || target == null)
                    return;
                Draw.SpriteBatch.Draw((RenderTarget2D) target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0.0f);
                if (easingOut)
                    return;
                GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height / 2 + (timer % 1.0 < 0.25 ? 6 : 0)));
            }
        }
    }
}
