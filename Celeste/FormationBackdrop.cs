// Decompiled with JetBrains decompiler
// Type: Celeste.FormationBackdrop
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class FormationBackdrop : Entity
    {
        public bool Display;
        public float Alpha = 1f;
        private bool wasDisplayed;
        private float fade;

        public FormationBackdrop()
        {
            Tag = (int)Tags.FrozenUpdate | (int)Tags.Global;
            Depth = -1999900;
        }

        public override void Update()
        {
            fade = Calc.Approach(fade, Display ? 1f : 0.0f, Engine.RawDeltaTime * 3f);
            if (Display)
            {
                wasDisplayed = true;
            }

            if (wasDisplayed)
            {
                Level scene = Scene as Level;
                Snow snow = scene.Foreground.Get<Snow>();
                if (snow != null)
                {
                    snow.Alpha = 1f - fade;
                }

                WindSnowFG windSnowFg = scene.Foreground.Get<WindSnowFG>();
                if (windSnowFg != null)
                {
                    windSnowFg.Alpha = 1f - fade;
                }

                if (fade <= 0.0)
                {
                    wasDisplayed = false;
                }
            }
            base.Update();
        }

        public override void Render()
        {
            Level scene = Scene as Level;
            if (fade <= 0.0)
            {
                return;
            }

            Draw.Rect(scene.Camera.Left - 1f, scene.Camera.Top - 1f, 322f, 182f, Color.Black * fade * Alpha * 0.85f);
        }
    }
}
