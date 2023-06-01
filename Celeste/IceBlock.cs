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
        private LavaRect lava;
        private Solid solid;

        public IceBlock(Vector2 position, float width, float height)
            : base(position)
        {
            this.Collider = (Collider) new Hitbox(width, height);
            this.Add((Component) new CoreModeListener(new Action<Session.CoreModes>(this.OnChangeMode)));
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) (this.lava = new LavaRect(width, height, 2)));
            this.lava.UpdateMultiplier = 0.0f;
            this.lava.SurfaceColor = Calc.HexToColor("a6fff4");
            this.lava.EdgeColor = Calc.HexToColor("6cd6eb");
            this.lava.CenterColor = Calc.HexToColor("4ca8d6");
            this.lava.SmallWaveAmplitude = 1f;
            this.lava.BigWaveAmplitude = 1f;
            this.lava.CurveAmplitude = 1f;
            this.lava.Spikey = 3f;
            this.Depth = -8500;
        }

        public IceBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add((Entity) (this.solid = new Solid(this.Position + new Vector2(2f, 3f), this.Width - 4f, this.Height - 5f, false)));
            this.Collidable = this.solid.Collidable = this.SceneAs<Level>().CoreMode == Session.CoreModes.Cold;
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            this.Collidable = this.solid.Collidable = mode == Session.CoreModes.Cold;
            if (this.Collidable)
                return;
            Level level = this.SceneAs<Level>();
            Vector2 center = this.Center;
            for (int index1 = 0; (double) index1 < (double) this.Width; index1 += 4)
            {
                for (int index2 = 0; (double) index2 < (double) this.Height; index2 += 4)
                {
                    Vector2 position = this.Position + new Vector2((float) (index1 + 2), (float) (index2 + 2)) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                    level.Particles.Emit(IceBlock.P_Deactivate, position, (position - center).Angle());
                }
            }
        }

        private void OnPlayer(Player player) => player.Die((player.Center - this.Center).SafeNormalize());

        public override void Render()
        {
            if (!this.Collidable)
                return;
            base.Render();
        }
    }
}
