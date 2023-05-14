// Decompiled with JetBrains decompiler
// Type: Celeste.RumbleTrigger
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
    public class RumbleTrigger : Trigger
    {
        private readonly bool manualTrigger;
        private bool started;
        private readonly bool persistent;
        private EntityID id;
        private float rumble;
        private readonly float left;
        private readonly float right;
        private readonly List<Decal> decals = new();
        private readonly List<CrumbleWallOnRumble> crumbles = new();

        public RumbleTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            manualTrigger = data.Bool(nameof(manualTrigger));
            persistent = data.Bool(nameof(persistent));
            this.id = id;
            Vector2[] vector2Array = data.NodesOffset(offset);
            if (vector2Array.Length < 2)
            {
                return;
            }

            left = Math.Min(vector2Array[0].X, vector2Array[1].X);
            right = Math.Max(vector2Array[0].X, vector2Array[1].X);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level scene1 = Scene as Level;
            bool flag = false;
            if (persistent && scene1.Session.GetFlag(id.ToString()))
            {
                flag = true;
            }

            foreach (CrumbleWallOnRumble entity in scene.Tracker.GetEntities<CrumbleWallOnRumble>())
            {
                if ((double)entity.X >= left && (double)entity.X <= right)
                {
                    if (flag)
                    {
                        entity.RemoveSelf();
                    }
                    else
                    {
                        crumbles.Add(entity);
                    }
                }
            }
            if (!flag)
            {
                foreach (Decal decal in scene.Entities.FindAll<Decal>())
                {
                    if (decal.IsCrack && (double)decal.X >= left && (double)decal.X <= right)
                    {
                        decal.Visible = false;
                        decals.Add(decal);
                    }
                }
                crumbles.Sort((a, b) => !Calc.Random.Chance(0.5f) ? 1 : -1);
            }
            if (!flag)
            {
                return;
            }

            RemoveSelf();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (manualTrigger)
            {
                return;
            }

            Invoke();
        }

        private void Invoke(float delay = 0.0f)
        {
            if (started)
            {
                return;
            }

            started = true;
            if (persistent)
            {
                (Scene as Level).Session.SetFlag(id.ToString());
            }

            Add(new Coroutine(RumbleRoutine(delay)));
            Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
        }

        private IEnumerator RumbleRoutine(float delay)
        {
            RumbleTrigger rumbleTrigger = this;
            yield return delay;
            _ = rumbleTrigger.Scene;
            rumbleTrigger.rumble = 1f;
            _ = Audio.Play("event:/new_content/game/10_farewell/quake_onset", rumbleTrigger.Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            foreach (Entity decal in rumbleTrigger.decals)
            {
                decal.Visible = true;
            }

            foreach (CrumbleWallOnRumble crumble in rumbleTrigger.crumbles)
            {
                crumble.Break();
                yield return 0.05f;
            }
        }

        public override void Update()
        {
            base.Update();
            rumble = Calc.Approach(rumble, 0.0f, Engine.DeltaTime * 0.7f);
        }

        private void RenderDisplacement()
        {
            if (rumble <= 0.0 || Settings.Instance.ScreenShake == ScreenshakeAmount.Off)
            {
                return;
            }

            Camera camera = (Scene as Level).Camera;
            int num1 = (int)((double)camera.Left / 8.0) - 1;
            int num2 = (int)((double)camera.Right / 8.0) + 1;
            for (int index = num1; index <= num2; ++index)
            {
                Color color = new(0.5f, 0.5f + ((float)Math.Sin((Scene.TimeActive * 60.0) + (index * 0.40000000596046448)) * 0.06f * rumble), 0.0f, 1f);
                Draw.Rect(index * 8, camera.Top - 2f, 8f, 184f, color);
            }
        }

        public static void ManuallyTrigger(float x, float delay)
        {
            foreach (RumbleTrigger rumbleTrigger in Engine.Scene.Entities.FindAll<RumbleTrigger>())
            {
                if (rumbleTrigger.manualTrigger && (double)x >= rumbleTrigger.left && (double)x <= rumbleTrigger.right)
                {
                    rumbleTrigger.Invoke(delay);
                }
            }
        }
    }
}
