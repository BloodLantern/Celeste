// Decompiled with JetBrains decompiler
// Type: Celeste.NPC10_Gravestone
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class NPC10_Gravestone : NPC
    {
        private const string Flag = "gravestone";
        private readonly Player player;
        private Vector2 boostTarget;
        private readonly TalkComponent talk;

        public NPC10_Gravestone(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            boostTarget = data.FirstNodeNullable(new Vector2?(offset)) ?? Vector2.Zero;
            Add(talk = new TalkComponent(new Rectangle(-24, -8, 32, 8), new Vector2(-0.5f, -20f), new Action<Player>(Interact)));
            talk.PlayerMustBeFacing = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Level.Session.GetFlag("gravestone"))
            {
                return;
            }

            Level.Add(new BadelineBoost(new Vector2[1]
            {
                boostTarget
            }, false));
            talk.RemoveSelf();
        }

        private void Interact(Player player)
        {
            Level.Session.SetFlag("gravestone");
            Scene.Add(new CS10_Gravestone(player, this, boostTarget));
            talk.Enabled = false;
        }
    }
}
