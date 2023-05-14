// Decompiled with JetBrains decompiler
// Type: Celeste.GrabbyIcon
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class GrabbyIcon : Entity
    {
        private bool enabled;
        private readonly Wiggler wiggler;

        public GrabbyIcon()
        {
            Depth = -1000001;
            Tag = (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
            Add(wiggler = Wiggler.Create(0.1f, 0.3f));
        }

        public override void Update()
        {
            base.Update();
            bool flag = false;
            if (!SceneAs<Level>().InCutscene)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && !entity.Dead && Settings.Instance.GrabMode == GrabModes.Toggle && Input.GrabCheck)
                {
                    flag = true;
                }
            }
            if (flag == enabled)
            {
                return;
            }

            enabled = flag;
            wiggler.Start();
        }

        public override void Render()
        {
            if (!enabled)
            {
                return;
            }

            Vector2 scale = Vector2.One * (float)(1.0 + ((double)wiggler.Value * 0.20000000298023224));
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            GFX.Game["util/glove"].DrawJustified(new Vector2(entity.X, entity.Y - 16f), new Vector2(0.5f, 1f), Color.White, scale);
        }
    }
}
