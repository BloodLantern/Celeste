using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Selfie : Entity
    {
        private Level level;
        private Monocle.Image image;
        private Monocle.Image overImage;
        private bool waitForKeyPress;
        private float timer;
        private Tween tween;

        public Selfie(Level level)
        {
            this.Tag = (int) Tags.HUD;
            this.level = level;
        }

        public IEnumerator PictureRoutine(string photo = "selfie")
        {
            this.level.Flash(Color.White);
            yield return (object) 0.5f;
            yield return (object) this.OpenRoutine(photo);
            yield return (object) this.WaitForInput();
            yield return (object) this.EndRoutine();
        }

        public IEnumerator FilterRoutine()
        {
            yield return (object) this.OpenRoutine();
            yield return (object) 0.5f;
            MTexture tex = GFX.Portraits["selfieFilter"];
            this.overImage = new Monocle.Image(tex);
            this.overImage.Visible = false;
            this.overImage.CenterOrigin();
            int atWidth = 0;
            this.tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.4f, true);
            this.tween.OnUpdate = (Action<Tween>) (t =>
            {
                int num = (int) Math.Round((double) MathHelper.Lerp(0.0f, (float) tex.Width, t.Eased));
                if (num == atWidth)
                    return;
                atWidth = num;
                this.overImage.Texture = tex.GetSubtexture(tex.Width - atWidth, 0, atWidth, tex.Height);
                this.overImage.Visible = true;
                this.overImage.Origin.X = (float) (atWidth - tex.Width / 2);
            });
            Audio.Play("event:/game/02_old_site/theoselfie_photo_filter");
            yield return (object) this.tween.Wait();
            yield return (object) this.WaitForInput();
            yield return (object) this.EndRoutine();
        }

        public IEnumerator OpenRoutine(string selfie = "selfie")
        {
            Audio.Play("event:/game/02_old_site/theoselfie_photo_in");
            this.image = new Monocle.Image(GFX.Portraits[selfie]);
            this.image.CenterOrigin();
            float percent = 0.0f;
            while ((double) percent < 1.0)
            {
                percent += Engine.DeltaTime;
                this.image.Position = Vector2.Lerp(new Vector2(992f, (float) (1080.0 + (double) this.image.Height / 2.0)), new Vector2(960f, 540f), Ease.CubeOut(percent));
                this.image.Rotation = MathHelper.Lerp(0.5f, 0.0f, Ease.BackOut(percent));
                yield return (object) null;
            }
        }

        public IEnumerator WaitForInput()
        {
            this.waitForKeyPress = true;
            while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed)
                yield return (object) null;
            Audio.Play("event:/ui/main/button_lowkey");
            this.waitForKeyPress = false;
        }

        public IEnumerator EndRoutine()
        {
            Selfie selfie = this;
            Audio.Play("event:/game/02_old_site/theoselfie_photo_out");
            float percent = 0.0f;
            while ((double) percent < 1.0)
            {
                percent += Engine.DeltaTime * 2f;
                selfie.image.Position = Vector2.Lerp(new Vector2(960f, 540f), new Vector2(928f, (float) (-(double) selfie.image.Height / 2.0)), Ease.BackIn(percent));
                selfie.image.Rotation = MathHelper.Lerp(0.0f, -0.15f, Ease.BackIn(percent));
                yield return (object) null;
            }
            yield return (object) null;
            selfie.level.Remove((Entity) selfie);
        }

        public override void Update()
        {
            if (this.tween != null && this.tween.Active)
                this.tween.Update();
            if (!this.waitForKeyPress)
                return;
            this.timer += Engine.DeltaTime;
        }

        public override void Render()
        {
            if (this.Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
                return;
            if (this.image != null && this.image.Visible)
            {
                this.image.Render();
                if (this.overImage != null && this.overImage.Visible)
                {
                    this.overImage.Position = this.image.Position;
                    this.overImage.Rotation = this.image.Rotation;
                    this.overImage.Scale = this.image.Scale;
                    this.overImage.Render();
                }
            }
            if (!this.waitForKeyPress)
                return;
            GFX.Gui["textboxbutton"].DrawCentered(this.image.Position + new Vector2((float) ((double) this.image.Width / 2.0 + 40.0), (float) ((double) this.image.Height / 2.0 + ((double) this.timer % 1.0 < 0.25 ? 6.0 : 0.0))));
        }
    }
}
