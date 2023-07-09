using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Monocle
{
    public class Sprite : Image
    {
        public float Rate = 1f;
        public bool UseRawDeltaTime;
        public Vector2? Justify;
        public Action<string> OnFinish;
        public Action<string> OnLoop;
        public Action<string> OnFrameChange;
        public Action<string> OnLastFrame;
        public Action<string, string> OnChange;
        private Atlas atlas;
        public string Path;
        private Dictionary<string, Animation> animations;
        private Animation currentAnimation;
        private float animationTimer;
        private int width;
        private int height;

        public Sprite(Atlas atlas, string path)
            : base(null, true)
        {
            this.atlas = atlas;
            Path = path;
            animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
            CurrentAnimationID = "";
        }

        public void Reset(Atlas atlas, string path)
        {
            this.atlas = atlas;
            Path = path;
            animations = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
            currentAnimation = null;
            CurrentAnimationID = "";
            OnFinish = null;
            OnLoop = null;
            OnFrameChange = null;
            OnChange = null;
            Animating = false;
        }

        public MTexture GetFrame(string animation, int frame) => animations[animation].Frames[frame];

        public Vector2 Center => new(Width / 2f, Height / 2f);

        public override void Update()
        {
            if (!Animating)
                return;
            if (UseRawDeltaTime)
                animationTimer += Engine.RawDeltaTime * Rate;
            else
                animationTimer += Engine.DeltaTime * Rate;
            if ((double) Math.Abs(animationTimer) < currentAnimation.Delay)
                return;
            CurrentAnimationFrame += Math.Sign(animationTimer);
            animationTimer -= Math.Sign(animationTimer) * currentAnimation.Delay;
            if (CurrentAnimationFrame < 0 || CurrentAnimationFrame >= currentAnimation.Frames.Length)
            {
                string currentAnimationId1 = CurrentAnimationID;
                if (OnLastFrame != null)
                    OnLastFrame(CurrentAnimationID);
                string currentAnimationId2 = CurrentAnimationID;
                if (!(currentAnimationId1 == currentAnimationId2))
                    return;
                if (currentAnimation.Goto != null)
                {
                    CurrentAnimationID = currentAnimation.Goto.Choose();
                    if (OnChange != null)
                        OnChange(LastAnimationID, CurrentAnimationID);
                    LastAnimationID = CurrentAnimationID;
                    currentAnimation = animations[LastAnimationID];
                    CurrentAnimationFrame = CurrentAnimationFrame >= 0 ? 0 : currentAnimation.Frames.Length - 1;
                    SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
                    if (OnLoop == null)
                        return;
                    OnLoop(CurrentAnimationID);
                }
                else
                {
                    CurrentAnimationFrame = CurrentAnimationFrame >= 0 ? currentAnimation.Frames.Length - 1 : 0;
                    Animating = false;
                    string currentAnimationId3 = CurrentAnimationID;
                    CurrentAnimationID = "";
                    currentAnimation = null;
                    animationTimer = 0.0f;
                    if (OnFinish == null)
                        return;
                    OnFinish(currentAnimationId3);
                }
            }
            else
                SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
        }

        private void SetFrame(MTexture texture)
        {
            if (texture == Texture)
                return;
            Texture = texture;
            if (width == 0)
                width = texture.Width;
            if (height == 0)
                height = texture.Height;
            if (Justify.HasValue)
                Origin = new Vector2(Texture.Width * Justify.Value.X, Texture.Height * Justify.Value.Y);
            if (OnFrameChange == null)
                return;
            OnFrameChange(CurrentAnimationID);
        }

        public void SetAnimationFrame(int frame)
        {
            animationTimer = 0.0f;
            CurrentAnimationFrame = frame % currentAnimation.Frames.Length;
            SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
        }

        public void AddLoop(string id, string path, float delay) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path),
            Goto = new Chooser<string>(id, 1f)
        };

        public void AddLoop(string id, string path, float delay, params int[] frames) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path, frames),
            Goto = new Chooser<string>(id, 1f)
        };

        public void AddLoop(string id, float delay, params MTexture[] frames) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = frames,
            Goto = new Chooser<string>(id, 1f)
        };

        public void Add(string id, string path) => animations[id] = new Animation()
        {
            Delay = 0.0f,
            Frames = GetFrames(path),
            Goto = null
        };

        public void Add(string id, string path, float delay) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path),
            Goto = null
        };

        public void Add(string id, string path, float delay, params int[] frames) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path, frames),
            Goto = null
        };

        public void Add(string id, string path, float delay, string into) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path),
            Goto = Chooser<string>.FromString<string>(into)
        };

        public void Add(string id, string path, float delay, Chooser<string> into) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path),
            Goto = into
        };

        public void Add(string id, string path, float delay, string into, params int[] frames) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = GetFrames(path, frames),
            Goto = Chooser<string>.FromString<string>(into)
        };

        public void Add(string id, float delay, string into, params MTexture[] frames) => animations[id] = new Animation()
        {
            Delay = delay,
            Frames = frames,
            Goto = Chooser<string>.FromString<string>(into)
        };

        public void Add(
            string id,
            string path,
            float delay,
            Chooser<string> into,
            params int[] frames)
        {
            animations[id] = new Animation()
            {
                Delay = delay,
                Frames = GetFrames(path, frames),
                Goto = into
            };
        }

        private MTexture[] GetFrames(string path, int[] frames = null)
        {
            MTexture[] frames1;
            if (frames == null || frames.Length == 0)
            {
                frames1 = atlas.GetAtlasSubtextures(Path + path).ToArray();
            }
            else
            {
                string key = Path + path;
                MTexture[] mtextureArray = new MTexture[frames.Length];
                for (int index = 0; index < frames.Length; ++index)
                    mtextureArray[index] = atlas.GetAtlasSubtexturesAt(key, frames[index]) ?? throw new Exception("Can't find sprite " + key + " with index " + frames[index]);
                frames1 = mtextureArray;
            }
            width = Math.Max(frames1[0].Width, width);
            height = Math.Max(frames1[0].Height, height);
            return frames1;
        }

        public void ClearAnimations() => animations.Clear();

        public void Play(string id, bool restart = false, bool randomizeFrame = false)
        {
            if (!(CurrentAnimationID != id | restart))
                return;
            OnChange?.Invoke(LastAnimationID, id);
            LastAnimationID = CurrentAnimationID = id;
            currentAnimation = animations[id];
            Animating = currentAnimation.Delay > 0.0;
            if (randomizeFrame)
            {
                animationTimer = Calc.Random.NextFloat(currentAnimation.Delay);
                CurrentAnimationFrame = Calc.Random.Next(currentAnimation.Frames.Length);
            }
            else
            {
                animationTimer = 0.0f;
                CurrentAnimationFrame = 0;
            }
            SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
        }

        public void PlayOffset(string id, float offset, bool restart = false)
        {
            if (!(CurrentAnimationID != id | restart))
                return;
            if (OnChange != null)
                OnChange(LastAnimationID, id);
            LastAnimationID = CurrentAnimationID = id;
            currentAnimation = animations[id];
            if (currentAnimation.Delay > 0.0)
            {
                Animating = true;
                float num = currentAnimation.Delay * currentAnimation.Frames.Length * offset;
                CurrentAnimationFrame = 0;
                for (; (double) num >= currentAnimation.Delay; num -= currentAnimation.Delay)
                    ++CurrentAnimationFrame;
                CurrentAnimationFrame %= currentAnimation.Frames.Length;
                animationTimer = num;
                SetFrame(currentAnimation.Frames[CurrentAnimationFrame]);
            }
            else
            {
                animationTimer = 0.0f;
                Animating = false;
                CurrentAnimationFrame = 0;
                SetFrame(currentAnimation.Frames[0]);
            }
        }

        public IEnumerator PlayRoutine(string id, bool restart = false)
        {
            Play(id, restart);
            return PlayUtil();
        }

        public IEnumerator ReverseRoutine(string id, bool restart = false)
        {
            Reverse(id, restart);
            return PlayUtil();
        }

        private IEnumerator PlayUtil()
        {
            while (Animating)
                yield return null;
        }

        public void Reverse(string id, bool restart = false)
        {
            Play(id, restart);
            if (Rate <= 0.0)
                return;
            Rate *= -1f;
        }

        public bool Has(string id) => id != null && animations.ContainsKey(id);

        public void Stop()
        {
            Animating = false;
            currentAnimation = null;
            CurrentAnimationID = "";
        }

        public bool Animating { get; private set; }

        public string CurrentAnimationID { get; private set; }

        public string LastAnimationID { get; private set; }

        public int CurrentAnimationFrame { get; private set; }

        public int CurrentAnimationTotalFrames => currentAnimation != null ? currentAnimation.Frames.Length : 0;

        public override float Width => width;

        public override float Height => height;

        internal Sprite()
            : base(null, true)
        {
        }

        internal Sprite CreateClone() => CloneInto(new Sprite());

        internal Sprite CloneInto(Sprite clone)
        {
            clone.Texture = Texture;
            clone.Position = Position;
            clone.Justify = Justify;
            clone.Origin = Origin;
            clone.animations = new Dictionary<string, Animation>(animations, StringComparer.OrdinalIgnoreCase);
            clone.currentAnimation = currentAnimation;
            clone.animationTimer = animationTimer;
            clone.width = width;
            clone.height = height;
            clone.Animating = Animating;
            clone.CurrentAnimationID = CurrentAnimationID;
            clone.LastAnimationID = LastAnimationID;
            clone.CurrentAnimationFrame = CurrentAnimationFrame;
            return clone;
        }

        public void DrawSubrect(Vector2 offset, Rectangle rectangle)
        {
            if (Texture == null)
                return;
            Rectangle relativeRect = Texture.GetRelativeRect(rectangle);
            Vector2 vector2 = new(-Math.Min(rectangle.X - Texture.DrawOffset.X, 0.0f), -Math.Min(rectangle.Y - Texture.DrawOffset.Y, 0.0f));
            Draw.SpriteBatch.Draw(Texture.Texture.Texture, RenderPosition + offset, new Rectangle?(relativeRect), Color, Rotation, Origin - vector2, Scale, Effects, 0.0f);
        }

        public void LogAnimations()
        {
            StringBuilder stringBuilder = new();
            foreach (KeyValuePair<string, Animation> animation1 in animations)
            {
                Animation animation2 = animation1.Value;
                stringBuilder.Append(animation1.Key);
                stringBuilder.Append("\n{\n\t");
                stringBuilder.Append(string.Join("\n\t", (object[]) animation2.Frames));
                stringBuilder.Append("\n}\n");
            }
            Calc.Log(stringBuilder.ToString());
        }

        private class Animation
        {
            public float Delay;
            public MTexture[] Frames;
            public Chooser<string> Goto;
        }
    }
}
