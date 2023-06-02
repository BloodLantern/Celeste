using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class OshiroLobbyBell : Entity
    {
        private TalkComponent talker;

        public OshiroLobbyBell(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
            this.talker.Enabled = false;
        }

        private void OnTalk(Player player) => Audio.Play("event:/game/03_resort/deskbell_again", this.Position);

        public override void Update()
        {
            if (!this.talker.Enabled && this.Scene.Entities.FindFirst<NPC03_Oshiro_Lobby>() == null)
                this.talker.Enabled = true;
            base.Update();
        }
    }
}
