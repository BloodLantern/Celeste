// Decompiled with JetBrains decompiler
// Type: Celeste.OuiTitleScreen
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiTitleScreen : Oui
    {
        public static readonly MountainCamera MountainTarget = new(new Vector3(0.0f, 12f, 24f), MountainRenderer.RotateLookAt);
        private const float TextY = 1000f;
        private const float TextOutY = 1200f;
        private const int ReflectionSliceSize = 4;
        private float alpha;
        private float fade;
        private readonly string version = "v." + Celeste.Instance.Version;
        private bool hideConfirmButton;
        private readonly Monocle.Image logo;
        private readonly MTexture title;
        private readonly List<MTexture> reflections;
        private float textY;

        public OuiTitleScreen()
        {
            logo = new Monocle.Image(GFX.Gui[nameof(logo)]);
            _ = logo.CenterOrigin();
            logo.Position = new Vector2(1920f, 1080f) / 2f;
            title = GFX.Gui[nameof(title)];
            reflections = new List<MTexture>();
            for (int y = title.Height - 4; y > 0; y -= 4)
            {
                reflections.Add(title.GetSubtexture(0, y, title.Width, 4));
            }

            if (Celeste.PlayMode != Celeste.PlayModes.Normal)
            {
                if ("".Length > 0)
                {
                    version += "\n";
                }

                version = version + "\n" + Celeste.PlayMode.ToString() + " Build";
            }
            if (!Settings.Instance.LaunchWithFMODLiveUpdate)
            {
                return;
            }

            version += "\nFMOD Live Update Enabled";
        }

        public override bool IsStart(Overworld overworld, Overworld.StartMode start)
        {
            if (start == Overworld.StartMode.Titlescreen)
            {
                overworld.ShowInputUI = false;
                overworld.Mountain.SnapCamera(-1, OuiTitleScreen.MountainTarget);
                textY = 1000f;
                alpha = 1f;
                fade = 1f;
                return true;
            }
            textY = 1200f;
            return false;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiTitleScreen ouiTitleScreen = this;
            yield return null;
            ouiTitleScreen.Overworld.ShowInputUI = false;
            MountainCamera camera = ouiTitleScreen.Overworld.Mountain.Camera;
            Vector3 rotateLookAt = MountainRenderer.RotateLookAt;
            Vector3 vector3 = (camera.Position - new Vector3(rotateLookAt.X, camera.Position.Y - 2f, rotateLookAt.Z)).SafeNormalize();
            MountainCamera transform = new(MountainRenderer.RotateLookAt + (vector3 * 20f), camera.Target);
            ouiTitleScreen.Add(new Coroutine(ouiTitleScreen.FadeBgTo(1f)));
            ouiTitleScreen.hideConfirmButton = false;
            ouiTitleScreen.Visible = true;
            double num = (double)ouiTitleScreen.Overworld.Mountain.EaseCamera(-1, transform, new float?(2f), false);
            float start = ouiTitleScreen.textY;
            yield return 0.4f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.6f, true);
            tween.OnUpdate = t =>
            {
                alpha = t.Percent;
                textY = MathHelper.Lerp(start, 1000f, t.Eased);
            };
            ouiTitleScreen.Add(tween);
            yield return tween.Wait();
            ouiTitleScreen.Overworld.Mountain.SnapCamera(-1, OuiTitleScreen.MountainTarget);
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiTitleScreen ouiTitleScreen = this;
            ouiTitleScreen.Overworld.ShowInputUI = true;
            ouiTitleScreen.Overworld.Mountain.GotoRotationMode();
            float start = ouiTitleScreen.textY;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.6f, true);
            tween.OnUpdate = t =>
            {
                alpha = 1f - t.Percent;
                textY = MathHelper.Lerp(start, 1200f, t.Eased);
            };
            ouiTitleScreen.Add(tween);
            yield return tween.Wait();
            yield return ouiTitleScreen.FadeBgTo(0.0f);
            ouiTitleScreen.Visible = false;
        }

        private IEnumerator FadeBgTo(float to)
        {
            for (; fade != (double)to; fade = Calc.Approach(fade, to, Engine.DeltaTime * 2f))
            {
                yield return null;
            }
        }

        public override void Update()
        {
            if (Selected && Input.AnyGamepadConfirmPressed(out int gamepadIndex) && !hideConfirmButton)
            {
                Input.Gamepad = gamepadIndex;
                _ = Audio.Play("event:/ui/main/title_firstinput");
                _ = Overworld.Goto<OuiMainMenu>();
            }
            base.Update();
        }

        public override void Render()
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
            if (!hideConfirmButton)
            {
                Input.GuiButton(Input.MenuConfirm).DrawJustified(new Vector2(1840f, textY), new Vector2(1f, 1f), Color.White * alpha, 1f);
            }

            ActiveFont.Draw(version, new Vector2(80f, textY), new Vector2(0.0f, 1f), Vector2.One * 0.5f, Color.DarkSlateBlue);
            if (alpha <= 0.0)
            {
                return;
            }

            float num1 = MathHelper.Lerp(0.5f, 1f, Ease.SineOut(alpha));
            logo.Color = Color.White * alpha;
            logo.Scale = Vector2.One * num1;
            logo.Render();
            float a = Scene.TimeActive * 3f;
            float num2 = (float)(1.0 / reflections.Count * 6.2831854820251465 * 2.0);
            float num3 = title.Width / logo.Width * num1;
            for (int index = 0; index < reflections.Count; ++index)
            {
                float num4 = index / (float)reflections.Count;
                Vector2 position = (new Vector2(1920f, 1080f) / 2f) + (new Vector2((float)Math.Sin((double)a) * 32f * num4, (logo.Height * 0.5f) + (index * 4)) * num3);
                float num5 = (float)((double)Ease.CubeIn(1f - num4) * alpha * 0.89999997615814209);
                reflections[index].DrawJustified(position, new Vector2(0.5f, 0.5f), Color.White * num5, new Vector2(1f, -1f) * num3);
                a += num2 * (float)(Math.Sin(Scene.TimeActive + (index * 6.2831854820251465 * 0.039999999105930328)) + 1.0);
            }
        }
    }
}
