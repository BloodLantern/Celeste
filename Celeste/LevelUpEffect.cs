using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class LevelUpEffect : Entity
    {
        private Sprite sprite;

        public LevelUpEffect(Vector2 position)
            : base(position)
        {
            this.Depth = -1000000;
            Audio.Play("event:/game/06_reflection/hug_levelup_text_in", this.Position);
            this.Add((Component) (this.sprite = GFX.SpriteBank.Create("player_level_up")));
            this.sprite.OnLastFrame = (Action<string>) (anim => this.RemoveSelf());
            this.sprite.OnFrameChange = (Action<string>) (anim =>
            {
                if (this.sprite.CurrentAnimationFrame != 20)
                    return;
                Audio.Play("event:/game/06_reflection/hug_levelup_text_out");
            });
            this.sprite.Play("levelUp");
        }
    }
}
