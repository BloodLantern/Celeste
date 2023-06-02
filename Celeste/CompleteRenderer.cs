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
        private Atlas atlas;
        private XmlElement xml;
        private float fadeAlpha = 1f;
        private Coroutine routine;
        private Vector2 controlScroll;
        private float controlMult;
        public float SlideDuration = 1.5f;
        public List<CompleteRenderer.Layer> Layers = new List<CompleteRenderer.Layer>();
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
                    this.StartScroll = xml["start"].Position();
                if (xml["center"] != null)
                    this.CenterScroll = xml["center"].Position();
                if (xml["offset"] != null)
                    this.Offset = xml["offset"].Position();
                foreach (object obj in (XmlNode) xml["layers"])
                {
                    if (obj is XmlElement)
                    {
                        XmlElement xml1 = obj as XmlElement;
                        if (xml1.Name == "layer")
                            this.Layers.Add((CompleteRenderer.Layer) new CompleteRenderer.ImageLayer(this.Offset, atlas, xml1));
                        else if (xml1.Name == "ui")
                        {
                            this.HasUI = true;
                            this.Layers.Add((CompleteRenderer.Layer) new CompleteRenderer.UILayer(this, xml1));
                        }
                    }
                }
            }
            this.Scroll = this.StartScroll;
            this.routine = new Coroutine(this.SlideRoutine(delay, onDoneSlide));
        }

        public void Dispose()
        {
            if (this.atlas == null)
                return;
            this.atlas.Dispose();
        }

        private IEnumerator SlideRoutine(float delay, Action onDoneSlide)
        {
            yield return (object) delay;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime / this.SlideDuration)
            {
                yield return (object) null;
                this.Scroll = Vector2.Lerp(this.StartScroll, this.CenterScroll, Ease.SineOut(p));
                this.fadeAlpha = Calc.LerpClamp(1f, 0.0f, p * 2f);
            }
            this.Scroll = this.CenterScroll;
            this.fadeAlpha = 0.0f;
            yield return (object) 0.2f;
            if (onDoneSlide != null)
                onDoneSlide();
            while (true)
            {
                this.controlMult = Calc.Approach(this.controlMult, 1f, 5f * Engine.DeltaTime);
                yield return (object) null;
            }
        }

        public override void Update(Scene scene)
        {
            Vector2 target = Input.Aim.Value + Input.MountainAim.Value;
            if ((double) target.Length() > 1.0)
                target.Normalize();
            target *= 200f;
            this.controlScroll = Calc.Approach(this.controlScroll, target, 600f * Engine.DeltaTime);
            foreach (CompleteRenderer.Layer layer in this.Layers)
                layer.Update(scene);
            this.routine.Update();
        }

        public override void RenderContent(Scene scene)
        {
            HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
            foreach (CompleteRenderer.Layer layer in this.Layers)
                layer.Render(-this.Scroll - this.controlScroll * this.controlMult);
            if (this.RenderPostUI != null)
                this.RenderPostUI();
            if ((double) this.fadeAlpha > 0.0)
                Draw.Rect(-10f, -10f, (float) (Engine.Width + 20), (float) (Engine.Height + 20), Color.Black * this.fadeAlpha);
            HiresRenderer.EndRender();
        }

        public abstract class Layer
        {
            public Vector2 Position;
            public Vector2 ScrollFactor;

            public Layer(XmlElement xml)
            {
                this.Position = xml.Position(Vector2.Zero);
                if (xml.HasAttr("scroll"))
                {
                    this.ScrollFactor.X = this.ScrollFactor.Y = xml.AttrFloat("scroll");
                }
                else
                {
                    this.ScrollFactor.X = xml.AttrFloat("scrollX", 0.0f);
                    this.ScrollFactor.Y = xml.AttrFloat("scrollY", 0.0f);
                }
            }

            public virtual void Update(Scene scene)
            {
            }

            public abstract void Render(Vector2 scroll);

            public Vector2 GetScrollPosition(Vector2 scroll)
            {
                Vector2 position = this.Position;
                if (this.ScrollFactor != Vector2.Zero)
                {
                    position.X = MathHelper.Lerp(this.Position.X, this.Position.X + scroll.X, this.ScrollFactor.X);
                    position.Y = MathHelper.Lerp(this.Position.Y, this.Position.Y + scroll.Y, this.ScrollFactor.Y);
                }
                return position;
            }
        }

        public class UILayer : CompleteRenderer.Layer
        {
            private CompleteRenderer renderer;

            public UILayer(CompleteRenderer renderer, XmlElement xml)
                : base(xml)
            {
                this.renderer = renderer;
            }

            public override void Render(Vector2 scroll)
            {
                if (this.renderer.RenderUI == null)
                    return;
                this.renderer.RenderUI(scroll);
            }
        }

        public class ImageLayer : CompleteRenderer.Layer
        {
            public List<MTexture> Images = new List<MTexture>();
            public float Frame;
            public float FrameRate;
            public float Alpha;
            public Vector2 Offset;
            public Vector2 Speed;
            public float Scale;

            public ImageLayer(Vector2 offset, Atlas atlas, XmlElement xml)
                : base(xml)
            {
                this.Position = this.Position + offset;
                string str = xml.Attr("img");
                char[] chArray = new char[1]{ ',' };
                foreach (string id in str.Split(chArray))
                {
                    if (atlas.Has(id))
                        this.Images.Add(atlas[id]);
                    else
                        this.Images.Add((MTexture) null);
                }
                this.FrameRate = xml.AttrFloat("fps", 6f);
                this.Alpha = xml.AttrFloat("alpha", 1f);
                this.Speed = new Vector2(xml.AttrFloat("speedx", 0.0f), xml.AttrFloat("speedy", 0.0f));
                this.Scale = xml.AttrFloat("scale", 1f);
            }

            public override void Update(Scene scene)
            {
                this.Frame += Engine.DeltaTime * this.FrameRate;
                this.Offset += this.Speed * Engine.DeltaTime;
            }

            public override void Render(Vector2 scroll)
            {
                Vector2 position = this.GetScrollPosition(scroll).Floor();
                MTexture image = this.Images[(int) ((double) this.Frame % (double) this.Images.Count)];
                if (image == null)
                    return;
                bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
                if (flag)
                {
                    position.X = (float) (1920.0 - (double) position.X - (double) image.DrawOffset.X * (double) this.Scale - (double) image.Texture.Texture.Width * (double) this.Scale);
                    position.Y += image.DrawOffset.Y * this.Scale;
                }
                else
                    position += image.DrawOffset * this.Scale;
                Rectangle rectangle = image.ClipRect;
                int num = (double) this.Offset.X != 0.0 ? 1 : ((double) this.Offset.Y != 0.0 ? 1 : 0);
                if (num != 0)
                {
                    rectangle = new Rectangle((int) (-(double) this.Offset.X / (double) this.Scale) + 1, (int) (-(double) this.Offset.Y / (double) this.Scale) + 1, image.ClipRect.Width - 2, image.ClipRect.Height - 2);
                    HiresRenderer.EndRender();
                    HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearWrap);
                }
                Draw.SpriteBatch.Draw(image.Texture.Texture, position, new Rectangle?(rectangle), Color.White * this.Alpha, 0.0f, Vector2.Zero, this.Scale, flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.0f);
                if (num == 0)
                    return;
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.LinearClamp);
            }
        }
    }
}
