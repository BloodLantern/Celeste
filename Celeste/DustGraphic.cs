// Decompiled with JetBrains decompiler
// Type: Celeste.DustGraphic
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
    public class DustGraphic : Component
    {
        public Vector2 Position;
        public float Scale = 1f;
        private readonly MTexture center;
        public Action OnEstablish;
        private readonly List<DustGraphic.Node> nodes = new();
        public List<DustGraphic.Node> LeftNodes = new();
        public List<DustGraphic.Node> RightNodes = new();
        public List<DustGraphic.Node> TopNodes = new();
        public List<DustGraphic.Node> BottomNodes = new();
        public Vector2 EyeTargetDirection;
        public Vector2 EyeDirection;
        public int EyeFlip = 1;
        private readonly bool eyesExist;
        private readonly int eyeTextureIndex;
        private MTexture eyeTexture;
        private Vector2 eyeLookRange;
        private bool eyesMoveByRotation;
        private readonly bool autoControlEyes;
        private readonly bool eyesFollowPlayer;
        private Coroutine blink;
        private bool leftEyeVisible = true;
        private bool rightEyeVisible = true;
        private DustGraphic.Eyeballs eyes;
        private float timer;
        private readonly float offset;
        private readonly bool ignoreSolids;
        private readonly bool autoExpandDust;
        private float shakeTimer;
        private Vector2 shakeValue;
        private readonly int randomSeed;

        public bool Estableshed { get; private set; }

        public Vector2 RenderPosition => Entity.Position + Position + shakeValue;

        private bool InView
        {
            get
            {
                Camera camera = (Scene as Level).Camera;
                Vector2 position = Entity.Position;
                return position.X + 16.0 >= (double)camera.Left && position.Y + 16.0 >= (double)camera.Top && position.X - 16.0 <= (double)camera.Right && position.Y - 16.0 <= (double)camera.Bottom;
            }
        }

        public DustGraphic(bool ignoreSolids, bool autoControlEyes = false, bool autoExpandDust = false)
            : base(true, true)
        {
            this.ignoreSolids = ignoreSolids;
            this.autoControlEyes = autoControlEyes;
            this.autoExpandDust = autoExpandDust;
            center = Calc.Random.Choose<MTexture>(GFX.Game.GetAtlasSubtextures("danger/dustcreature/center"));
            offset = Calc.Random.NextFloat() * 4f;
            timer = Calc.Random.NextFloat();
            EyeTargetDirection = EyeDirection = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
            eyeTextureIndex = Calc.Random.Next(128);
            eyesExist = true;
            if (autoControlEyes)
            {
                eyesExist = Calc.Random.Chance(0.5f);
                eyesFollowPlayer = Calc.Random.Chance(0.3f);
            }
            randomSeed = Calc.Random.Next();
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(new TransitionListener()
            {
                OnIn = f => AddDustNodesIfInCamera()
            });
            entity.Add(new DustEdge(new Action(Render)));
        }

        public override void Update()
        {
            timer += Engine.DeltaTime * 0.6f;
            bool inView = InView;
            if (shakeTimer > 0.0)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0.0)
                {
                    shakeValue = Vector2.Zero;
                }
                else if (Scene.OnInterval(0.05f))
                {
                    shakeValue = Calc.Random.ShakeVector();
                }
            }
            if (eyesExist)
            {
                if (EyeDirection != EyeTargetDirection & inView)
                {
                    if (!eyesMoveByRotation)
                    {
                        EyeDirection = Calc.Approach(EyeDirection, EyeTargetDirection, 12f * Engine.DeltaTime);
                    }
                    else
                    {
                        float val = EyeDirection.Angle();
                        float target = EyeTargetDirection.Angle();
                        float angleRadians = Calc.AngleApproach(val, target, 8f * Engine.DeltaTime);
                        EyeDirection = (double)angleRadians != (double)target ? Calc.AngleToVector(angleRadians, 1f) : EyeTargetDirection;
                    }
                }
                if (eyesFollowPlayer & inView)
                {
                    Player entity = Entity.Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                    {
                        Vector2 vector = (entity.Position - Entity.Position).SafeNormalize();
                        if (eyesMoveByRotation)
                        {
                            float target = vector.Angle();
                            EyeTargetDirection = Calc.AngleToVector(Calc.AngleApproach(eyeLookRange.Angle(), target, 0.7853982f), 1f);
                        }
                        else
                        {
                            EyeTargetDirection = vector;
                        }
                    }
                }
                blink?.Update();
            }
            if (nodes.Count <= 0 && Entity.Scene != null && !Estableshed)
            {
                AddDustNodesIfInCamera();
            }
            else
            {
                foreach (DustGraphic.Node node in nodes)
                {
                    node.Rotation += Engine.DeltaTime * 0.5f;
                }
            }
        }

        public void OnHitPlayer()
        {
            if (SaveData.Instance.Assists.Invincible)
            {
                return;
            }

            shakeTimer = 0.6f;
            if (!eyesExist)
            {
                return;
            }

            blink = null;
            leftEyeVisible = true;
            rightEyeVisible = true;
            eyeTexture = GFX.Game["danger/dustcreature/deadEyes"];
        }

        public void AddDustNodesIfInCamera()
        {
            if (nodes.Count > 0 || !InView || DustEdges.DustGraphicEstabledCounter > 25 || Estableshed)
            {
                return;
            }

            Calc.PushRandom(randomSeed);
            int x = (int)Entity.X;
            int y = (int)Entity.Y;
            Vector2 vector2 = new Vector2(1f, 1f).SafeNormalize();
            AddNode(new Vector2(-vector2.X, -vector2.Y), ignoreSolids || !Entity.Scene.CollideCheck<Solid>(new Rectangle(x - 8, y - 8, 8, 8)));
            AddNode(new Vector2(vector2.X, -vector2.Y), ignoreSolids || !Entity.Scene.CollideCheck<Solid>(new Rectangle(x, y - 8, 8, 8)));
            AddNode(new Vector2(-vector2.X, vector2.Y), ignoreSolids || !Entity.Scene.CollideCheck<Solid>(new Rectangle(x - 8, y, 8, 8)));
            AddNode(new Vector2(vector2.X, vector2.Y), ignoreSolids || !Entity.Scene.CollideCheck<Solid>(new Rectangle(x, y, 8, 8)));
            if (nodes[0].Enabled || nodes[2].Enabled)
            {
                --Position.X;
            }

            if (nodes[1].Enabled || nodes[3].Enabled)
            {
                ++Position.X;
            }

            if (nodes[0].Enabled || nodes[1].Enabled)
            {
                --Position.Y;
            }

            if (nodes[2].Enabled || nodes[3].Enabled)
            {
                ++Position.Y;
            }

            int num = 0;
            foreach (DustGraphic.Node node in nodes)
            {
                if (node.Enabled)
                {
                    ++num;
                }
            }
            eyesMoveByRotation = num < 4;
            if (autoControlEyes && eyesExist && eyesMoveByRotation)
            {
                eyeLookRange = Vector2.Zero;
                if (nodes[0].Enabled)
                {
                    eyeLookRange += new Vector2(-1f, -1f).SafeNormalize();
                }

                if (nodes[1].Enabled)
                {
                    eyeLookRange += new Vector2(1f, -1f).SafeNormalize();
                }

                if (nodes[2].Enabled)
                {
                    eyeLookRange += new Vector2(-1f, 1f).SafeNormalize();
                }

                if (nodes[3].Enabled)
                {
                    eyeLookRange += new Vector2(1f, 1f).SafeNormalize();
                }

                if (num > 0 && (double)eyeLookRange.Length() > 0.0)
                {
                    eyeLookRange /= num;
                    eyeLookRange = eyeLookRange.SafeNormalize();
                }
                EyeTargetDirection = EyeDirection = eyeLookRange;
            }
            if (eyesExist)
            {
                blink = new Coroutine(BlinkRoutine());
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(DustStyles.Get(Scene).EyeTextures);
                eyeTexture = atlasSubtextures[eyeTextureIndex % atlasSubtextures.Count];
                Entity.Scene.Add(eyes = new DustGraphic.Eyeballs(this));
            }
            ++DustEdges.DustGraphicEstabledCounter;
            Estableshed = true;
            OnEstablish?.Invoke();
            Calc.PopRandom();
        }

        private void AddNode(Vector2 angle, bool enabled)
        {
            Vector2 vector2 = new(1f, 1f);
            if (autoExpandDust)
            {
                int num1 = Math.Sign(angle.X);
                int num2 = Math.Sign(angle.Y);
                Entity.Collidable = false;
                if (Scene.CollideCheck<Solid>(new Rectangle((int)((double)Entity.X - 4.0 + (num1 * 16)), (int)((double)Entity.Y - 4.0 + (num2 * 4)), 8, 8)) || Scene.CollideCheck<DustStaticSpinner>(new Rectangle((int)((double)Entity.X - 4.0 + (num1 * 16)), (int)((double)Entity.Y - 4.0 + (num2 * 4)), 8, 8)))
                {
                    vector2.X = 5f;
                }

                if (Scene.CollideCheck<Solid>(new Rectangle((int)((double)Entity.X - 4.0 + (num1 * 4)), (int)((double)Entity.Y - 4.0 + (num2 * 16)), 8, 8)) || Scene.CollideCheck<DustStaticSpinner>(new Rectangle((int)((double)Entity.X - 4.0 + (num1 * 4)), (int)((double)Entity.Y - 4.0 + (num2 * 16)), 8, 8)))
                {
                    vector2.Y = 5f;
                }

                Entity.Collidable = true;
            }
            DustGraphic.Node node = new()
            {
                Base = Calc.Random.Choose<MTexture>(GFX.Game.GetAtlasSubtextures("danger/dustcreature/base")),
                Overlay = Calc.Random.Choose<MTexture>(GFX.Game.GetAtlasSubtextures("danger/dustcreature/overlay")),
                Rotation = Calc.Random.NextFloat(6.28318548f),
                Angle = angle * vector2,
                Enabled = enabled
            };
            nodes.Add(node);
            if (angle.X < 0.0)
            {
                LeftNodes.Add(node);
            }
            else
            {
                RightNodes.Add(node);
            }

            if (angle.Y < 0.0)
            {
                TopNodes.Add(node);
            }
            else
            {
                BottomNodes.Add(node);
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return (float)(2.0 + (double)Calc.Random.NextFloat(1.5f));
                leftEyeVisible = false;
                yield return (float)(0.019999999552965164 + (double)Calc.Random.NextFloat(0.05f));
                rightEyeVisible = false;
                yield return 0.25f;
                leftEyeVisible = rightEyeVisible = true;
            }
        }

        public override void Render()
        {
            if (!InView)
            {
                return;
            }

            Vector2 renderPosition = RenderPosition;
            foreach (DustGraphic.Node node in nodes)
            {
                if (node.Enabled)
                {
                    node.Base.DrawCentered(renderPosition + (node.Angle * Scale), Color.White, Scale, node.Rotation);
                    node.Overlay.DrawCentered(renderPosition + (node.Angle * Scale), Color.White, Scale, -node.Rotation);
                }
            }
            center.DrawCentered(renderPosition, Color.White, Scale, timer);
        }

        public class Node
        {
            public MTexture Base;
            public MTexture Overlay;
            public float Rotation;
            public Vector2 Angle;
            public bool Enabled;
        }

        private class Eyeballs : Entity
        {
            public DustGraphic Dust;
            public Color Color;

            public Eyeballs(DustGraphic dust)
            {
                Dust = dust;
                Depth = Dust.Entity.Depth - 1;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Color = DustStyles.Get(scene).EyeColor;
            }

            public override void Update()
            {
                base.Update();
                if (Dust.Entity != null && Dust.Scene != null)
                {
                    return;
                }

                RemoveSelf();
            }

            public override void Render()
            {
                if (!Dust.Visible || !Dust.Entity.Visible)
                {
                    return;
                }

                Vector2 vector2 = new Vector2(-Dust.EyeDirection.Y, Dust.EyeDirection.X).SafeNormalize();
                if (Dust.leftEyeVisible)
                {
                    Dust.eyeTexture.DrawCentered(Dust.RenderPosition + (((Dust.EyeDirection * 5f) + (vector2 * 3f)) * Dust.Scale), Color, Dust.Scale);
                }

                if (!Dust.rightEyeVisible)
                {
                    return;
                }

                Dust.eyeTexture.DrawCentered(Dust.RenderPosition + (((Dust.EyeDirection * 5f) - (vector2 * 3f)) * Dust.Scale), Color, Dust.Scale);
            }
        }
    }
}
