// Decompiled with JetBrains decompiler
// Type: Celeste.LevelUpEffect
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class LevelUpEffect : Entity
    {
        private readonly Sprite sprite;

        public LevelUpEffect(Vector2 position)
            : base(position)
        {
            Depth = -1000000;
            _ = Audio.Play("event:/game/06_reflection/hug_levelup_text_in", Position);
            Add(sprite = GFX.SpriteBank.Create("player_level_up"));
            sprite.OnLastFrame = anim => RemoveSelf();
            sprite.OnFrameChange = anim =>
            {
                if (sprite.CurrentAnimationFrame != 20)
                {
                    return;
                }

                _ = Audio.Play("event:/game/06_reflection/hug_levelup_text_out");
            };
            sprite.Play("levelUp");
        }
    }
}
