using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class FlingBirdIntro : Entity
    {
        public Vector2 BirdEndPosition;
        public Sprite Sprite;
        public SoundEmitter CrashSfxEmitter;
        private Vector2[] nodes;
        private bool startedRoutine;
        private Vector2 start;
        private InvisibleBarrier fakeRightWall;
        private bool crashes;
        private Coroutine flyToRoutine;
        private bool emitParticles;
        private bool inCutscene;

        public FlingBirdIntro(Vector2 position, Vector2[] nodes, bool crashes)
            : base(position)
        {
            this.crashes = crashes;
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.Play(crashes ? "hoverStressed" : "hover");
            Sprite.Scale.X = crashes ? -1f : 1f;
            Sprite.OnFrameChange = anim =>
            {
                if (inCutscene)
                    return;
                BirdNPC.FlapSfxCheck(Sprite);
            };
            Collider = new Circle(16f, y: -8f);
            Add(new PlayerCollider(OnPlayer));
            this.nodes = nodes;
            start = position;
            BirdEndPosition = nodes[nodes.Length - 1];
        }

        public FlingBirdIntro(EntityData data, Vector2 levelOffset)
            : this(data.Position + levelOffset, data.NodesOffset(levelOffset), data.Bool(nameof (crashes)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!crashes && (scene as Level).Session.GetFlag("MissTheBird"))
            {
                RemoveSelf();
            }
            else
            {
                Player entity1 = Scene.Tracker.GetEntity<Player>();
                if (entity1 != null && entity1.X > (double) X)
                {
                    if (crashes)
                        CS10_CatchTheBird.HandlePostCutsceneSpawn(this, scene as Level);
                    CassetteBlockManager entity2 = Scene.Tracker.GetEntity<CassetteBlockManager>();
                    if (entity2 != null)
                    {
                        entity2.StopBlocks();
                        entity2.Finish();
                    }
                    RemoveSelf();
                }
                else
                    scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(X + 160f, Y - 200f), 8f, 400f));
                if (crashes)
                    return;
                Vector2 position = Position;
                Position = new Vector2(X - 150f, (scene as Level).Bounds.Top - 8);
                Add(flyToRoutine = new Coroutine(FlyTo(position)));
            }
        }

        private IEnumerator FlyTo(Vector2 to)
        {
            FlingBirdIntro flingBirdIntro = this;
            flingBirdIntro.Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_flappyscene_entry"));
            flingBirdIntro.Sprite.Play("fly");
            Vector2 from = flingBirdIntro.Position;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 0.3f)
            {
                flingBirdIntro.Position = from + (to - from) * Ease.SineOut(p);
                yield return null;
            }
            flingBirdIntro.Sprite.Play("hover");
            float sine = 0.0f;
            while (true)
            {
                flingBirdIntro.Position = to + Vector2.UnitY * (float) Math.Sin(sine) * 8f;
                sine += Engine.DeltaTime * 2f;
                yield return null;
            }
        }

        public override void Removed(Scene scene)
        {
            if (fakeRightWall != null)
                fakeRightWall.RemoveSelf();
            fakeRightWall = null;
            base.Removed(scene);
        }

        private void OnPlayer(Player player)
        {
            if (player.Dead || startedRoutine)
                return;
            if (flyToRoutine != null)
                flyToRoutine.RemoveSelf();
            startedRoutine = true;
            player.Speed = Vector2.Zero;
            Depth = player.Depth - 5;
            Sprite.Play("hoverStressed");
            Sprite.Scale.X = 1f;
            fakeRightWall.RemoveSelf();
            fakeRightWall = null;
            if (!crashes)
            {
                Scene.Add(new CS10_MissTheBird(player, this));
            }
            else
            {
                CassetteBlockManager entity = Scene.Tracker.GetEntity<CassetteBlockManager>();
                if (entity != null)
                {
                    entity.StopBlocks();
                    entity.Finish();
                }
                Scene.Add(new CS10_CatchTheBird(player, this));
            }
        }

        public override void Update()
        {
            if (!startedRoutine && fakeRightWall != null)
            {
                Level scene = Scene as Level;
                if (scene.Camera.X > fakeRightWall.X - 320.0 - 16.0)
                    scene.Camera.X = (float) (fakeRightWall.X - 320.0 - 16.0);
            }
            if (emitParticles && Scene.OnInterval(0.1f))
                SceneAs<Level>().ParticlesBG.Emit(FlingBird.P_Feather, 1, Position + new Vector2(0.0f, -8f), new Vector2(6f, 4f));
            base.Update();
        }

        public IEnumerator DoGrabbingRoutine(Player player)
        {
            FlingBirdIntro follow = this;
            Level level = follow.Scene as Level;
            follow.inCutscene = true;
            follow.CrashSfxEmitter = follow.crashes ? SoundEmitter.Play("event:/new_content/game/10_farewell/bird_crashscene_start", follow) : SoundEmitter.Play("event:/new_content/game/10_farewell/bird_flappyscene", follow);
            player.StateMachine.State = 11;
            player.DummyGravity = false;
            player.DummyAutoAnimate = false;
            player.ForceCameraUpdate = true;
            player.Sprite.Play("jumpSlow_carry");
            player.Speed = Vector2.Zero;
            player.Facing = Facings.Right;
            Celeste.Freeze(0.1f);
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            follow.emitParticles = true;
            follow.Add(new Coroutine(level.ZoomTo(new Vector2(140f, 120f), 1.5f, 4f)));
            float sin = 0.0f;
            int index = 0;
            while (index < follow.nodes.Length - 1)
            {
                Vector2 position = follow.Position;
                Vector2 node = follow.nodes[index];
                SimpleCurve curve = new SimpleCurve(position, node, position + (node - position) * 0.5f + new Vector2(0.0f, -24f));
                float duration = curve.GetLengthParametric(32) / 100f;
                if (node.Y < (double) position.Y)
                {
                    duration *= 1.1f;
                    follow.Sprite.Rate = 2f;
                }
                else
                {
                    duration *= 0.8f;
                    follow.Sprite.Rate = 1f;
                }
                if (!follow.crashes)
                {
                    if (index == 0)
                        duration = 0.7f;
                    if (index == 1)
                        duration += 0.191f;
                    if (index == 2)
                        duration += 0.191f;
                }
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / duration)
                {
                    sin += Engine.DeltaTime * 10f;
                    follow.Position = (curve.GetPoint(p) + Vector2.UnitY * (float) Math.Sin(sin) * 8f).Floor();
                    player.Position = follow.Position + new Vector2(2f, 10f);
                    switch (follow.Sprite.CurrentAnimationFrame)
                    {
                        case 1:
                            Player player1 = player;
                            player1.Position += new Vector2(1f, -1f);
                            break;
                        case 2:
                            Player player2 = player;
                            player2.Position += new Vector2(-1f, 0.0f);
                            break;
                        case 3:
                            Player player3 = player;
                            player3.Position += new Vector2(-1f, 1f);
                            break;
                        case 4:
                            Player player4 = player;
                            player4.Position += new Vector2(1f, 3f);
                            break;
                        case 5:
                            Player player5 = player;
                            player5.Position += new Vector2(2f, 5f);
                            break;
                    }
                    yield return null;
                }
                level.Shake();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                ++index;
                curve = new SimpleCurve();
            }
            follow.Sprite.Rate = 1f;
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            level.Flash(Color.White);
            follow.emitParticles = false;
            follow.inCutscene = false;
        }
    }
}
