using Microsoft.Xna.Framework;
using Monocle;
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
            Events = new List<string>();
            Events.Add(data.Attr("event"));
            Collider = new Hitbox(data.Width, data.Height);
            for (int index = 2; index < 100 && data.Has("event_" + index) && !string.IsNullOrEmpty(data.Attr("event_" + index)); ++index)
                Events.Add(data.Attr("event_" + index));
            Vector2 drawAt = new Vector2(data.Width / 2, 0.0f);
            if (data.Nodes.Length != 0)
                drawAt = data.Nodes[0] - data.Position;
            Add(Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), drawAt, OnTalk));
            Talker.PlayerMustBeFacing = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (scene as Level).Session;
            for (int index = 0; index < Events.Count; ++index)
            {
                if (session.GetFlag("it_" + Events[index]))
                    ++eventIndex;
            }
            if (eventIndex >= Events.Count)
            {
                RemoveSelf();
            }
            else
            {
                if (!(Events[eventIndex] == "ch5_theo_phone"))
                    return;
                scene.Add(new TheoPhone(Position + new Vector2((float) (Width / 2.0 - 8.0), Height - 1f)));
            }
        }

        public void OnTalk(Player player)
        {
            if (used)
                return;
            bool flag = true;
            switch (Events[eventIndex])
            {
                case "ch2_poem":
                    Scene.Add(new CS02_Journal(player));
                    flag = false;
                    break;
                case "ch3_diary":
                    Scene.Add(new CS03_Diary(player));
                    flag = false;
                    break;
                case "ch3_guestbook":
                    Scene.Add(new CS03_Guestbook(player));
                    flag = false;
                    break;
                case "ch3_memo":
                    Scene.Add(new CS03_Memo(player));
                    flag = false;
                    break;
                case "ch5_mirror_reflection":
                    Scene.Add(new CS05_Reflection1(player));
                    break;
                case "ch5_see_theo":
                    Scene.Add(new CS05_SeeTheo(player, 0));
                    break;
                case "ch5_see_theo_b":
                    Scene.Add(new CS05_SeeTheo(player, 1));
                    break;
                case "ch5_theo_phone":
                    Scene.Add(new CS05_TheoPhone(player, Center.X));
                    break;
            }
            if (!flag)
                return;
            (Scene as Level).Session.SetFlag("it_" + Events[eventIndex]);
            ++eventIndex;
            if (eventIndex < Events.Count)
                return;
            used = true;
            timeout = 0.25f;
        }

        public override void Update()
        {
            if (used)
            {
                timeout -= Engine.DeltaTime;
                if (timeout <= 0.0)
                    RemoveSelf();
            }
            else
            {
                while (eventIndex < Events.Count && (Scene as Level).Session.GetFlag("it_" + Events[eventIndex]))
                    ++eventIndex;
                if (eventIndex >= Events.Count)
                    RemoveSelf();
            }
            base.Update();
        }
    }
}
