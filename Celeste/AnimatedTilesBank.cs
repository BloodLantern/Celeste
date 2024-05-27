using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class AnimatedTilesBank
    {
        public Dictionary<string, Animation> AnimationsByName = new();
        public List<Animation> Animations = new();

        public void Add(
            string name,
            float delay,
            Vector2 offset,
            Vector2 origin,
            List<MTexture> textures)
        {
            Animation animation = new Animation
                {
                Name = name,
                Delay = delay,
                Offset = offset,
                Origin = origin,
                Frames = textures.ToArray()
            } with
            {
                ID = Animations.Count
            };
            Animations.Add(animation);
            AnimationsByName.Add(name, animation);
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
