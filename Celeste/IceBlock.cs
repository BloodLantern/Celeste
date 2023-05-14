// Decompiled with JetBrains decompiler
// Type: Celeste.IceBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class IceBlock : Entity
    {
        public static ParticleType P_Deactivate;
        private readonly LavaRect lava;
        private Solid solid;

        public IceBlock(Vector2 position, float width, float height)
            : base(position)
        {
            Collider = new Hitbox(width, height);
            Add(new CoreModeListener(new Action<Session.CoreModes>(OnChangeMode)));
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(lava = new LavaRect(width, height, 2));
            lava.UpdateMultiplier = 0.0f;
            lava.SurfaceColor = Calc.HexToColor("a6fff4");
            lava.EdgeColor = Calc.HexToColor("6cd6eb");
            lava.CenterColor = Calc.HexToColor("4ca8d6");
            lava.SmallWaveAmplitude = 1f;
            lava.BigWaveAmplitude = 1f;
            lava.CurveAmplitude = 1f;
            lava.Spikey = 3f;
            Depth = -8500;
        }

        public IceBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(solid = new Solid(Position + new Vector2(2f, 3f), Width - 4f, Height - 5f, false));
            Collidable = solid.Collidable = SceneAs<Level>().CoreMode == Session.CoreModes.Cold;
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            Collidable = solid.Collidable = mode == Session.CoreModes.Cold;
            if (Collidable)
            {
                return;
            }

            Level level = SceneAs<Level>();
            Vector2 center = Center;
            for (int index1 = 0; index1 < (double)Width; index1 += 4)
            {
                for (int index2 = 0; index2 < (double)Height; index2 += 4)
                {
                    Vector2 position = Position + new Vector2(index1 + 2, index2 + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                    level.Particles.Emit(IceBlock.P_Deactivate, position, (position - center).Angle());
                }
            }
        }

        private void OnPlayer(Player player)
        {
            _ = player.Die((player.Center - Center).SafeNormalize());
        }

        public override void Render()
        {
            if (!Collidable)
            {
                return;
            }

            base.Render();
        }
    }
}
