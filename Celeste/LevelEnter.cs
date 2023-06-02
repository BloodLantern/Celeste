using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class LevelEnter : Scene
    {
        private Session session;
        private Postcard postcard;
        private bool fromSaveData;

        public static void Go(Session session, bool fromSaveData)
        {
            HiresSnow snow = (HiresSnow) null;
            if (Engine.Scene is Overworld)
                snow = (Engine.Scene as Overworld).Snow;
            bool flag = !fromSaveData && session.StartedFromBeginning;
            if (flag && session.Area.ID == 0)
                Engine.Scene = (Scene) new IntroVignette(session, snow);
            else if (flag && session.Area.ID == 7 && session.Area.Mode == AreaMode.Normal)
                Engine.Scene = (Scene) new SummitVignette(session);
            else if (flag && session.Area.ID == 9 && session.Area.Mode == AreaMode.Normal)
                Engine.Scene = (Scene) new CoreVignette(session, snow);
            else
                Engine.Scene = (Scene) new LevelEnter(session, fromSaveData);
        }

        private LevelEnter(Session session, bool fromSaveData)
        {
            this.session = session;
            this.fromSaveData = fromSaveData;
            this.Add(new Entity()
            {
                (Component) new Coroutine(this.Routine())
            });
            this.Add((Monocle.Renderer) new HudRenderer());
        }

        private IEnumerator Routine()
        {
            LevelEnter levelEnter = this;
            int area = -1;
            if (levelEnter.session.StartedFromBeginning && !levelEnter.fromSaveData && levelEnter.session.Area.Mode == AreaMode.Normal && (!SaveData.Instance.Areas[levelEnter.session.Area.ID].Modes[0].Completed || SaveData.Instance.DebugMode) && levelEnter.session.Area.ID >= 1 && levelEnter.session.Area.ID <= 6)
                area = levelEnter.session.Area.ID;
            if (area >= 0)
            {
                yield return (object) 1f;
                levelEnter.Add((Entity) (levelEnter.postcard = new Postcard(Dialog.Get("postcard_area_" + (object) area), area)));
                yield return (object) levelEnter.postcard.DisplayRoutine();
            }
            if (levelEnter.session.StartedFromBeginning && !levelEnter.fromSaveData && levelEnter.session.Area.Mode == AreaMode.BSide)
            {
                LevelEnter.BSideTitle title = new LevelEnter.BSideTitle(levelEnter.session);
                levelEnter.Add((Entity) title);
                Audio.Play("event:/ui/main/bside_intro_text");
                yield return (object) title.EaseIn();
                yield return (object) 0.25f;
                yield return (object) title.EaseOut();
                yield return (object) 0.25f;
                title = (LevelEnter.BSideTitle) null;
            }
            Input.SetLightbarColor(AreaData.Get(levelEnter.session.Area).TitleBaseColor);
            Engine.Scene = (Scene) new LevelLoader(levelEnter.session);
        }

        public override void BeforeRender()
        {
            base.BeforeRender();
            if (this.postcard == null)
                return;
            this.postcard.BeforeRender();
        }

        private class BSideTitle : Entity
        {
            private string title;
            private string musicBy;
            private string artist;
            private MTexture artistImage;
            private string album;
            private float musicByWidth;
            private float[] fade = new float[3];
            private float[] offsets = new float[3];
            private float offset;

            public BSideTitle(Session session)
            {
                this.Tag = (int) Tags.HUD;
                switch (session.Area.ID)
                {
                    case 1:
                        this.artist = Credits.Remixers[0];
                        break;
                    case 2:
                        this.artist = Credits.Remixers[1];
                        break;
                    case 3:
                        this.artist = Credits.Remixers[2];
                        break;
                    case 4:
                        this.artist = Credits.Remixers[3];
                        break;
                    case 5:
                        this.artist = Credits.Remixers[4];
                        break;
                    case 6:
                        this.artist = Credits.Remixers[5];
                        break;
                    case 7:
                        this.artist = Credits.Remixers[6];
                        break;
                    case 9:
                        this.artist = Credits.Remixers[7];
                        break;
                }
                if (this.artist.StartsWith("image:"))
                    this.artistImage = GFX.Gui[this.artist.Substring(6)];
                this.title = Dialog.Get(AreaData.Get(session).Name) + " " + Dialog.Get(AreaData.Get(session).Name + "_remix");
                this.musicBy = Dialog.Get("remix_by") + " ";
                this.musicByWidth = ActiveFont.Measure(this.musicBy).X;
                this.album = Dialog.Get("remix_album");
            }

            public IEnumerator EaseIn()
            {
                LevelEnter.BSideTitle bsideTitle = this;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(0, 1f, 1f)));
                yield return (object) 0.2f;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(1, 1f, 1f)));
                yield return (object) 0.2f;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(2, 1f, 1f)));
                yield return (object) 1.8f;
            }

            public IEnumerator EaseOut()
            {
                LevelEnter.BSideTitle bsideTitle = this;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(0, 0.0f, 1f)));
                yield return (object) 0.2f;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(1, 0.0f, 1f)));
                yield return (object) 0.2f;
                bsideTitle.Add((Component) new Coroutine(bsideTitle.FadeTo(2, 0.0f, 1f)));
                yield return (object) 1f;
            }

            private IEnumerator FadeTo(int index, float target, float duration)
            {
                while ((double) (this.fade[index] = Calc.Approach(this.fade[index], target, Engine.DeltaTime / duration)) != (double) target)
                {
                    this.offsets[index] = (double) target != 0.0 ? (float) (-(double) Ease.CubeIn(1f - this.fade[index]) * 32.0) : Ease.CubeIn(1f - this.fade[index]) * 32f;
                    yield return (object) null;
                }
            }

            public override void Update()
            {
                base.Update();
                this.offset += Engine.DeltaTime * 32f;
            }

            public override void Render()
            {
                Vector2 vector2 = new Vector2(60f + this.offset, 800f);
                ActiveFont.Draw(this.title, vector2 + new Vector2(this.offsets[0], 0.0f), Color.White * this.fade[0]);
                ActiveFont.Draw(this.musicBy, vector2 + new Vector2(this.offsets[1], 60f), Color.White * this.fade[1]);
                if (this.artistImage != null)
                    this.artistImage.Draw(vector2 + new Vector2(this.musicByWidth + this.offsets[1], 68f), Vector2.Zero, Color.White * this.fade[1]);
                else
                    ActiveFont.Draw(this.artist, vector2 + new Vector2(this.musicByWidth + this.offsets[1], 60f), Color.White * this.fade[1]);
                ActiveFont.Draw(this.album, vector2 + new Vector2(this.offsets[2], 120f), Color.White * this.fade[2]);
            }
        }
    }
}
