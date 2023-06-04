using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class LevelEnter : Scene
    {
        private readonly Session session;
        private Postcard postcard;
        private readonly bool fromSaveData;

        public static void Go(Session session, bool fromSaveData)
        {
            HiresSnow snow = null;
            if (Engine.Scene is Overworld)
                snow = (Engine.Scene as Overworld).Snow;
            bool flag = !fromSaveData && session.StartedFromBeginning;
            if (flag && session.Area.ID == 0)
                Engine.Scene = new IntroVignette(session, snow);
            else if (flag && session.Area.ID == 7 && session.Area.Mode == AreaMode.Normal)
                Engine.Scene = new SummitVignette(session);
            else if (flag && session.Area.ID == 9 && session.Area.Mode == AreaMode.Normal)
                Engine.Scene = new CoreVignette(session, snow);
            else
                Engine.Scene = new LevelEnter(session, fromSaveData);
        }

        private LevelEnter(Session session, bool fromSaveData)
        {
            this.session = session;
            this.fromSaveData = fromSaveData;
            Add(new Entity()
            {
                 new Coroutine(Routine())
            });
            Add(new HudRenderer());
        }

        private IEnumerator Routine()
        {
            int area = -1;
            if (session.StartedFromBeginning && !fromSaveData && session.Area.Mode == AreaMode.Normal && (!SaveData.Instance.Areas[session.Area.ID].Modes[0].Completed || SaveData.Instance.DebugMode) && session.Area.ID >= 1 && session.Area.ID <= 6)
                area = session.Area.ID;
            if (area >= 0)
            {
                yield return 1f;
                Add(postcard = new Postcard(Dialog.Get("postcard_area_" + area), area));
                yield return postcard.DisplayRoutine();
            }
            if (session.StartedFromBeginning && !fromSaveData && session.Area.Mode == AreaMode.BSide)
            {
                BSideTitle title = new(session);
                Add(title);
                Audio.Play("event:/ui/main/bside_intro_text");
                yield return title.EaseIn();
                yield return 0.25f;
                yield return title.EaseOut();
                yield return 0.25f;
            }
            Input.SetLightbarColor(AreaData.Get(session.Area).TitleBaseColor);
            Engine.Scene = new LevelLoader(session);
        }

        public override void BeforeRender()
        {
            base.BeforeRender();
            if (postcard == null)
                return;
            postcard.BeforeRender();
        }

        private class BSideTitle : Entity
        {
            private readonly string title;
            private readonly string musicBy;
            private readonly string artist;
            private readonly MTexture artistImage;
            private readonly string album;
            private readonly float musicByWidth;
            private readonly float[] fade = new float[3];
            private readonly float[] offsets = new float[3];
            private float offset;

            public BSideTitle(Session session)
            {
                Tag = (int) Tags.HUD;
                switch (session.Area.ID)
                {
                    case 1:
                        artist = Credits.Remixers[0];
                        break;
                    case 2:
                        artist = Credits.Remixers[1];
                        break;
                    case 3:
                        artist = Credits.Remixers[2];
                        break;
                    case 4:
                        artist = Credits.Remixers[3];
                        break;
                    case 5:
                        artist = Credits.Remixers[4];
                        break;
                    case 6:
                        artist = Credits.Remixers[5];
                        break;
                    case 7:
                        artist = Credits.Remixers[6];
                        break;
                    case 9:
                        artist = Credits.Remixers[7];
                        break;
                }
                if (artist.StartsWith("image:"))
                    artistImage = GFX.Gui[artist.Substring(6)];
                title = Dialog.Get(AreaData.Get(session).Name) + " " + Dialog.Get(AreaData.Get(session).Name + "_remix");
                musicBy = Dialog.Get("remix_by") + " ";
                musicByWidth = ActiveFont.Measure(musicBy).X;
                album = Dialog.Get("remix_album");
            }

            public IEnumerator EaseIn()
            {
                BSideTitle bsideTitle = this;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(0, 1f, 1f)));
                yield return 0.2f;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(1, 1f, 1f)));
                yield return 0.2f;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(2, 1f, 1f)));
                yield return 1.8f;
            }

            public IEnumerator EaseOut()
            {
                BSideTitle bsideTitle = this;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(0, 0.0f, 1f)));
                yield return 0.2f;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(1, 0.0f, 1f)));
                yield return 0.2f;
                bsideTitle.Add(new Coroutine(bsideTitle.FadeTo(2, 0.0f, 1f)));
                yield return 1f;
            }

            private IEnumerator FadeTo(int index, float target, float duration)
            {
                while ((double) (fade[index] = Calc.Approach(fade[index], target, Engine.DeltaTime / duration)) != (double) target)
                {
                    offsets[index] = (double) target != 0.0 ? (float) (-(double) Ease.CubeIn(1f - fade[index]) * 32.0) : Ease.CubeIn(1f - fade[index]) * 32f;
                    yield return null;
                }
            }

            public override void Update()
            {
                base.Update();
                offset += Engine.DeltaTime * 32f;
            }

            public override void Render()
            {
                Vector2 vector2 = new(60f + offset, 800f);
                ActiveFont.Draw(title, vector2 + new Vector2(offsets[0], 0.0f), Color.White * fade[0]);
                ActiveFont.Draw(musicBy, vector2 + new Vector2(offsets[1], 60f), Color.White * fade[1]);
                if (artistImage != null)
                    artistImage.Draw(vector2 + new Vector2(musicByWidth + offsets[1], 68f), Vector2.Zero, Color.White * fade[1]);
                else
                    ActiveFont.Draw(artist, vector2 + new Vector2(musicByWidth + offsets[1], 60f), Color.White * fade[1]);
                ActiveFont.Draw(album, vector2 + new Vector2(offsets[2], 120f), Color.White * fade[2]);
            }
        }
    }
}
