using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CrushBlock : Solid
    {
        public static ParticleType P_Impact;
        public static ParticleType P_Crushing;
        public static ParticleType P_Activate;
        private const float CrushSpeed = 240f;
        private const float CrushAccel = 500f;
        private const float ReturnSpeed = 60f;
        private const float ReturnAccel = 160f;
        private Color fill = Calc.HexToColor("62222b");
        private Level level;
        private bool canActivate;
        private Vector2 crushDir;
        private List<MoveState> returnStack;
        private Coroutine attackCoroutine;
        private bool canMoveVertically;
        private bool canMoveHorizontally;
        private bool chillOut;
        private bool giant;
        private Sprite face;
        private string nextFaceDirection;
        private List<Image> idleImages = new List<Image>();
        private List<Image> activeTopImages = new List<Image>();
        private List<Image> activeRightImages = new List<Image>();
        private List<Image> activeLeftImages = new List<Image>();
        private List<Image> activeBottomImages = new List<Image>();
        private SoundSource currentMoveLoopSfx;
        private SoundSource returnLoopSfx;

        public CrushBlock(
            Vector2 position,
            float width,
            float height,
            Axes axes,
            bool chillOut = false)
            : base(position, width, height, false)
        {
            OnDashCollide = OnDashed;
            returnStack = new List<MoveState>();
            this.chillOut = chillOut;
            giant = ((Width < 48.0 ? 0 : (Height >= 48.0 ? 1 : 0)) & (chillOut ? 1 : 0)) != 0;
            canActivate = true;
            attackCoroutine = new Coroutine();
            attackCoroutine.RemoveOnComplete = false;
            Add(attackCoroutine);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/crushblock/block");
            MTexture idle;
            switch (axes)
            {
                case Axes.Horizontal:
                    idle = atlasSubtextures[1];
                    canMoveHorizontally = true;
                    canMoveVertically = false;
                    break;
                case Axes.Vertical:
                    idle = atlasSubtextures[2];
                    canMoveHorizontally = false;
                    canMoveVertically = true;
                    break;
                default:
                    idle = atlasSubtextures[3];
                    canMoveHorizontally = canMoveVertically = true;
                    break;
            }
            Add(face = GFX.SpriteBank.Create(giant ? "giant_crushblock_face" : "crushblock_face"));
            face.Position = new Vector2(Width, Height) / 2f;
            face.Play("idle");
            face.OnLastFrame = f =>
            {
                if (!(f == "hit"))
                    return;
                face.Play(nextFaceDirection);
            };
            int x1 = (int) (Width / 8.0) - 1;
            int y1 = (int) (Height / 8.0) - 1;
            AddImage(idle, 0, 0, 0, 0, -1, -1);
            AddImage(idle, x1, 0, 3, 0, 1, -1);
            AddImage(idle, 0, y1, 0, 3, -1, 1);
            AddImage(idle, x1, y1, 3, 3, 1, 1);
            for (int x2 = 1; x2 < x1; ++x2)
            {
                AddImage(idle, x2, 0, Calc.Random.Choose(1, 2), 0, borderY: -1);
                AddImage(idle, x2, y1, Calc.Random.Choose(1, 2), 3, borderY: 1);
            }
            for (int y2 = 1; y2 < y1; ++y2)
            {
                AddImage(idle, 0, y2, 0, Calc.Random.Choose(1, 2), -1);
                AddImage(idle, x1, y2, 3, Calc.Random.Choose(1, 2), 1);
            }
            Add(new LightOcclude(0.2f));
            Add(returnLoopSfx = new SoundSource());
            Add(new WaterInteraction(() => crushDir != Vector2.Zero));
        }

        public CrushBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Enum<Axes>("axes"), data.Bool("chillout"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            if (crushDir == Vector2.Zero)
            {
                face.Position = new Vector2(Width, Height) / 2f;
                if (CollideCheck<Player>(Position + new Vector2(-1f, 0.0f)))
                    --face.X;
                else if (CollideCheck<Player>(Position + new Vector2(1f, 0.0f)))
                    ++face.X;
                else if (CollideCheck<Player>(Position + new Vector2(0.0f, -1f)))
                    --face.Y;
            }
            if (currentMoveLoopSfx != null)
                currentMoveLoopSfx.Param("submerged", Submerged ? 1f : 0.0f);
            if (returnLoopSfx == null)
                return;
            returnLoopSfx.Param("submerged", Submerged ? 1f : 0.0f);
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            Draw.Rect(X + 2f, Y + 2f, Width - 4f, Height - 4f, fill);
            base.Render();
            Position = position;
        }

        private bool Submerged => Scene.CollideCheck<Water>(new Rectangle((int) (Center.X - 4.0), (int) Center.Y, 8, 4));

        private void AddImage(
            MTexture idle,
            int x,
            int y,
            int tx,
            int ty,
            int borderX = 0,
            int borderY = 0)
        {
            MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
            Vector2 vector2 = new Vector2(x * 8, y * 8);
            if (borderX != 0)
            {
                Image image = new Image(subtexture);
                image.Color = Color.Black;
                image.Position = vector2 + new Vector2(borderX, 0.0f);
                Add(image);
            }
            if (borderY != 0)
            {
                Image image = new Image(subtexture);
                image.Color = Color.Black;
                image.Position = vector2 + new Vector2(0.0f, borderY);
                Add(image);
            }
            Image image1 = new Image(subtexture);
            image1.Position = vector2;
            Add(image1);
            idleImages.Add(image1);
            if (borderX == 0 && borderY == 0)
                return;
            if (borderX < 0)
            {
                Image image2 = new Image(GFX.Game["objects/crushblock/lit_left"].GetSubtexture(0, ty * 8, 8, 8));
                activeLeftImages.Add(image2);
                image2.Position = vector2;
                image2.Visible = false;
                Add(image2);
            }
            else if (borderX > 0)
            {
                Image image3 = new Image(GFX.Game["objects/crushblock/lit_right"].GetSubtexture(0, ty * 8, 8, 8));
                activeRightImages.Add(image3);
                image3.Position = vector2;
                image3.Visible = false;
                Add(image3);
            }
            if (borderY < 0)
            {
                Image image4 = new Image(GFX.Game["objects/crushblock/lit_top"].GetSubtexture(tx * 8, 0, 8, 8));
                activeTopImages.Add(image4);
                image4.Position = vector2;
                image4.Visible = false;
                Add(image4);
            }
            else
            {
                if (borderY <= 0)
                    return;
                Image image5 = new Image(GFX.Game["objects/crushblock/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8));
                activeBottomImages.Add(image5);
                image5.Position = vector2;
                image5.Visible = false;
                Add(image5);
            }
        }

        private void TurnOffImages()
        {
            foreach (Component activeLeftImage in activeLeftImages)
                activeLeftImage.Visible = false;
            foreach (Component activeRightImage in activeRightImages)
                activeRightImage.Visible = false;
            foreach (Component activeTopImage in activeTopImages)
                activeTopImage.Visible = false;
            foreach (Component activeBottomImage in activeBottomImages)
                activeBottomImage.Visible = false;
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!CanActivate(-direction))
                return DashCollisionResults.NormalCollision;
            Attack(-direction);
            return DashCollisionResults.Rebound;
        }

        private bool CanActivate(Vector2 direction) => (!giant || direction.X > 0.0) && canActivate && crushDir != direction && (direction.X == 0.0 || canMoveHorizontally) && (direction.Y == 0.0 || canMoveVertically);

        private void Attack(Vector2 direction)
        {
            Audio.Play("event:/game/06_reflection/crushblock_activate", Center);
            if (currentMoveLoopSfx != null)
            {
                currentMoveLoopSfx.Param("end", 1f);
                SoundSource sfx = currentMoveLoopSfx;
                Alarm.Set(this, 0.5f, () => sfx.RemoveSelf());
            }
            Add(currentMoveLoopSfx = new SoundSource());
            currentMoveLoopSfx.Position = new Vector2(Width, Height) / 2f;
            if (SaveData.Instance != null && SaveData.Instance.Name != null && SaveData.Instance.Name.StartsWith("FWAHAHA", StringComparison.InvariantCultureIgnoreCase))
                currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop_covert");
            else
                currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop");
            face.Play("hit");
            crushDir = direction;
            canActivate = false;
            attackCoroutine.Replace(AttackSequence());
            ClearRemainder();
            TurnOffImages();
            ActivateParticles(crushDir);
            if (crushDir.X < 0.0)
            {
                foreach (Component activeLeftImage in activeLeftImages)
                    activeLeftImage.Visible = true;
                nextFaceDirection = "left";
            }
            else if (crushDir.X > 0.0)
            {
                foreach (Component activeRightImage in activeRightImages)
                    activeRightImage.Visible = true;
                nextFaceDirection = "right";
            }
            else if (crushDir.Y < 0.0)
            {
                foreach (Component activeTopImage in activeTopImages)
                    activeTopImage.Visible = true;
                nextFaceDirection = "up";
            }
            else if (crushDir.Y > 0.0)
            {
                foreach (Component activeBottomImage in activeBottomImages)
                    activeBottomImage.Visible = true;
                nextFaceDirection = "down";
            }
            bool flag = true;
            if (returnStack.Count > 0)
            {
                MoveState moveState = returnStack[returnStack.Count - 1];
                if (moveState.Direction == direction || moveState.Direction == -direction)
                    flag = false;
            }
            if (!flag)
                return;
            returnStack.Add(new MoveState(Position, crushDir));
        }

        private void ActivateParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (dir == Vector2.UnitX)
            {
                direction = 0.0f;
                position = CenterRight - Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                num = (int) (Height / 8.0) * 4;
            }
            else if (dir == -Vector2.UnitX)
            {
                direction = 3.14159274f;
                position = CenterLeft + Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                num = (int) (Height / 8.0) * 4;
            }
            else if (dir == Vector2.UnitY)
            {
                direction = 1.57079637f;
                position = BottomCenter - Vector2.UnitY;
                positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                num = (int) (Width / 8.0) * 4;
            }
            else
            {
                direction = -1.57079637f;
                position = TopCenter + Vector2.UnitY;
                positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                num = (int) (Width / 8.0) * 4;
            }
            int amount = num + 2;
            level.Particles.Emit(CrushBlock.P_Activate, amount, position, positionRange, direction);
        }

        private IEnumerator AttackSequence()
        {
            CrushBlock crushBlock = this;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            crushBlock.StartShaking(0.4f);
            yield return 0.4f;
            if (!crushBlock.chillOut)
                crushBlock.canActivate = true;
            crushBlock.StopPlayerRunIntoAnimation = false;
            bool slowing = false;
            float speed = 0.0f;
            while (true)
            {
                if (!crushBlock.chillOut)
                    speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
                else if (slowing || crushBlock.CollideCheck<SolidTiles>(crushBlock.Position + crushBlock.crushDir * 256f))
                {
                    speed = Calc.Approach(speed, 24f, (float) (500.0 * Engine.DeltaTime * 0.25));
                    if (!slowing)
                    {
                        slowing = true;
                        Alarm.Set(crushBlock, 0.5f, () =>
                        {
                            face.Play("hurt");
                            currentMoveLoopSfx.Stop();
                            TurnOffImages();
                        });
                    }
                }
                else
                    speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
                bool flag = crushBlock.crushDir.X == 0.0 ? crushBlock.MoveVCheck(speed * crushBlock.crushDir.Y * Engine.DeltaTime) : crushBlock.MoveHCheck(speed * crushBlock.crushDir.X * Engine.DeltaTime);
                if (crushBlock.Top < (double) (crushBlock.level.Bounds.Bottom + 32))
                {
                    if (!flag)
                    {
                        if (crushBlock.Scene.OnInterval(0.02f))
                        {
                            Vector2 position;
                            float direction;
                            if (crushBlock.crushDir == Vector2.UnitX)
                            {
                                position = new Vector2(crushBlock.Left + 1f, Calc.Random.Range(crushBlock.Top + 3f, crushBlock.Bottom - 3f));
                                direction = 3.14159274f;
                            }
                            else if (crushBlock.crushDir == -Vector2.UnitX)
                            {
                                position = new Vector2(crushBlock.Right - 1f, Calc.Random.Range(crushBlock.Top + 3f, crushBlock.Bottom - 3f));
                                direction = 0.0f;
                            }
                            else if (crushBlock.crushDir == Vector2.UnitY)
                            {
                                position = new Vector2(Calc.Random.Range(crushBlock.Left + 3f, crushBlock.Right - 3f), crushBlock.Top + 1f);
                                direction = -1.57079637f;
                            }
                            else
                            {
                                position = new Vector2(Calc.Random.Range(crushBlock.Left + 3f, crushBlock.Right - 3f), crushBlock.Bottom - 1f);
                                direction = 1.57079637f;
                            }
                            crushBlock.level.Particles.Emit(CrushBlock.P_Crushing, position, direction);
                        }
                        yield return null;
                    }
                    else
                        goto label_13;
                }
                else
                    break;
            }
            crushBlock.RemoveSelf();
            yield break;
label_13:
            FallingBlock fallingBlock = crushBlock.CollideFirst<FallingBlock>(crushBlock.Position + crushBlock.crushDir);
            if (fallingBlock != null)
                fallingBlock.Triggered = true;
            if (crushBlock.crushDir == -Vector2.UnitX)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; index < crushBlock.Height / 8.0; ++index)
                {
                    Vector2 point = new Vector2(crushBlock.Left - 1f, crushBlock.Top + 4f + index * 8);
                    if (!crushBlock.Scene.CollideCheck<Water>(point) && crushBlock.Scene.CollideCheck<Solid>(point))
                    {
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 0.0f);
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 0.0f);
                    }
                }
            }
            else if (crushBlock.crushDir == Vector2.UnitX)
            {
                Vector2 vector2 = new Vector2(0.0f, 2f);
                for (int index = 0; index < crushBlock.Height / 8.0; ++index)
                {
                    Vector2 point = new Vector2(crushBlock.Right + 1f, crushBlock.Top + 4f + index * 8);
                    if (!crushBlock.Scene.CollideCheck<Water>(point) && crushBlock.Scene.CollideCheck<Solid>(point))
                    {
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 3.14159274f);
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 3.14159274f);
                    }
                }
            }
            else if (crushBlock.crushDir == -Vector2.UnitY)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; index < crushBlock.Width / 8.0; ++index)
                {
                    Vector2 point = new Vector2(crushBlock.Left + 4f + index * 8, crushBlock.Top - 1f);
                    if (!crushBlock.Scene.CollideCheck<Water>(point) && crushBlock.Scene.CollideCheck<Solid>(point))
                    {
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 1.57079637f);
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 1.57079637f);
                    }
                }
            }
            else if (crushBlock.crushDir == Vector2.UnitY)
            {
                Vector2 vector2 = new Vector2(2f, 0.0f);
                for (int index = 0; index < crushBlock.Width / 8.0; ++index)
                {
                    Vector2 point = new Vector2(crushBlock.Left + 4f + index * 8, crushBlock.Bottom + 1f);
                    if (!crushBlock.Scene.CollideCheck<Water>(point) && crushBlock.Scene.CollideCheck<Solid>(point))
                    {
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, -1.57079637f);
                        crushBlock.SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, -1.57079637f);
                    }
                }
            }
            Audio.Play("event:/game/06_reflection/crushblock_impact", crushBlock.Center);
            crushBlock.level.DirectionalShake(crushBlock.crushDir);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            crushBlock.StartShaking(0.4f);
            crushBlock.StopPlayerRunIntoAnimation = true;
            SoundSource sfx = crushBlock.currentMoveLoopSfx;
            crushBlock.currentMoveLoopSfx.Param("end", 1f);
            crushBlock.currentMoveLoopSfx = null;
            Alarm.Set(crushBlock, 0.5f, () => sfx.RemoveSelf());
            crushBlock.crushDir = Vector2.Zero;
            crushBlock.TurnOffImages();
            if (!crushBlock.chillOut)
            {
                crushBlock.face.Play("hurt");
                crushBlock.returnLoopSfx.Play("event:/game/06_reflection/crushblock_return_loop");
                yield return 0.4f;
                speed = 0.0f;
                float waypointSfxDelay = 0.0f;
                while (crushBlock.returnStack.Count > 0)
                {
                    yield return null;
                    crushBlock.StopPlayerRunIntoAnimation = false;
                    MoveState moveState = crushBlock.returnStack[crushBlock.returnStack.Count - 1];
                    speed = Calc.Approach(speed, 60f, 160f * Engine.DeltaTime);
                    waypointSfxDelay -= Engine.DeltaTime;
                    if (moveState.Direction.X != 0.0)
                        crushBlock.MoveTowardsX(moveState.From.X, speed * Engine.DeltaTime);
                    if (moveState.Direction.Y != 0.0)
                        crushBlock.MoveTowardsY(moveState.From.Y, speed * Engine.DeltaTime);
                    if ((moveState.Direction.X == 0.0 || crushBlock.ExactPosition.X == (double) moveState.From.X ? (moveState.Direction.Y == 0.0 ? 1 : (crushBlock.ExactPosition.Y == (double) moveState.From.Y ? 1 : 0)) : 0) != 0)
                    {
                        speed = 0.0f;
                        crushBlock.returnStack.RemoveAt(crushBlock.returnStack.Count - 1);
                        crushBlock.StopPlayerRunIntoAnimation = true;
                        if (crushBlock.returnStack.Count <= 0)
                        {
                            crushBlock.face.Play("idle");
                            crushBlock.returnLoopSfx.Stop();
                            if (waypointSfxDelay <= 0.0)
                                Audio.Play("event:/game/06_reflection/crushblock_rest", crushBlock.Center);
                        }
                        else if (waypointSfxDelay <= 0.0)
                            Audio.Play("event:/game/06_reflection/crushblock_rest_waypoint", crushBlock.Center);
                        waypointSfxDelay = 0.1f;
                        crushBlock.StartShaking(0.2f);
                        yield return 0.2f;
                    }
                }
            }
        }

        private bool MoveHCheck(float amount)
        {
            if (!MoveHCollideSolidsAndBounds(level, amount, true))
                return false;
            if (amount < 0.0 && Left <= (double) level.Bounds.Left || amount > 0.0 && Right >= (double) level.Bounds.Right)
                return true;
            for (int index1 = 1; index1 <= 4; ++index1)
            {
                for (int index2 = 1; index2 >= -1; index2 -= 2)
                {
                    if (!CollideCheck<Solid>(Position + new Vector2(Math.Sign(amount), index1 * index2)))
                    {
                        MoveVExact(index1 * index2);
                        MoveHExact(Math.Sign(amount));
                        return false;
                    }
                }
            }
            return true;
        }

        private bool MoveVCheck(float amount)
        {
            if (!MoveVCollideSolidsAndBounds(level, amount, true, checkBottom: false))
                return false;
            if (amount < 0.0 && Top <= (double) level.Bounds.Top)
                return true;
            for (int index1 = 1; index1 <= 4; ++index1)
            {
                for (int index2 = 1; index2 >= -1; index2 -= 2)
                {
                    if (!CollideCheck<Solid>(Position + new Vector2(index1 * index2, Math.Sign(amount))))
                    {
                        MoveHExact(index1 * index2);
                        MoveVExact(Math.Sign(amount));
                        return false;
                    }
                }
            }
            return true;
        }

        public enum Axes
        {
            Both,
            Horizontal,
            Vertical,
        }

        private struct MoveState
        {
            public Vector2 From;
            public Vector2 Direction;

            public MoveState(Vector2 from, Vector2 direction)
            {
                From = from;
                Direction = direction;
            }
        }
    }
}
