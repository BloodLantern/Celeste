// Decompiled with JetBrains decompiler
// Type: Celeste.IntroCrusher
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class IntroCrusher : Solid
    {
        private Vector2 shake;
        private Vector2 start;
        private Vector2 end;
        private TileGrid tilegrid;
        private SoundSource shakingSfx;

        public IntroCrusher(Vector2 position, int width, int height, Vector2 node)
            : base(position, (float) width, (float) height, true)
        {
            this.start = position;
            this.end = node;
            this.Depth = -10501;
            this.SurfaceSoundIndex = 4;
            this.Add((Component) (this.tilegrid = GFX.FGAutotiler.GenerateBox('3', width / 8, height / 8).TileGrid));
            this.Add((Component) (this.shakingSfx = new SoundSource()));
        }

        public IntroCrusher(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (this.SceneAs<Level>().Session.GetLevelFlag("1") || this.SceneAs<Level>().Session.GetLevelFlag("0b"))
                this.Position = this.end;
            else
                this.Add((Component) new Coroutine(this.Sequence()));
        }

        public override void Update()
        {
            this.tilegrid.Position = this.shake;
            base.Update();
        }

        private IEnumerator Sequence()
        {
            IntroCrusher introCrusher = this;
            Player entity1;
            do
            {
                yield return (object) null;
                entity1 = introCrusher.Scene.Tracker.GetEntity<Player>();
            }
            while (entity1 == null || (double) entity1.X < (double) introCrusher.X + 30.0 || (double) entity1.X > (double) introCrusher.Right + 8.0);
            introCrusher.shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            float time = 1.2f;
            // ISSUE: reference to a compiler-generated method
            Shaker shaker = new Shaker(time, true, delegate (Vector2 v)
            {
                    this.shake = v;
            });
            introCrusher.Add((Component) shaker);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (; (double) time > 0.0; time -= Engine.DeltaTime)
            {
                Player entity2 = introCrusher.Scene.Tracker.GetEntity<Player>();
                if (entity2 != null && ((double) entity2.X >= (double) introCrusher.X + (double) introCrusher.Width - 8.0 || (double) entity2.X < (double) introCrusher.X + 28.0))
                {
                    shaker.RemoveSelf();
                    break;
                }
                yield return (object) null;
            }
            shaker = (Shaker) null;
            for (int index = 2; (double) index < (double) introCrusher.Width; index += 4)
            {
                introCrusher.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(introCrusher.X + (float) index, introCrusher.Y), Vector2.One * 4f, 1.57079637f);
                introCrusher.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(introCrusher.X + (float) index, introCrusher.Y), Vector2.One * 4f);
            }
            introCrusher.shakingSfx.Param("release", 1f);
            time = 0.0f;
            do
            {
                yield return (object) null;
                time = Calc.Approach(time, 1f, 2f * Engine.DeltaTime);
                introCrusher.MoveTo(Vector2.Lerp(introCrusher.start, introCrusher.end, Ease.CubeIn(time)));
            }
            while ((double) time < 1.0);
            for (int index = 0; (double) index <= (double) introCrusher.Width; index += 4)
            {
                introCrusher.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(introCrusher.X + (float) index, introCrusher.Bottom), Vector2.One * 4f, -1.57079637f);
                float direction = (double) index >= (double) introCrusher.Width / 2.0 ? 0.0f : 3.14159274f;
                introCrusher.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(introCrusher.X + (float) index, introCrusher.Bottom), Vector2.One * 4f, direction);
            }
            introCrusher.shakingSfx.Stop();
            Audio.Play("event:/game/00_prologue/fallblock_first_impact", introCrusher.Position);
            introCrusher.SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            // ISSUE: reference to a compiler-generated method
            introCrusher.Add(new Shaker(0.25f, true, delegate (Vector2 v)
            {
                    this.shake = v;
            }));
        }
    }
}
