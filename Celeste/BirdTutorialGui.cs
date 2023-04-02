// Decompiled with JetBrains decompiler
// Type: Celeste.BirdTutorialGui
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class BirdTutorialGui : Entity
    {
        public Entity Entity;
        public bool Open;
        public float Scale;
        private readonly object info;
        private readonly List<object> controls;
        private float controlsWidth;
        private readonly float infoWidth;
        private readonly float infoHeight;
        private readonly float buttonPadding = 8f;
        private Color bgColor = Calc.HexToColor("061526");
        private Color lineColor = new(1f, 1f, 1f);
        private Color textColor = Calc.HexToColor("6179e2");

        public BirdTutorialGui(Entity entity, Vector2 position, object info, params object[] controls)
        {
            AddTag(Tags.HUD);
            Entity = entity;
            Position = position;
            this.info = info;
            this.controls = new List<object>(controls);
            switch (info)
            {
                case string _:
                    infoWidth = ActiveFont.Measure((string)info).X;
                    infoHeight = ActiveFont.LineHeight;
                    break;
                case MTexture _:
                    infoWidth = ((MTexture)info).Width;
                    infoHeight = ((MTexture)info).Height;
                    break;
            }
            UpdateControlsSize();
        }

        public void UpdateControlsSize()
        {
            controlsWidth = 0.0f;
            foreach (object control in controls)
            {
                switch (control)
                {
                    case ButtonPrompt prompt:
                        controlsWidth += Input.GuiButton(ButtonPromptToVirtualButton(prompt)).Width + (buttonPadding * 2f);
                        continue;
                    case Vector2 direction:
                        controlsWidth += Input.GuiDirection(direction).Width + (buttonPadding * 2f);
                        continue;
                    case string:
                        controlsWidth += ActiveFont.Measure(control.ToString()).X;
                        continue;
                    case MTexture:
                        controlsWidth += ((MTexture)control).Width;
                        continue;
                    default:
                        continue;
                }
            }
        }

        public override void Update()
        {
            UpdateControlsSize();
            Scale = Calc.Approach(Scale, Open ? 1 : 0, Engine.RawDeltaTime * 8f);
            base.Update();
        }

        public override void Render()
        {
            Level scene = Scene as Level;
            if (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || Scale <= 0)
                return;

            Vector2 vector2_1 = Entity.Position + Position - SceneAs<Level>().Camera.Position.Floor();
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                vector2_1.X = 320f - vector2_1.X;

            vector2_1.X *= 6f;
            vector2_1.Y *= 6f;
            float lineHeight = ActiveFont.LineHeight;
            float width1 = (Math.Max(controlsWidth, infoWidth) + 64f) * Scale;
            float height = (infoHeight + lineHeight + 32);
            float x1 = vector2_1.X - (width1 / 2);
            float y = (vector2_1.Y - height - 32);
            Draw.Rect(x1 - 6, y - 6f, width1 + 12f, height + 12f, lineColor);
            Draw.Rect(x1, y, width1, height, bgColor);
            for (int i = 0; i <= 36; i++)
            {
                float width2 = (73 - (i * 2)) * Scale;
                Draw.Rect(vector2_1.X - (width2 / 2f), y + height + i, width2, 1f, lineColor);
                if (width2 > 12)
                    Draw.Rect((vector2_1.X - (width2 / 2) + 6), y + height + i, width2 - 12f, 1f, bgColor);
            }
            if (width1 <= 3)
                return;

            Vector2 position = new(vector2_1.X, y + 16f);
            if (info is string sInfo)
                ActiveFont.Draw(sInfo, position, new Vector2(0.5f, 0.0f), new Vector2(Scale, 1f), textColor);
            else if (info is MTexture texture)
                texture.DrawJustified(position, new Vector2(0.5f, 0.0f), Color.White, new Vector2(Scale, 1f));

            position.Y += infoHeight + (lineHeight * 0.5f);
            Vector2 vector2_2 = new(-controlsWidth / 2, 0.0f);
            foreach (object control in controls)
            {
                switch (control)
                {
                    case ButtonPrompt prompt:
                        MTexture mtexture1 = Input.GuiButton(ButtonPromptToVirtualButton(prompt));
                        vector2_2.X += buttonPadding;
                        mtexture1.Draw(position, new Vector2(-vector2_2.X, mtexture1.Height / 2), Color.White, new Vector2(Scale, 1f));
                        vector2_2.X += mtexture1.Width + buttonPadding;
                        continue;
                    case Vector2 direction:
                        if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                            direction.X = -direction.X;

                        MTexture mtexture2 = Input.GuiDirection(direction);
                        vector2_2.X += buttonPadding;
                        mtexture2.Draw(position, new Vector2(-vector2_2.X, mtexture2.Height / 2), Color.White, new Vector2(Scale, 1f));
                        vector2_2.X += mtexture2.Width + buttonPadding;
                        continue;
                    case string:
                        string text = control.ToString();
                        float x2 = ActiveFont.Measure(text).X;
                        ActiveFont.Draw(text, position + new Vector2(1f, 2f), new Vector2(-vector2_2.X / x2, 0.5f), new Vector2(Scale, 1f), textColor);
                        ActiveFont.Draw(text, position + new Vector2(1f, -2f), new Vector2(-vector2_2.X / x2, 0.5f), new Vector2(Scale, 1f), Color.White);
                        vector2_2.X += x2 + 1f;
                        continue;
                    case MTexture:
                        MTexture mtexture3 = (MTexture)control;
                        mtexture3.Draw(position, new Vector2(-vector2_2.X, mtexture3.Height / 2), Color.White, new Vector2(Scale, 1f));
                        vector2_2.X += mtexture3.Width;
                        continue;
                    default:
                        continue;
                }
            }
        }

        public static VirtualButton ButtonPromptToVirtualButton(
            ButtonPrompt prompt)
        {
            return prompt switch
            {
                ButtonPrompt.Dash => Input.Dash,
                ButtonPrompt.Jump => Input.Jump,
                ButtonPrompt.Grab => Input.Grab,
                ButtonPrompt.Talk => Input.Talk,
                _ => Input.Jump,
            };
        }

        public enum ButtonPrompt
        {
            Dash,
            Jump,
            Grab,
            Talk,
        }
    }
}
