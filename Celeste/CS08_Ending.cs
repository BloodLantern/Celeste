using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS08_Ending : CutsceneEntity
    {
        private Player player;
        private NPC08_Granny granny;
        private NPC08_Theo theo;
        private BadelineDummy badeline;
        private Entity oshiro;
        private Image vignette;
        private Image vignettebg;
        private string endingDialog;
        private float fade;
        private bool showVersion;
        private float versionAlpha;
        private Coroutine cutscene;
        private string version = Celeste.Instance.Version.ToString();

        public CS08_Ending()
            : base(false, true)
        {
            Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
            RemoveOnSkipped = false;
        }

        public override void OnBegin(Level level)
        {
            level.SaveQuitDisabled = true;
            int totalStrawberries = SaveData.Instance.TotalStrawberries;
            string id;
            if (totalStrawberries < 20)
            {
                id = "final1";
                endingDialog = "EP_PIE_DISAPPOINTED";
            }
            else if (totalStrawberries < 50)
            {
                id = "final2";
                endingDialog = "EP_PIE_GROSSED_OUT";
            }
            else if (totalStrawberries < 90)
            {
                id = "final3";
                endingDialog = "EP_PIE_OKAY";
            }
            else if (totalStrawberries < 150)
            {
                id = "final4";
                endingDialog = "EP_PIE_REALLY_GOOD";
            }
            else
            {
                id = "final5";
                endingDialog = "EP_PIE_AMAZING";
            }
            Add(vignettebg = new Image(GFX.Portraits["finalbg"]));
            vignettebg.Visible = false;
            Add(vignette = new Image(GFX.Portraits[id]));
            vignette.Visible = false;
            vignette.CenterOrigin();
            vignette.Position = Celeste.TargetCenter;
            Add(cutscene = new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS08_Ending cs08Ending = this;
            level.ZoomSnap(new Vector2(164f, 120f), 2f);
            level.Wipe.Cancel();
            FadeWipe fadeWipe1 = new FadeWipe(level, true);
            while (cs08Ending.player == null)
            {
                cs08Ending.granny = level.Entities.FindFirst<NPC08_Granny>();
                cs08Ending.theo = level.Entities.FindFirst<NPC08_Theo>();
                cs08Ending.player = level.Tracker.GetEntity<Player>();
                yield return null;
            }
            cs08Ending.player.StateMachine.State = 11;
            yield return 1f;
            yield return cs08Ending.player.DummyWalkToExact((int) cs08Ending.player.X + 16);
            yield return 0.25f;
            yield return Textbox.Say("EP_CABIN", cs08Ending.BadelineEmerges, cs08Ending.OshiroEnters, cs08Ending.OshiroSettles, cs08Ending.MaddyTurns);
            FadeWipe fadeWipe2 = new FadeWipe(cs08Ending.Level, false);
            fadeWipe2.Duration = 1.5f;
            yield return fadeWipe2.Wait();
            cs08Ending.fade = 1f;
            yield return Textbox.Say("EP_PIE_START");
            yield return 0.5f;
            cs08Ending.vignettebg.Visible = true;
            cs08Ending.vignette.Visible = true;
            cs08Ending.vignettebg.Color = Color.Black;
            cs08Ending.vignette.Color = Color.White * 0.0f;
            cs08Ending.Add(cs08Ending.vignette);
            float p1;
            for (p1 = 0.0f; p1 < 1.0; p1 += Engine.DeltaTime)
            {
                cs08Ending.vignette.Color = Color.White * Ease.CubeIn(p1);
                cs08Ending.vignette.Scale = Vector2.One * (float) (1.0 + 0.25 * (1.0 - p1));
                cs08Ending.vignette.Rotation = (float) (0.05000000074505806 * (1.0 - p1));
                yield return null;
            }
            cs08Ending.vignette.Color = Color.White;
            cs08Ending.vignettebg.Color = Color.White;
            yield return 2f;
            p1 = 1f;
            float p2;
            for (p2 = 0.0f; p2 < 1.0; p2 += Engine.DeltaTime / p1)
            {
                float amount = Ease.CubeOut(p2);
                cs08Ending.vignette.Position = Vector2.Lerp(Celeste.TargetCenter, Celeste.TargetCenter + new Vector2(0.0f, 140f), amount);
                cs08Ending.vignette.Scale = Vector2.One * (float) (0.64999997615814209 + 0.34999999403953552 * (1.0 - amount));
                cs08Ending.vignette.Rotation = -0.025f * amount;
                yield return null;
            }
            yield return Textbox.Say(cs08Ending.endingDialog);
            yield return 0.25f;
            p1 = 2f;
            Vector2 posFrom = cs08Ending.vignette.Position;
            p2 = cs08Ending.vignette.Rotation;
            float scaleFrom = cs08Ending.vignette.Scale.X;
            for (float p3 = 0.0f; p3 < 1.0; p3 += Engine.DeltaTime / p1)
            {
                float amount = Ease.CubeOut(p3);
                cs08Ending.vignette.Position = Vector2.Lerp(posFrom, Celeste.TargetCenter, amount);
                cs08Ending.vignette.Scale = Vector2.One * MathHelper.Lerp(scaleFrom, 1f, amount);
                cs08Ending.vignette.Rotation = MathHelper.Lerp(p2, 0.0f, amount);
                yield return null;
            }
            posFrom = new Vector2();
            cs08Ending.EndCutscene(level, false);
        }

        public override void OnEnd(Level level)
        {
            vignette.Visible = true;
            vignette.Color = Color.White;
            vignette.Position = Celeste.TargetCenter;
            vignette.Scale = Vector2.One;
            vignette.Rotation = 0.0f;
            if (player != null)
                player.Speed = Vector2.Zero;
            Scene.Entities.FindFirst<Textbox>()?.RemoveSelf();
            cutscene.RemoveSelf();
            Add(new Coroutine(EndingRoutine()));
        }

        private IEnumerator EndingRoutine()
        {
            CS08_Ending cs08Ending = this;
            cs08Ending.Level.InCutscene = true;
            cs08Ending.Level.PauseLock = true;
            yield return 0.5f;
            TimeSpan timeSpan = TimeSpan.FromTicks(SaveData.Instance.Time);
            string str = ((int) timeSpan.TotalHours) + timeSpan.ToString("\\:mm\\:ss\\.fff");
            StrawberriesCounter strawbs = new StrawberriesCounter(true, SaveData.Instance.TotalStrawberries, 175, true);
            DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, true, SaveData.Instance.TotalDeaths);
            TimeDisplay time = new TimeDisplay(str);
            float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(str);
            cs08Ending.Add(strawbs);
            cs08Ending.Add(deaths);
            cs08Ending.Add(time);
            Vector2 from = new Vector2(960f, 1180f);
            Vector2 to = new Vector2(960f, 940f);
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.5f)
            {
                Vector2 vector2 = Vector2.Lerp(from, to, Ease.CubeOut(p));
                strawbs.Position = vector2 + new Vector2(-170f, 0.0f);
                deaths.Position = vector2 + new Vector2(170f, 0.0f);
                time.Position = vector2 + new Vector2((float) (-(double) timeWidth / 2.0), 100f);
                yield return null;
            }
            strawbs = null;
            deaths = null;
            time = null;
            from = new Vector2();
            to = new Vector2();
            cs08Ending.showVersion = true;
            yield return 0.25f;
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            cs08Ending.showVersion = false;
            yield return 0.25f;
            cs08Ending.Level.CompleteArea(false);
        }

        private IEnumerator MaddyTurns()
        {
            yield return 0.1f;
            player.Facing = (Facings) (-(int) player.Facing);
            yield return 0.1f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator BadelineEmerges()
        {
                Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
                Level.Session.Inventory.Dashes = 1;
                player.Dashes = 1;
                Level.Add(badeline = new BadelineDummy(player.Position));
                Audio.Play("event:/char/badeline/maddy_split", player.Position);
                badeline.Sprite.Scale.X = 1f;
                yield return badeline.FloatTo(player.Position + new Vector2(-12f, -16f), 1, false);
        }

        private IEnumerator OshiroEnters()
        {
            CS08_Ending cs08Ending = this;
            FadeWipe fadeWipe = new FadeWipe(cs08Ending.Level, false);
            fadeWipe.Duration = 1.5f;
            yield return fadeWipe.Wait();
            cs08Ending.fade = 1f;
            yield return 0.25f;
            float x = cs08Ending.player.X;
            cs08Ending.player.X = cs08Ending.granny.X + 8f;
            cs08Ending.badeline.X = cs08Ending.player.X + 12f;
            cs08Ending.player.Facing = Facings.Left;
            cs08Ending.badeline.Sprite.Scale.X = -1f;
            cs08Ending.granny.X = x + 8f;
            cs08Ending.theo.X += 16f;
            cs08Ending.Level.Add(cs08Ending.oshiro = new Entity(new Vector2(cs08Ending.granny.X - 24f, cs08Ending.granny.Y + 4f)));
            OshiroSprite oshiroSprite = new OshiroSprite(1);
            cs08Ending.oshiro.Add(oshiroSprite);
            cs08Ending.fade = 0.0f;
            new FadeWipe(cs08Ending.Level, true).Duration = 1f;
            yield return 0.25f;
            while (cs08Ending.oshiro.Y > cs08Ending.granny.Y - 4.0)
            {
                cs08Ending.oshiro.Y -= Engine.DeltaTime * 32f;
                yield return null;
            }
        }

        private IEnumerator OshiroSettles()
        {
            Vector2 from = oshiro.Position;
            Vector2 to = oshiro.Position + new Vector2(40f, 8f);
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                oshiro.Position = Vector2.Lerp(from, to, p);
                yield return null;
            }
            granny.Sprite.Scale.X = 1f;
            yield return null;
        }

        public override void Update()
        {
            versionAlpha = Calc.Approach(versionAlpha, showVersion ? 1f : 0.0f, Engine.DeltaTime * 5f);
            base.Update();
        }

        public override void Render()
        {
            if (fade > 0.0)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
            base.Render();
            if (Settings.Instance.SpeedrunClock == SpeedrunType.Off || versionAlpha <= 0.0)
                return;
            AreaComplete.VersionNumberAndVariants(version, versionAlpha, 1f);
        }

        public class TimeDisplay : Component
        {
            public Vector2 Position;
            public string Time;

            public TimeDisplay(string time)
                : base(true, true)
            {
                Time = time;
            }

            public override void Render() => SpeedrunTimerDisplay.DrawTime(RenderPosition, Time);

            public Vector2 RenderPosition => ((Entity != null ? Entity.Position : Vector2.Zero) + Position).Round();
        }
    }
}
