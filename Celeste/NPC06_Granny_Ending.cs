// Decompiled with JetBrains decompiler
// Type: Celeste.NPC06_Granny_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC06_Granny_Ending : NPC
    {
        private bool talked;

        public NPC06_Granny_Ending(EntityData data, Vector2 position)
            : base(data.Position + position)
        {
            Add(Sprite = GFX.SpriteBank.Create("granny"));
            Sprite.Scale.X = -1f;
            Sprite.Play("idle");
            IdleAnim = "idle";
            MoveAnim = "walk";
            Maxspeed = 30f;
            Visible = false;
            Add(Light = new VertexLight(new Vector2(0.0f, -8f), Color.White, 1f, 16, 32));
            SetupGrannySpriteSounds();
        }

        public override void Update()
        {
            base.Update();
            if (talked)
            {
                return;
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !entity.OnGround())
            {
                return;
            }

            talked = true;
            Scene.Add(new CS06_Ending(entity, this));
        }
    }
}
