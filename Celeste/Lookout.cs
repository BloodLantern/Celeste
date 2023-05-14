// Decompiled with JetBrains decompiler
// Type: Celeste.Lookout
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
    [Tracked(false)]
    public class Lookout : Entity
    {
        private TalkComponent talk;
        private Lookout.Hud hud;
        private readonly Sprite sprite;
        private readonly Tween lightTween;
        private bool interacting;
        private readonly bool onlyY;
        private readonly List<Vector2> nodes;
        private int node;
        private float nodePercent;
        private readonly bool summit;
        private string animPrefix = "";

        public Lookout(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = -8500;
            Add(talk = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), new Action<Player>(Interact)));
            talk.PlayerMustBeFacing = false;
            summit = data.Bool(nameof(summit));
            onlyY = data.Bool(nameof(onlyY));
            Collider = new Hitbox(4f, 4f, -2f, -4f);
            VertexLight vertexLight = new(new Vector2(-1f, -11f), Color.White, 0.8f, 16, 24);
            Add(vertexLight);
            lightTween = vertexLight.CreatePulseTween();
            Add(lightTween);
            Add(sprite = GFX.SpriteBank.Create("lookout"));
            sprite.OnFrameChange = s =>
            {
                if ((!(s == "idle") && !(s == "badeline_idle") && !(s == "nobackpack_idle")) || sprite.CurrentAnimationFrame != sprite.CurrentAnimationTotalFrames - 1)
                {
                    return;
                }

                lightTween.Start();
            };
            Vector2[] collection = data.NodesOffset(offset);
            if (collection == null || collection.Length == 0)
            {
                return;
            }

            nodes = new List<Vector2>(collection);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (!interacting)
            {
                return;
            }

            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            entity.StateMachine.State = 0;
        }

        private void Interact(Player player)
        {
            animPrefix = player.DefaultSpriteMode == PlayerSpriteMode.MadelineAsBadeline || SaveData.Instance.Assists.PlayAsBadeline ? "badeline_" : (player.DefaultSpriteMode != PlayerSpriteMode.MadelineNoBackpack ? "" : "nobackpack_");
            Add(new Coroutine(LookRoutine(player))
            {
                RemoveOnComplete = true
            });
            interacting = true;
        }

        public void StopInteracting()
        {
            interacting = false;
            sprite.Play(animPrefix + "idle");
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                sprite.Active = interacting || entity.StateMachine.State != 11;
                if (!sprite.Active)
                {
                    sprite.SetAnimationFrame(0);
                }
            }
            if (talk == null || !CollideCheck<Solid>())
            {
                return;
            }

            Remove(talk);
            talk = null;
        }

        private IEnumerator LookRoutine(Player player)
        {
            Lookout lookout = this;
            Level level = lookout.SceneAs<Level>();
            SandwichLava first = lookout.Scene.Entities.FindFirst<SandwichLava>();
            if (first != null)
            {
                first.Waiting = true;
            }

            if (player.Holding != null)
            {
                player.Drop();
            }

            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)lookout.X, cancelOnFall: true);
            if ((double)Math.Abs(lookout.X - player.X) > 4.0 || player.Dead || !player.OnGround())
            {
                if (!player.Dead)
                {
                    player.StateMachine.State = 0;
                }
            }
            else
            {
                _ = Audio.Play("event:/game/general/lookout_use", lookout.Position);
                if (player.Facing == Facings.Right)
                {
                    lookout.sprite.Play(lookout.animPrefix + "lookRight");
                }
                else
                {
                    lookout.sprite.Play(lookout.animPrefix + "lookLeft");
                }

                player.Sprite.Visible = player.Hair.Visible = false;
                yield return 0.2f;
                lookout.Scene.Add(lookout.hud = new Lookout.Hud());
                lookout.hud.TrackMode = lookout.nodes != null;
                lookout.hud.OnlyY = lookout.onlyY;
                lookout.nodePercent = 0.0f;
                lookout.node = 0;
                _ = Audio.Play("event:/ui/game/lookout_on");
                while ((double)(lookout.hud.Easer = Calc.Approach(lookout.hud.Easer, 1f, Engine.DeltaTime * 3f)) < 1.0)
                {
                    level.ScreenPadding = (int)((double)Ease.CubeInOut(lookout.hud.Easer) * 16.0);
                    yield return null;
                }
                float accel = 800f;
                float maxspd = 240f;
                Vector2 cam = level.Camera.Position;
                Vector2 speed = Vector2.Zero;
                Vector2 lastDir = Vector2.Zero;
                Vector2 camStart = level.Camera.Position;
                Vector2 camStartCenter = camStart + new Vector2(160f, 90f);
                while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && lookout.interacting)
                {
                    Vector2 vector2_1 = Input.Aim.Value;
                    if (lookout.onlyY)
                    {
                        vector2_1.X = 0.0f;
                    }

                    if (Math.Sign(vector2_1.X) != Math.Sign(lastDir.X) || Math.Sign(vector2_1.Y) != Math.Sign(lastDir.Y))
                    {
                        _ = Audio.Play("event:/game/general/lookout_move", lookout.Position);
                    }

                    lastDir = vector2_1;
                    if (lookout.sprite.CurrentAnimationID is not "lookLeft" and not "lookRight")
                    {
                        if (vector2_1.X == 0.0)
                        {
                            if (vector2_1.Y == 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "looking");
                            }
                            else if (vector2_1.Y > 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingDown");
                            }
                            else
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingUp");
                            }
                        }
                        else if (vector2_1.X > 0.0)
                        {
                            if (vector2_1.Y == 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingRight");
                            }
                            else if (vector2_1.Y > 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingDownRight");
                            }
                            else
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingUpRight");
                            }
                        }
                        else if (vector2_1.X < 0.0)
                        {
                            if (vector2_1.Y == 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingLeft");
                            }
                            else if (vector2_1.Y > 0.0)
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingDownLeft");
                            }
                            else
                            {
                                lookout.sprite.Play(lookout.animPrefix + "lookingUpLeft");
                            }
                        }
                    }
                    if (lookout.nodes == null)
                    {
                        speed += accel * vector2_1 * Engine.DeltaTime;
                        if (vector2_1.X == 0.0)
                        {
                            speed.X = Calc.Approach(speed.X, 0.0f, accel * 2f * Engine.DeltaTime);
                        }

                        if (vector2_1.Y == 0.0)
                        {
                            speed.Y = Calc.Approach(speed.Y, 0.0f, accel * 2f * Engine.DeltaTime);
                        }

                        if ((double)speed.Length() > (double)maxspd)
                        {
                            speed = speed.SafeNormalize(maxspd);
                        }

                        Vector2 vector2_2 = cam;
                        List<Entity> entities = lookout.Scene.Tracker.GetEntities<LookoutBlocker>();
                        cam.X += speed.X * Engine.DeltaTime;
                        Rectangle bounds1;
                        if (cam.X >= (double)level.Bounds.Left)
                        {
                            double num = cam.X + 320.0;
                            bounds1 = level.Bounds;
                            double right = bounds1.Right;
                            if (num <= right)
                            {
                                goto label_47;
                            }
                        }
                        speed.X = 0.0f;
                    label_47:
                        double x = cam.X;
                        bounds1 = level.Bounds;
                        double left = bounds1.Left;
                        bounds1 = level.Bounds;
                        double max1 = bounds1.Right - 320;
                        double num1 = (double)Calc.Clamp((float)x, (float)left, (float)max1);
                        cam.X = (float)num1;
                        foreach (Entity entity in entities)
                        {
                            if (cam.X + 320.0 > (double)entity.Left && cam.Y + 180.0 > (double)entity.Top && cam.X < (double)entity.Right && cam.Y < (double)entity.Bottom)
                            {
                                cam.X = vector2_2.X;
                                speed.X = 0.0f;
                            }
                        }
                        cam.Y += speed.Y * Engine.DeltaTime;
                        Rectangle bounds2;
                        if (cam.Y >= (double)level.Bounds.Top)
                        {
                            double num2 = cam.Y + 180.0;
                            bounds2 = level.Bounds;
                            double bottom = bounds2.Bottom;
                            if (num2 <= bottom)
                            {
                                goto label_56;
                            }
                        }
                        speed.Y = 0.0f;
                    label_56:
                        double y = cam.Y;
                        bounds2 = level.Bounds;
                        double top = bounds2.Top;
                        bounds2 = level.Bounds;
                        double max2 = bounds2.Bottom - 180;
                        double num3 = (double)Calc.Clamp((float)y, (float)top, (float)max2);
                        cam.Y = (float)num3;
                        foreach (Entity entity in entities)
                        {
                            if (cam.X + 320.0 > (double)entity.Left && cam.Y + 180.0 > (double)entity.Top && cam.X < (double)entity.Right && cam.Y < (double)entity.Bottom)
                            {
                                cam.Y = vector2_2.Y;
                                speed.Y = 0.0f;
                            }
                        }
                        level.Camera.Position = cam;
                    }
                    else
                    {
                        Vector2 control = lookout.node <= 0 ? camStartCenter : lookout.nodes[lookout.node - 1];
                        Vector2 node1 = lookout.nodes[lookout.node];
                        float num4 = (control - node1).Length();
                        _ = (node1 - control).SafeNormalize();
                        if (lookout.nodePercent < 0.25 && lookout.node > 0)
                        {
                            level.Camera.Position = new SimpleCurve(Vector2.Lerp(lookout.node <= 1 ? camStartCenter : lookout.nodes[lookout.node - 2], control, 0.75f), Vector2.Lerp(control, node1, 0.25f), control).GetPoint((float)(0.5 + (lookout.nodePercent / 0.25 * 0.5)));
                        }
                        else if (lookout.nodePercent > 0.75 && lookout.node < lookout.nodes.Count - 1)
                        {
                            Vector2 node2 = lookout.nodes[lookout.node + 1];
                            level.Camera.Position = new SimpleCurve(Vector2.Lerp(control, node1, 0.75f), Vector2.Lerp(node1, node2, 0.25f), node1).GetPoint((float)((lookout.nodePercent - 0.75) / 0.25 * 0.5));
                        }
                        else
                        {
                            level.Camera.Position = Vector2.Lerp(control, node1, lookout.nodePercent);
                        }

                        level.Camera.Position += new Vector2(-160f, -90f);
                        lookout.nodePercent -= vector2_1.Y * (maxspd / num4) * Engine.DeltaTime;
                        if (lookout.nodePercent < 0.0)
                        {
                            if (lookout.node > 0)
                            {
                                --lookout.node;
                                lookout.nodePercent = 1f;
                            }
                            else
                            {
                                lookout.nodePercent = 0.0f;
                            }
                        }
                        else if (lookout.nodePercent > 1.0)
                        {
                            if (lookout.node < lookout.nodes.Count - 1)
                            {
                                ++lookout.node;
                                lookout.nodePercent = 0.0f;
                            }
                            else
                            {
                                lookout.nodePercent = 1f;
                                if (lookout.summit)
                                {
                                    break;
                                }
                            }
                        }
                        float num5 = 0.0f;
                        float num6 = 0.0f;
                        for (int index = 0; index < lookout.nodes.Count; ++index)
                        {
                            float num7 = ((index == 0 ? camStartCenter : lookout.nodes[index - 1]) - lookout.nodes[index]).Length();
                            num6 += num7;
                            if (index < lookout.node)
                            {
                                num5 += num7;
                            }
                            else if (index == lookout.node)
                            {
                                num5 += num7 * lookout.nodePercent;
                            }
                        }
                        lookout.hud.TrackPercent = num5 / num6;
                    }
                    yield return null;
                }
                player.Sprite.Visible = player.Hair.Visible = true;
                lookout.sprite.Play(lookout.animPrefix + "idle");
                _ = Audio.Play("event:/ui/game/lookout_off");
                while ((double)(lookout.hud.Easer = Calc.Approach(lookout.hud.Easer, 0.0f, Engine.DeltaTime * 3f)) > 0.0)
                {
                    level.ScreenPadding = (int)((double)Ease.CubeInOut(lookout.hud.Easer) * 16.0);
                    yield return null;
                }
                bool atSummitTop = lookout.summit && lookout.node >= lookout.nodes.Count - 1 && lookout.nodePercent >= 0.949999988079071;
                float duration;
                float approach;
                if (atSummitTop)
                {
                    yield return 0.5f;
                    duration = 3f;
                    approach = 0.0f;
                    Coroutine coroutine = new(level.ZoomTo(new Vector2(160f, 90f), 2f, duration));
                    lookout.Add(coroutine);
                    while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed && !Input.Dash.Pressed && !Input.Jump.Pressed && lookout.interacting)
                    {
                        approach = Calc.Approach(approach, 1f, Engine.DeltaTime / duration);
                        Audio.SetMusicParam("escape", approach);
                        yield return null;
                    }
                }
                if ((double)(camStart - level.Camera.Position).Length() > 600.0)
                {
                    Vector2 was = level.Camera.Position;
                    Vector2 direction = (was - camStart).SafeNormalize();
                    approach = atSummitTop ? 1f : 0.5f;
                    new FadeWipe(lookout.Scene, false).Duration = approach;
                    for (duration = 0.0f; (double)duration < 1.0; duration += Engine.DeltaTime / approach)
                    {
                        level.Camera.Position = was - (direction * MathHelper.Lerp(0.0f, 64f, Ease.CubeIn(duration)));
                        yield return null;
                    }
                    level.Camera.Position = camStart + (direction * 32f);
                    _ = new FadeWipe(lookout.Scene, true);
                    _ = new Vector2();
                    _ = new Vector2();
                }
                Audio.SetMusicParam("escape", 0.0f);
                level.ScreenPadding = 0.0f;
                level.ZoomSnap(Vector2.Zero, 1f);
                lookout.Scene.Remove(lookout.hud);
                lookout.interacting = false;
                player.StateMachine.State = 0;
                yield return null;
            }
        }

        private class Hud : Entity
        {
            public bool TrackMode;
            public float TrackPercent;
            public bool OnlyY;
            public float Easer;
            private float timerUp;
            private float timerDown;
            private float timerLeft;
            private float timerRight;
            private float multUp;
            private float multDown;
            private float multLeft;
            private float multRight;
            private float left;
            private float right;
            private float up;
            private float down;
            private Vector2 aim;
            private readonly MTexture halfDot = GFX.Gui["dot"].GetSubtexture(0, 0, 64, 32);

            public Hud()
            {
                AddTag((int)Tags.HUD);
            }

            public override void Update()
            {
                Level level = SceneAs<Level>();
                Vector2 position = level.Camera.Position;
                Rectangle bounds = level.Bounds;
                int width = 320;
                int height = 180;
                bool flag1 = Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)(position.X - 8.0), (int)position.Y, width, height));
                bool flag2 = Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)(position.X + 8.0), (int)position.Y, width, height));
                bool flag3 = (TrackMode && TrackPercent >= 1.0) || Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)position.X, (int)(position.Y - 8.0), width, height));
                bool flag4 = (TrackMode && TrackPercent <= 0.0) || Scene.CollideCheck<LookoutBlocker>(new Rectangle((int)position.X, (int)(position.Y + 8.0), width, height));
                left = Calc.Approach(left, flag1 || position.X <= (double)(bounds.Left + 2) ? 0.0f : 1f, Engine.DeltaTime * 8f);
                right = Calc.Approach(right, flag2 || position.X + (double)width >= bounds.Right - 2 ? 0.0f : 1f, Engine.DeltaTime * 8f);
                up = Calc.Approach(up, flag3 || position.Y <= (double)(bounds.Top + 2) ? 0.0f : 1f, Engine.DeltaTime * 8f);
                down = Calc.Approach(down, flag4 || position.Y + (double)height >= bounds.Bottom - 2 ? 0.0f : 1f, Engine.DeltaTime * 8f);
                aim = Input.Aim.Value;
                if (aim.X < 0.0)
                {
                    multLeft = Calc.Approach(multLeft, 0.0f, Engine.DeltaTime * 2f);
                    timerLeft += Engine.DeltaTime * 12f;
                }
                else
                {
                    multLeft = Calc.Approach(multLeft, 1f, Engine.DeltaTime * 2f);
                    timerLeft += Engine.DeltaTime * 6f;
                }
                if (aim.X > 0.0)
                {
                    multRight = Calc.Approach(multRight, 0.0f, Engine.DeltaTime * 2f);
                    timerRight += Engine.DeltaTime * 12f;
                }
                else
                {
                    multRight = Calc.Approach(multRight, 1f, Engine.DeltaTime * 2f);
                    timerRight += Engine.DeltaTime * 6f;
                }
                if (aim.Y < 0.0)
                {
                    multUp = Calc.Approach(multUp, 0.0f, Engine.DeltaTime * 2f);
                    timerUp += Engine.DeltaTime * 12f;
                }
                else
                {
                    multUp = Calc.Approach(multUp, 1f, Engine.DeltaTime * 2f);
                    timerUp += Engine.DeltaTime * 6f;
                }
                if (aim.Y > 0.0)
                {
                    multDown = Calc.Approach(multDown, 0.0f, Engine.DeltaTime * 2f);
                    timerDown += Engine.DeltaTime * 12f;
                }
                else
                {
                    multDown = Calc.Approach(multDown, 1f, Engine.DeltaTime * 2f);
                    timerDown += Engine.DeltaTime * 6f;
                }
                base.Update();
            }

            public override void Render()
            {
                Level scene = Scene as Level;
                float num1 = Ease.CubeInOut(Easer);
                Color color = Color.White * num1;
                int x1 = (int)(80.0 * (double)num1);
                int y1 = (int)(80.0 * (double)num1 * (9.0 / 16.0));
                int height = 8;
                if (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null)
                {
                    color *= 0.25f;
                }

                Draw.Rect(x1, y1, 1920 - (x1 * 2) - height, height, color);
                Draw.Rect(x1, y1 + height, height + 2, 1080 - (y1 * 2) - height, color);
                Draw.Rect(1920 - x1 - height - 2, y1, height + 2, 1080 - (y1 * 2) - height, color);
                Draw.Rect(x1 + height, 1080 - y1 - height, 1920 - (x1 * 2) - height, height, color);
                if (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null)
                {
                    return;
                }

                MTexture mtexture = GFX.Gui["towerarrow"];
                float y2 = (float)((y1 * (double)up) - (double)((float)(Math.Sin(timerUp) * 18.0) * MathHelper.Lerp(0.5f, 1f, multUp)) - ((1.0 - multUp) * 12.0));
                mtexture.DrawCentered(new Vector2(960f, y2), color * up, 1f, 1.57079637f);
                float y3 = (float)(1080.0 - (y1 * (double)down) + (double)((float)(Math.Sin(timerDown) * 18.0) * MathHelper.Lerp(0.5f, 1f, multDown)) + ((1.0 - multDown) * 12.0));
                mtexture.DrawCentered(new Vector2(960f, y3), color * down, 1f, 4.712389f);
                if (!TrackMode && !OnlyY)
                {
                    float num2 = left;
                    float amount1 = multLeft;
                    float a1 = timerLeft;
                    float num3 = right;
                    float amount2 = multRight;
                    float a2 = timerRight;
                    if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    {
                        num2 = right;
                        amount1 = multRight;
                        a1 = timerRight;
                        num3 = left;
                        amount2 = multLeft;
                        a2 = timerLeft;
                    }
                    float x2 = (float)((x1 * (double)num2) - (double)((float)(Math.Sin((double)a1) * 18.0) * MathHelper.Lerp(0.5f, 1f, amount1)) - ((1.0 - (double)amount1) * 12.0));
                    mtexture.DrawCentered(new Vector2(x2, 540f), color * num2);
                    float x3 = (float)(1920.0 - (x1 * (double)num3) + (double)((float)(Math.Sin((double)a2) * 18.0) * MathHelper.Lerp(0.5f, 1f, amount2)) + ((1.0 - (double)amount2) * 12.0));
                    mtexture.DrawCentered(new Vector2(x3, 540f), color * num3, 1f, 3.14159274f);
                }
                else
                {
                    if (!TrackMode)
                    {
                        return;
                    }

                    int num4 = 1080 - (y1 * 2) - 128 - 64;
                    int x4 = 1920 - x1 - 64;
                    float num5 = (float)(((1080 - num4) / 2.0) + 32.0);
                    Draw.Rect(x4 - 7, num5 + 7f, 14f, num4 - 14, Color.Black * num1);
                    halfDot.DrawJustified(new Vector2(x4, num5 + 7f), new Vector2(0.5f, 1f), Color.Black * num1);
                    halfDot.DrawJustified(new Vector2(x4, (float)((double)num5 + num4 - 7.0)), new Vector2(0.5f, 1f), Color.Black * num1, new Vector2(1f, -1f));
                    GFX.Gui["lookout/cursor"].DrawCentered(new Vector2(x4, num5 + ((1f - TrackPercent) * num4)), Color.White * num1, 1f);
                    GFX.Gui["lookout/summit"].DrawCentered(new Vector2(x4, num5 - 64f), Color.White * num1, 0.65f);
                }
            }
        }
    }
}
