using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class Postcard : Entity
    {
        private const float TextScale = 0.7f;
        private MTexture postcard;
        private VirtualRenderTarget target;
        private FancyText.Text text;
        private float alpha = 1f;
        private float scale = 1f;
        private float rotation;
        private float buttonEase;
        private string sfxEventIn;
        private string sfxEventOut;
        private Coroutine easeButtonIn;

        public Postcard(string msg, int area)
            : this(msg, "event:/ui/main/postcard_ch" + area + "_in", "event:/ui/main/postcard_ch" + area + "_out")
        {
        }

        public Postcard(string msg, string sfxEventIn, string sfxEventOut)
        {
            Visible = false;
            Tag = (int) Tags.HUD;
            this.sfxEventIn = sfxEventIn;
            this.sfxEventOut = sfxEventOut;
            postcard = GFX.Gui[nameof (postcard)];
            text = FancyText.Parse(msg, (int) ((postcard.Width - 120) / 0.699999988079071), -1, defaultColor: Color.Black * 0.6f);
        }

        public IEnumerator DisplayRoutine()
        {
            yield return EaseIn();
            yield return 0.75f;
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            Audio.Play("event:/ui/main/button_lowkey");
            yield return EaseOut();
            yield return 1.2f;
        }

        public IEnumerator EaseIn()
        {
            Postcard postcard = this;
            Audio.Play(postcard.sfxEventIn);
            Vector2 vector2 = new Vector2(Engine.Width, Engine.Height) / 2f;
            Vector2 from = vector2 + new Vector2(0.0f, 200f);
            Vector2 to = vector2;
            float rFrom = -0.1f;
            float rTo = 0.05f;
            postcard.Visible = true;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 0.8f)
            {
                postcard.Position = from + (to - from) * Ease.CubeOut(p);
                postcard.alpha = Ease.CubeOut(p);
                postcard.rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
                yield return null;
            }
            postcard.Add(postcard.easeButtonIn = new Coroutine(postcard.EaseButtinIn()));
        }

        private IEnumerator EaseButtinIn()
        {
            yield return 0.75f;
            while ((buttonEase += Engine.DeltaTime * 2f) < 1.0)
                yield return null;
        }

        public IEnumerator EaseOut()
        {
            Postcard postcard = this;
            Audio.Play(postcard.sfxEventOut);
            if (postcard.easeButtonIn != null)
                postcard.easeButtonIn.RemoveSelf();
            Vector2 from = postcard.Position;
            Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0.0f, -200f);
            float rFrom = postcard.rotation;
            float rTo = postcard.rotation + 0.1f;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                postcard.Position = from + (to - from) * Ease.CubeIn(p);
                postcard.alpha = 1f - Ease.CubeIn(p);
                postcard.rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
                postcard.buttonEase = Calc.Approach(postcard.buttonEase, 0.0f, Engine.DeltaTime * 8f);
                yield return null;
            }
            postcard.alpha = 0.0f;
            postcard.RemoveSelf();
        }

        public void BeforeRender()
        {
            if (target == null)
                target = VirtualContent.CreateRenderTarget("postcard", postcard.Width, postcard.Height);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            string text = Dialog.Clean("FILE_DEFAULT");
            if (SaveData.Instance != null && Dialog.Language.CanDisplay(SaveData.Instance.Name))
                text = SaveData.Instance.Name;
            postcard.Draw(Vector2.Zero);
            ActiveFont.Draw(text, new Vector2(115f, 30f), Vector2.Zero, Vector2.One * 0.9f, Color.Black * 0.7f);
            this.text.DrawJustifyPerLine(new Vector2(postcard.Width, postcard.Height) / 2f + new Vector2(0.0f, 40f), new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            if (target != null)
                Draw.SpriteBatch.Draw((RenderTarget2D) target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0.0f);
            if (buttonEase <= 0.0)
                return;
            Input.GuiButton(Input.MenuConfirm).DrawCentered(new Vector2(Engine.Width - 120, Engine.Height - 100 - 20f * Ease.CubeOut(buttonEase)), Color.White * Ease.CubeOut(buttonEase));
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (target != null)
                target.Dispose();
            target = null;
        }
    }
}
