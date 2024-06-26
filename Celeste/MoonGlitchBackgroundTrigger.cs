﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class MoonGlitchBackgroundTrigger : Trigger
    {
        private Duration duration;
        private bool triggered;
        private bool stayOn;
        private bool running;
        private bool doGlitch;

        public MoonGlitchBackgroundTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            duration = data.Enum<Duration>(nameof (duration));
            stayOn = data.Bool("stay");
            doGlitch = data.Bool("glitch", true);
        }

        public override void OnEnter(Player player) => Invoke();

        public void Invoke()
        {
            if (triggered)
                return;
            triggered = true;
            if (doGlitch)
            {
                Add(new Coroutine(InternalGlitchRoutine()));
            }
            else
            {
                if (stayOn)
                    return;
                MoonGlitchBackgroundTrigger.Toggle(false);
            }
        }

        private IEnumerator InternalGlitchRoutine()
        {
            MoonGlitchBackgroundTrigger backgroundTrigger = this;
            backgroundTrigger.running = true;
            backgroundTrigger.Tag = (int) Tags.Persistent;
            float duration;
            if (backgroundTrigger.duration == Duration.Short)
            {
                duration = 0.2f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("event:/new_content/game/10_farewell/glitch_short");
            }
            else if (backgroundTrigger.duration == Duration.Medium)
            {
                duration = 0.5f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                Audio.Play("event:/new_content/game/10_farewell/glitch_medium");
            }
            else
            {
                duration = 1.25f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                Audio.Play("event:/new_content/game/10_farewell/glitch_long");
            }
            yield return MoonGlitchBackgroundTrigger.GlitchRoutine(duration, backgroundTrigger.stayOn);
            backgroundTrigger.Tag = 0;
            backgroundTrigger.running = false;
        }

        private static void Toggle(bool on)
        {
            Level scene = Engine.Scene as Level;
            foreach (Backdrop backdrop in scene.Background.GetEach<Backdrop>("blackhole"))
                backdrop.ForceVisible = on;
            foreach (Backdrop backdrop in scene.Foreground.GetEach<Backdrop>("blackhole"))
                backdrop.ForceVisible = on;
        }

        private static void Fade(float alpha, bool max = false)
        {
            Level scene = Engine.Scene as Level;
            foreach (Backdrop backdrop in scene.Background.GetEach<Backdrop>("blackhole"))
                backdrop.FadeAlphaMultiplier = max ? Math.Max(backdrop.FadeAlphaMultiplier, alpha) : alpha;
            foreach (Backdrop backdrop in scene.Foreground.GetEach<Backdrop>("blackhole"))
                backdrop.FadeAlphaMultiplier = max ? Math.Max(backdrop.FadeAlphaMultiplier, alpha) : alpha;
        }

        public static IEnumerator GlitchRoutine(float duration, bool stayOn)
        {
            MoonGlitchBackgroundTrigger.Toggle(true);
            if (Settings.Instance.DisableFlashes)
            {
                float a;
                for (a = 0.0f; a < 1.0; a += Engine.DeltaTime / 0.1f)
                {
                    MoonGlitchBackgroundTrigger.Fade(a, true);
                    yield return null;
                }
                MoonGlitchBackgroundTrigger.Fade(1f);
                yield return duration;
                if (!stayOn)
                {
                    for (a = 0.0f; a < 1.0; a += Engine.DeltaTime / 0.1f)
                    {
                        MoonGlitchBackgroundTrigger.Fade(1f - a);
                        yield return null;
                    }
                    MoonGlitchBackgroundTrigger.Fade(1f);
                }
            }
            else if (duration > 0.40000000596046448)
            {
                Glitch.Value = 0.3f;
                yield return 0.2f;
                Glitch.Value = 0.0f;
                yield return (float) (duration - 0.40000000596046448);
                if (!stayOn)
                    Glitch.Value = 0.3f;
                yield return 0.2f;
                Glitch.Value = 0.0f;
            }
            else
            {
                Glitch.Value = 0.3f;
                yield return duration;
                Glitch.Value = 0.0f;
            }
            if (!stayOn)
                MoonGlitchBackgroundTrigger.Toggle(false);
        }

        public override void Removed(Scene scene)
        {
            if (running)
            {
                Glitch.Value = 0.0f;
                MoonGlitchBackgroundTrigger.Fade(1f);
                if (!stayOn)
                    MoonGlitchBackgroundTrigger.Toggle(false);
            }
            base.Removed(scene);
        }

        private enum Duration
        {
            Short,
            Medium,
            Long,
        }
    }
}
