using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class RidgeGate : Solid
    {
        private Vector2? node;

        public RidgeGate(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height, data.FirstNodeNullable(new Vector2?(offset)), data.Bool("ridge", true))
        {
        }

        public RidgeGate(Vector2 position, float width, float height, Vector2? node, bool ridgeImage = true)
            : base(position, width, height, true)
        {
            this.node = node;
            this.Add((Component) new Monocle.Image(GFX.Game[ridgeImage ? "objects/ridgeGate" : "objects/farewellGate"]));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!this.node.HasValue || !this.CollideCheck<Player>())
                return;
            this.Visible = this.Collidable = false;
            Vector2 position = this.Position;
            this.Position = this.node.Value;
            this.Add((Component) new Coroutine(this.EnterSequence(position)));
        }

        private IEnumerator EnterSequence(Vector2 moveTo)
        {
            RidgeGate ridgeGate = this;
            ridgeGate.Visible = ridgeGate.Collidable = true;
            yield return (object) 0.25f;
            Audio.Play("event:/game/04_cliffside/stone_blockade", ridgeGate.Position);
            yield return (object) 0.25f;
            Vector2 start = ridgeGate.Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, start: true);
            tween.OnUpdate = (Action<Tween>) (t => this.MoveTo(Vector2.Lerp(start, moveTo, t.Eased)));
            ridgeGate.Add((Component) tween);
        }
    }
}
