// Decompiled with JetBrains decompiler
// Type: Celeste.StarTrackSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class StarTrackSpinner : TrackSpinner
    {
        public static ParticleType[] P_Trail;
        public Sprite Sprite;
        private bool hasStarted;
        private int colorID;
        private bool trail;

        public StarTrackSpinner(EntityData data, Vector2 offset)
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
            if (!trail || !Scene.OnInterval(0.03f))
            {
                return;
            }

            SceneAs<Level>().ParticlesBG.Emit(StarTrackSpinner.P_Trail[colorID], 1, Position, Vector2.One * 3f);
        }

        public override void OnTrackStart()
        {
            ++colorID;
            colorID %= 3;
            Sprite.Play("spin" + colorID);
            if (hasStarted)
            {
                _ = Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
            }

            hasStarted = true;
            trail = true;
        }

        public override void OnTrackEnd()
        {
            trail = false;
        }
    }
}
