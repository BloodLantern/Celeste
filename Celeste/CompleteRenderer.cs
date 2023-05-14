// Decompiled with JetBrains decompiler
// Type: Celeste.CompleteRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    public class CompleteRenderer : HiresRenderer, IDisposable
    {
        private const float ScrollRange = 200f;
        private const float ScrollSpeed = 600f;
        private readonly Atlas atlas;
        private readonly XmlElement xml;
        private float fadeAlpha = 1f;
        private readonly Coroutine routine;
        private Vector2 controlScroll;
        private float controlMult;
        public float SlideDuration = 1.5f;
        public List<CompleteRenderer.Layer> Layers = new();
        public Vector2 Scroll;
        public Vector2 StartScroll;
        public Vector2 CenterScroll;
        public Vector2 Offset;
        public float Scale;
        public Action<Vector2> RenderUI;
        public Action RenderPostUI;

        public bool HasUI { get; private set; }

        public CompleteRenderer(XmlElement xml, Atlas atlas, float delay, Action onDoneSlide = null)
        {
            this.atlas = atlas;
            this.xml = xml;
            if (xml != null)
            {
                if (xml["start"] != null)
                {
                    StartScroll = xml["start"].Position();
                }

                if (xml["center"] != null)
                {
                    CenterScroll = xml["center"].Position();
                }

                if (xml["offset"] != null)
                {
                    Offset = xml["offset"].Position();
                }

                foreach (object obj in (XmlNode)xml["layers"])
                {
                    if (obj is XmlElement)
                    {
                        XmlElement xml1 = obj as XmlElement;
                        if (xml1.Name == "layer")
                        {
                            Layers.Add(new CompleteRenderer.ImageLayer(Offset, atlas, xml1));
                        }
                        else if (xml1.Name == "ui")
                        {
                            HasUI = true;
                            Layers.Add(new CompleteRenderer.UILayer(this, xml1));
                        }
                    }
                }
            }
            Scroll = StartScroll;
            routine = new Coroutine(SlideRoutine(delay, onDoneSlide));
        }

        public void Dispose()
        {
            if (atlas == null)
            {
                return;
            }

            atlas.Dispose();
        }

        private IEnumerator SlideRoutine(float delay, Action onDoneSlide)
        {
            yield return delay;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / SlideDuration)
            {
                yield return null;
                Scroll = Vector2.Lerp(StartScroll, CenterScroll, Ease.SineOut(p));
                fadeAlpha = Calc.LerpClamp(1f, 0.0f, p * 2f);
            }
            Scroll = CenterScroll;
            fadeAlpha = 0.0f;
            yield return 0.2f;
            onDoneSlide?.Invoke();
            while (true)
            {
                controlMult = Calc.Approach(controlMult, 1f, 5f * Engine.DeltaTime);
                yield return null;
            }
        }

        public override void Update(Scene scene)
        {
            Vector2 target = Input.Aim.Value + Input.MountainAim.Value;
            if ((double)target.Length() > 1.0)
            {
                target.Normalize();
            }

            target *= 200f;
            controlScroll = Calc.Approach(controlScroll, target, 600f * Engine.DeltaTime);
            foreach (CompleteRenderer.Layer layer in Layers)
            {
                layer.Update(scene);
            }

            routine.Update();
        }

        public override void RenderContent(Scene scene)
        {
            HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
            foreach (CompleteRenderer.Layer layer in Layers)
            {
                layer.Render(-Scroll - (controlScroll * controlMult));
            }

            RenderPostUI?.Invoke();
            if (fadeAlpha > 0.0)
            {
                Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, Color.Black * fadeAlpha);
            }

            HiresRenderer.EndRender();
        }

        public abstract class Layer
        {
            public Vector2 Position;
            public Vector2 ScrollFactor;

            public Layer(XmlElement xml)
            {
                Position = xml.Position(Vector2.Zero);
                if (xml.HasAttr("scroll"))
                {
                    ScrollFactor.X = ScrollFactor.Y = xml.AttrFloat("scroll");
                }
                else
                {
                    ScrollFactor.X = xml.AttrFloat("scrollX", 0.0f);
                    ScrollFactor.Y = xml.AttrFloat("scrollY", 0.0f);
                }
            }

            public virtual void Update(Scene scene)
            {
            }

            public abstract void Render(Vector2 scroll);

            public Vector2 GetScrollPosition(Vector2 scroll)
            {
                Vector2 position = Position;
                if (ScrollFactor != Vector2.Zero)
                {
                    position.X = MathHelper.Lerp(Position.X, Position.X + scroll.X, ScrollFactor.X);
                    position.Y = MathHelper.Lerp(Position.Y, Position.Y + scroll.Y, ScrollFactor.Y);
                }
                return position;
            }
        }

        public class UILayer : CompleteRenderer.Layer
        {
            private readonly CompleteRenderer renderer;

            public UILayer(CompleteRenderer renderer, XmlElement xml)
                : base(xml)
            {
                this.renderer = renderer;
            }

            public override void Render(Vector2 scroll)
            {
                if (renderer.RenderUI == null)
                {
                    return;
                }

                renderer.RenderUI(scroll);
            }
        }

        public class ImageLayer : CompleteRenderer.Layer
        {
            public List<MTexture> Images = new();
            public float Frame;
            public float FrameRate;
            public float Alpha;
            public Vector2 Offset;
            public Vector2 Speed;
            public float Scale;

            public ImageLayer(Vector2 offset, Atlas atlas, XmlElement xml)
                : base(xml)
            {
                Position += offset;
                string str = xml.Attr("img");
                char[] chArray = new char[1] { ',' };
                foreach (string id in str.Split(chArray))
                {
                    if (atlas.Has(id))
                    {
                        Images.Add(atlas[id]);
                    }
                    else
                    {
                        Images.Add(null);
                    }
                }
                FrameRate = xml.AttrFloat("fps", 6f);
                Alpha = xml.AttrFloat("alpha", 1f);
                Speed = new Vector2(xml.AttrFloat("speedx", 0.0f), xml.AttrFloat("speedy", 0.0f));
                Scale = xml.AttrFloat("scale", 1f);
            }

            public override void Update(Scene scene)
            {
                Frame += Engine.DeltaTime * FrameRate;
                Offset += Speed * Engine.DeltaTime;
            }

            public override void Render(Vector2 scroll)
            {
                Vector2 position = GetScrollPosition(scroll).Floor();
                MTexture image = Images[(int)(Frame % (double)Images.Count)];
                if (image == null)
                {
                    return;
                }

                bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
                if (flag)
                {
                    position.X = (float)(1920.0 - position.X - (image.DrawOffset.X * (double)Scale) - (image.Texture.Texture.Width * (double)Scale));
                    position.Y += image.DrawOffset.Y * Scale;
                }
                else
                {
                    position += image.DrawOffset * Scale;
                }

                Rectangle rectangle = image.ClipRect;
                int num = Offset.X != 0.0 ? 1 : (Offset.Y != 0.0 ? 1 : 0);
                if (num != 0)
                {
                    rectangle = new Rectangle((int)(-(double)Offset.X / Scale) + 1, (int)(-(double)Offset.Y / Scale) + 1, image.ClipRect.Width - 2, image.ClipRect.Height - 2);
                    HiresRenderer.EndRender();
                    HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearWrap);
                }
                Draw.SpriteBatch.Draw(image.Texture.Texture, position, new Rectangle?(rectangle), Color.White * Alpha, 0.0f, Vector2.Zero, Scale, flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                if (num == 0)
                {
                    return;
                }

                HiresRenderer.EndRender();
                HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
            }
        }
    }
}
