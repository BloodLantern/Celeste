using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class OverworldReflectionsFall : Scene
    {
        private Level returnTo;
        private Action returnCallback;
        private Maddy3D maddy;
        private MountainRenderer mountain;
        private MountainCamera startCamera = new MountainCamera(new Vector3(-8f, 12f, -0.4f), new Vector3(-2f, 9f, -0.5f));
        private MountainCamera fallCamera = new MountainCamera(new Vector3(-10f, 6f, -0.4f), new Vector3(-4.25f, 1.5f, -1.25f));

        public OverworldReflectionsFall(Level returnTo, Action returnCallback)
        {
            this.returnTo = returnTo;
            this.returnCallback = returnCallback;
            Add(mountain = new MountainRenderer());
            mountain.SnapCamera(-1, new MountainCamera(startCamera.Position + (startCamera.Target - startCamera.Position).SafeNormalize() * 2f, startCamera.Target));
            Add(new HiresSnow
            {
                ParticleAlpha = 0.0f
            });
            Add(new Snow3D(mountain.Model));
            Add(maddy = new Maddy3D(mountain));
            maddy.Falling();
            Add(new Entity
            {
                new Coroutine(Routine())
            });
        }

        private IEnumerator Routine()
        {
            double num1 = mountain.EaseCamera(-1, startCamera, 0.4f);
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
            double num2 = mountain.EaseCamera(-1, fallCamera, duration);
            mountain.ForceNearFog = true;
            yield return duration;
            yield return 0.25f;
            double num3 = mountain.EaseCamera(-1, new MountainCamera(fallCamera.Position + mountain.Model.Forward * 3f, fallCamera.Target), 0.5f);
            Return();
        }

        private IEnumerator MaddyFall(float duration)
        {
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / duration)
            {
                maddy.Position = Vector3.Lerp(startCamera.Target, fallCamera.Target, p);
                yield return null;
            }
        }

        private void Return()
        {
            FadeWipe fadeWipe = new FadeWipe(this, false, () =>
            {
                mountain.Dispose();
                if (returnTo != null)
                    Engine.Scene = returnTo;
                returnCallback();
            });
        }
    }
}
