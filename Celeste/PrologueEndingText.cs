// Decompiled with JetBrains decompiler
// Type: Celeste.PrologueEndingText
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            this.Tag = (int) Tags.HUD;
            this.text = FancyText.Parse(Dialog.Clean("CH0_END"), 960, 4, 0.0f);
            this.Add((Component) new Coroutine(this.Routine(instant)));
        }

        private IEnumerator Routine(bool instant)
        {
            if (!instant)
                yield return (object) 4f;
            for (int i = 0; i < this.text.Count; ++i)
            {
                if (this.text[i] is FancyText.Char c)
                {
                    while ((double) (c.Fade += Engine.DeltaTime * 20f) < 1.0)
                        yield return (object) null;
                    c.Fade = 1f;
                    c = (FancyText.Char) null;
                }
            }
        }

        public override void Render() => this.text.Draw(this.Position, new Vector2(0.5f, 0.5f), Vector2.One, 1f);
    }
}
