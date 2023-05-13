// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterBlock
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
        public HashSet<ClutterBlock> HasBelow = new();
        public List<ClutterBlock> Below = new();
        public List<ClutterBlock> Above = new();
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

            floatTarget = 0f;
            floatDelay = 0.1f;
        }

        public override void Update()
        {
            base.Update();
            if (OnTheGround)
                return;

            if (floatDelay <= 0)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && ((!TopSideOpen ? 0 : (entity.Right <= Left || entity.Left >= Right || entity.Bottom < Top - 1 ? 0 : (entity.Bottom <= Top + 4 ? 1 : 0))) | (entity.StateMachine.State != 1 || !LeftSideOpen || entity.Right < Left - 1 || entity.Right >= Left + 4 || entity.Bottom <= Top ? 0 : (entity.Top < Bottom ? 1 : 0)) | (entity.StateMachine.State != 1 || !RightSideOpen || entity.Left > Right + 1 || entity.Left <= Right - 4 || entity.Bottom <= Top ? 0 : (entity.Top < Bottom ? 1 : 0))) != 0)
                    WeightDown();
            }
            floatTimer += Engine.DeltaTime;
            floatDelay -= Engine.DeltaTime;
            if (floatDelay <= 0)
            {
                floatTarget = Calc.Approach(floatTarget, WaveTarget, Engine.DeltaTime * 4f);
            }

            Image.Y = floatTarget;
        }

        private float WaveTarget => -(((float) Math.Sin(Position.X / 16 * 0.25f + floatTimer * 2) + 1) / 2) - 1;

        public void Absorb(ClutterAbsorbEffect effect)
        {
            effect.FlyClutter(Position + new Vector2(Image.Width * 0.5f, (Image.Height * 0.5f) + floatTarget), Image.Texture, true, Calc.Random.NextFloat(0.5f));
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
