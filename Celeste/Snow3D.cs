// Decompiled with JetBrains decompiler
// Type: Celeste.Snow3D
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Snow3D : Entity
    {
        private static readonly Color[] alphas = new Color[32];
        private readonly List<Snow3D.Particle> particles = new();
        private readonly BoundingFrustum Frustum = new(Matrix.Identity);
        private readonly BoundingFrustum LastFrustum = new(Matrix.Identity);
        private readonly MountainModel Model;
        private float Range = 30f;

        public Snow3D(MountainModel model)
        {
            Model = model;
            for (int index = 0; index < Snow3D.alphas.Length; ++index)
            {
                Snow3D.alphas[index] = Color.White * (index / (float)Snow3D.alphas.Length);
            }

            for (int index = 0; index < 400; ++index)
            {
                Snow3D.Particle particle = new(this, 1f);
                particles.Add(particle);
                Add(particle);
            }
        }

        public override void Update()
        {
            Range = 20f;
            if (SaveData.Instance != null && Scene is Overworld scene && (scene.IsCurrent<OuiChapterPanel>() || scene.IsCurrent<OuiChapterSelect>()))
            {
                switch (SaveData.Instance.LastArea.ID)
                {
                    case 0:
                    case 2:
                    case 8:
                        Range = 3f;
                        break;
                    case 1:
                        Range = 12f;
                        break;
                }
            }
            Matrix perspectiveFieldOfView = Matrix.CreatePerspectiveFieldOfView(0.981747746f, Engine.Width / (float)Engine.Height, 0.1f, Range);
            Matrix matrix = Matrix.CreateTranslation(-Model.Camera.Position) * Matrix.CreateFromQuaternion(Model.Camera.Rotation) * perspectiveFieldOfView;
            if (Scene.OnInterval(0.05f))
            {
                LastFrustum.Matrix = matrix;
            }

            Frustum.Matrix = matrix;
            base.Update();
        }

        [Tracked(false)]
        public class Particle : Billboard
        {
            public Snow3D Manager;
            public Vector2 Float;
            public float Percent;
            public float Duration;
            public float Speed;
            private readonly float size;

            public Particle(Snow3D manager, float size)
                : base(OVR.Atlas["snow"], Vector3.Zero)
            {
                Manager = manager;
                this.size = size;
                Size = Vector2.One * size;
                Reset(Calc.Random.NextFloat());
                ResetPosition();
            }

            public void ResetPosition()
            {
                float range = Manager.Range;
                Position = Manager.Model.Camera.Position + (Manager.Model.Forward * range * 0.5f) + new Vector3(Calc.Random.Range(-range, range), Calc.Random.Range(-range, range), Calc.Random.Range(-range, range));
            }

            public void Reset(float percent = 0.0f)
            {
                float num = Manager.Range / 30f;
                Speed = Calc.Random.Range(1f, 6f) * num;
                Percent = percent;
                Duration = Calc.Random.Range(1f, 5f);
                Float = new Vector2(Calc.Random.Range(-1, 1), Calc.Random.Range(-1, 1)).SafeNormalize() * 0.25f;
                Scale = Vector2.One * 0.05f * num;
            }

            public override void Update()
            {
                if (Percent > 1.0 || !InView())
                {
                    ResetPosition();
                    int num = 0;
                    while (!InView() && num++ < 10)
                    {
                        ResetPosition();
                    }

                    if (num <= 10)
                    {
                        Reset(!InLastView() ? Calc.Random.NextFloat() : 0.0f);
                    }
                    else
                    {
                        Color = Color.Transparent;
                        return;
                    }
                }
                Percent += Engine.DeltaTime / Duration;
                float num1 = Calc.YoYo(Percent);
                if (Manager.Model.SnowForceFloat > 0.0)
                {
                    num1 *= Manager.Model.SnowForceFloat;
                }
                else if (Manager.Model.StarEase > 0.0)
                {
                    num1 *= Calc.Map(Manager.Model.StarEase, 0.0f, 1f, 1f, 0.0f);
                }

                Color = Color.White * num1;
                Size.Y = size + (Manager.Model.SnowStretch * (1f - Manager.Model.SnowForceFloat));
                Position.Y -= (float)((Speed + (double)Manager.Model.SnowSpeedAddition) * (1.0 - Manager.Model.SnowForceFloat)) * Engine.DeltaTime;
                Position.X += Float.X * Engine.DeltaTime;
                Position.Z += Float.Y * Engine.DeltaTime;
            }

            private bool InView()
            {
                return Manager.Frustum.Contains(Position) == ContainmentType.Contains && Position.Y > 0.0;
            }

            private bool InLastView()
            {
                return Manager.LastFrustum != null && Manager.LastFrustum.Contains(Position) == ContainmentType.Contains;
            }
        }
    }
}
