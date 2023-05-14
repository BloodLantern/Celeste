// Decompiled with JetBrains decompiler
// Type: Monocle.Spritesheet`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class Spritesheet<T> : Image
    {
        public int CurrentFrame;
        public float Rate = 1f;
        public bool UseRawDeltaTime;
        public Action<T> OnFinish;
        public Action<T> OnLoop;
        public Action<T> OnAnimate;
        private readonly Dictionary<T, Spritesheet<T>.Animation> animations;
        private Spritesheet<T>.Animation currentAnimation;
        private float animationTimer;
        private bool played;

        public Spritesheet(MTexture texture, int frameWidth, int frameHeight, int frameSep = 0)
            : base(texture, true)
        {
            SetFrames(texture, frameWidth, frameHeight, frameSep);
            animations = new Dictionary<T, Spritesheet<T>.Animation>();
        }

        public void SetFrames(MTexture texture, int frameWidth, int frameHeight, int frameSep = 0)
        {
            List<MTexture> mtextureList = new();
            int x = 0;
            int y = 0;
            while (y <= texture.Height - frameHeight)
            {
                for (; x <= texture.Width - frameWidth; x += frameWidth + frameSep)
                {
                    mtextureList.Add(texture.GetSubtexture(x, y, frameWidth, frameHeight));
                }

                y += frameHeight + frameSep;
                x = 0;
            }
            Frames = mtextureList.ToArray();
        }

        public override void Update()
        {
            if (!Animating || currentAnimation.Delay <= 0.0)
            {
                return;
            }

            if (UseRawDeltaTime)
            {
                animationTimer += Engine.RawDeltaTime * Rate;
            }
            else
            {
                animationTimer += Engine.DeltaTime * Rate;
            }

            if ((double)Math.Abs(animationTimer) < currentAnimation.Delay)
            {
                return;
            }

            CurrentAnimationFrame += Math.Sign(animationTimer);
            animationTimer -= Math.Sign(animationTimer) * currentAnimation.Delay;
            if (CurrentAnimationFrame < 0 || CurrentAnimationFrame >= currentAnimation.Frames.Length)
            {
                if (currentAnimation.Loop)
                {
                    CurrentAnimationFrame -= Math.Sign(CurrentAnimationFrame) * currentAnimation.Frames.Length;
                    CurrentFrame = currentAnimation.Frames[CurrentAnimationFrame];
                    OnAnimate?.Invoke(CurrentAnimationID);
                    if (OnLoop == null)
                    {
                        return;
                    }

                    OnLoop(CurrentAnimationID);
                }
                else
                {
                    CurrentAnimationFrame = CurrentAnimationFrame >= 0 ? currentAnimation.Frames.Length - 1 : 0;
                    Animating = false;
                    animationTimer = 0.0f;
                    if (OnFinish == null)
                    {
                        return;
                    }

                    OnFinish(CurrentAnimationID);
                }
            }
            else
            {
                CurrentFrame = currentAnimation.Frames[CurrentAnimationFrame];
                if (OnAnimate == null)
                {
                    return;
                }

                OnAnimate(CurrentAnimationID);
            }
        }

        public override void Render()
        {
            Texture = Frames[CurrentFrame];
            base.Render();
        }

        public void Add(T id, bool loop, float delay, params int[] frames)
        {
            animations[id] = new Spritesheet<T>.Animation()
            {
                Delay = delay,
                Frames = frames,
                Loop = loop
            };
        }

        public void Add(T id, float delay, params int[] frames)
        {
            Add(id, true, delay, frames);
        }

        public void Add(T id, int frame)
        {
            Add(id, false, 0.0f, frame);
        }

        public void ClearAnimations()
        {
            animations.Clear();
        }

        public bool IsPlaying(T id)
        {
            return played && (CurrentAnimationID == null ? id == null : CurrentAnimationID.Equals(id));
        }

        public void Play(T id, bool restart = false)
        {
            if (!(!IsPlaying(id) | restart))
            {
                return;
            }

            CurrentAnimationID = id;
            currentAnimation = animations[id];
            animationTimer = 0.0f;
            CurrentAnimationFrame = 0;
            played = true;
            Animating = currentAnimation.Frames.Length > 1;
            CurrentFrame = currentAnimation.Frames[0];
        }

        public void Reverse(T id, bool restart = false)
        {
            Play(id, restart);
            if (Rate <= 0.0)
            {
                return;
            }

            Rate *= -1f;
        }

        public void Stop()
        {
            Animating = false;
            played = false;
        }

        public MTexture[] Frames { get; private set; }

        public bool Animating { get; private set; }

        public T CurrentAnimationID { get; private set; }

        public int CurrentAnimationFrame { get; private set; }

        public override float Width => Frames.Length != 0 ? Frames[0].Width : 0.0f;

        public override float Height => Frames.Length != 0 ? Frames[0].Height : 0.0f;

        private struct Animation
        {
            public float Delay;
            public int[] Frames;
            public bool Loop;
        }
    }
}
