﻿using Microsoft.Xna.Framework;
using Monocle;
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
            : base(position, width, height, true)
        {
            start = position;
            end = node;
            Depth = -10501;
            SurfaceSoundIndex = 4;
            Add(tilegrid = GFX.FGAutotiler.GenerateBox('3', width / 8, height / 8).TileGrid);
            Add(shakingSfx = new SoundSource());
        }

        public IntroCrusher(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Session.GetLevelFlag("1") || SceneAs<Level>().Session.GetLevelFlag("0b"))
                Position = end;
            else
                Add(new Coroutine(Sequence()));
        }

        public override void Update()
        {
            tilegrid.Position = shake;
            base.Update();
        }

        private IEnumerator Sequence()
        {
            IntroCrusher introCrusher = this;
            Player entity1;
            do
            {
                yield return null;
                entity1 = introCrusher.Scene.Tracker.GetEntity<Player>();
            }
            while (entity1 == null || entity1.X < introCrusher.X + 30.0 || entity1.X > introCrusher.Right + 8.0);
            introCrusher.shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
            float time = 1.2f;
            // ISSUE: reference to a compiler-generated method
            Shaker shaker = new Shaker(time, true, delegate (Vector2 v)
            {
                    shake = v;
            });
            introCrusher.Add(shaker);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (; time > 0.0; time -= Engine.DeltaTime)
            {
                Player entity2 = introCrusher.Scene.Tracker.GetEntity<Player>();
                if (entity2 != null && (entity2.X >= introCrusher.X + (double) introCrusher.Width - 8.0 || entity2.X < introCrusher.X + 28.0))
                {
                    shaker.RemoveSelf();
                    break;
                }
                yield return null;
            }
            shaker = null;
            for (int index = 2; index < (double) introCrusher.Width; index += 4)
            {
                introCrusher.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(introCrusher.X + index, introCrusher.Y), Vector2.One * 4f, 1.57079637f);
                introCrusher.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(introCrusher.X + index, introCrusher.Y), Vector2.One * 4f);
            }
            introCrusher.shakingSfx.Param("release", 1f);
            time = 0.0f;
            do
            {
                yield return null;
                time = Calc.Approach(time, 1f, 2f * Engine.DeltaTime);
                introCrusher.MoveTo(Vector2.Lerp(introCrusher.start, introCrusher.end, Ease.CubeIn(time)));
            }
            while (time < 1.0);
            for (int index = 0; index <= (double) introCrusher.Width; index += 4)
            {
                introCrusher.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(introCrusher.X + index, introCrusher.Bottom), Vector2.One * 4f, -1.57079637f);
                float direction = index >= introCrusher.Width / 2.0 ? 0.0f : 3.14159274f;
                introCrusher.SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(introCrusher.X + index, introCrusher.Bottom), Vector2.One * 4f, direction);
            }
            introCrusher.shakingSfx.Stop();
            Audio.Play("event:/game/00_prologue/fallblock_first_impact", introCrusher.Position);
            introCrusher.SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            // ISSUE: reference to a compiler-generated method
            introCrusher.Add(new Shaker(0.25f, true, delegate (Vector2 v)
            {
                    shake = v;
            }));
        }
    }
}
