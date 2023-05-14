// Decompiled with JetBrains decompiler
// Type: Celeste.WaveDashPage00
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Globalization;

namespace Celeste
{
    public class WaveDashPage00 : WaveDashPage
    {
        private Color taskbarColor = Calc.HexToColor("d9d3b1");
        private readonly string time;
        private Vector2 pptIcon;
        private Vector2 cursor;
        private bool selected;

        public WaveDashPage00()
        {
            AutoProgress = true;
            ClearColor = Calc.HexToColor("118475");
            time = DateTime.Now.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
            pptIcon = new Vector2(600f, 500f);
            cursor = new Vector2(1000f, 700f);
        }

        public override IEnumerator Routine()
        {
            WaveDashPage00 waveDashPage00 = this;
            yield return 1f;
            yield return waveDashPage00.MoveCursor(waveDashPage00.cursor + new Vector2(0.0f, -80f), 0.3f);
            yield return 0.2f;
            yield return waveDashPage00.MoveCursor(waveDashPage00.pptIcon, 0.8f);
            yield return 0.7f;
            waveDashPage00.selected = true;
            _ = Audio.Play("event:/new_content/game/10_farewell/ppt_doubleclick");
            yield return 0.1f;
            waveDashPage00.selected = false;
            yield return 0.1f;
            waveDashPage00.selected = true;
            yield return 0.08f;
            waveDashPage00.selected = false;
            yield return 0.5f;
            waveDashPage00.Presentation.ScaleInPoint = waveDashPage00.pptIcon;
        }

        private IEnumerator MoveCursor(Vector2 to, float time)
        {
            Vector2 from = cursor;
            for (float t = 0.0f; (double)t < 1.0; t += Engine.DeltaTime / time)
            {
                cursor = from + ((to - from) * Ease.SineOut(t));
                yield return null;
            }
        }

        public override void Update()
        {
        }

        public override void Render()
        {
            DrawIcon(new Vector2(160f, 120f), "desktop/mymountain_icon", Dialog.Clean("WAVEDASH_DESKTOP_MYPC"));
            DrawIcon(new Vector2(160f, 320f), "desktop/recyclebin_icon", Dialog.Clean("WAVEDASH_DESKTOP_RECYCLEBIN"));
            DrawIcon(pptIcon, "desktop/wavedashing_icon", Dialog.Clean("WAVEDASH_DESKTOP_POWERPOINT"));
            DrawTaskbar();
            Presentation.Gfx["desktop/cursor"].DrawCentered(cursor);
        }

        public void DrawTaskbar()
        {
            Draw.Rect(0.0f, Height - 80f, Width, 80f, taskbarColor);
            Draw.Rect(0.0f, Height - 80f, Width, 4f, Color.White * 0.5f);
            MTexture mtexture = Presentation.Gfx["desktop/startberry"];
            float height = 64f;
            float num1 = (float)((double)height / mtexture.Height * 0.699999988079071);
            string text = Dialog.Clean("WAVEDASH_DESKTOP_STARTBUTTON");
            float num2 = 0.6f;
            float width = (float)((mtexture.Width * (double)num1) + (ActiveFont.Measure(text).X * (double)num2) + 32.0);
            Vector2 vector2 = new(8f, (float)(Height - 80.0 + 8.0));
            Draw.Rect(vector2.X, vector2.Y, width, height, Color.White * 0.5f);
            mtexture.DrawJustified(vector2 + new Vector2(8f, height / 2f), new Vector2(0.0f, 0.5f), Color.White, Vector2.One * num1);
            ActiveFont.Draw(text, vector2 + new Vector2((float)((mtexture.Width * (double)num1) + 16.0), height / 2f), new Vector2(0.0f, 0.5f), Vector2.One * num2, Color.Black * 0.8f);
            ActiveFont.Draw(time, new Vector2(Width - 24f, Height - 40f), new Vector2(1f, 0.5f), Vector2.One * 0.6f, Color.Black * 0.8f);
        }

        private void DrawIcon(Vector2 position, string icon, string text)
        {
            bool flag = cursor.X > position.X - 64.0 && cursor.Y > position.Y - 64.0 && cursor.X < position.X + 64.0 && cursor.Y < position.Y + 80.0;
            if (selected & flag)
            {
                Draw.Rect(position.X - 80f, position.Y - 80f, 160f, 200f, Color.White * 0.25f);
            }

            if (flag)
            {
                DrawDottedRect(position.X - 80f, position.Y - 80f, 160f, 200f);
            }

            MTexture mtexture = Presentation.Gfx[icon];
            float scale = 128f / mtexture.Height;
            mtexture.DrawCentered(position, Color.White, scale);
            ActiveFont.Draw(text, position + new Vector2(0.0f, 80f), new Vector2(0.5f, 0.0f), Vector2.One * 0.6f, selected & flag ? Color.Black : Color.White);
        }

        private void DrawDottedRect(float x, float y, float w, float h)
        {
            float num1 = 4f;
            Draw.Rect(x, y, w, num1, Color.White);
            Draw.Rect(x + w - num1, y, num1, h, Color.White);
            Draw.Rect(x, y, num1, h, Color.White);
            Draw.Rect(x, y + h - num1, w, num1, Color.White);
            if (selected)
            {
                return;
            }

            for (float num2 = 4f; (double)num2 < (double)w; num2 += num1 * 2f)
            {
                Draw.Rect(x + num2, y, num1, num1, ClearColor);
                Draw.Rect(x + w - num2, y + h - num1, num1, num1, ClearColor);
            }
            for (float num3 = 4f; (double)num3 < (double)h; num3 += num1 * 2f)
            {
                Draw.Rect(x, y + num3, num1, num1, ClearColor);
                Draw.Rect(x + w - num1, y + h - num3, num1, num1, ClearColor);
            }
        }
    }
}
