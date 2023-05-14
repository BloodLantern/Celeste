// Decompiled with JetBrains decompiler
// Type: Celeste.GrannyLaughSfx
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
    public class GrannyLaughSfx : Component
    {
        public bool FirstPlay;
        private readonly Sprite sprite;
        private bool ready = true;

        public GrannyLaughSfx(Sprite sprite)
            : base(true, false)
        {
            this.sprite = sprite;
        }

        public override void Update()
        {
            if (sprite.CurrentAnimationID == "laugh" && sprite.CurrentAnimationFrame == 0 && ready)
            {
                _ = FirstPlay
                    ? Audio.Play("event:/char/granny/laugh_firstphrase", Entity.Position)
                    : Audio.Play("event:/char/granny/laugh_oneha", Entity.Position);

                ready = false;
            }
            if (FirstPlay || (!(sprite.CurrentAnimationID != "laugh") && sprite.CurrentAnimationFrame <= 0))
            {
                return;
            }

            ready = true;
        }
    }
}
