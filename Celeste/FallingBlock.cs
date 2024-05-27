using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    [Tracked]
    public class FallingBlock : Solid
    {
        public static ParticleType P_FallDustA;
        public static ParticleType P_FallDustB;
        public static ParticleType P_LandDust;
        public bool Triggered;
        public float FallDelay;
        private char TileType;
        private TileGrid tiles;
        private TileGrid highlight;
        private bool finalBoss;
        private bool climbFall;

        public FallingBlock(
            Vector2 position,
            char tile,
            int width,
            int height,
            bool finalBoss,
            bool behind,
            bool climbFall)
            : base(position, width, height, false)
        {
            this.finalBoss = finalBoss;
            this.climbFall = climbFall;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, width / 8, height / 8).TileGrid);
            Calc.PopRandom();
            if (finalBoss)
            {
                Calc.PushRandom(newSeed);
                Add(highlight = GFX.FGAutotiler.GenerateBox('G', width / 8, height / 8).TileGrid);
                Calc.PopRandom();
                highlight.Alpha = 0.0f;
            }
            Add(new Coroutine(Sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, false));
            TileType = tile;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
            if (!behind)
                return;
            Depth = 5000;
        }

        public FallingBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, false, data.Bool("behind"), data.Bool(nameof (climbFall), true))
        {
        }

        public static FallingBlock CreateFinalBossBlock(EntityData data, Vector2 offset) => new FallingBlock(data.Position + offset, 'g', data.Width, data.Height, true, false, false);

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
            if (highlight == null)
                return;
            highlight.Position += amount;
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            if (finalBoss)
                return;
            Triggered = true;
        }

        public bool HasStartedFalling { get; private set; }

        private bool PlayerFallCheck() => climbFall ? HasPlayerRider() : HasPlayerOnTop();

        private bool PlayerWaitCheck()
        {
            if (Triggered || PlayerFallCheck())
                return true;
            if (!climbFall)
                return false;
            return CollideCheck<Player>(Position - Vector2.UnitX) || CollideCheck<Player>(Position + Vector2.UnitX);
        }

        private IEnumerator Sequence()
        {
            FallingBlock fallingBlock = this;
            while (!fallingBlock.Triggered && (fallingBlock.finalBoss || !fallingBlock.PlayerFallCheck()))
                yield return null;
            while (fallingBlock.FallDelay > 0.0)
            {
                fallingBlock.FallDelay -= Engine.DeltaTime;
                yield return null;
            }
            fallingBlock.HasStartedFalling = true;
label_6:
            fallingBlock.ShakeSfx();
            fallingBlock.StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (fallingBlock.finalBoss)
                fallingBlock.Add(new Coroutine(fallingBlock.HighlightFade(1f)));
            yield return 0.2f;
            float timer = 0.4f;
            if (fallingBlock.finalBoss)
                timer = 0.2f;
            for (; timer > 0.0 && fallingBlock.PlayerWaitCheck(); timer -= Engine.DeltaTime)
                yield return null;
            fallingBlock.StopShaking();
            for (int x = 2; x < (double) fallingBlock.Width; x += 4)
            {
                if (fallingBlock.Scene.CollideCheck<Solid>(fallingBlock.TopLeft + new Vector2(x, -2f)))
                    fallingBlock.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(fallingBlock.X + x, fallingBlock.Y), Vector2.One * 4f, 1.57079637f);
                fallingBlock.SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(fallingBlock.X + x, fallingBlock.Y), Vector2.One * 4f);
            }
            float speed = 0.0f;
            float maxSpeed = fallingBlock.finalBoss ? 130f : 160f;
            Level level;
            while (true)
            {
                level = fallingBlock.SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                if (!fallingBlock.MoveVCollideSolids(speed * Engine.DeltaTime, true))
                {
                    if (fallingBlock.Top <= (double) (level.Bounds.Bottom + 16) && (fallingBlock.Top <= (double) (level.Bounds.Bottom - 1) || !fallingBlock.CollideCheck<Solid>(fallingBlock.Position + new Vector2(0.0f, 1f))))
                    {
                        yield return null;
                        level = null;
                    }
                    else
                        goto label_23;
                }
                else
                    break;
            }
            fallingBlock.ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            fallingBlock.SceneAs<Level>().DirectionalShake(Vector2.UnitY, fallingBlock.finalBoss ? 0.2f : 0.3f);
            if (fallingBlock.finalBoss)
                fallingBlock.Add(new Coroutine(fallingBlock.HighlightFade(0.0f)));
            fallingBlock.StartShaking();
            fallingBlock.LandParticles();
            yield return 0.2f;
            fallingBlock.StopShaking();
            if (fallingBlock.CollideCheck<SolidTiles>(fallingBlock.Position + new Vector2(0.0f, 1f)))
            {
                fallingBlock.Safe = true;
                yield break;
            }
            while (fallingBlock.CollideCheck<Platform>(fallingBlock.Position + new Vector2(0.0f, 1f)))
                yield return 0.1f;
            goto label_6;
            label_23:
            fallingBlock.Collidable = fallingBlock.Visible = false;
            yield return 0.2f;
            if (level.Session.MapData.CanTransitionTo(level, new Vector2(fallingBlock.Center.X, fallingBlock.Bottom + 12f)))
            {
                yield return 0.2f;
                fallingBlock.SceneAs<Level>().Shake();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            fallingBlock.RemoveSelf();
            fallingBlock.DestroyStaticMovers();
        }

        private IEnumerator HighlightFade(float to)
        {
            float from = highlight.Alpha;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.5f)
            {
                highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
                tiles.Alpha = 1f - highlight.Alpha;
                yield return null;
            }
            highlight.Alpha = to;
            tiles.Alpha = 1f - to;
        }

        private void LandParticles()
        {
            for (int x = 2; x <= (double) Width; x += 4)
            {
                if (Scene.CollideCheck<Solid>(BottomLeft + new Vector2(x, 3f)))
                {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + x, Bottom), Vector2.One * 4f, -1.57079637f);
                    float direction = x >= Width / 2.0 ? 0.0f : 3.14159274f;
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + x, Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private void ShakeSfx()
        {
            if (TileType == '3')
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", Center);
            else if (TileType == '9')
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", Center);
            else if (TileType == 'g')
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", Center);
            else
                Audio.Play("event:/game/general/fallblock_shake", Center);
        }

        private void ImpactSfx()
        {
            if (TileType == '3')
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", BottomCenter);
            else if (TileType == '9')
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", BottomCenter);
            else if (TileType == 'g')
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", BottomCenter);
            else
                Audio.Play("event:/game/general/fallblock_impact", BottomCenter);
        }
    }
}
