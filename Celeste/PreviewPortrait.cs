using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    public class PreviewPortrait : Scene
    {
        private Sprite animation;
        private List<string> options = new List<string>();
        private List<string> animations = new List<string>();
        private Vector2 topleft = new Vector2(64f, 64f);
        private string currentPortrait;

        public PreviewPortrait(float scroll = 64f)
        {
            foreach (KeyValuePair<string, SpriteData> keyValuePair in GFX.PortraitsSpriteBank.SpriteData)
            {
                if (keyValuePair.Key.StartsWith("portrait"))
                    options.Add(keyValuePair.Key);
            }
            topleft.Y = scroll;
        }

        public override void Update()
        {
            if (animation != null)
            {
                animation.Update();
                if (MInput.Mouse.PressedLeftButton)
                {
                    for (int index = 0; index < animations.Count; ++index)
                    {
                        if (MouseOverOption(index))
                        {
                            if (index == 0)
                            {
                                animation = null;
                                break;
                            }
                            animation.Play(animations[index]);
                            break;
                        }
                    }
                }
            }
            else if (MInput.Mouse.PressedLeftButton)
            {
                for (int index = 0; index < options.Count; ++index)
                {
                    if (MouseOverOption(index))
                    {
                        currentPortrait = options[index].Split('_')[1];
                        animation = GFX.PortraitsSpriteBank.Create(options[index]);
                        animations.Clear();
                        animations.Add("<-BACK");
                        XmlElement xml1 = GFX.PortraitsSpriteBank.SpriteData[options[index]].Sources[0].XML;
                        foreach (XmlElement xml2 in xml1.GetElementsByTagName("Anim"))
                            animations.Add(xml2.Attr("id"));
                        IEnumerator enumerator = xml1.GetElementsByTagName("Loop").GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                                animations.Add(((XmlElement) enumerator.Current).Attr("id"));
                            break;
                        }
                        finally
                        {
                            if (enumerator is IDisposable disposable)
                                disposable.Dispose();
                        }
                    }
                }
            }
            topleft.Y += MInput.Mouse.WheelDelta * Engine.DeltaTime * ActiveFont.LineHeight;
            if (!MInput.Keyboard.Pressed(Keys.F1))
                return;
            Celeste.ReloadPortraits();
            Engine.Scene = new PreviewPortrait(topleft.Y);
        }

        public Vector2 Mouse => Vector2.Transform(new Vector2(MInput.Mouse.CurrentState.X, MInput.Mouse.CurrentState.Y), Matrix.Invert(Engine.ScreenMatrix));

        public override void Render()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            Draw.Rect(0.0f, 0.0f, 960f, 1080f, Color.DarkSlateGray * 0.25f);
            if (this.animation != null)
            {
                this.animation.Scale = Vector2.One;
                this.animation.Position = new Vector2(1440f, 540f);
                this.animation.Render();
                int i = 0;
                foreach (string animation in animations)
                {
                    Color color = Color.Gray;
                    if (MouseOverOption(i))
                        color = Color.White;
                    else if (this.animation.CurrentAnimationID == animation)
                        color = Color.Yellow;
                    ActiveFont.Draw(animation, topleft + new Vector2(0.0f, i * ActiveFont.LineHeight), color);
                    ++i;
                }
                if (!string.IsNullOrEmpty(this.animation.CurrentAnimationID))
                {
                    string[] strArray = animation.CurrentAnimationID.Split('_');
                    if (strArray.Length > 1)
                        ActiveFont.Draw(currentPortrait + " " + strArray[1], new Vector2(1440f, 1016f), new Vector2(0.5f, 1f), Vector2.One, Color.White);
                }
            }
            else
            {
                int i = 0;
                foreach (string option in options)
                {
                    ActiveFont.Draw(option, topleft + new Vector2(0.0f, i * ActiveFont.LineHeight), MouseOverOption(i) ? Color.White : Color.Gray);
                    ++i;
                }
            }
            Draw.Rect(Mouse.X - 12f, Mouse.Y - 4f, 24f, 8f, Color.Red);
            Draw.Rect(Mouse.X - 4f, Mouse.Y - 12f, 8f, 24f, Color.Red);
            Draw.SpriteBatch.End();
        }

        private bool MouseOverOption(int i) => Mouse.X > (double) topleft.X && Mouse.Y > topleft.Y + i * (double) ActiveFont.LineHeight && MInput.Mouse.X < 960.0 && Mouse.Y < topleft.Y + (i + 1) * (double) ActiveFont.LineHeight;
    }
}
