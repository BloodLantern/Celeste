// Decompiled with JetBrains decompiler
// Type: Celeste.TheoCrystalPedestal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TheoCrystalPedestal : Solid
    {
        public Monocle.Image sprite;
        public bool DroppedTheo;

        public TheoCrystalPedestal(EntityData data, Vector2 offset)
            : base(data.Position + offset, 32f, 32f, false)
        {
            Add(sprite = new Monocle.Image(GFX.Game["characters/theoCrystal/pedestal"]));
            EnableAssistModeChecks = false;
            _ = sprite.JustifyOrigin(0.5f, 1f);
            Depth = 8998;
            Collider.Position = new Vector2(-16f, -64f);
            Collidable = false;
            OnDashCollide = (player, direction) =>
            {
                TheoCrystal entity = Scene.Tracker.GetEntity<TheoCrystal>();
                entity.OnPedestal = false;
                entity.Speed = new Vector2(0.0f, -300f);
                DroppedTheo = true;
                Collidable = false;
                (Scene as Level).Flash(Color.White);
                Celeste.Freeze(0.1f);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                _ = Audio.Play("event:/game/05_mirror_temple/crystaltheo_break_free", entity.Position);
                return DashCollisionResults.Rebound;
            };
            Tag = (int)Tags.TransitionUpdate;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("foundTheoInCrystal"))
            {
                DroppedTheo = true;
            }
            else
            {
                TheoCrystal first = Scene.Entities.FindFirst<TheoCrystal>();
                if (first == null)
                {
                    return;
                }

                first.Depth = Depth + 1;
            }
        }

        public override void Update()
        {
            TheoCrystal entity = Scene.Tracker.GetEntity<TheoCrystal>();
            if (entity != null && !DroppedTheo)
            {
                entity.Position = Position + new Vector2(0.0f, -32f);
                entity.OnPedestal = true;
            }
            base.Update();
        }
    }
}
