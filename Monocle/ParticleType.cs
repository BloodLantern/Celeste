// Decompiled with JetBrains decompiler
// Type: Monocle.ParticleType
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle
{
    public class ParticleType
    {
        private static readonly List<ParticleType> AllTypes = new();
        public MTexture Source;
        public Chooser<MTexture> SourceChooser;
        public Color Color;
        public Color Color2;
        public ParticleType.ColorModes ColorMode;
        public ParticleType.FadeModes FadeMode;
        public float SpeedMin;
        public float SpeedMax;
        public float SpeedMultiplier;
        public Vector2 Acceleration;
        public float Friction;
        public float Direction;
        public float DirectionRange;
        public float LifeMin;
        public float LifeMax;
        public float Size;
        public float SizeRange;
        public float SpinMin;
        public float SpinMax;
        public bool SpinFlippedChance;
        public ParticleType.RotationModes RotationMode;
        public bool ScaleOut;
        public bool UseActualDeltaTime;

        public ParticleType()
        {
            Color = Color2 = Color.White;
            ColorMode = ParticleType.ColorModes.Static;
            FadeMode = ParticleType.FadeModes.None;
            SpeedMin = SpeedMax = 0.0f;
            SpeedMultiplier = 1f;
            Acceleration = Vector2.Zero;
            Friction = 0.0f;
            Direction = DirectionRange = 0.0f;
            LifeMin = LifeMax = 0.0f;
            Size = 2f;
            SizeRange = 0.0f;
            SpinMin = SpinMax = 0.0f;
            SpinFlippedChance = false;
            RotationMode = ParticleType.RotationModes.None;
            ParticleType.AllTypes.Add(this);
        }

        public ParticleType(ParticleType copyFrom)
        {
            Source = copyFrom.Source;
            SourceChooser = copyFrom.SourceChooser;
            Color = copyFrom.Color;
            Color2 = copyFrom.Color2;
            ColorMode = copyFrom.ColorMode;
            FadeMode = copyFrom.FadeMode;
            SpeedMin = copyFrom.SpeedMin;
            SpeedMax = copyFrom.SpeedMax;
            SpeedMultiplier = copyFrom.SpeedMultiplier;
            Acceleration = copyFrom.Acceleration;
            Friction = copyFrom.Friction;
            Direction = copyFrom.Direction;
            DirectionRange = copyFrom.DirectionRange;
            LifeMin = copyFrom.LifeMin;
            LifeMax = copyFrom.LifeMax;
            Size = copyFrom.Size;
            SizeRange = copyFrom.SizeRange;
            RotationMode = copyFrom.RotationMode;
            SpinMin = copyFrom.SpinMin;
            SpinMax = copyFrom.SpinMax;
            SpinFlippedChance = copyFrom.SpinFlippedChance;
            ScaleOut = copyFrom.ScaleOut;
            UseActualDeltaTime = copyFrom.UseActualDeltaTime;
            ParticleType.AllTypes.Add(this);
        }

        public Particle Create(ref Particle particle, Vector2 position)
        {
            return Create(ref particle, position, Direction);
        }

        public Particle Create(ref Particle particle, Vector2 position, Color color)
        {
            return Create(ref particle, null, position, Direction, color);
        }

        public Particle Create(ref Particle particle, Vector2 position, float direction)
        {
            return Create(ref particle, null, position, direction, Color);
        }

        public Particle Create(
            ref Particle particle,
            Vector2 position,
            Color color,
            float direction)
        {
            return Create(ref particle, null, position, direction, color);
        }

        public Particle Create(
            ref Particle particle,
            Entity entity,
            Vector2 position,
            float direction,
            Color color)
        {
            particle.Track = entity;
            particle.Type = this;
            particle.Active = true;
            particle.Position = position;
            particle.Source = SourceChooser == null ? (Source ?? Draw.Particle) : SourceChooser.Choose();
            particle.StartSize = SizeRange == 0.0 ? (particle.Size = Size) : (particle.Size = Size - (SizeRange * 0.5f) + Calc.Random.NextFloat(SizeRange));
            particle.StartColor = ColorMode != ParticleType.ColorModes.Choose ? (particle.Color = color) : (particle.Color = Calc.Random.Choose<Color>(color, Color2));
            float angleRadians = (float)((double)direction - (DirectionRange / 2.0) + ((double)Calc.Random.NextFloat() * DirectionRange));
            particle.Speed = Calc.AngleToVector(angleRadians, Calc.Random.Range(SpeedMin, SpeedMax));
            particle.StartLife = particle.Life = Calc.Random.Range(LifeMin, LifeMax);
            particle.Rotation = RotationMode != ParticleType.RotationModes.Random ? (RotationMode != ParticleType.RotationModes.SameAsDirection ? 0.0f : angleRadians) : Calc.Random.NextAngle();
            particle.Spin = Calc.Random.Range(SpinMin, SpinMax);
            if (SpinFlippedChance)
            {
                particle.Spin *= Calc.Random.Choose<int>(1, -1);
            }

            return particle;
        }

        public enum ColorModes
        {
            Static,
            Choose,
            Blink,
            Fade,
        }

        public enum FadeModes
        {
            None,
            Linear,
            Late,
            InAndOut,
        }

        public enum RotationModes
        {
            None,
            Random,
            SameAsDirection,
        }
    }
}
