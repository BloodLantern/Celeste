// Decompiled with JetBrains decompiler
// Type: Celeste.SoundEmitter
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class SoundEmitter : Entity
    {
        public static SoundEmitter Play(string sfx)
        {
            SoundEmitter soundEmitter = new(sfx);
            Engine.Scene.Add(soundEmitter);
            return soundEmitter;
        }

        public static SoundEmitter Play(string sfx, Entity follow, Vector2? offset = null)
        {
            SoundEmitter soundEmitter = new(sfx, follow, offset.HasValue ? offset.Value : Vector2.Zero);
            Engine.Scene.Add(soundEmitter);
            return soundEmitter;
        }

        public SoundSource Source { get; private set; }

        private SoundEmitter(string sfx)
        {
            Add(Source = new SoundSource());
            _ = Source.Play(sfx);
            Source.DisposeOnTransition = false;
            Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
            Add(new LevelEndingHook(new Action(OnLevelEnding)));
        }

        private SoundEmitter(string sfx, Entity follow, Vector2 offset)
        {
            Add(Source = new SoundSource());
            Position = follow.Position + offset;
            _ = Source.Play(sfx);
            Source.DisposeOnTransition = false;
            Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
            Add(new LevelEndingHook(new Action(OnLevelEnding)));
        }

        public override void Update()
        {
            base.Update();
            if (Source.Playing)
            {
                return;
            }

            RemoveSelf();
        }

        private void OnLevelEnding()
        {
            _ = Source.Stop();
        }
    }
}
