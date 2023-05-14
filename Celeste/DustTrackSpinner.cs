// Decompiled with JetBrains decompiler
// Type: Celeste.DustTrackSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class DustTrackSpinner : TrackSpinner
    {
        private readonly DustGraphic dusty;
        private Vector2 outwards;

        public DustTrackSpinner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Add(dusty = new DustGraphic(true));
            dusty.EyeDirection = dusty.EyeTargetDirection = (End - Start).SafeNormalize();
            dusty.OnEstablish = new Action(Establish);
            Depth = -50;
        }

        private void Establish()
        {
            Vector2 vector2_1 = (End - Start).SafeNormalize();
            Vector2 vector2_2 = new(-vector2_1.Y, vector2_1.X);
            bool flag = Scene.CollideCheck<Solid>(new Rectangle((int)((double)X + (vector2_2.X * 4.0)) - 2, (int)((double)Y + (vector2_2.Y * 4.0)) - 2, 4, 4));
            if (!flag)
            {
                vector2_2 = -vector2_2;
                flag = Scene.CollideCheck<Solid>(new Rectangle((int)((double)X + (vector2_2.X * 4.0)) - 2, (int)((double)Y + (vector2_2.Y * 4.0)) - 2, 4, 4));
            }
            if (!flag)
            {
                return;
            }

            Vector2 vector2_3 = End - Start;
            float num = vector2_3.Length();
            for (int index = 8; index < (double)num & flag; index += 8)
            {
                flag = flag && Scene.CollideCheck<Solid>(new Rectangle((int)((double)X + (vector2_2.X * 4.0) + (vector2_1.X * (double)index)) - 2, (int)((double)Y + (vector2_2.Y * 4.0) + (vector2_1.Y * (double)index)) - 2, 4, 4));
            }

            if (!flag)
            {
                return;
            }

            List<DustGraphic.Node> nodeList = null;
            if (vector2_2.X < 0.0)
            {
                nodeList = dusty.LeftNodes;
            }
            else if (vector2_2.X > 0.0)
            {
                nodeList = dusty.RightNodes;
            }
            else if (vector2_2.Y < 0.0)
            {
                nodeList = dusty.TopNodes;
            }
            else if (vector2_2.Y > 0.0)
            {
                nodeList = dusty.BottomNodes;
            }

            if (nodeList != null)
            {
                foreach (DustGraphic.Node node in nodeList)
                {
                    node.Enabled = false;
                }
            }
            outwards = -vector2_2;
            dusty.Position -= vector2_2;
            DustGraphic dusty1 = dusty;
            DustGraphic dusty2 = dusty;
            double startAngle = (double)outwards.Angle();
            double endAngle = Up ? Angle + 3.1415927410125732 : Angle;
            Vector2 vector;
            vector2_3 = vector = Calc.AngleToVector(Calc.AngleLerp((float)startAngle, (float)endAngle, 0.3f), 1f);
            dusty2.EyeTargetDirection = vector;
            Vector2 vector2_4 = vector2_3;
            dusty1.EyeDirection = vector2_4;
        }

        public override void Update()
        {
            base.Update();
            if (!Moving || PauseTimer >= 0.0 || !Scene.OnInterval(0.02f))
            {
                return;
            }

            SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
        }

        public override void OnPlayer(Player player)
        {
            base.OnPlayer(player);
            dusty.OnHitPlayer();
        }

        public override void OnTrackEnd()
        {
            if (outwards != Vector2.Zero)
            {
                dusty.EyeTargetDirection = Calc.AngleToVector(Calc.AngleLerp(outwards.Angle(), Up ? Angle + 3.14159274f : Angle, 0.3f), 1f);
            }
            else
            {
                dusty.EyeTargetDirection = Calc.AngleToVector(Up ? Angle + 3.14159274f : Angle, 1f);
                dusty.EyeFlip = -dusty.EyeFlip;
            }
        }
    }
}
