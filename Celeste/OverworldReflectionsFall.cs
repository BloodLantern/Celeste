// Decompiled with JetBrains decompiler
// Type: Celeste.OverworldReflectionsFall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class OverworldReflectionsFall : Scene
    {
        private readonly Level returnTo;
        private readonly Action returnCallback;
        private readonly Maddy3D maddy;
        private readonly MountainRenderer mountain;
        private MountainCamera startCamera = new(new Vector3(-8f, 12f, -0.4f), new Vector3(-2f, 9f, -0.5f));
        private MountainCamera fallCamera = new(new Vector3(-10f, 6f, -0.4f), new Vector3(-4.25f, 1.5f, -1.25f));

        public OverworldReflectionsFall(Level returnTo, Action returnCallback)
        {
            this.returnTo = returnTo;
            this.returnCallback = returnCallback;
            Add(mountain = new MountainRenderer());
            mountain.SnapCamera(-1, new MountainCamera(startCamera.Position + ((startCamera.Target - startCamera.Position).SafeNormalize() * 2f), startCamera.Target));
            Add(new HiresSnow()
            {
                ParticleAlpha = 0.0f
            });
            Add(new Snow3D(mountain.Model));
            Add(maddy = new Maddy3D(mountain));
            maddy.Falling();
            Add(new Entity()
            {
                 new Coroutine(Routine())
            });
        }

        private IEnumerator Routine()
        {
            _ = (double)mountain.EaseCamera(-1, startCamera, new float?(0.4f));
            float duration = 4f;
            maddy.Position = startCamera.Target;
            for (int i = 0; i < 30; ++i)
            {
                maddy.Position = startCamera.Target + new Vector3(Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f), Calc.Random.Range(-0.05f, 0.05f));
                yield return 0.01f;
            }
            yield return 0.1f;
            maddy.Add(new Coroutine(MaddyFall(duration + 0.1f)));
            yield return 0.1f;
            _ = (double)mountain.EaseCamera(-1, fallCamera, new float?(duration));
            mountain.ForceNearFog = true;
            yield return duration;
            yield return 0.25f;
            _ = (double)mountain.EaseCamera(-1, new MountainCamera(fallCamera.Position + (mountain.Model.Forward * 3f), fallCamera.Target), new float?(0.5f));
            Return();
        }

        private IEnumerator MaddyFall(float duration)
        {
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / duration)
            {
                maddy.Position = Vector3.Lerp(startCamera.Target, fallCamera.Target, p);
                yield return null;
            }
        }

        private void Return()
        {
            FadeWipe fadeWipe = new(this, false, () =>
            {
                mountain.Dispose();
                if (returnTo != null)
                {
                    Engine.Scene = returnTo;
                }

                returnCallback();
            });
        }
    }
}
