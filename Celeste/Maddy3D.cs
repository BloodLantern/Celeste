// Decompiled with JetBrains decompiler
// Type: Celeste.Maddy3D
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Maddy3D : Entity
    {
        public MountainRenderer Renderer;
        public Billboard Image;
        public Wiggler Wiggler;
        public Vector2 Scale = Vector2.One;
        public new Vector3 Position;
        public bool Show = true;
        public bool Disabled;
        private List<MTexture> frames;
        private float frame;
        private float frameSpeed;
        private float alpha = 1f;
        private int hideDown;
        private bool running;

        public Maddy3D(MountainRenderer renderer)
        {
            Renderer = renderer;
            Add(Image = new Billboard(null, Vector3.Zero));
            Image.BeforeRender = () =>
            {
                if (Disabled)
                {
                    Image.Color = Color.Transparent;
                }
                else
                {
                    Image.Position = Position + (hideDown * Vector3.Up * (1f - Ease.CubeOut(alpha)) * 0.25f);
                    Image.Scale = Scale + (Vector2.One * Wiggler.Value * Scale * 0.2f);
                    Image.Scale *= (Renderer.Model.Camera.Position - Position).Length() / 20f;
                    Image.Color = Color.White * alpha;
                }
            };
            Add(Wiggler = Wiggler.Create(0.5f, 3f));
            Running(renderer.Area < 7);
        }

        public void Running(bool backpack = true)
        {
            running = true;
            Show = true;
            hideDown = -1;
            SetRunAnim();
            frameSpeed = 8f;
            frame = 0.0f;
            Image.Size = new Vector2(frames[0].ClipRect.Width, frames[0].ClipRect.Height) / frames[0].ClipRect.Width;
        }

        public void Falling()
        {
            running = false;
            Show = true;
            hideDown = -1;
            frames = MTN.Mountain.GetAtlasSubtextures("marker/Fall");
            frameSpeed = 2f;
            frame = 0.0f;
            Image.Size = new Vector2(frames[0].ClipRect.Width, frames[0].ClipRect.Height) / frames[0].ClipRect.Width;
        }

        public void Hide(bool down = true)
        {
            running = false;
            Show = false;
            hideDown = down ? -1 : 1;
        }

        private void SetRunAnim()
        {
            frames = Renderer.Area < 7
                ? MTN.Mountain.GetAtlasSubtextures("marker/runBackpack")
                : MTN.Mountain.GetAtlasSubtextures("marker/runNoBackpack");
        }

        public override void Update()
        {
            base.Update();
            if (running)
            {
                SetRunAnim();
            }

            if (frames != null && frames.Count > 0)
            {
                frame += Engine.DeltaTime * frameSpeed;
                if (frame >= (double)frames.Count)
                {
                    frame -= frames.Count;
                }

                Image.Texture = frames[(int)frame];
            }
            alpha = Calc.Approach(alpha, Show ? 1f : 0.0f, Engine.DeltaTime * 4f);
        }
    }
}
