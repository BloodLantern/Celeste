using Microsoft.Xna.Framework;

namespace Celeste
{
    public class AltMusicTrigger : Trigger
    {
        public string Track;
        public bool ResetOnLeave;

        public AltMusicTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Track = data.Attr("track");
            this.ResetOnLeave = data.Bool("resetOnLeave", true);
        }

        public override void OnEnter(Player player) => Audio.SetAltMusic(SFX.EventnameByHandle(this.Track));

        public override void OnLeave(Player player)
        {
            if (!this.ResetOnLeave)
                return;
            Audio.SetAltMusic((string) null);
        }
    }
}
