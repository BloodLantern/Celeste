﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class StaminaDisplay : Component
    {
        private Player player;
        private float drawStamina;
        private float displayTimer;
        private Level level;

        public StaminaDisplay()
            : base(true, false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            level = SceneAs<Level>();
            player = EntityAs<Player>();
            drawStamina = player.Stamina;
        }

        public override void Update()
        {
            base.Update();
            drawStamina = Calc.Approach(drawStamina, player.Stamina, 300f * Engine.DeltaTime);
            if (drawStamina < 110.0 && drawStamina > 0.0)
            {
                displayTimer = 0.75f;
            }
            else
            {
                if (displayTimer <= 0.0)
                    return;
                displayTimer -= Engine.DeltaTime;
            }
        }

        public void RenderHUD()
        {
            if (displayTimer <= 0.0)
                return;
            Vector2 vector2 = level.Camera.CameraToScreen(player.Position + new Vector2(0.0f, -18f)) * 6f;
            Color color = drawStamina >= 20.0 ? Color.Lime : Color.Red;
            Draw.Rect((float) (vector2.X - 48.0 - 1.0), (float) (vector2.Y - 6.0 - 1.0), 98f, 14f, Color.Black);
            Draw.Rect(vector2.X - 48f, vector2.Y - 6f, (float) (96.0 * (drawStamina / 110.0)), 12f, color);
        }
    }
}
