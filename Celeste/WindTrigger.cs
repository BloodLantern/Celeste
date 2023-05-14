// Decompiled with JetBrains decompiler
// Type: Celeste.WindTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class WindTrigger : Trigger
    {
        public WindController.Patterns Pattern;

        public WindTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Pattern = data.Enum<WindController.Patterns>("pattern");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            WindController first = Scene.Entities.FindFirst<WindController>();
            if (first == null)
            {
                Scene.Add(new WindController(Pattern));
            }
            else
            {
                first.SetPattern(Pattern);
            }
        }
    }
}
