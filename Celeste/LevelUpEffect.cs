using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class LevelUpEffect : Entity
    {
        private Sprite sprite;

        public LevelUpEffect(Vector2 position)
            : base(position)
        {
            Depth = -1000000;
            Audio.Play("event:/game/06_reflection/hug_levelup_text_in", Position);
            Add(sprite = GFX.SpriteBank.Create("player_level_up"));
            sprite.OnLastFrame = anim => RemoveSelf();
            sprite.OnFrameChange = anim =>
            {
                if (sprite.CurrentAnimationFrame != 20)
                    return;
                Audio.Play("event:/game/06_reflection/hug_levelup_text_out");
            };
            sprite.Play("levelUp");
        }
    }
}
