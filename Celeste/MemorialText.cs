using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class MemorialText : Entity
    {
        public bool Show;
        public bool Dreamy;
        public Memorial Memorial;
        private float index;
        private string message;
        private float alpha;
        private float timer;
        private float widestCharacter;
        private int firstLineLength;
        private SoundSource textSfx;
        private bool textSfxPlaying;

        public MemorialText(Memorial memorial, bool dreamy)
        {
            AddTag((int) Tags.HUD);
            AddTag((int) Tags.PauseUpdate);
            Add(textSfx = new SoundSource());
            Dreamy = dreamy;
            Memorial = memorial;
            message = Dialog.Clean(nameof (memorial));
            firstLineLength = CountToNewline(0);
            for (int index = 0; index < message.Length; ++index)
            {
                float x = ActiveFont.Measure(message[index]).X;
                if (x > (double) widestCharacter)
                    widestCharacter = x;
            }
            widestCharacter *= 0.9f;
        }

        public override void Update()
        {
            base.Update();
            if ((Scene as Level).Paused)
            {
                textSfx.Pause();
            }
            else
            {
                timer += Engine.DeltaTime;
                if (!Show)
                {
                    alpha = Calc.Approach(alpha, 0.0f, Engine.DeltaTime);
                    if (alpha <= 0.0)
                        index = firstLineLength;
                }
                else
                {
                    alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
                    if (alpha >= 1.0)
                        index = Calc.Approach(index, message.Length, 32f * Engine.DeltaTime);
                }
                if (Show && alpha >= 1.0 && index < (double) message.Length)
                {
                    if (!textSfxPlaying)
                    {
                        textSfxPlaying = true;
                        textSfx.Play(Dreamy ? "event:/ui/game/memorial_dream_text_loop" : "event:/ui/game/memorial_text_loop");
                        textSfx.Param("end", 0.0f);
                    }
                }
                else if (textSfxPlaying)
                {
                    textSfxPlaying = false;
                    textSfx.Stop();
                    textSfx.Param("end", 1f);
                }
                textSfx.Resume();
            }
        }

        private int CountToNewline(int start)
        {
            int index = start;
            while (index < message.Length && message[index] != '\n')
                ++index;
            return index - start;
        }

        public override void Render()
        {
            if ((Scene as Level).FrozenOrPaused || (Scene as Level).Completed || this.index <= 0.0 || alpha <= 0.0)
                return;
            Camera camera = SceneAs<Level>().Camera;
            Vector2 vector2 = new Vector2((float) ((Memorial.X - (double) camera.X) * 6.0), (float) ((Memorial.Y - (double) camera.Y) * 6.0 - 350.0 - ActiveFont.LineHeight * 3.2999999523162842));
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                vector2.X = 1920f - vector2.X;
            float num1 = Ease.CubeInOut(alpha);
            int num2 = (int) Math.Min(message.Length, this.index);
            int num3 = 0;
            float num4 = (float) (64.0 * (1.0 - num1));
            int newline = CountToNewline(0);
            for (int index = 0; index < num2; ++index)
            {
                char character = message[index];
                if (character == '\n')
                {
                    num3 = 0;
                    newline = CountToNewline(index + 1);
                    num4 += ActiveFont.LineHeight * 1.1f;
                }
                else
                {
                    float x1 = 1f;
                    float x2 = (float) (-newline * (double) widestCharacter / 2.0 + (num3 + 0.5) * widestCharacter);
                    float num5 = 0.0f;
                    if (Dreamy && character != ' ' && character != '-' && character != '\n')
                    {
                        character = message[(index + (int) (Math.Sin(timer * 2.0 + index / 8.0) * 4.0) + message.Length) % message.Length];
                        num5 = (float) Math.Sin(timer * 2.0 + index / 8.0) * 8f;
                        x1 = Math.Sin(timer * 4.0 + index / 16.0) < 0.0 ? -1f : 1f;
                    }
                    ActiveFont.Draw(character, vector2 + new Vector2(x2, num4 + num5), new Vector2(0.5f, 1f), new Vector2(x1, 1f), Color.White * num1);
                    ++num3;
                }
            }
        }
    }
}
