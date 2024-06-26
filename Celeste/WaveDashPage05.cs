﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class WaveDashPage05 : WaveDashPage
    {
        private List<Display> displays = new List<Display>();

        public WaveDashPage05()
        {
            Transition = Transitions.Spiral;
            ClearColor = Calc.HexToColor("fff2cc");
        }

        public override void Added(WaveDashPresentation presentation)
        {
            base.Added(presentation);
            displays.Add(new Display(new Vector2(Width * 0.28f, Height - 600), Dialog.Get("WAVEDASH_PAGE5_INFO1"), "too_close", new Vector2(-50f, 20f)));
            displays.Add(new Display(new Vector2(Width * 0.72f, Height - 600), Dialog.Get("WAVEDASH_PAGE5_INFO2"), "too_far", new Vector2(-50f, -35f)));
        }

        public override IEnumerator Routine()
        {
            yield return 0.5f;
        }

        public override void Update()
        {
            foreach (Display display in displays)
                display.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEDASH_PAGE5_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            foreach (Display display in displays)
                display.Render();
        }

        private class Display
        {
            public Vector2 Position;
            public FancyText.Text Info;
            public WaveDashPlaybackTutorial Tutorial;
            private Coroutine routine;
            private float xEase;
            private float time;

            public Display(Vector2 position, string text, string tutorial, Vector2 tutorialOffset)
            {
                Position = position;
                Info = FancyText.Parse(text, 896, 8, defaultColor: Color.Black * 0.6f);
                Tutorial = new WaveDashPlaybackTutorial(tutorial, tutorialOffset, new Vector2(1f, 1f), new Vector2(1f, 1f));
                Tutorial.OnRender = () => Draw.Line(-64f, 20f, 64f, 20f, Color.Black);
                routine = new Coroutine(Routine());
            }

            private IEnumerator Routine()
            {
                PlayerPlayback playback = Tutorial.Playback;
                int step = 0;
                while (true)
                {
                    int frameIndex1 = playback.FrameIndex;
                    if (step % 2 == 0)
                        Tutorial.Update();
                    int frameIndex2 = playback.FrameIndex;
                    if (frameIndex1 != frameIndex2 && playback.FrameIndex == playback.FrameCount - 1)
                    {
                        while (time < 3.0)
                            yield return null;
                        yield return 0.1f;
                        while (xEase < 1.0)
                        {
                            xEase = Calc.Approach(xEase, 1f, Engine.DeltaTime * 4f);
                            yield return null;
                        }
                        xEase = 1f;
                        yield return 0.5f;
                        xEase = 0.0f;
                        time = 0.0f;
                    }
                    ++step;
                    yield return null;
                }
            }

            public void Update()
            {
                time += Engine.DeltaTime;
                routine.Update();
            }

            public void Render()
            {
                Tutorial.Render(Position, 4f);
                Info.DrawJustifyPerLine(Position + Vector2.UnitY * 200f, new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, 1f);
                if (xEase <= 0.0)
                    return;
                Vector2 vector = Calc.AngleToVector((float) ((1.0 - xEase) * 0.10000000149011612 + 0.78539818525314331), 1f);
                Vector2 vector2 = vector.Perpendicular();
                float num1 = (float) (0.5 + (1.0 - xEase) * 0.5);
                float thickness = 64f * num1;
                float num2 = 300f * num1;
                Vector2 position = Position;
                Draw.Line(position - vector * num2, position + vector * num2, Color.Red, thickness);
                Draw.Line(position - vector2 * num2, position + vector2 * num2, Color.Red, thickness);
            }
        }
    }
}
