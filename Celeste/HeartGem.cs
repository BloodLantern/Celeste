using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class HeartGem : Entity
    {
        private const string FAKE_HEART_FLAG = "fake_heart";
        public static ParticleType P_BlueShine;
        public static ParticleType P_RedShine;
        public static ParticleType P_GoldShine;
        public static ParticleType P_FakeShine;
        public bool IsGhost;
        public const float GhostAlpha = 0.8f;
        public bool IsFake;
        private Sprite sprite;
        private Sprite white;
        private ParticleType shineParticle;
        public Wiggler ScaleWiggler;
        private Wiggler moveWiggler;
        private Vector2 moveWiggleDir;
        private BloomPoint bloom;
        private VertexLight light;
        private Poem poem;
        private BirdNPC bird;
        private float timer;
        private bool collected;
        private bool autoPulse = true;
        private float bounceSfxDelay;
        private bool removeCameraTriggers;
        private SoundEmitter sfx;
        private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();
        private HoldableCollider holdableCollider;
        private EntityID entityID;
        private InvisibleBarrier fakeRightWall;

        public HeartGem(Vector2 position)
            : base(position)
        {
            Add(holdableCollider = new HoldableCollider(OnHoldable));
            Add(new MirrorReflection());
        }

        public HeartGem(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
            removeCameraTriggers = data.Bool(nameof (removeCameraTriggers));
            IsFake = data.Bool("fake");
            entityID = new EntityID(data.Level.Name, data.ID);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AreaKey area = (Scene as Level).Session.Area;
            IsGhost = !IsFake && SaveData.Instance.Areas[area.ID].Modes[(int) area.Mode].HeartGem;
            string id = !IsFake ? (!IsGhost ? "heartgem" + (int) area.Mode : "heartGemGhost") : "heartgem3";
            Add(sprite = GFX.SpriteBank.Create(id));
            sprite.Play("spin");
            sprite.OnLoop = anim =>
            {
                if (!Visible || !(anim == "spin") || !autoPulse)
                    return;
                if (IsFake)
                    Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
                else
                    Audio.Play("event:/game/general/crystalheart_pulse", Position);
                ScaleWiggler.Start();
                (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
            };
            if (IsGhost)
                sprite.Color = Color.White * 0.8f;
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (float) (1.0 + f * 0.25)));
            Add(bloom = new BloomPoint(0.75f, 16f));
            Color color;
            if (IsFake)
            {
                color = Calc.HexToColor("dad8cc");
                shineParticle = HeartGem.P_FakeShine;
            }
            else if (area.Mode == AreaMode.Normal)
            {
                color = Color.Aqua;
                shineParticle = HeartGem.P_BlueShine;
            }
            else if (area.Mode == AreaMode.BSide)
            {
                color = Color.Red;
                shineParticle = HeartGem.P_RedShine;
            }
            else
            {
                color = Color.Gold;
                shineParticle = HeartGem.P_GoldShine;
            }
            Add(light = new VertexLight(Color.Lerp(color, Color.White, 0.5f), 1f, 32, 64));
            if (IsFake)
            {
                bloom.Alpha = 0.0f;
                light.Alpha = 0.0f;
            }
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            if (!IsFake)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.X > (double) X || (scene as Level).Session.GetFlag("fake_heart"))
            {
                Visible = false;
                Alarm.Set(this, 0.0001f, () =>
                {
                    FakeRemoveCameraTrigger();
                    RemoveSelf();
                });
            }
            else
                scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(X + 160f, Y - 200f), 8f, 400f));
        }

        public override void Update()
        {
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = Vector2.UnitY * (float) Math.Sin(timer * 2.0) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
            if (white != null)
            {
                white.Position = sprite.Position;
                white.Scale = sprite.Scale;
                if (white.CurrentAnimationID != sprite.CurrentAnimationID)
                    white.Play(sprite.CurrentAnimationID);
                white.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
            if (collected)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null || entity.Dead)
                    EndCutscene();
            }
            base.Update();
            if (collected || !Scene.OnInterval(0.1f))
                return;
            SceneAs<Level>().Particles.Emit(shineParticle, 1, Center, Vector2.One * 8f);
        }

        public void OnHoldable(Holdable h)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (collected || entity == null || !h.Dangerous(holdableCollider))
                return;
            Collect(entity);
        }

        public void OnPlayer(Player player)
        {
            if (collected || (Scene as Level).Frozen)
                return;
            if (player.DashAttacking)
            {
                Collect(player);
            }
            else
            {
                if (bounceSfxDelay <= 0.0)
                {
                    if (IsFake)
                        Audio.Play("event:/new_content/game/10_farewell/fakeheart_bounce", Position);
                    else
                        Audio.Play("event:/game/general/crystalheart_bounce", Position);
                    bounceSfxDelay = 0.1f;
                }
                player.PointBounce(Center);
                moveWiggler.Start();
                ScaleWiggler.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }

        private void Collect(Player player)
        {
            Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
            Add(new Coroutine(CollectRoutine(player))
            {
                UseRawDeltaTime = true
            });
            collected = true;
            if (!removeCameraTriggers)
                return;
            foreach (Entity entity in Scene.Entities.FindAll<CameraOffsetTrigger>())
                entity.RemoveSelf();
        }

        private IEnumerator CollectRoutine(Player player)
        {
            HeartGem follow = this;
            Level level = follow.Scene as Level;
            AreaKey area = level.Session.Area;
            string poemID = AreaData.Get(level).Mode[(int) area.Mode].PoemID;
            bool completeArea = !follow.IsFake && (area.Mode != AreaMode.Normal || area.ID == 9);
            if (follow.IsFake)
                level.StartCutscene(follow.SkipFakeHeartCutscene);
            else
                level.CanRetry = false;
            if (completeArea || follow.IsFake)
            {
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
            }
            if (completeArea)
            {
                List<Strawberry> strawberryList = new List<Strawberry>();
                foreach (Follower follower in player.Leader.Followers)
                {
                    if (follower.Entity is Strawberry)
                        strawberryList.Add(follower.Entity as Strawberry);
                }
                foreach (Strawberry strawberry in strawberryList)
                    strawberry.OnCollect();
            }
            string sfx = "event:/game/general/crystalheart_blue_get";
            if (follow.IsFake)
                sfx = "event:/new_content/game/10_farewell/fakeheart_get";
            else if (area.Mode == AreaMode.BSide)
                sfx = "event:/game/general/crystalheart_red_get";
            else if (area.Mode == AreaMode.CSide)
                sfx = "event:/game/general/crystalheart_gold_get";
            follow.sfx = SoundEmitter.Play(sfx, follow);
            // ISSUE: reference to a compiler-generated method
            follow.Add(new LevelEndingHook(delegate
            {
                    this.sfx.Source.Stop();
            }));
            follow.walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
            List<InvisibleBarrier> walls1 = follow.walls;
            Rectangle bounds = level.Bounds;
            double x = bounds.Left - 8;
            bounds = level.Bounds;
            double top = bounds.Top;
            InvisibleBarrier invisibleBarrier1 = new InvisibleBarrier(new Vector2((float) x, (float) top), 8f, level.Bounds.Height);
            walls1.Add(invisibleBarrier1);
            List<InvisibleBarrier> walls2 = follow.walls;
            bounds = level.Bounds;
            double left = bounds.Left;
            bounds = level.Bounds;
            double y = bounds.Top - 8;
            InvisibleBarrier invisibleBarrier2 = new InvisibleBarrier(new Vector2((float) left, (float) y), level.Bounds.Width, 8f);
            walls2.Add(invisibleBarrier2);
            foreach (InvisibleBarrier wall in follow.walls)
                follow.Scene.Add(wall);
            follow.Add(follow.white = GFX.SpriteBank.Create("heartGemWhite"));
            follow.Depth = -2000000;
            yield return null;
            Celeste.Freeze(0.2f);
            yield return null;
            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            for (int index = 0; index < 10; ++index)
                follow.Scene.Add(new AbsorbOrb(follow.Position));
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            follow.light.Alpha = follow.bloom.Alpha = 0.0f;
            follow.Visible = false;
            float t;
            for (t = 0.0f; t < 2.0; t += Engine.RawDeltaTime)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0.0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }
            yield return null;
            if (player.Dead)
                yield return 100f;
            Engine.TimeRate = 1f;
            follow.Tag = (int) Tags.FrozenUpdate;
            level.Frozen = true;
            if (!follow.IsFake)
            {
                follow.RegisterAsCollected(level, poemID);
                if (completeArea)
                {
                    level.TimerStopped = true;
                    level.RegisterAreaComplete();
                }
            }
            string text = null;
            if (!string.IsNullOrEmpty(poemID))
                text = Dialog.Clean("poem_" + poemID);
            follow.poem = new Poem(text, follow.IsFake ? 3 : (int) area.Mode, area.Mode == AreaMode.CSide || follow.IsFake ? 1f : 0.6f);
            follow.poem.Alpha = 0.0f;
            follow.Scene.Add(follow.poem);
            for (t = 0.0f; t < 1.0; t += Engine.RawDeltaTime)
            {
                follow.poem.Alpha = Ease.CubeOut(t);
                yield return null;
            }
            if (follow.IsFake)
            {
                yield return follow.DoFakeRoutineWithBird(player);
            }
            else
            {
                while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
                    yield return null;
                follow.sfx.Source.Param("end", 1f);
                if (!completeArea)
                {
                    level.FormationBackdrop.Display = false;
                    for (t = 0.0f; t < 1.0; t += Engine.RawDeltaTime * 2f)
                    {
                        follow.poem.Alpha = Ease.CubeIn(1f - t);
                        yield return null;
                    }
                    player.Depth = 0;
                    follow.EndCutscene();
                }
                else
                {
                    FadeWipe fadeWipe = new FadeWipe(level, false);
                    fadeWipe.Duration = 3.25f;
                    yield return fadeWipe.Duration;
                    level.CompleteArea(false, true);
                }
            }
        }

        private void EndCutscene()
        {
            Level scene = Scene as Level;
            scene.Frozen = false;
            scene.CanRetry = true;
            scene.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (poem != null)
                poem.RemoveSelf();
            foreach (Entity wall in walls)
                wall.RemoveSelf();
            RemoveSelf();
        }

        private void RegisterAsCollected(Level level, string poemID)
        {
            level.Session.HeartGem = true;
            level.Session.UpdateLevelStartDashes();
            int unlockedModes = SaveData.Instance.UnlockedModes;
            SaveData.Instance.RegisterHeartGem(level.Session.Area);
            if (!string.IsNullOrEmpty(poemID))
                SaveData.Instance.RegisterPoemEntry(poemID);
            if (unlockedModes < 3 && SaveData.Instance.UnlockedModes >= 3)
                level.Session.UnlockedCSide = true;
            if (SaveData.Instance.TotalHeartGems < 24)
                return;
            //Achievements.Register(Achievement.CSIDES);
        }

        private IEnumerator DoFakeRoutineWithBird(Player player)
        {
            HeartGem heartGem = this;
            Level level = heartGem.Scene as Level;
            int panAmount = 64;
            Vector2 panFrom = level.Camera.Position;
            Vector2 panTo = level.Camera.Position + new Vector2(-panAmount, 0.0f);
            Vector2 birdFrom = new Vector2(panTo.X - 16f, player.Y - 20f);
            Vector2 birdTo = new Vector2((float) (panFrom.X + 320.0 + 16.0), player.Y - 20f);
            yield return 2f;
            Glitch.Value = 0.75f;
            while (Glitch.Value > 0.0)
            {
                Glitch.Value = Calc.Approach(Glitch.Value, 0.0f, Engine.RawDeltaTime * 4f);
                level.Shake();
                yield return null;
            }
            yield return 1.1f;
            Glitch.Value = 0.75f;
            while (Glitch.Value > 0.0)
            {
                Glitch.Value = Calc.Approach(Glitch.Value, 0.0f, Engine.RawDeltaTime * 4f);
                level.Shake();
                yield return null;
            }
            yield return 0.4f;
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 2f)
            {
                level.Camera.Position = panFrom + (panTo - panFrom) * Ease.CubeInOut(p);
                heartGem.poem.Offset = new Vector2(panAmount * 8, 0.0f) * Ease.CubeInOut(p);
                yield return null;
            }
            heartGem.bird = new BirdNPC(birdFrom, BirdNPC.Modes.None);
            heartGem.bird.Sprite.Play("fly");
            heartGem.bird.Sprite.UseRawDeltaTime = true;
            heartGem.bird.Facing = Facings.Right;
            heartGem.bird.Depth = -2000100;
            heartGem.bird.Tag = (int) Tags.FrozenUpdate;
            heartGem.bird.Add(new VertexLight(Color.White, 0.5f, 8, 32));
            heartGem.bird.Add(new BloomPoint(0.5f, 12f));
            level.Add(heartGem.bird);
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 2.6f)
            {
                level.Camera.Position = panTo + (panFrom - panTo) * Ease.CubeInOut(p);
                heartGem.poem.Offset = new Vector2(panAmount * 8, 0.0f) * Ease.CubeInOut(1f - p);
                float num1 = 0.1f;
                float num2 = 0.9f;
                if (p > (double) num1 && p <= (double) num2)
                {
                    float num3 = (float) ((p - (double) num1) / (num2 - (double) num1));
                    heartGem.bird.Position = birdFrom + (birdTo - birdFrom) * num3 + Vector2.UnitY * (float) Math.Sin(num3 * 8.0) * 8f;
                }
                if (level.OnRawInterval(0.2f))
                    TrailManager.Add(heartGem.bird, Calc.HexToColor("639bff"), frozenUpdate: true, useRawDeltaTime: true);
                yield return null;
            }
            heartGem.bird.RemoveSelf();
            heartGem.bird = null;
            Engine.TimeRate = 0.0f;
            level.Frozen = false;
            player.Active = false;
            player.StateMachine.State = 11;
            while (Engine.TimeRate != 1.0)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 0.5f * Engine.RawDeltaTime);
                yield return null;
            }
            Engine.TimeRate = 1f;
            yield return Textbox.Say("CH9_FAKE_HEART");
            heartGem.sfx.Source.Param("end", 1f);
            yield return 0.283f;
            level.FormationBackdrop.Display = false;
            for (p = 0.0f; p < 1.0; p += Engine.RawDeltaTime / 0.2f)
            {
                heartGem.poem.TextAlpha = Ease.CubeIn(1f - p);
                heartGem.poem.ParticleSpeed = heartGem.poem.TextAlpha;
                yield return null;
            }
            heartGem.poem.Heart.Play("break");
            while (heartGem.poem.Heart.Animating)
            {
                heartGem.poem.Shake += Engine.DeltaTime;
                yield return null;
            }
            heartGem.poem.RemoveSelf();
            heartGem.poem = null;
            for (int index = 0; index < 10; ++index)
            {
                Vector2 position = level.Camera.Position + new Vector2(320f, 180f) * 0.5f;
                Vector2 vector2 = level.Camera.Position + new Vector2(160f, -64f);
                heartGem.Scene.Add(new AbsorbOrb(position, absorbTarget: vector2));
            }
            level.Shake();
            Glitch.Value = 0.8f;
            while (Glitch.Value > 0.0)
            {
                Glitch.Value -= Engine.DeltaTime * 4f;
                yield return null;
            }
            yield return 0.25f;
            level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/intermission_heartgroove";
            level.Session.Audio.Apply();
            player.Active = true;
            player.Depth = 0;
            player.StateMachine.State = 11;
            while (!player.OnGround() && player.Bottom < (double) level.Bounds.Bottom)
                yield return null;
            player.Facing = Facings.Right;
            yield return 0.5f;
            yield return Textbox.Say("CH9_KEEP_GOING", heartGem.PlayerStepForward);
            heartGem.SkipFakeHeartCutscene(level);
            level.EndCutscene();
        }

        private IEnumerator PlayerStepForward()
        {
            HeartGem heartGem = this;
            yield return 0.1f;
            Player entity = heartGem.Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.CollideCheck<Solid>(entity.Position + new Vector2(12f, 1f)))
                yield return entity.DummyWalkToExact((int) entity.X + 10);
            yield return 0.2f;
        }

        private void SkipFakeHeartCutscene(Level level)
        {
            Engine.TimeRate = 1f;
            Glitch.Value = 0.0f;
            if (sfx != null)
                sfx.Source.Stop();
            level.Session.SetFlag("fake_heart");
            level.Frozen = false;
            level.FormationBackdrop.Display = false;
            level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/intermission_heartgroove";
            level.Session.Audio.Apply();
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            if (entity1 != null)
            {
                entity1.Sprite.Play("idle");
                entity1.Active = true;
                entity1.StateMachine.State = 0;
                entity1.Dashes = 1;
                entity1.Speed = Vector2.Zero;
                entity1.MoveV(200f);
                entity1.Depth = 0;
                for (int index = 0; index < 10; ++index)
                    entity1.UpdateHair(true);
            }
            foreach (Entity entity2 in Scene.Entities.FindAll<AbsorbOrb>())
                entity2.RemoveSelf();
            if (poem != null)
                poem.RemoveSelf();
            if (bird != null)
                bird.RemoveSelf();
            if (fakeRightWall != null)
                fakeRightWall.RemoveSelf();
            FakeRemoveCameraTrigger();
            foreach (Entity wall in walls)
                wall.RemoveSelf();
            RemoveSelf();
        }

        private void FakeRemoveCameraTrigger()
        {
            CameraTargetTrigger cameraTargetTrigger = CollideFirst<CameraTargetTrigger>();
            if (cameraTargetTrigger == null)
                return;
            cameraTargetTrigger.LerpStrength = 0.0f;
        }
    }
}
