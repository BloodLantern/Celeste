// Decompiled with JetBrains decompiler
// Type: Celeste.MusicTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
    public class MusicTrigger : Trigger
    {
        public string Track;
        public bool SetInSession;
        public bool ResetOnLeave;
        public int Progress;
        private string oldTrack;

        public MusicTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            this.Track = data.Attr("track");
            this.ResetOnLeave = data.Bool("resetOnLeave", true);
            this.Progress = data.Int("progress");
        }

        public override void OnEnter(Player player)
        {
            if (this.ResetOnLeave)
                this.oldTrack = Audio.CurrentMusic;
            Session session = this.SceneAs<Level>().Session;
            session.Audio.Music.Event = SFX.EventnameByHandle(this.Track);
            if (this.Progress != 0)
                session.Audio.Music.Progress = this.Progress;
            session.Audio.Apply();
        }

        public override void OnLeave(Player player)
        {
            if (!this.ResetOnLeave)
                return;
            Session session = this.SceneAs<Level>().Session;
            session.Audio.Music.Event = this.oldTrack;
            session.Audio.Apply();
        }
    }
}
