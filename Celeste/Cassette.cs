// Decompiled with JetBrains decompiler
// Type: Celeste.Cassette
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Cassette : Entity
    {
        public static ParticleType P_Shine;
        public static ParticleType P_Collect;
        public bool IsGhost;
        private Sprite sprite;
        private SineWave hover;
        private BloomPoint bloom;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private bool collected;
        private readonly Vector2[] nodes;
        private EventInstance remixSfx;
        private bool collecting;

        public Cassette(Vector2 position, Vector2[] nodes)
            : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            this.nodes = nodes;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public Cassette(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.NodesOffset(offset))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            IsGhost = SaveData.Instance.Areas[SceneAs<Level>().Session.Area.ID].Cassette;
            Add(sprite = GFX.SpriteBank.Create(IsGhost ? "cassetteGhost" : "cassette"));
            sprite.Play("idle");
            Add(scaleWiggler = Wiggler.Create(0.25f, 4f, f => sprite.Scale = Vector2.One * (1 + (f * 0.25f))));
            Add(bloom = new BloomPoint(0.25f, 16f));
            Add(light = new VertexLight(Color.White, 0.4f, 32, 64));
            Add(hover = new SineWave(0.5f));
            hover.OnUpdate = f => sprite.Y = light.Y = bloom.Y = f * 2f;
            if (!IsGhost)
                return;

            sprite.Color = Color.White * 0.8f;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(remixSfx);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(remixSfx);
        }

        public override void Update()
        {
            base.Update();
            if (collecting || !Scene.OnInterval(0.1f))
                return;

            SceneAs<Level>().Particles.Emit(P_Shine, 1, Center, new Vector2(12f, 10f));
        }

        private void OnPlayer(Player player)
        {
            if (collected)
                return;

            player?.RefillStamina();
            _ = Audio.Play("event:/game/general/cassette_get", Position);
            collected = true;
            Celeste.Freeze(0.1f);
            Add(new Coroutine(CollectRoutine(player)));
        }

        private IEnumerator CollectRoutine(Player player)
        {
            Cassette cassette = this;
            cassette.collecting = true;
            Level level = cassette.Scene as Level;
            CassetteBlockManager cbm = cassette.Scene.Tracker.GetEntity<CassetteBlockManager>();
            level.PauseLock = true;
            level.Frozen = true;
            cassette.Tag = (int)Tags.FrozenUpdate;
            level.Session.Cassette = true;
            level.Session.RespawnPoint = new Vector2?(level.GetSpawnPoint(cassette.nodes[1]));
            level.Session.UpdateLevelStartDashes();
            SaveData.Instance.RegisterCassette(level.Session.Area);
            cbm?.StopBlocks();
            cassette.Depth = -1000000;
            level.Shake();
            level.Flash(Color.White);
            level.Displacement.Clear();
            Vector2 camWas = level.Camera.Position;
            Vector2 camTo = (cassette.Position - new Vector2(160f, 90f)).Clamp(level.Bounds.Left - 64, level.Bounds.Top - 32, level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
            level.Camera.Position = camTo;
            level.ZoomSnap((cassette.Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2f);
            cassette.sprite.Play("spin", true);
            cassette.sprite.Rate = 2f;
            float p;
            for (p = 0f; p < 1.5; p += Engine.DeltaTime)
            {
                cassette.sprite.Rate += Engine.DeltaTime * 4f;
                yield return null;
            }
            cassette.sprite.Rate = 0f;
            cassette.sprite.SetAnimationFrame(0);
            cassette.scaleWiggler.Start();
            yield return 0.25f;
            Vector2 from = cassette.Position;
            Vector2 to = new(cassette.X, level.Camera.Top - 16f);
            float duration = 0.4f;
            for (p = 0f; p < 1; p += Engine.DeltaTime / duration)
            {
                cassette.sprite.Scale.X = MathHelper.Lerp(1f, 0.1f, p);
                cassette.sprite.Scale.Y = MathHelper.Lerp(1f, 3f, p);
                cassette.Position = Vector2.Lerp(from, to, Ease.CubeIn(p));
                yield return null;
            }
            cassette.Visible = false;
            from = new Vector2();
            to = new Vector2();
            cassette.remixSfx = Audio.Play("event:/game/general/cassette_preview", "remix", level.Session.Area.ID);
            UnlockedBSide message = new();
            cassette.Scene.Add(message);
            yield return message.EaseIn();
            while (!Input.MenuConfirm.Pressed)
                yield return null;

            Audio.SetParameter(cassette.remixSfx, "end", 1f);
            yield return message.EaseOut();
            message = null;
            duration = 0.25f;
            cassette.Add(new Coroutine(level.ZoomBack(duration - 0.05f)));
            for (p = 0f; p < 1; p += Engine.DeltaTime / duration)
            {
                level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p));
                yield return null;
            }
            if (!player.Dead && cassette.nodes != null && cassette.nodes.Length >= 2)
            {
                _ = Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(cassette.nodes[1], cassette.nodes[0]);
            }
            foreach (SandwichLava sandwichLava in level.Entities.FindAll<SandwichLava>())
                sandwichLava.Leave();

            level.Frozen = false;
            yield return 0.25f;
            cbm?.Finish();
            level.PauseLock = false;
            level.ResetZoom();
            cassette.RemoveSelf();
        }

        private class UnlockedBSide : Entity
        {
            private float alpha;
            private string text;
            private bool waitForKeyPress;
            private float timer;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
                text = ActiveFont.FontSize.AutoNewline(Dialog.Clean("UI_REMIX_UNLOCKED"), 900);
                Depth = -10000;
            }

            public IEnumerator EaseIn()
            {
                UnlockedBSide unlockedBside = this;
                _ = unlockedBside.Scene;
                while ((unlockedBside.alpha += Engine.DeltaTime / 0.5f) < 1)
                    yield return null;

                unlockedBside.alpha = 1f;
                yield return 1.5f;
                unlockedBside.waitForKeyPress = true;
            }

            public IEnumerator EaseOut()
            {
                UnlockedBSide unlockedBside = this;
                unlockedBside.waitForKeyPress = false;
                while ((unlockedBside.alpha -= Engine.DeltaTime / 0.5f) > 0)
                    yield return null;

                unlockedBside.alpha = 0f;
                unlockedBside.RemoveSelf();
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            public override void Render()
            {
                float num = Ease.CubeOut(alpha);
                Vector2 vector2_1 = Celeste.TargetCenter + new Vector2(0f, 64f);
                Vector2 vector2_2 = Vector2.UnitY * 64f * (1f - num);
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
                GFX.Gui["collectables/cassette"].DrawJustified(vector2_1 - vector2_2 + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * num);
                ActiveFont.Draw(text, vector2_1 + vector2_2, new Vector2(0.5f, 0f), Vector2.One, Color.White * num);
                if (!waitForKeyPress)
                    return;

                GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + (timer % 1 < 0.25f ? 6 : 0)));
            }
        }
    }
}
