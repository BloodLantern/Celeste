using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class InteractTrigger : Entity
    {
        public const string FlagPrefix = "it_";
        public TalkComponent Talker;
        public List<string> Events;
        private int eventIndex;
        private float timeout;
        private bool used;

        public InteractTrigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Events = new List<string>();
            this.Events.Add(data.Attr("event"));
            this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
            for (int index = 2; index < 100 && data.Has("event_" + (object) index) && !string.IsNullOrEmpty(data.Attr("event_" + (object) index)); ++index)
                this.Events.Add(data.Attr("event_" + (object) index));
            Vector2 drawAt = new Vector2((float) (data.Width / 2), 0.0f);
            if (data.Nodes.Length != 0)
                drawAt = data.Nodes[0] - data.Position;
            this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, new Action<Player>(this.OnTalk))));
            this.Talker.PlayerMustBeFacing = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (scene as Level).Session;
            for (int index = 0; index < this.Events.Count; ++index)
            {
                if (session.GetFlag("it_" + this.Events[index]))
                    ++this.eventIndex;
            }
            if (this.eventIndex >= this.Events.Count)
            {
                this.RemoveSelf();
            }
            else
            {
                if (!(this.Events[this.eventIndex] == "ch5_theo_phone"))
                    return;
                scene.Add((Entity) new TheoPhone(this.Position + new Vector2((float) ((double) this.Width / 2.0 - 8.0), this.Height - 1f)));
            }
        }

        public void OnTalk(Player player)
        {
            if (this.used)
                return;
            bool flag = true;
            switch (this.Events[this.eventIndex])
            {
                case "ch2_poem":
                    this.Scene.Add((Entity) new CS02_Journal(player));
                    flag = false;
                    break;
                case "ch3_diary":
                    this.Scene.Add((Entity) new CS03_Diary(player));
                    flag = false;
                    break;
                case "ch3_guestbook":
                    this.Scene.Add((Entity) new CS03_Guestbook(player));
                    flag = false;
                    break;
                case "ch3_memo":
                    this.Scene.Add((Entity) new CS03_Memo(player));
                    flag = false;
                    break;
                case "ch5_mirror_reflection":
                    this.Scene.Add((Entity) new CS05_Reflection1(player));
                    break;
                case "ch5_see_theo":
                    this.Scene.Add((Entity) new CS05_SeeTheo(player, 0));
                    break;
                case "ch5_see_theo_b":
                    this.Scene.Add((Entity) new CS05_SeeTheo(player, 1));
                    break;
                case "ch5_theo_phone":
                    this.Scene.Add((Entity) new CS05_TheoPhone(player, this.Center.X));
                    break;
            }
            if (!flag)
                return;
            (this.Scene as Level).Session.SetFlag("it_" + this.Events[this.eventIndex]);
            ++this.eventIndex;
            if (this.eventIndex < this.Events.Count)
                return;
            this.used = true;
            this.timeout = 0.25f;
        }

        public override void Update()
        {
            if (this.used)
            {
                this.timeout -= Engine.DeltaTime;
                if ((double) this.timeout <= 0.0)
                    this.RemoveSelf();
            }
            else
            {
                while (this.eventIndex < this.Events.Count && (this.Scene as Level).Session.GetFlag("it_" + this.Events[this.eventIndex]))
                    ++this.eventIndex;
                if (this.eventIndex >= this.Events.Count)
                    this.RemoveSelf();
            }
            base.Update();
        }
    }
}
