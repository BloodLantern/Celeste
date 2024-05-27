using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class PrologueEndingText : Entity
    {
        private FancyText.Text text;

        public PrologueEndingText(bool instant)
        {
            Tag = (int) Tags.HUD;
            text = FancyText.Parse(Dialog.Clean("CH0_END"), 960, 4, 0.0f);
            Add(new Coroutine(Routine(instant)));
        }

        private IEnumerator Routine(bool instant)
        {
            if (!instant)
                yield return 4f;
            for (int i = 0; i < text.Count; ++i)
            {
                if (text[i] is FancyText.Char c)
                {
                    while ((c.Fade += Engine.DeltaTime * 20f) < 1.0)
                        yield return null;
                    c.Fade = 1f;
                    c = null;
                }
            }
        }

        public override void Render() => text.Draw(Position, new Vector2(0.5f, 0.5f), Vector2.One, 1f);
    }
}
