// Decompiled with JetBrains decompiler
// Type: Celeste.HeightDisplay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class HeightDisplay : Entity
    {
        private readonly int index;
        private readonly string text = "";
        private readonly string leftText = "";
        private readonly string rightText = "";
        private readonly float leftSize;
        private readonly float rightSize;
        private readonly float numberSize;
        private Vector2 size;
        private readonly int height;
        private float approach;
        private float ease;
        private float pulse;
        private string spawnedLevel;
        private bool setAudioProgression;
        private bool easingCamera = true;

        private bool drawText => index >= 0 && ease > 0.0 && !string.IsNullOrEmpty(text);

        public HeightDisplay(int index)
        {
            Tag = (int)Tags.HUD | (int)Tags.Persistent;
            this.index = index;
            string name = "CH7_HEIGHT_" + (index < 0 ? "START" : index.ToString());
            if (index >= 0 && Dialog.Has(name))
            {
                text = Dialog.Get(name);
                text = text.ToUpper();
                height = (index + 1) * 500;
                approach = index * 500;
                int length = text.IndexOf("{X}");
                leftText = text.Substring(0, length);
                leftSize = ActiveFont.Measure(leftText).X;
                rightText = text.Substring(length + 3);
                numberSize = ActiveFont.Measure(height.ToString()).X;
                rightSize = ActiveFont.Measure(rightText).X;
                size = ActiveFont.Measure(leftText + height + rightText);
            }
            Add(new Coroutine(Routine()));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            spawnedLevel = (scene as Level).Session.Level;
        }

        private IEnumerator Routine()
        {
            HeightDisplay heightDisplay = this;
            Player player;
            while (true)
            {
                player = heightDisplay.Scene.Tracker.GetEntity<Player>();
                if (player == null || !((heightDisplay.Scene as Level).Session.Level != heightDisplay.spawnedLevel))
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            heightDisplay.StepAudioProgression();
            heightDisplay.easingCamera = false;
            yield return 0.1f;
            heightDisplay.Add(new Coroutine(heightDisplay.CameraUp()));
            if (!string.IsNullOrEmpty(heightDisplay.text) && heightDisplay.index >= 0)
            {
                _ = Audio.Play("event:/game/07_summit/altitude_count");
            }

            while ((double)(heightDisplay.ease += Engine.DeltaTime / 0.15f) < 1.0)
            {
                yield return null;
            }

            while (heightDisplay.approach < (double)heightDisplay.height && !player.OnGround())
            {
                yield return null;
            }

            heightDisplay.approach = heightDisplay.height;
            heightDisplay.pulse = 1f;
            while ((double)(heightDisplay.pulse -= Engine.DeltaTime * 4f) > 0.0)
            {
                yield return null;
            }

            heightDisplay.pulse = 0.0f;
            yield return 1f;
            while ((double)(heightDisplay.ease -= Engine.DeltaTime / 0.15f) > 0.0)
            {
                yield return null;
            }

            heightDisplay.RemoveSelf();
        }

        private IEnumerator CameraUp()
        {
            HeightDisplay heightDisplay = this;
            heightDisplay.easingCamera = true;
            Level level = heightDisplay.Scene as Level;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 1.5f)
            {
                level.Camera.Y = level.Bounds.Bottom - 180 + (float)(64.0 * (1.0 - (double)Ease.CubeOut(p)));
                yield return null;
            }
        }

        private void StepAudioProgression()
        {
            Session session = (Scene as Level).Session;
            if (setAudioProgression || index < 0 || session.Area.Mode != AreaMode.Normal)
            {
                return;
            }

            setAudioProgression = true;
            int num = index + 1;
            if (num <= 5)
            {
                session.Audio.Music.Progress = num;
            }
            else
            {
                session.Audio.Music.Event = "event:/music/lvl7/final_ascent";
            }

            session.Audio.Apply();
        }

        public override void Update()
        {
            if (index >= 0 && ease > 0.0)
            {
                if (height - (double)approach > 100.0)
                {
                    approach += 1000f * Engine.DeltaTime;
                }
                else if (height - (double)approach > 25.0)
                {
                    approach += 200f * Engine.DeltaTime;
                }
                else if (height - (double)approach > 5.0)
                {
                    approach += 50f * Engine.DeltaTime;
                }
                else if (height - (double)approach > 0.0)
                {
                    approach += 10f * Engine.DeltaTime;
                }
                else
                {
                    approach = height;
                }
            }
            Level scene = Scene as Level;
            if (!easingCamera)
            {
                scene.Camera.Y = scene.Bounds.Bottom - 180 + 64;
            }

            base.Update();
        }

        public override void Render()
        {
            if (Scene.Paused || !drawText)
            {
                return;
            }

            Vector2 vector2_1 = new Vector2(1920f, 1080f) / 2f;
            float num = (float)(1.2000000476837158 + (pulse * 0.20000000298023224));
            Vector2 vector2_2 = size * num;
            float y = Ease.SineInOut(ease);
            Vector2 vector2_3 = new(1f, y);
            Draw.Rect(vector2_1.X - ((float)((vector2_2.X + 64.0) * 0.5) * vector2_3.X), vector2_1.Y - ((float)((vector2_2.Y + 32.0) * 0.5) * vector2_3.Y), (vector2_2.X + 64f) * vector2_3.X, (vector2_2.Y + 32f) * vector2_3.Y, Color.Black);
            Vector2 position = vector2_1 + new Vector2((float)(-(double)vector2_2.X * 0.5), 0.0f);
            Vector2 scale = vector2_3 * num;
            Color color = Color.White * y;
            ActiveFont.Draw(leftText, position, new Vector2(0.0f, 0.5f), scale, color);
            ActiveFont.Draw(rightText, position + (Vector2.UnitX * (leftSize + numberSize) * num), new Vector2(0.0f, 0.5f), scale, color);
            ActiveFont.Draw(((int)approach).ToString(), position + (Vector2.UnitX * (leftSize + (numberSize * 0.5f)) * num), new Vector2(0.5f, 0.5f), scale, color);
        }

        public override void Removed(Scene scene)
        {
            StepAudioProgression();
            base.Removed(scene);
        }
    }
}
