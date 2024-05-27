using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public abstract class ScreenWipe : Renderer
    {
        public static Color WipeColor = Color.Black;
        public Scene Scene;
        public bool WipeIn;
        public float Percent;
        public Action OnComplete;
        public bool Completed;
        public float Duration = 0.5f;
        public float EndTimer;
        private bool ending;
        public const int Left = -10;
        public const int Top = -10;

        public int Right => 1930;

        public int Bottom => 1090;

        public ScreenWipe(Scene scene, bool wipeIn, Action onComplete = null)
        {
            Scene = scene;
            WipeIn = wipeIn;
            if (Scene is Level)
                (Scene as Level).Wipe = this;
            Scene.Add(this);
            OnComplete = onComplete;
        }

        public IEnumerator Wait()
        {
            while (Percent < 1.0)
                yield return null;
        }

        public override void Update(Scene scene)
        {
            if (!Completed)
            {
                if (Percent < 1.0)
                    Percent = Calc.Approach(Percent, 1f, Engine.RawDeltaTime / Duration);
                else if (EndTimer > 0.0)
                    EndTimer -= Engine.RawDeltaTime;
                else
                    Completed = true;
            }
            else
            {
                if (ending)
                    return;
                ending = true;
                scene.Remove(this);
                if (scene is Level && (scene as Level).Wipe == this)
                    (scene as Level).Wipe = null;
                if (OnComplete == null)
                    return;
                OnComplete();
            }
        }

        public virtual void Cancel()
        {
            Scene.Remove(this);
            if (Scene is not Level)
                return;
            (Scene as Level).Wipe = null;
        }

        public static void DrawPrimitives(VertexPositionColor[] vertices) => GFX.DrawVertices(Matrix.CreateScale(Engine.Graphics.GraphicsDevice.Viewport.Width / 1920f), vertices, vertices.Length);
    }
}
