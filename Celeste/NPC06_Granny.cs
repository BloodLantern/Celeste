﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC06_Granny : NPC
    {
        public Hahaha Hahaha;
        private int cutsceneIndex;

        public NPC06_Granny(EntityData data, Vector2 position)
            : base(data.Position + position)
        {
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Scale.X = -1f;
            Sprite.Play("idle");
            Add(new GrannyLaughSfx(Sprite));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
            Hahaha.Enabled = false;
            while (Session.GetFlag("granny_" + cutsceneIndex))
                ++cutsceneIndex;
            Add(Talker = new TalkComponent(new Rectangle(-20, -8, 30, 8), new Vector2(0.0f, -24f), OnTalk));
            Talker.Enabled = cutsceneIndex > 0 && cutsceneIndex < 3;
        }

        public override void Update()
        {
            if (cutsceneIndex == 0)
            {
                Player entity = Level.Tracker.GetEntity<Player>();
                if (entity != null && entity.X > X - 60.0)
                    OnTalk(entity);
            }
            Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private void OnTalk(Player player)
        {
            Scene.Add(new CS06_Granny(this, player, cutsceneIndex));
            ++cutsceneIndex;
            Talker.Enabled = cutsceneIndex > 0 && cutsceneIndex < 3;
        }
    }
}
