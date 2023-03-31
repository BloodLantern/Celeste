// Decompiled with JetBrains decompiler
// Type: Celeste.AmbienceParamTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class AmbienceParamTrigger : Trigger
    {
        public string Parameter;
        public float From;
        public float To;
        public Trigger.PositionModes PositionMode;

        public AmbienceParamTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Parameter = data.Attr("parameter");
            From = data.Float("from");
            To = data.Float("to");
            PositionMode = data.Enum<Trigger.PositionModes>("direction");
        }

        public override void OnStay(Player player)
        {
            float num = Calc.ClampedMap(GetPositionLerp(player, PositionMode), 0.0f, 1f, From, To);
            Level scene = Scene as Level;
            _ = scene.Session.Audio.Ambience.Param(Parameter, num);
            scene.Session.Audio.Apply();
        }
    }
}
