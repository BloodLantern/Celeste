﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class MoonParticle3D : Entity
    {
        private MountainModel model;
        private List<Particle> particles = new List<Particle>();

        public MoonParticle3D(MountainModel model, Vector3 center)
        {
            this.model = model;
            Visible = false;
            Matrix rotationZ = Matrix.CreateRotationZ(0.4f);
            Color[] colorArray1 = new Color[2]
            {
                Calc.HexToColor("53f3dd"),
                Calc.HexToColor("53c9f3")
            };
            for (int index = 0; index < 20; ++index)
                Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(colorArray1), center, 1f, rotationZ));
            for (int index = 0; index < 30; ++index)
                Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(colorArray1), center, 0.3f, rotationZ));
            Matrix matrix = Matrix.CreateRotationZ(0.8f) * Matrix.CreateRotationX(0.4f);
            Color[] colorArray2 = new Color[2]
            {
                Calc.HexToColor("ab6ffa"),
                Calc.HexToColor("fa70ea")
            };
            for (int index = 0; index < 20; ++index)
                Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(colorArray2), center, 1f, matrix));
            for (int index = 0; index < 30; ++index)
                Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(colorArray2), center, 0.3f, matrix));
        }

        public override void Update()
        {
            base.Update();
            Visible = model.StarEase > 0.0;
        }

        public class Particle : Billboard
        {
            public Vector3 Center;
            public Matrix Matrix;
            public float Rotation;
            public float Distance;
            public float YOff;
            public float Spd;

            public Particle(MTexture texture, Color color, Vector3 center, float size, Matrix matrix)
                : base(texture, Vector3.Zero, color: color)
            {
                Center = center;
                Matrix = matrix;
                Size = Vector2.One * Calc.Random.Range(0.05f, 0.15f) * size;
                Distance = Calc.Random.Range(1.8f, 1.9f);
                Rotation = Calc.Random.NextFloat(6.28318548f);
                YOff = Calc.Random.Range(-0.1f, 0.1f);
                Spd = Calc.Random.Range(0.8f, 1.2f);
            }

            public override void Update()
            {
                Rotation += Engine.DeltaTime * 0.4f * Spd;
                Position = Center + Vector3.Transform(new Vector3((float) Math.Cos(Rotation) * Distance, (float) Math.Sin(Rotation * 3.0) * 0.25f + YOff, (float) Math.Sin(Rotation) * Distance), Matrix);
            }
        }
    }
}
