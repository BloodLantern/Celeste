using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class RidgeGate : Solid
    {
        private Vector2? node;

        public RidgeGate(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("ridge", true))
        {
        }

        public RidgeGate(Vector2 position, float width, float height, Vector2? node, bool ridgeImage = true)
            : base(position, width, height, true)
        {
            this.node = node;
            Add(new Image(GFX.Game[ridgeImage ? "objects/ridgeGate" : "objects/farewellGate"]));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!node.HasValue || !CollideCheck<Player>())
                return;
            Visible = Collidable = false;
            Vector2 position = Position;
            Position = node.Value;
            Add(new Coroutine(EnterSequence(position)));
        }

        private IEnumerator EnterSequence(Vector2 moveTo)
        {
            RidgeGate ridgeGate = this;
            ridgeGate.Visible = ridgeGate.Collidable = true;
            yield return 0.25f;
            Audio.Play("event:/game/04_cliffside/stone_blockade", ridgeGate.Position);
            yield return 0.25f;
            Vector2 start = ridgeGate.Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, start: true);
            tween.OnUpdate = t => MoveTo(Vector2.Lerp(start, moveTo, t.Eased));
            ridgeGate.Add(tween);
        }
    }
}
