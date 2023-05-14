// Decompiled with JetBrains decompiler
// Type: Celeste.WaveDashTutorialMachine
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
    public class WaveDashTutorialMachine : JumpThru
    {
        private Entity frontEntity;
        private readonly Monocle.Image backSprite;
        private Monocle.Image frontRightSprite;
        private readonly Monocle.Image frontLeftSprite;
        private readonly Sprite noise;
        private Sprite neon;
        private Solid frontWall;
        private float insideEase;
        private float cameraEase;
        private bool playerInside;
        private bool inCutscene;
        private Coroutine routine;
        private WaveDashPresentation presentation;
        private float interactStartZoom;
        private EventInstance snapshot;
        private EventInstance usingSfx;
        private SoundSource signSfx;
        private readonly TalkComponent talk;

        public WaveDashTutorialMachine(Vector2 position)
            : base(position, 88, true)
        {
            Tag = (int)Tags.TransitionUpdate;
            Depth = 1000;
            Hitbox.Position = new Vector2(-41f, -59f);
            Add(backSprite = new Monocle.Image(GFX.Game["objects/wavedashtutorial/building_back"]));
            _ = backSprite.JustifyOrigin(0.5f, 1f);
            Add(noise = new Sprite(GFX.Game, "objects/wavedashtutorial/noise"));
            noise.AddLoop("static", "", 0.05f);
            noise.Play("static");
            _ = noise.CenterOrigin();
            noise.Position = new Vector2(0.0f, -30f);
            noise.Color = Color.White * 0.5f;
            Add(frontLeftSprite = new Monocle.Image(GFX.Game["objects/wavedashtutorial/building_front_left"]));
            _ = frontLeftSprite.JustifyOrigin(0.5f, 1f);
            Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0.0f, -50f), new Action<Player>(OnInteract)));
            talk.Enabled = false;
            SurfaceSoundIndex = 42;
        }

        public WaveDashTutorialMachine(EntityData data, Vector2 position)
            : this(data.Position + position)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(frontEntity = new Entity(Position));
            frontEntity.Tag = (int)Tags.TransitionUpdate;
            frontEntity.Depth = -10500;
            frontEntity.Add(frontRightSprite = new Monocle.Image(GFX.Game["objects/wavedashtutorial/building_front_right"]));
            _ = frontRightSprite.JustifyOrigin(0.5f, 1f);
            frontEntity.Add(neon = new Sprite(GFX.Game, "objects/wavedashtutorial/neon_"));
            neon.AddLoop("loop", "", 0.07f);
            neon.Play("loop");
            _ = neon.JustifyOrigin(0.5f, 1f);
            scene.Add(frontWall = new Solid(Position + new Vector2(-41f, -59f), 88f, 38f, true));
            frontWall.SurfaceSoundIndex = 42;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(signSfx = new SoundSource(new Vector2(8f, -16f), "event:/new_content/env/local/cafe_sign"));
        }

        public override void Update()
        {
            base.Update();
            if (!inCutscene)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    frontWall.Collidable = true;
                    bool flag = ((double)entity.X > (double)X - 37.0 && (double)entity.X < (double)X + 46.0 && (double)entity.Y > (double)Y - 58.0) || frontWall.CollideCheck(entity);
                    if (flag != playerInside)
                    {
                        playerInside = flag;
                        if (playerInside)
                        {
                            _ = signSfx.Stop();
                            snapshot = Audio.CreateSnapshot("snapshot:/game_10_inside_cafe");
                        }
                        else
                        {
                            _ = signSfx.Play("event:/new_content/env/local/cafe_sign");
                            Audio.ReleaseSnapshot(snapshot);
                            snapshot = null;
                        }
                    }
                }
                SceneAs<Level>().ZoomSnap(new Vector2(160f, 90f), (float)(1.0 + ((double)Ease.QuadInOut(cameraEase) * 0.75)));
            }
            talk.Enabled = playerInside;
            frontWall.Collidable = !playerInside;
            insideEase = Calc.Approach(insideEase, playerInside ? 1f : 0.0f, Engine.DeltaTime * 4f);
            cameraEase = Calc.Approach(cameraEase, playerInside ? 1f : 0.0f, Engine.DeltaTime * 2f);
            frontRightSprite.Color = Color.White * (1f - insideEase);
            frontLeftSprite.Color = frontRightSprite.Color;
            neon.Color = frontRightSprite.Color;
            frontRightSprite.Visible = insideEase < 1.0;
            frontLeftSprite.Visible = insideEase < 1.0;
            neon.Visible = insideEase < 1.0;
            if (!Scene.OnInterval(0.05f))
            {
                return;
            }

            noise.Scale = Calc.Random.Choose<Vector2>(new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f));
        }

        private void OnInteract(Player player)
        {
            if (inCutscene)
            {
                return;
            }

            Level scene = Scene as Level;
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "end", 1f);
                Audio.Stop(usingSfx);
            }
            inCutscene = true;
            interactStartZoom = scene.ZoomTarget;
            scene.StartCutscene(new Action<Level>(SkipInteraction), resetZoomOnSkip: false);
            Add(routine = new Coroutine(InteractRoutine(player)));
        }

        private IEnumerator InteractRoutine(Player player)
        {
            WaveDashTutorialMachine dashTutorialMachine = this;
            Level level = dashTutorialMachine.Scene as Level;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return CutsceneEntity.CameraTo(new Vector2(dashTutorialMachine.X, dashTutorialMachine.Y - 30f) - new Vector2(160f, 90f), 0.25f, Ease.CubeOut);
            yield return level.ZoomTo(new Vector2(160f, 90f), 10f, 1f);
            dashTutorialMachine.usingSfx = Audio.Play("event:/state/cafe_computer_active", player.Position);
            _ = Audio.Play("event:/new_content/game/10_farewell/cafe_computer_on", player.Position);
            _ = Audio.Play("event:/new_content/game/10_farewell/cafe_computer_startupsfx", player.Position);
            dashTutorialMachine.presentation = new WaveDashPresentation(dashTutorialMachine.usingSfx);
            dashTutorialMachine.Scene.Add(dashTutorialMachine.presentation);
            while (dashTutorialMachine.presentation.Viewing)
            {
                yield return null;
            }

            yield return level.ZoomTo(new Vector2(160f, 90f), dashTutorialMachine.interactStartZoom, 1f);
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            dashTutorialMachine.inCutscene = false;
            level.EndCutscene();
            Audio.SetAltMusic(null);
        }

        private void SkipInteraction(Level level)
        {
            Audio.SetAltMusic(null);
            inCutscene = false;
            level.ZoomSnap(new Vector2(160f, 90f), interactStartZoom);
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "end", 1f);
                _ = (int)usingSfx.release();
            }
            presentation?.RemoveSelf();
            presentation = null;
            routine?.RemoveSelf();
            routine = null;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        private void Dispose()
        {
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "quit", 1f);
                _ = (int)usingSfx.release();
                usingSfx = null;
            }
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
        }
    }
}
