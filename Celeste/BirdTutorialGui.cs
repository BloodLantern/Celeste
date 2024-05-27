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
        private object info;
        private List<object> controls;
        private float controlsWidth;
        private float infoWidth;
        private float infoHeight;
        private float buttonPadding = 8f;
        private Color bgColor = Calc.HexToColor("061526");
        private Color lineColor = new Color(1f, 1f, 1f);
        private Color textColor = Calc.HexToColor("6179e2");

        public BirdTutorialGui(Entity entity, Vector2 position, object info, params object[] controls)
        {
            AddTag((int) Tags.HUD);
            Entity = entity;
            Position = position;
            this.info = info;
            this.controls = new List<object>(controls);
            switch (info)
            {
                case string _:
                    infoWidth = ActiveFont.Measure((string) info).X;
                    infoHeight = ActiveFont.LineHeight;
                    break;
                case MTexture _:
                    infoWidth = ((MTexture) info).Width;
                    infoHeight = ((MTexture) info).Height;
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
                        controlsWidth += Input.GuiButton(BirdTutorialGui.ButtonPromptToVirtualButton(prompt)).Width + buttonPadding * 2f;
                        continue;
                    case Vector2 direction:
                        controlsWidth += Input.GuiDirection(direction).Width + buttonPadding * 2f;
                        continue;
                    case string _:
                        controlsWidth += ActiveFont.Measure(control.ToString()).X;
                        continue;
                    case MTexture _:
                        controlsWidth += ((MTexture) control).Width;
                        continue;
                    default:
                        continue;
                }
            }
        }

        public override void Update()
        {
            UpdateControlsSize();
            Scale = Calc.Approach(Scale, Open ? 1f : 0.0f, Engine.RawDeltaTime * 8f);
            base.Update();
        }

        public override void Render()
        {
            Level scene = Scene as Level;
            if (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || Scale <= 0.0)
                return;
            Vector2 vector2_1 = Entity.Position + Position - SceneAs<Level>().Camera.Position.Floor();
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                vector2_1.X = 320f - vector2_1.X;
            vector2_1.X *= 6f;
            vector2_1.Y *= 6f;
            float lineHeight = ActiveFont.LineHeight;
            float width1 = (Math.Max(controlsWidth, infoWidth) + 64f) * Scale;
            float height = (float) (infoHeight + (double) lineHeight + 32.0);
            double x1 = vector2_1.X - width1 / 2.0;
            float y = (float) (vector2_1.Y - (double) height - 32.0);
            Draw.Rect((float) (x1 - 6.0), y - 6f, width1 + 12f, height + 12f, lineColor);
            Draw.Rect((float) x1, y, width1, height, bgColor);
            for (int index = 0; index <= 36; ++index)
            {
                float width2 = (73 - index * 2) * Scale;
                Draw.Rect(vector2_1.X - width2 / 2f, y + height + index, width2, 1f, lineColor);
                if (width2 > 12.0)
                    Draw.Rect((float) (vector2_1.X - width2 / 2.0 + 6.0), y + height + index, width2 - 12f, 1f, bgColor);
            }
            if (width1 <= 3.0)
                return;
            Vector2 position = new Vector2(vector2_1.X, y + 16f);
            if (info is string)
                ActiveFont.Draw((string) info, position, new Vector2(0.5f, 0.0f), new Vector2(Scale, 1f), textColor);
            else if (info is MTexture)
                ((MTexture) info).DrawJustified(position, new Vector2(0.5f, 0.0f), Color.White, new Vector2(Scale, 1f));
            position.Y += infoHeight + lineHeight * 0.5f;
            Vector2 vector2_2 = new Vector2((float) (-(double) controlsWidth / 2.0), 0.0f);
            foreach (object control in controls)
            {
                switch (control)
                {
                    case ButtonPrompt prompt:
                        MTexture mtexture1 = Input.GuiButton(BirdTutorialGui.ButtonPromptToVirtualButton(prompt));
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
                    case string _:
                        string text = control.ToString();
                        float x2 = ActiveFont.Measure(text).X;
                        ActiveFont.Draw(text, position + new Vector2(1f, 2f), new Vector2(-vector2_2.X / x2, 0.5f), new Vector2(Scale, 1f), textColor);
                        ActiveFont.Draw(text, position + new Vector2(1f, -2f), new Vector2(-vector2_2.X / x2, 0.5f), new Vector2(Scale, 1f), Color.White);
                        vector2_2.X += x2 + 1f;
                        continue;
                    case MTexture _:
                        MTexture mtexture3 = (MTexture) control;
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
            switch (prompt)
            {
                case ButtonPrompt.Dash:
                    return Input.Dash;
                case ButtonPrompt.Jump:
                    return Input.Jump;
                case ButtonPrompt.Grab:
                    return Input.Grab;
                case ButtonPrompt.Talk:
                    return Input.Talk;
                default:
                    return Input.Jump;
            }
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
