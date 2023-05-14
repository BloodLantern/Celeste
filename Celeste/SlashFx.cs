// Decompiled with JetBrains decompiler
// Type: Celeste.SlashFx
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    [Pooled]
    public class SlashFx : Entity
    {
        public Sprite Sprite;
        public Vector2 Direction;

        public SlashFx()
        {
            Add(Sprite = new Sprite(GFX.Game, "effects/slash/"));
            Sprite.Add("play", "", 0.1f, 0, 1, 2, 3);
            _ = Sprite.CenterOrigin();
            Sprite.OnFinish = anim => RemoveSelf();
            Depth = -100;
        }

        public override void Update()
        {
            Position += Direction * 8f * Engine.DeltaTime;
            base.Update();
        }

        public static SlashFx Burst(Vector2 position, float direction)
        {
            Scene scene = Engine.Scene;
            SlashFx slashFx1 = Engine.Pooler.Create<SlashFx>();
            SlashFx slashFx2 = slashFx1;
            scene.Add(slashFx2);
            slashFx1.Position = position;
            slashFx1.Direction = Calc.AngleToVector(direction, 1f);
            slashFx1.Sprite.Play("play", true);
            slashFx1.Sprite.Scale = Vector2.One;
            slashFx1.Sprite.Rotation = 0.0f;
            if ((double)Math.Abs(direction - 3.14159274f) > 0.0099999997764825821)
            {
                slashFx1.Sprite.Rotation = direction;
            }

            slashFx1.Visible = slashFx1.Active = true;
            return slashFx1;
        }
    }
}
