// Decompiled with JetBrains decompiler
// Type: Celeste.OshiroLobbyBell
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class OshiroLobbyBell : Entity
    {
        private readonly TalkComponent talker;

        public OshiroLobbyBell(Vector2 position)
            : base(position)
        {
            Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0.0f, -24f), new Action<Player>(OnTalk)));
            talker.Enabled = false;
        }

        private void OnTalk(Player player)
        {
            _ = Audio.Play("event:/game/03_resort/deskbell_again", Position);
        }

        public override void Update()
        {
            if (!talker.Enabled && Scene.Entities.FindFirst<NPC03_Oshiro_Lobby>() == null)
            {
                talker.Enabled = true;
            }

            base.Update();
        }
    }
}
