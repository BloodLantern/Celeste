// Decompiled with JetBrains decompiler
// Type: Celeste.Selfie
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Selfie : Entity
    {
        private readonly Level level;
        private Monocle.Image image;
        private Monocle.Image overImage;
        private bool waitForKeyPress;
        private float timer;
        private Tween tween;

        public Selfie(Level level)
        {
            Tag = (int)Tags.HUD;
            this.level = level;
        }

        public IEnumerator PictureRoutine(string photo = "selfie")
        {
            level.Flash(Color.White);
            yield return 0.5f;
            yield return OpenRoutine(photo);
            yield return WaitForInput();
            yield return EndRoutine();
        }

        public IEnumerator FilterRoutine()
        {
            yield return OpenRoutine();
            yield return 0.5f;
            MTexture tex = GFX.Portraits["selfieFilter"];
            overImage = new Monocle.Image(tex)
            {
                Visible = false
            };
            _ = overImage.CenterOrigin();
            int atWidth = 0;
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.4f, true);
            tween.OnUpdate = t =>
            {
                int num = (int)Math.Round((double)MathHelper.Lerp(0.0f, tex.Width, t.Eased));
                if (num == atWidth)
                {
                    return;
                }

                atWidth = num;
                overImage.Texture = tex.GetSubtexture(tex.Width - atWidth, 0, atWidth, tex.Height);
                overImage.Visible = true;
                overImage.Origin.X = atWidth - (tex.Width / 2);
            };
            _ = Audio.Play("event:/game/02_old_site/theoselfie_photo_filter");
            yield return tween.Wait();
            yield return WaitForInput();
            yield return EndRoutine();
        }

        public IEnumerator OpenRoutine(string selfie = "selfie")
        {
            _ = Audio.Play("event:/game/02_old_site/theoselfie_photo_in");
            image = new Monocle.Image(GFX.Portraits[selfie]);
            _ = image.CenterOrigin();
            float percent = 0.0f;
            while ((double)percent < 1.0)
            {
                percent += Engine.DeltaTime;
                image.Position = Vector2.Lerp(new Vector2(992f, (float)(1080.0 + ((double)image.Height / 2.0))), new Vector2(960f, 540f), Ease.CubeOut(percent));
                image.Rotation = MathHelper.Lerp(0.5f, 0.0f, Ease.BackOut(percent));
                yield return null;
            }
        }

        public IEnumerator WaitForInput()
        {
            waitForKeyPress = true;
            while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed)
            {
                yield return null;
            }

            _ = Audio.Play("event:/ui/main/button_lowkey");
            waitForKeyPress = false;
        }

        public IEnumerator EndRoutine()
        {
            Selfie selfie = this;
            _ = Audio.Play("event:/game/02_old_site/theoselfie_photo_out");
            float percent = 0.0f;
            while ((double)percent < 1.0)
            {
                percent += Engine.DeltaTime * 2f;
                selfie.image.Position = Vector2.Lerp(new Vector2(960f, 540f), new Vector2(928f, (float)(-(double)selfie.image.Height / 2.0)), Ease.BackIn(percent));
                selfie.image.Rotation = MathHelper.Lerp(0.0f, -0.15f, Ease.BackIn(percent));
                yield return null;
            }
            yield return null;
            selfie.level.Remove(selfie);
        }

        public override void Update()
        {
            if (tween != null && tween.Active)
            {
                tween.Update();
            }

            if (!waitForKeyPress)
            {
                return;
            }

            timer += Engine.DeltaTime;
        }

        public override void Render()
        {
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
            {
                return;
            }

            if (image != null && image.Visible)
            {
                image.Render();
                if (overImage != null && overImage.Visible)
                {
                    overImage.Position = image.Position;
                    overImage.Rotation = image.Rotation;
                    overImage.Scale = image.Scale;
                    overImage.Render();
                }
            }
            if (!waitForKeyPress)
            {
                return;
            }

            GFX.Gui["textboxbutton"].DrawCentered(image.Position + new Vector2((float)(((double)image.Width / 2.0) + 40.0), (float)(((double)image.Height / 2.0) + (timer % 1.0 < 0.25 ? 6.0 : 0.0))));
        }
    }
}
