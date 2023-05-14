// Decompiled with JetBrains decompiler
// Type: Celeste.Hahaha
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class Hahaha : Entity
    {
        private bool enabled;
        private readonly string ifSet;
        private float timer;
        private int counter;
        private readonly List<Hahaha.Ha> has = new();
        private readonly bool autoTriggerLaughSfx;
        private Vector2 autoTriggerLaughOrigin;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (!enabled & value)
                {
                    timer = 0.0f;
                    counter = 0;
                }
                enabled = value;
            }
        }

        public Hahaha(
            Vector2 position,
            string ifSet = "",
            bool triggerLaughSfx = false,
            Vector2? triggerLaughSfxOrigin = null)
        {
            Depth = -10001;
            Position = position;
            this.ifSet = ifSet;
            if (!triggerLaughSfx)
            {
                return;
            }

            autoTriggerLaughSfx = triggerLaughSfx;
            autoTriggerLaughOrigin = triggerLaughSfxOrigin.Value;
        }

        public Hahaha(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr("ifset"), data.Bool("triggerLaughSfx"), new Vector2?(data.Nodes.Length != 0 ? offset + data.Nodes[0] : Vector2.Zero))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(ifSet) || (Scene as Level).Session.GetFlag(ifSet))
            {
                return;
            }

            Enabled = false;
        }

        public override void Update()
        {
            if (Enabled)
            {
                timer -= Engine.DeltaTime;
                if (timer <= 0.0)
                {
                    has.Add(new Hahaha.Ha());
                    ++counter;
                    if (counter >= 3)
                    {
                        counter = 0;
                        timer = 1.5f;
                    }
                    else
                    {
                        timer = 0.6f;
                    }
                }
                if (autoTriggerLaughSfx && Scene.OnInterval(0.4f))
                {
                    _ = Audio.Play("event:/char/granny/laugh_oneha", autoTriggerLaughOrigin);
                }
            }
            for (int index = has.Count - 1; index >= 0; --index)
            {
                if (has[index].Percent > 1.0)
                {
                    has.RemoveAt(index);
                }
                else
                {
                    has[index].Sprite.Update();
                    has[index].Percent += Engine.DeltaTime / has[index].Duration;
                }
            }
            if (!Enabled && !string.IsNullOrEmpty(ifSet) && (Scene as Level).Session.GetFlag(ifSet))
            {
                Enabled = true;
            }

            base.Update();
        }

        public override void Render()
        {
            foreach (Hahaha.Ha ha in has)
            {
                ha.Sprite.Position = Position + new Vector2(ha.Percent * 60f, (float)((-Math.Sin(ha.Percent * 13.0) * 4.0) - 10.0 + (ha.Percent * -16.0)));
                ha.Sprite.Render();
            }
        }

        private class Ha
        {
            public Sprite Sprite;
            public float Percent;
            public float Duration;

            public Ha()
            {
                Sprite = new Sprite(GFX.Game, "characters/oldlady/");
                Sprite.Add("normal", "ha", 0.15f, 0, 1, 0, 1, 0, 1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
                Sprite.Play("normal");
                _ = Sprite.JustifyOrigin(0.5f, 0.5f);
                Duration = Sprite.CurrentAnimationTotalFrames * 0.15f;
            }
        }
    }
}
