// Decompiled with JetBrains decompiler
// Type: Celeste.TemplePortalTorch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TemplePortalTorch : Entity
    {
        private readonly Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private SoundSource loopSfx;

        public TemplePortalTorch(Vector2 pos)
            : base(pos)
        {
            Add(sprite = new Sprite(GFX.Game, "objects/temple/portal/portaltorch"));
            sprite.AddLoop("idle", "", 0.0f, new int[1]);
            sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
            sprite.Play("idle");
            sprite.Origin = new Vector2(32f, 64f);
            Depth = 8999;
        }

        public void Light(int count = 0)
        {
            sprite.Play("lit");
            Add(bloom = new BloomPoint(1f, 16f));
            Add(light = new VertexLight(Color.LightSeaGreen, 0.0f, 32, 128));
            _ = Audio.Play(count == 0 ? "event:/game/05_mirror_temple/mainmirror_torch_lit_1" : "event:/game/05_mirror_temple/mainmirror_torch_lit_2", Position);
            Add(loopSfx = new SoundSource());
            _ = loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
        }

        public override void Update()
        {
            base.Update();
            if (bloom != null && bloom.Alpha > 0.5)
            {
                bloom.Alpha -= Engine.DeltaTime;
            }

            if (light == null || light.Alpha >= 1.0)
            {
                return;
            }

            light.Alpha = Calc.Approach(light.Alpha, 1f, Engine.DeltaTime);
        }
    }
}
