using Microsoft.Xna.Framework;

namespace Celeste
{
    public class LightFadeTrigger : Trigger
    {
        public float LightAddFrom;
        public float LightAddTo;
        public PositionModes PositionMode;

        public LightFadeTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            AddTag((int) Tags.TransitionUpdate);
            LightAddFrom = data.Float("lightAddFrom");
            LightAddTo = data.Float("lightAddTo");
            PositionMode = data.Enum<PositionModes>("positionMode");
        }

        public override void OnStay(Player player)
        {
            Level scene = Scene as Level;
            Session session = scene!.Session;
            float num1 = LightAddFrom + (LightAddTo - LightAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f);
            session.LightingAlphaAdd = num1;
            scene.Lighting.Alpha = scene.BaseLightingAlpha + num1;
        }
    }
}
