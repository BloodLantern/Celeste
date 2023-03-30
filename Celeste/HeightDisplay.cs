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
        private int index;
        private string text = "";
        private string leftText = "";
        private string rightText = "";
        private float leftSize;
        private float rightSize;
        private float numberSize;
        private Vector2 size;
        private int height;
        private float approach;
        private float ease;
        private float pulse;
        private string spawnedLevel;
        private bool setAudioProgression;
        private bool easingCamera = true;

        private bool drawText => this.index >= 0 && (double) this.ease > 0.0 && !string.IsNullOrEmpty(this.text);

        public HeightDisplay(int index)
        {
            this.Tag = (int) Tags.HUD | (int) Tags.Persistent;
            this.index = index;
            string name = "CH7_HEIGHT_" + (index < 0 ? "START" : index.ToString());
            if (index >= 0 && Dialog.Has(name))
            {
                this.text = Dialog.Get(name);
                this.text = this.text.ToUpper();
                this.height = (index + 1) * 500;
                this.approach = (float) (index * 500);
                int length = this.text.IndexOf("{X}");
                this.leftText = this.text.Substring(0, length);
                this.leftSize = ActiveFont.Measure(this.leftText).X;
                this.rightText = this.text.Substring(length + 3);
                this.numberSize = ActiveFont.Measure(this.height.ToString()).X;
                this.rightSize = ActiveFont.Measure(this.rightText).X;
                this.size = ActiveFont.Measure(this.leftText + (object) this.height + this.rightText);
            }
            this.Add((Component) new Coroutine(this.Routine()));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.spawnedLevel = (scene as Level).Session.Level;
        }

        private IEnumerator Routine()
        {
            HeightDisplay heightDisplay = this;
            Player player;
            while (true)
            {
                player = heightDisplay.Scene.Tracker.GetEntity<Player>();
                if (player == null || !((heightDisplay.Scene as Level).Session.Level != heightDisplay.spawnedLevel))
                    yield return (object) null;
                else
                    break;
            }
            heightDisplay.StepAudioProgression();
            heightDisplay.easingCamera = false;
            yield return (object) 0.1f;
            heightDisplay.Add((Component) new Coroutine(heightDisplay.CameraUp()));
            if (!string.IsNullOrEmpty(heightDisplay.text) && heightDisplay.index >= 0)
                Audio.Play("event:/game/07_summit/altitude_count");
            while ((double) (heightDisplay.ease += Engine.DeltaTime / 0.15f) < 1.0)
                yield return (object) null;
            while ((double) heightDisplay.approach < (double) heightDisplay.height && !player.OnGround())
                yield return (object) null;
            heightDisplay.approach = (float) heightDisplay.height;
            heightDisplay.pulse = 1f;
            while ((double) (heightDisplay.pulse -= Engine.DeltaTime * 4f) > 0.0)
                yield return (object) null;
            heightDisplay.pulse = 0.0f;
            yield return (object) 1f;
            while ((double) (heightDisplay.ease -= Engine.DeltaTime / 0.15f) > 0.0)
                yield return (object) null;
            heightDisplay.RemoveSelf();
        }

        private IEnumerator CameraUp()
        {
            HeightDisplay heightDisplay = this;
            heightDisplay.easingCamera = true;
            Level level = heightDisplay.Scene as Level;
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 1.5f)
            {
                level.Camera.Y = (float) (level.Bounds.Bottom - 180) + (float) (64.0 * (1.0 - (double) Ease.CubeOut(p)));
                yield return (object) null;
            }
        }

        private void StepAudioProgression()
        {
            Session session = (this.Scene as Level).Session;
            if (this.setAudioProgression || this.index < 0 || session.Area.Mode != AreaMode.Normal)
                return;
            this.setAudioProgression = true;
            int num = this.index + 1;
            if (num <= 5)
                session.Audio.Music.Progress = num;
            else
                session.Audio.Music.Event = "event:/music/lvl7/final_ascent";
            session.Audio.Apply();
        }

        public override void Update()
        {
            if (this.index >= 0 && (double) this.ease > 0.0)
            {
                if ((double) this.height - (double) this.approach > 100.0)
                    this.approach += 1000f * Engine.DeltaTime;
                else if ((double) this.height - (double) this.approach > 25.0)
                    this.approach += 200f * Engine.DeltaTime;
                else if ((double) this.height - (double) this.approach > 5.0)
                    this.approach += 50f * Engine.DeltaTime;
                else if ((double) this.height - (double) this.approach > 0.0)
                    this.approach += 10f * Engine.DeltaTime;
                else
                    this.approach = (float) this.height;
            }
            Level scene = this.Scene as Level;
            if (!this.easingCamera)
                scene.Camera.Y = (float) (scene.Bounds.Bottom - 180 + 64);
            base.Update();
        }

        public override void Render()
        {
            if (this.Scene.Paused || !this.drawText)
                return;
            Vector2 vector2_1 = new Vector2(1920f, 1080f) / 2f;
            float num = (float) (1.2000000476837158 + (double) this.pulse * 0.20000000298023224);
            Vector2 vector2_2 = this.size * num;
            float y = Ease.SineInOut(this.ease);
            Vector2 vector2_3 = new Vector2(1f, y);
            Draw.Rect(vector2_1.X - (float) (((double) vector2_2.X + 64.0) * 0.5) * vector2_3.X, vector2_1.Y - (float) (((double) vector2_2.Y + 32.0) * 0.5) * vector2_3.Y, (vector2_2.X + 64f) * vector2_3.X, (vector2_2.Y + 32f) * vector2_3.Y, Color.Black);
            Vector2 position = vector2_1 + new Vector2((float) (-(double) vector2_2.X * 0.5), 0.0f);
            Vector2 scale = vector2_3 * num;
            Color color = Color.White * y;
            ActiveFont.Draw(this.leftText, position, new Vector2(0.0f, 0.5f), scale, color);
            ActiveFont.Draw(this.rightText, position + Vector2.UnitX * (this.leftSize + this.numberSize) * num, new Vector2(0.0f, 0.5f), scale, color);
            ActiveFont.Draw(((int) this.approach).ToString(), position + Vector2.UnitX * (this.leftSize + this.numberSize * 0.5f) * num, new Vector2(0.5f, 0.5f), scale, color);
        }

        public override void Removed(Scene scene)
        {
            this.StepAudioProgression();
            base.Removed(scene);
        }
    }
}
