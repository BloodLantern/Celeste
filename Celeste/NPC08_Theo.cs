// Decompiled with JetBrains decompiler
// Type: Celeste.NPC08_Theo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC08_Theo : NPC
    {
        public NPC08_Theo(EntityData data, Vector2 position)
            : base(data.Position + position)
        {
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Scale.X = -1f;
            Sprite.Play("idle");
            IdleAnim = "idle";
            MoveAnim = "walk";
            Maxspeed = 30f;
        }
    }
}
