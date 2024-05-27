using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class PreviewDialog : Scene
    {
        private Language language;
        private List<string> list = new List<string>();
        private Vector2 listScroll = new Vector2(64f, 64f);
        private const float scale = 0.6f;
        private string current;
        private List<object> elements = new List<object>();
        private Vector2 textboxScroll = new Vector2(0.0f, 0.0f);
        private float delay;

        public PreviewDialog(Language language = null, float listScroll = 64f, float textboxScroll = 0.0f, string dialog = null)
        {
            this.listScroll.Y = listScroll;
            this.textboxScroll.Y = textboxScroll;
            if (language != null)
                SetLanguage(language);
            if (dialog != null)
                SetCurrent(dialog);
            Add(new Renderer(this));
        }

        public override void End()
        {
            base.End();
            UnsetLanguage();
        }

        public override void Update()
        {
            if (!Engine.Instance.IsActive)
                delay = 0.1f;
            else if (delay > 0.0)
            {
                delay -= Engine.DeltaTime;
            }
            else
            {
                if (this.current != null)
                {
                    float num = 1f;
                    foreach (object element in elements)
                    {
                        if (element is Textbox textbox)
                        {
                            textbox.RenderOffset = textboxScroll + Vector2.UnitY * num;
                            num += 300f;
                            if (textbox.Scene != null)
                                textbox.Update();
                        }
                        else
                            num += language.FontSize.LineHeight + 50;
                    }
                    textboxScroll.Y += MInput.Mouse.WheelDelta * Engine.DeltaTime * ActiveFont.LineHeight;
                    textboxScroll.Y -= (float) (Input.Aim.Value.Y * (double) Engine.DeltaTime * ActiveFont.LineHeight * 20.0);
                    textboxScroll.Y = Calc.Clamp(textboxScroll.Y, 716f - num, 64f);
                    if (MInput.Keyboard.Pressed(Keys.Escape) || Input.MenuConfirm.Pressed)
                        ClearTextboxes();
                    else if (MInput.Keyboard.Pressed(Keys.Space))
                    {
                        string current = this.current;
                        ClearTextboxes();
                        int index = list.IndexOf(current) + 1;
                        if (index < list.Count)
                            SetCurrent(list[index]);
                    }
                }
                else
                {
                    listScroll.Y += MInput.Mouse.WheelDelta * Engine.DeltaTime * ActiveFont.LineHeight;
                    listScroll.Y -= (float) (Input.Aim.Value.Y * (double) Engine.DeltaTime * ActiveFont.LineHeight * 20.0);
                    listScroll.Y = Calc.Clamp(listScroll.Y, (float) (1016.0 - list.Count * (double) ActiveFont.LineHeight * 0.60000002384185791), 64f);
                    if (this.language != null)
                    {
                        if (MInput.Mouse.PressedLeftButton)
                        {
                            for (int index = 0; index < list.Count; ++index)
                            {
                                if (MouseOverOption(index))
                                {
                                    SetCurrent(list[index]);
                                    break;
                                }
                            }
                        }
                        if (MInput.Keyboard.Pressed(Keys.Escape) || Input.MenuConfirm.Pressed)
                        {
                            listScroll = new Vector2(64f, 64f);
                            UnsetLanguage();
                        }
                    }
                    else if (MInput.Mouse.PressedLeftButton)
                    {
                        int i = 0;
                        foreach (KeyValuePair<string, Language> language in Dialog.Languages)
                        {
                            if (MouseOverOption(i))
                            {
                                SetLanguage(language.Value);
                                listScroll = new Vector2(64f, 64f);
                                break;
                            }
                            ++i;
                        }
                    }
                }
                if (MInput.Keyboard.Pressed(Keys.F2))
                {
                    Celeste.ReloadPortraits();
                    Engine.Scene = new PreviewDialog(language, listScroll.Y, textboxScroll.Y, current);
                }
                if (!MInput.Keyboard.Pressed(Keys.F1) || this.language == null)
                    return;
                Celeste.ReloadDialog();
                Engine.Scene = new PreviewDialog(Dialog.Languages[this.language.Id], listScroll.Y, textboxScroll.Y, this.current);
            }
        }

        private void ClearTextboxes()
        {
            foreach (object element in elements)
            {
                if (element is Textbox)
                    Remove(element as Textbox);
            }
            current = null;
            textboxScroll = Vector2.Zero;
        }

        private void SetCurrent(string id)
        {
            current = id;
            elements.Clear();
            Textbox textbox1 = null;
            int page = 0;
            while (true)
            {
                Textbox textbox2 = new Textbox(id, language);
                if (textbox2.SkipToPage(page))
                {
                    if (textbox1 != null)
                    {
                        for (int index = textbox1.Start + 1; index <= textbox2.Start && index < textbox1.Nodes.Count; ++index)
                        {
                            if (textbox1.Nodes[index] is FancyText.Trigger node)
                                elements.Add((node.Silent ? "Silent " : (object) "") + "Trigger [" + node.Index + "] " + node.Label);
                        }
                    }
                    Add(textbox2);
                    elements.Add(textbox2);
                    textbox2.RenderOffset = textboxScroll + Vector2.UnitY * (1 + page * 300);
                    textbox1 = textbox2;
                    ++page;
                }
                else
                    break;
            }
        }

        private void SetLanguage(Language lan)
        {
            Fonts.Load(lan.FontFace);
            language = lan;
            list.Clear();
            bool flag = false;
            foreach (KeyValuePair<string, string> keyValuePair in language.Dialog)
            {
                if (!flag && keyValuePair.Key.StartsWith("CH0", StringComparison.OrdinalIgnoreCase))
                    flag = true;
                if (flag && !keyValuePair.Key.StartsWith("poem_", StringComparison.OrdinalIgnoreCase) && !keyValuePair.Key.StartsWith("journal_", StringComparison.OrdinalIgnoreCase))
                    list.Add(keyValuePair.Key);
            }
        }

        private void UnsetLanguage()
        {
            if (language != null && language.Id != Settings.Instance.Language && language.FontFace != Dialog.Languages["english"].FontFace)
                Fonts.Unload(language.FontFace);
            language = null;
        }

        public Vector2 Mouse => Vector2.Transform(new Vector2(MInput.Mouse.CurrentState.X, MInput.Mouse.CurrentState.Y), Matrix.Invert(Engine.ScreenMatrix));

        private void RenderContent()
        {
            Draw.Rect(0.0f, 0.0f, 960f, 1080f, Color.DarkSlateGray * 0.25f);
            if (current != null)
            {
                int num1 = 1;
                int num2 = 0;
                foreach (object element in elements)
                {
                    if (element is Textbox textbox)
                    {
                        if (textbox.Opened && language.Font.Sizes.Count > 0)
                        {
                            textbox.Render();
                            language.Font.DrawOutline(language.FontFaceSize, "#" + num1, textbox.RenderOffset + new Vector2(32f, 64f), Vector2.Zero, Vector2.One * 0.5f, Color.White, 2f, Color.Black);
                            ++num1;
                            num2 += 300;
                        }
                    }
                    else
                    {
                        language.Font.DrawOutline(language.FontFaceSize, element.ToString(), textboxScroll + new Vector2(128f, num2 + 50 + language.FontSize.LineHeight), new Vector2(0.0f, 0.5f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
                        num2 += language.FontSize.LineHeight + 50;
                    }
                }
                ActiveFont.DrawOutline(current, new Vector2(1888f, 32f), new Vector2(1f, 0.0f), Vector2.One * 0.5f, Color.Red, 2f, Color.Black);
            }
            else if (this.language != null)
            {
                int i = 0;
                foreach (string text in list)
                {
                    if (language.Font.Sizes.Count > 0)
                        language.Font.Draw(language.FontFaceSize, text, listScroll + new Vector2(0.0f, (float) (i * (double) ActiveFont.LineHeight * 0.60000002384185791)), Vector2.Zero, Vector2.One * 0.6f, MouseOverOption(i) ? Color.White : Color.Gray);
                    ++i;
                }
            }
            else
            {
                int i = 0;
                foreach (KeyValuePair<string, Language> language in Dialog.Languages)
                {
                    ActiveFont.Draw(language.Value.Id, listScroll + new Vector2(0.0f, (float) (i * (double) ActiveFont.LineHeight * 0.60000002384185791)), Vector2.Zero, Vector2.One * 0.6f, MouseOverOption(i) ? Color.White : Color.Gray);
                    ++i;
                }
            }
            Draw.Rect(Mouse.X - 12f, Mouse.Y - 4f, 24f, 8f, Color.Red);
            Draw.Rect(Mouse.X - 4f, Mouse.Y - 12f, 8f, 24f, Color.Red);
        }

        private bool MouseOverOption(int i) => Mouse.X > (double) listScroll.X && Mouse.Y > listScroll.Y + i * (double) ActiveFont.LineHeight * 0.60000002384185791 && MInput.Mouse.X < 960.0 && Mouse.Y < listScroll.Y + (i + 1) * (double) ActiveFont.LineHeight * 0.60000002384185791;

        private class Renderer : HiresRenderer
        {
            public PreviewDialog previewer;

            public Renderer(PreviewDialog previewer) => this.previewer = previewer;

            public override void RenderContent(Scene scene)
            {
                HiresRenderer.BeginRender();
                previewer.RenderContent();
                HiresRenderer.EndRender();
            }
        }
    }
}
