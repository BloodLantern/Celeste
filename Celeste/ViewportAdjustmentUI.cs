// Decompiled with JetBrains decompiler
// Type: Celeste.ViewportAdjustmentUI
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class ViewportAdjustmentUI : Entity
    {
        private const float minPadding = 0.0f;
        private const float maxPadding = 128f;
        private readonly float originalPadding = Engine.ViewPadding;
        private float viewPadding = Engine.ViewPadding;
        private float inputDelay;
        private bool closing;
        private bool canceling;
        private float leftAlpha = 1f;
        private float rightAlpha = 1f;
        public Action OnClose;

        public float Alpha { get; private set; }

        public bool Open { get; private set; }

        public ViewportAdjustmentUI()
        {
            Open = true;
            Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (scene is not Overworld)
            {
                return;
            } (scene as Overworld).Mountain.Model.LockBufferResizing = true;
        }

        public override void Removed(Scene scene)
        {
            if (scene is Overworld)
            {
                (scene as Overworld).Mountain.Model.LockBufferResizing = false;
            }

            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            if (!closing)
            {
                inputDelay += Engine.RawDeltaTime;
                if (inputDelay > 0.25)
                {
                    if ((Input.MenuCancel.Pressed ? 1 : (Input.ESC.Pressed ? 1 : 0)) != 0)
                    {
                        closing = canceling = true;
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        closing = true;
                    }
                }
            }
            else if ((double)Alpha <= 0.0)
            {
                if (canceling)
                {
                    Engine.ViewPadding = (int)originalPadding;
                }
                else
                {
                    Settings.Instance.ViewportPadding = (int)viewPadding;
                }

                Settings.Instance.SetViewportOnce = true;
                Open = false;
                RemoveSelf();
                if (OnClose == null)
                {
                    return;
                }

                OnClose();
                return;
            }
            Alpha = Calc.Approach(Alpha, closing ? 0.0f : 1f, Engine.RawDeltaTime * 4f);
            viewPadding -= Input.Aim.Value.X * 48f * Engine.RawDeltaTime;
            viewPadding = Calc.Clamp(viewPadding, 0.0f, 128f);
            leftAlpha = Calc.Approach(leftAlpha, viewPadding < 128.0 ? 1f : 0.25f, Engine.DeltaTime * 4f);
            rightAlpha = Calc.Approach(rightAlpha, viewPadding > 0.0 ? 1f : 0.25f, Engine.DeltaTime * 4f);
            Engine.ViewPadding = (int)viewPadding;
        }

        public override void Render()
        {
            float alpha = Ease.SineInOut(Alpha);
            Color color1 = Color.Black * 0.75f * alpha;
            Color color2 = Color.White * alpha;
            if (Scene is not Level)
            {
                Draw.Rect(-1f, -1f, Engine.Width + 2, Engine.Height + 2, color1);
            }

            Draw.Rect(0.0f, 0.0f, Engine.Width, 16f, color2);
            Draw.Rect(0.0f, 16f, 16f, Engine.Height - 32, color2);
            Draw.Rect(Engine.Width - 16, 16f, 16f, Engine.Height - 32, color2);
            Draw.Rect(0.0f, Engine.Height - 16, Engine.Width, 16f, color2);
            Draw.LineAngle(new Vector2(8f, 8f), 0.7853982f, 128f, color2, 16f);
            Draw.LineAngle(new Vector2(Engine.Width - 8, 8f), 2.3561945f, 128f, color2, 16f);
            Draw.LineAngle(new Vector2(8f, Engine.Height - 8), -0.7853982f, 128f, color2, 16f);
            Draw.LineAngle(new Vector2(Engine.Width - 8, Engine.Height - 8), -2.3561945f, 128f, color2, 16f);
            string text = Dialog.Clean("OPTIONS_VIEWPORT_PC");
            _ = ActiveFont.Measure(text);
            float num1 = (float)Math.Sin(Scene.RawTimeActive * 2.0) * 16f;
            Vector2 vector2_1 = new Vector2(Engine.Width, Engine.Height) * 0.5f;
            ActiveFont.Draw(text, vector2_1 + new Vector2(0.0f, -60f), new Vector2(0.5f, 0.5f), Vector2.One * 1.2f, color2);
            float num2 = ButtonUI.Width(Dialog.Clean("ui_confirm"), Input.MenuConfirm) * 0.8f;
            ButtonUI.Render(vector2_1 + new Vector2(0.0f, 60f), Dialog.Clean("ui_confirm"), Input.MenuConfirm, 0.8f, alpha: alpha);
            Vector2 vector2_2 = vector2_1 + new Vector2((float)(((double)num2 * 0.60000002384185791) + 80.0) + num1, 60f);
            GFX.Gui["adjustarrowright"].DrawCentered(vector2_2 + new Vector2(8f, 4f), color2 * rightAlpha, Vector2.One);
            Vector2 vector2_3 = vector2_1 + new Vector2((float)-(((double)num2 * 0.60000002384185791) + 80.0 + (double)num1), 60f);
            GFX.Gui["adjustarrowleft"].DrawCentered(vector2_3 + new Vector2(-8f, 4f), color2 * leftAlpha, Vector2.One);
        }
    }
}
