// Decompiled with JetBrains decompiler
// Type: Celeste.AnimatedTilesBank
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class AnimatedTilesBank
    {
        public Dictionary<string, AnimatedTilesBank.Animation> AnimationsByName = new Dictionary<string, AnimatedTilesBank.Animation>();
        public List<AnimatedTilesBank.Animation> Animations = new List<AnimatedTilesBank.Animation>();

        public void Add(
            string name,
            float delay,
            Vector2 offset,
            Vector2 origin,
            List<MTexture> textures)
        {
            AnimatedTilesBank.Animation animation = new AnimatedTilesBank.Animation()
            {
                Name = name,
                Delay = delay,
                Offset = offset,
                Origin = origin,
                Frames = textures.ToArray()
            } with
            {
                ID = this.Animations.Count
            };
            this.Animations.Add(animation);
            this.AnimationsByName.Add(name, animation);
        }

        public struct Animation
        {
            public int ID;
            public string Name;
            public float Delay;
            public Vector2 Offset;
            public Vector2 Origin;
            public MTexture[] Frames;
        }
    }
}
