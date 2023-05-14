// Decompiled with JetBrains decompiler
// Type: Celeste.CreditsTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CreditsTrigger : Trigger
    {
        public string Event;

        public CreditsTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Event = data.Attr("event");
        }

        public override void OnEnter(Player player)
        {
            Triggered = true;
            if (CS07_Credits.Instance == null)
            {
                return;
            }

            CS07_Credits.Instance.Event = Event;
        }
    }
}
