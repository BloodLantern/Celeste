﻿using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class NPC10_Gravestone : NPC
    {
        private const string Flag = "gravestone";
        private Player player;
        private Vector2 boostTarget;
        private TalkComponent talk;

        public NPC10_Gravestone(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.boostTarget = data.FirstNodeNullable(new Vector2?(offset)) ?? Vector2.Zero;
            this.Add((Component) (this.talk = new TalkComponent(new Rectangle(-24, -8, 32, 8), new Vector2(-0.5f, -20f), new Action<Player>(this.Interact))));
            this.talk.PlayerMustBeFacing = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!this.Level.Session.GetFlag("gravestone"))
                return;
            this.Level.Add((Entity) new BadelineBoost(new Vector2[1]
            {
                this.boostTarget
            }, false));
            this.talk.RemoveSelf();
        }

        private void Interact(Player player)
        {
            this.Level.Session.SetFlag("gravestone");
            this.Scene.Add((Entity) new CS10_Gravestone(player, this, this.boostTarget));
            this.talk.Enabled = false;
        }
    }
}
