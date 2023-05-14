// Decompiled with JetBrains decompiler
// Type: Celeste.StarRotateSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class StarRotateSpinner : RotateSpinner
    {
        public Sprite Sprite;
        private int colorID;

        public StarRotateSpinner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("moonBlade"));
            colorID = Calc.Random.Choose<int>(0, 1, 2);
            Sprite.Play("idle" + colorID);
            Depth = -50;
            Add(new MirrorReflection());
        }

        public override void Update()
        {
            base.Update();
            if (Moving && Scene.OnInterval(0.03f))
            {
                SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[colorID], 1, Position, Vector2.One * 3f);
            }

            if (!Scene.OnInterval(0.8f))
            {
                return;
            }

            ++colorID;
            colorID %= 3;
            Sprite.Play("spin" + colorID);
        }
    }
}
