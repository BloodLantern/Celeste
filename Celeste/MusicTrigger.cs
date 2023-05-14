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
            Track = data.Attr("track");
            ResetOnLeave = data.Bool("resetOnLeave", true);
            Progress = data.Int("progress");
        }

        public override void OnEnter(Player player)
        {
            if (ResetOnLeave)
            {
                oldTrack = Audio.CurrentMusic;
            }

            Session session = SceneAs<Level>().Session;
            session.Audio.Music.Event = SFX.EventnameByHandle(Track);
            if (Progress != 0)
            {
                session.Audio.Music.Progress = Progress;
            }

            session.Audio.Apply();
        }

        public override void OnLeave(Player player)
        {
            if (!ResetOnLeave)
            {
                return;
            }

            Session session = SceneAs<Level>().Session;
            session.Audio.Music.Event = oldTrack;
            session.Audio.Apply();
        }
    }
}
