// Decompiled with JetBrains decompiler
// Type: Celeste._3dSoundTest
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class _3dSoundTest : Entity
    {
        public SoundSource sfx;

        public _3dSoundTest(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sfx = new SoundSource());
            sfx.Play("event:/3d_testing");
        }

        public override void Render()
        {
            Draw.Rect(X - 8f, Y - 8f, 16f, 16f, Color.Yellow);
            Camera camera = (Scene as Level).Camera;
            Draw.HollowRect(X - 320f, camera.Y, 640f, 180f, Color.Red);
            Draw.HollowRect(X - 160f, camera.Y, 320f, 180f, Color.Yellow);
            Draw.HollowRect(X - 160f - 320f, camera.Y, 960f, 180f, Color.Yellow);
        }
    }
}
