using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class ClutterBlock : Entity
    {
        public Colors BlockColor;
        public Image Image;
        public HashSet<ClutterBlock> HasBelow = new HashSet<ClutterBlock>();
        public List<ClutterBlock> Below = new List<ClutterBlock>();
        public List<ClutterBlock> Above = new List<ClutterBlock>();
        public bool OnTheGround;
        public bool TopSideOpen;
        public bool LeftSideOpen;
        public bool RightSideOpen;
        private float floatTarget;
        private float floatDelay;
        private float floatTimer;

        public ClutterBlock(Vector2 position, MTexture texture, Colors color)
            : base(position)
        {
            BlockColor = color;
            Add(Image = new Image(texture));
            Collider = new Hitbox(texture.Width, texture.Height);
            Depth = -9998;
        }

        public void WeightDown()
        {
            foreach (ClutterBlock clutterBlock in Below)
                clutterBlock.WeightDown();
            floatTarget = 0.0f;
            floatDelay = 0.1f;
        }

        public override void Update()
        {
            base.Update();
            if (OnTheGround)
                return;
            if (floatDelay <= 0.0)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && ((!TopSideOpen ? 0 : (entity.Right <= (double) Left || entity.Left >= (double) Right || entity.Bottom < Top - 1.0 ? 0 : (entity.Bottom <= Top + 4.0 ? 1 : 0))) | (entity.StateMachine.State != 1 || !LeftSideOpen || entity.Right < Left - 1.0 || entity.Right >= Left + 4.0 || entity.Bottom <= (double) Top ? 0 : (entity.Top < (double) Bottom ? 1 : 0)) | (entity.StateMachine.State != 1 || !RightSideOpen || entity.Left > Right + 1.0 || entity.Left <= Right - 4.0 || entity.Bottom <= (double) Top ? 0 : (entity.Top < (double) Bottom ? 1 : 0))) != 0)
                    WeightDown();
            }
            floatTimer += Engine.DeltaTime;
            floatDelay -= Engine.DeltaTime;
            if (floatDelay <= 0.0)
                floatTarget = Calc.Approach(floatTarget, WaveTarget, Engine.DeltaTime * 4f);
            Image.Y = floatTarget;
        }

        private float WaveTarget => (float) (-((Math.Sin((int) Position.X / 16 * 0.25 + floatTimer * 2.0) + 1.0) / 2.0) - 1.0);

        public void Absorb(ClutterAbsorbEffect effect)
        {
            effect.FlyClutter(Position + new Vector2(Image.Width * 0.5f, Image.Height * 0.5f + floatTarget), Image.Texture, true, Calc.Random.NextFloat(0.5f));
            Scene.Remove(this);
        }

        public enum Colors
        {
            Red,
            Green,
            Yellow,
            Lightning,
        }
    }
}
