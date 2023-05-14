// Decompiled with JetBrains decompiler
// Type: Celeste.SummitCheckpoint
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class SummitCheckpoint : Entity
    {
        private const string Flag = "summit_checkpoint_";
        public bool Activated;
        public readonly int Number;
        private readonly string numberString;
        private Vector2 respawn;
        private readonly MTexture baseEmpty;
        private readonly MTexture baseToggle;
        private readonly MTexture baseActive;
        private readonly List<MTexture> numbersEmpty;
        private readonly List<MTexture> numbersActive;

        public SummitCheckpoint(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Number = data.Int("number");
            numberString = Number.ToString("D2");
            baseEmpty = GFX.Game["scenery/summitcheckpoints/base00"];
            baseToggle = GFX.Game["scenery/summitcheckpoints/base01"];
            baseActive = GFX.Game["scenery/summitcheckpoints/base02"];
            numbersEmpty = GFX.Game.GetAtlasSubtextures("scenery/summitcheckpoints/numberbg");
            numbersActive = GFX.Game.GetAtlasSubtextures("scenery/summitcheckpoints/number");
            Collider = new Hitbox(32f, 32f, -16f, -8f);
            Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((scene as Level).Session.GetFlag("summit_checkpoint_" + Number))
            {
                Activated = true;
            }

            respawn = SceneAs<Level>().GetSpawnPoint(Position);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Activated || !CollideCheck<Player>())
            {
                return;
            }

            Activated = true;
            Level scene1 = Scene as Level;
            scene1.Session.SetFlag("summit_checkpoint_" + Number);
            scene1.Session.RespawnPoint = new Vector2?(respawn);
        }

        public override void Update()
        {
            if (Activated)
            {
                return;
            }

            Player player = CollideFirst<Player>();
            if (player == null || !player.OnGround() || player.Speed.Y < 0.0)
            {
                return;
            }

            Level scene = Scene as Level;
            Activated = true;
            scene.Session.SetFlag("summit_checkpoint_" + Number);
            scene.Session.RespawnPoint = new Vector2?(respawn);
            scene.Session.UpdateLevelStartDashes();
            scene.Session.HitCheckpoint = true;
            _ = scene.Displacement.AddBurst(Position, 0.5f, 4f, 24f, 0.5f);
            scene.Add(new SummitCheckpoint.ConfettiRenderer(Position));
            _ = Audio.Play("event:/game/07_summit/checkpoint_confetti", Position);
        }

        public override void Render()
        {
            List<MTexture> mtextureList = Activated ? numbersActive : numbersEmpty;
            MTexture mtexture = baseActive;
            if (!Activated)
            {
                mtexture = Scene.BetweenInterval(0.25f) ? baseEmpty : baseToggle;
            }

            mtexture.Draw(Position - new Vector2((mtexture.Width / 2) + 1, mtexture.Height / 2));
            mtextureList[numberString[0] - 48].DrawJustified(Position + new Vector2(-1f, 1f), new Vector2(1f, 0.0f));
            mtextureList[numberString[1] - 48].DrawJustified(Position + new Vector2(0.0f, 1f), new Vector2(0.0f, 0.0f));
        }

        public class ConfettiRenderer : Entity
        {
            private static readonly Color[] confettiColors = new Color[3]
            {
                Calc.HexToColor("fe2074"),
                Calc.HexToColor("205efe"),
                Calc.HexToColor("cefe20")
            };
            private readonly SummitCheckpoint.ConfettiRenderer.Particle[] particles = new SummitCheckpoint.ConfettiRenderer.Particle[30];

            public ConfettiRenderer(Vector2 position)
                : base(position)
            {
                Depth = -10010;
                for (int index = 0; index < particles.Length; ++index)
                {
                    particles[index].Position = Position + new Vector2(Calc.Random.Range(-3, 3), Calc.Random.Range(-3, 3));
                    particles[index].Color = Calc.Random.Choose<Color>(SummitCheckpoint.ConfettiRenderer.confettiColors);
                    particles[index].Timer = Calc.Random.NextFloat();
                    particles[index].Duration = Calc.Random.Range(2, 4);
                    particles[index].Alpha = 1f;
                    float angleRadians = Calc.Random.Range(-0.5f, 0.5f) - 1.57079637f;
                    int length = Calc.Random.Range(140, 220);
                    particles[index].Speed = Calc.AngleToVector(angleRadians, length);
                }
            }

            public override void Update()
            {
                for (int index = 0; index < particles.Length; ++index)
                {
                    particles[index].Position += particles[index].Speed * Engine.DeltaTime;
                    particles[index].Speed.X = Calc.Approach(particles[index].Speed.X, 0.0f, 80f * Engine.DeltaTime);
                    particles[index].Speed.Y = Calc.Approach(particles[index].Speed.Y, 20f, 500f * Engine.DeltaTime);
                    particles[index].Timer += Engine.DeltaTime;
                    particles[index].Percent += Engine.DeltaTime / particles[index].Duration;
                    particles[index].Alpha = Calc.ClampedMap(particles[index].Percent, 0.9f, 1f, 1f, 0.0f);
                    if (particles[index].Speed.Y > 0.0)
                    {
                        particles[index].Approach = Calc.Approach(particles[index].Approach, 5f, Engine.DeltaTime * 16f);
                    }
                }
            }

            public override void Render()
            {
                for (int index = 0; index < particles.Length; ++index)
                {
                    Vector2 position = particles[index].Position;
                    float rotation;
                    if (particles[index].Speed.Y < 0.0)
                    {
                        rotation = particles[index].Speed.Angle();
                    }
                    else
                    {
                        rotation = (float)Math.Sin(particles[index].Timer * 4.0) * 1f;
                        position += Calc.AngleToVector(1.57079637f + rotation, particles[index].Approach);
                    }
                    GFX.Game["particles/confetti"].DrawCentered(position + Vector2.UnitY, Color.Black * (particles[index].Alpha * 0.5f), 1f, rotation);
                    GFX.Game["particles/confetti"].DrawCentered(position, particles[index].Color * particles[index].Alpha, 1f, rotation);
                }
            }

            private struct Particle
            {
                public Vector2 Position;
                public Color Color;
                public Vector2 Speed;
                public float Timer;
                public float Percent;
                public float Duration;
                public float Alpha;
                public float Approach;
            }
        }
    }
}
