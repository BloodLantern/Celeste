// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Oshiro_Lobby
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class NPC03_Oshiro_Lobby : NPC
    {
        public static ParticleType P_AppearSpark;
        private float startX;

        public NPC03_Oshiro_Lobby(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = (Sprite) new OshiroSprite(-1)));
            this.Sprite.Visible = false;
            MTexture mtexture = GFX.Gui["hover/resort"];
            if (GFX.Gui.Has("hover/resort_" + Settings.Instance.Language))
                mtexture = GFX.Gui["hover/resort_" + Settings.Instance.Language];
            this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-30, -16, 42, 32), new Vector2(-12f, -24f), new Action<Player>(this.OnTalk), new TalkComponent.HoverDisplay()
            {
                Texture = mtexture,
                InputPosition = new Vector2(0.0f, -75f),
                SfxIn = "event:/ui/game/hotspot_note_in",
                SfxOut = "event:/ui/game/hotspot_note_out"
            })));
            this.Talker.PlayerMustBeFacing = false;
            this.MoveAnim = "move";
            this.IdleAnim = "idle";
            this.Depth = 9001;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (this.Session.GetFlag("oshiro_resort_talked_1"))
            {
                this.Session.Audio.Music.Event = "event:/music/lvl3/explore";
                this.Session.Audio.Music.Progress = 1;
                this.Session.Audio.Apply();
                this.RemoveSelf();
            }
            else
            {
                this.Session.Audio.Music.Event = (string) null;
                this.Session.Audio.Apply();
            }
            scene.Add((Entity) new OshiroLobbyBell(new Vector2(this.X - 14f, this.Y)));
            this.startX = this.Position.X;
        }

        private void OnTalk(Player player)
        {
            this.Scene.Add((Entity) new CS03_OshiroLobby(player, (NPC) this));
            this.Talker.Enabled = false;
        }

        public override void Update()
        {
            base.Update();
            if ((double) this.X < (double) this.startX + 12.0)
                return;
            this.Depth = 1000;
        }
    }
}
