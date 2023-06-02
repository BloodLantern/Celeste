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
            this.Tag = (int) Tags.TransitionUpdate;
            this.Add((Component) (this.sfx = new SoundSource()));
            this.eventName = SFX.EventnameByHandle(data.Attr("sound"));
            this.Visible = true;
            this.Depth = -8500;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.sfx.Play(this.eventName);
        }
    }
}
