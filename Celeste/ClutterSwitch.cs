// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterSwitch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class ClutterSwitch : Solid
    {
        public const float LightingAlphaAdd = 0.05f;
        public static ParticleType P_Pressed;
        public static ParticleType P_ClutterFly;
        private const int PressedAdd = 10;
        private const int PressedSpriteAdd = 2;
        private const int UnpressedLightRadius = 32;
        private const int PressedLightRadius = 24;
        private const int BrightLightRadius = 64;
        private readonly ClutterBlock.Colors color;
        private readonly float startY;
        private float atY;
        private float speedY;
        private bool pressed;
        private readonly Sprite sprite;
        private readonly Monocle.Image icon;
        private float targetXScale = 1f;
        private readonly VertexLight vertexLight;
        private bool playerWasOnTop;
        private SoundSource cutsceneSfx;

        public ClutterSwitch(Vector2 position, ClutterBlock.Colors color)
            : base(position, 32f, 16f, true)
        {
            this.color = color;
            startY = atY = Y;
            OnDashCollide = new DashCollision(OnDashed);
            SurfaceSoundIndex = 21;
            Add(sprite = GFX.SpriteBank.Create("clutterSwitch"));
            sprite.Position = new Vector2(16f, 16f);
            sprite.Play("idle");
            Add(icon = new Monocle.Image(GFX.Game["objects/resortclutter/icon_" + color.ToString()]));
            _ = icon.CenterOrigin();
            icon.Position = new Vector2(16f, 8f);
            Add(vertexLight = new VertexLight(new Vector2(CenterX - X, -1f), Color.Aqua, 1f, 32, 64));
        }

        public ClutterSwitch(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Enum<ClutterBlock.Colors>("type", ClutterBlock.Colors.Green))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (color == ClutterBlock.Colors.Lightning && SceneAs<Level>().Session.GetFlag("disable_lightning"))
            {
                BePressed();
            }
            else
            {
                if (!SceneAs<Level>().Session.GetFlag("oshiro_clutter_cleared_" + (int)color))
                {
                    return;
                }

                BePressed();
            }
        }

        private void BePressed()
        {
            pressed = true;
            atY += 10f;
            Y += 10f;
            sprite.Y += 2f;
            sprite.Play("active");
            Remove(icon);
            vertexLight.StartRadius = 24f;
            vertexLight.EndRadius = 48f;
        }

        public override void Update()
        {
            base.Update();
            if (HasPlayerOnTop())
            {
                if (speedY < 0.0)
                {
                    speedY = 0.0f;
                }

                speedY = Calc.Approach(speedY, 70f, 200f * Engine.DeltaTime);
                MoveTowardsY(atY + (pressed ? 2f : 4f), speedY * Engine.DeltaTime);
                targetXScale = 1.2f;
                if (!playerWasOnTop)
                {
                    _ = Audio.Play("event:/game/03_resort/clutterswitch_squish", Position);
                }

                playerWasOnTop = true;
            }
            else
            {
                if (speedY > 0.0)
                {
                    speedY = 0.0f;
                }

                speedY = Calc.Approach(speedY, -150f, 200f * Engine.DeltaTime);
                MoveTowardsY(atY, -speedY * Engine.DeltaTime);
                targetXScale = 1f;
                if (playerWasOnTop)
                {
                    _ = Audio.Play("event:/game/03_resort/clutterswitch_return", Position);
                }

                playerWasOnTop = false;
            }
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, targetXScale, 0.8f * Engine.DeltaTime);
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!pressed && direction == Vector2.UnitY)
            {
                Celeste.Freeze(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Level scene = Scene as Level;
                scene.Session.SetFlag("oshiro_clutter_cleared_" + (int)color);
                scene.Session.SetFlag("oshiro_clutter_door_open", false);
                vertexLight.StartRadius = 64f;
                vertexLight.EndRadius = 128f;
                scene.DirectionalShake(Vector2.UnitY, 0.6f);
                scene.Particles.Emit(ClutterSwitch.P_Pressed, 20, TopCenter - (Vector2.UnitY * 10f), new Vector2(16f, 8f));
                BePressed();
                sprite.Scale.X = 1.5f;
                if (color == ClutterBlock.Colors.Lightning)
                {
                    Add(new Coroutine(LightningRoutine(player)));
                }
                else
                {
                    Add(new Coroutine(AbsorbRoutine(player)));
                }
            }
            return DashCollisionResults.NormalCollision;
        }

        private IEnumerator LightningRoutine(Player player)
        {
            // ISSUE: reference to a compiler-generated field
            Level level = base.SceneAs<Level>();
            level.Session.SetFlag("disable_lightning", true);
            AudioTrackState music = level.Session.Audio.Music;
            int progress = music.Progress;
            music.Progress = progress + 1;
            level.Session.Audio.Apply(false);
            yield return Lightning.RemoveRoutine(level, null);
            yield break;
        }

        private IEnumerator AbsorbRoutine(Player player)
        {
            ClutterSwitch clutterSwitch = this;
            clutterSwitch.Add(clutterSwitch.cutsceneSfx = new SoundSource());
            float duration = 0.0f;
            if (clutterSwitch.color == ClutterBlock.Colors.Green)
            {
                _ = clutterSwitch.cutsceneSfx.Play("event:/game/03_resort/clutterswitch_books");
                duration = 6.366f;
            }
            else if (clutterSwitch.color == ClutterBlock.Colors.Red)
            {
                _ = clutterSwitch.cutsceneSfx.Play("event:/game/03_resort/clutterswitch_linens");
                duration = 6.15f;
            }
            else if (clutterSwitch.color == ClutterBlock.Colors.Yellow)
            {
                _ = clutterSwitch.cutsceneSfx.Play("event:/game/03_resort/clutterswitch_boxes");
                duration = 6.066f;
            }
            clutterSwitch.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => Audio.Play("event:/game/03_resort/clutterswitch_finish", Position), duration, true));
            player.StateMachine.State = 11;
            Vector2 target = clutterSwitch.Position + new Vector2(clutterSwitch.Width / 2f, 0.0f);
            ClutterAbsorbEffect effect = new();
            clutterSwitch.Scene.Add(effect);
            clutterSwitch.sprite.Play("break");
            Level level = clutterSwitch.SceneAs<Level>();
            ++level.Session.Audio.Music.Progress;
            level.Session.Audio.Apply();
            level.Session.LightingAlphaAdd -= 0.05f;
            float start1 = level.Lighting.Alpha;
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, true);
            tween1.OnUpdate = t => level.Lighting.Alpha = MathHelper.Lerp(start1, 0.05f, t.Eased);
            clutterSwitch.Add(tween1);
            _ = Alarm.Set(clutterSwitch, 3f, () =>
            {
                float start2 = vertexLight.StartRadius;
                float end = vertexLight.EndRadius;
                Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, true);
                tween2.OnUpdate = t =>
                {
                    level.Lighting.Alpha = MathHelper.Lerp(0.05f, level.BaseLightingAlpha + level.Session.LightingAlphaAdd, t.Eased);
                    vertexLight.StartRadius = (int)Math.Round((double)MathHelper.Lerp(start2, 24f, t.Eased));
                    vertexLight.EndRadius = (int)Math.Round((double)MathHelper.Lerp(end, 48f, t.Eased));
                };
                Add(tween2);
            });
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            foreach (ClutterBlock clutterBlock in clutterSwitch.Scene.Entities.FindAll<ClutterBlock>())
            {
                if (clutterBlock.BlockColor == clutterSwitch.color)
                {
                    clutterBlock.Absorb(effect);
                }
            }
            foreach (ClutterBlockBase clutterBlockBase in clutterSwitch.Scene.Entities.FindAll<ClutterBlockBase>())
            {
                if (clutterBlockBase.BlockColor == clutterSwitch.color)
                {
                    clutterBlockBase.Deactivate();
                }
            }
            yield return 1.5f;
            player.StateMachine.State = 0;
            List<MTexture> images = GFX.Game.GetAtlasSubtextures("objects/resortclutter/" + clutterSwitch.color.ToString() + "_");
            for (int i = 0; i < 25; ++i)
            {
                for (int index = 0; index < 5; ++index)
                {
                    effect.FlyClutter(target + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 320f), Calc.Random.Choose<MTexture>(images), false, 0.0f);
                }

                level.Shake();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
                yield return 0.05f;
            }
            yield return 1.5f;
            effect.CloseCabinets();
            yield return 0.2f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
            yield return 0.3f;
        }

        public override void Removed(Scene scene)
        {
            Level level = scene as Level;
            level.Lighting.Alpha = level.BaseLightingAlpha + level.Session.LightingAlphaAdd;
            base.Removed(scene);
        }
    }
}
