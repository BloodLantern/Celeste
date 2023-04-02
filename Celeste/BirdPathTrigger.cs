// Decompiled with JetBrains decompiler
// Type: Celeste.BirdPathTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BirdPathTrigger : Trigger
    {
        private BirdPath bird;
        private bool triggered;

        public BirdPathTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            BirdPath first = Scene.Entities.FindFirst<BirdPath>();
            if (first != null)
            {
                bird = first;
                bird.WaitForTrigger();
            }
            else
            {
                RemoveSelf();
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (triggered)
            {
                return;
            }

            bird.Trigger();
            triggered = true;
        }
    }
}
