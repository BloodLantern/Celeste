// Decompiled with JetBrains decompiler
// Type: Celeste.OldSiteChaseMusicHandler
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
    public class OldSiteChaseMusicHandler : Entity
    {
        public OldSiteChaseMusicHandler()
        {
            Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
        }

        public override void Update()
        {
            base.Update();
            int num1 = 1150;
            int num2 = 2832;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !(Audio.CurrentMusic == "event:/music/lvl2/chase"))
            {
                return;
            }

            Audio.SetMusicParam("escape", (entity.X - num1) / (num2 - num1));
        }
    }
}
