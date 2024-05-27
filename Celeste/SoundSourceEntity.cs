using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class SoundSourceEntity : Entity
    {
        private string eventName;
        private SoundSource sfx;

        public SoundSourceEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = (int) Tags.TransitionUpdate;
            Add(sfx = new SoundSource());
            eventName = SFX.EventnameByHandle(data.Attr("sound"));
            Visible = true;
            Depth = -8500;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            sfx.Play(eventName);
        }
    }
}
