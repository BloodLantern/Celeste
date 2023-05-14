// Decompiled with JetBrains decompiler
// Type: Celeste.SoundSourceEntity
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class SoundSourceEntity : Entity
    {
        private readonly string eventName;
        private readonly SoundSource sfx;

        public SoundSourceEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = (int)Tags.TransitionUpdate;
            Add(sfx = new SoundSource());
            eventName = SFX.EventnameByHandle(data.Attr("sound"));
            Visible = true;
            Depth = -8500;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _ = sfx.Play(eventName);
        }
    }
}
