using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle
{
    public class ParticleType
    {
        private static List<ParticleType> AllTypes = new List<ParticleType>();
        public MTexture Source;
        public Chooser<MTexture> SourceChooser;
        public Color Color;
        public Color Color2;
        public ColorModes ColorMode;
        public FadeModes FadeMode;
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
        public RotationModes RotationMode;
        public bool ScaleOut;
        public bool UseActualDeltaTime;

        public ParticleType()
        {
            Color = Color2 = Color.White;
            ColorMode = ColorModes.Static;
            FadeMode = FadeModes.None;
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
            RotationMode = RotationModes.None;
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

        public Particle Create(ref Particle particle, Vector2 position) => Create(ref particle, position, Direction);

        public Particle Create(ref Particle particle, Vector2 position, Color color) => Create(ref particle, null, position, Direction, color);

        public Particle Create(ref Particle particle, Vector2 position, float direction) => Create(ref particle, null, position, direction, Color);

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
            particle.Source = SourceChooser == null ? (Source == null ? Draw.Particle : Source) : SourceChooser.Choose();
            particle.StartSize = SizeRange == 0.0 ? (particle.Size = Size) : (particle.Size = Size - SizeRange * 0.5f + Calc.Random.NextFloat(SizeRange));
            particle.StartColor = ColorMode != ColorModes.Choose ? (particle.Color = color) : (particle.Color = Calc.Random.Choose(color, Color2));
            float angleRadians = (float) (direction - DirectionRange / 2.0 + Calc.Random.NextFloat() * (double) DirectionRange);
            particle.Speed = Calc.AngleToVector(angleRadians, Calc.Random.Range(SpeedMin, SpeedMax));
            particle.StartLife = particle.Life = Calc.Random.Range(LifeMin, LifeMax);
            particle.Rotation = RotationMode != RotationModes.Random ? (RotationMode != RotationModes.SameAsDirection ? 0.0f : angleRadians) : Calc.Random.NextAngle();
            particle.Spin = Calc.Random.Range(SpinMin, SpinMax);
            if (SpinFlippedChance)
                particle.Spin *= Calc.Random.Choose(1, -1);
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
