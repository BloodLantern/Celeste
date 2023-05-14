// Decompiled with JetBrains decompiler
// Type: Celeste.OshiroTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OshiroTrigger : Trigger
    {
        public bool State;

        public OshiroTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            State = data.Bool("state", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (State)
            {
                Level level = SceneAs<Level>();
                Scene.Add(new AngryOshiro(new Vector2(level.Bounds.Left - 32, level.Bounds.Top + (level.Bounds.Height / 2)), false));
                RemoveSelf();
            }
            else
            {
                Scene.Tracker.GetEntity<AngryOshiro>()?.Leave();
                RemoveSelf();
            }
        }
    }
}
