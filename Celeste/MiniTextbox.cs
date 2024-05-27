using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    [Tracked]
    public class MiniTextbox : Entity
    {
        public const float TextScale = 0.75f;
        public const float BoxWidth = 1688f;
        public const float BoxHeight = 144f;
        public const float HudElementHeight = 180f;
        private int index;
        private FancyText.Text text;
        private MTexture box;
        private float ease;
        private bool closing;
        private Coroutine routine;
        private Sprite portrait;
        private FancyText.Portrait portraitData;
        private float portraitSize;
        private float portraitScale;
        private SoundSource talkerSfx;

        public static bool Displayed
        {
            get
            {
                foreach (MiniTextbox entity in Engine.Scene.Tracker.GetEntities<MiniTextbox>())
                {
                    if (!entity.closing && entity.ease > 0.25)
                        return true;
                }
                return false;
            }
        }

        public MiniTextbox(string dialogId)
        {
            Tag = (int) Tags.HUD | (int) Tags.TransitionUpdate;
            portraitSize = 112f;
            box = GFX.Portraits["textbox/default_mini"];
            text = FancyText.Parse(Dialog.Get(dialogId.Trim()), (int) (1688.0 - portraitSize - 32.0), 2);
            foreach (FancyText.Node node in text.Nodes)
            {
                if (node is FancyText.Portrait)
                {
                    FancyText.Portrait portrait = node as FancyText.Portrait;
                    portraitData = portrait;
                    this.portrait = GFX.PortraitsSpriteBank.Create("portrait_" + portrait.Sprite);
                    XmlElement xml = GFX.PortraitsSpriteBank.SpriteData["portrait_" + portrait.Sprite].Sources[0].XML;
                    portraitScale = portraitSize / xml.AttrFloat("size", 160f);
                    string id = "textbox/" + xml.Attr("textbox", "default") + "_mini";
                    if (GFX.Portraits.Has(id))
                        box = GFX.Portraits[id];
                    Add(this.portrait);
                }
            }
            Add(routine = new Coroutine(Routine()));
            routine.UseRawDeltaTime = true;
            Add(new TransitionListener
            {
                OnOutBegin = () =>
                {
                    if (closing)
                        return;
                    routine.Replace(Close());
                }
            });
            if (Level.DialogSnapshot == null)
                Level.DialogSnapshot = Audio.CreateSnapshot("snapshot:/dialogue_in_progress", false);
            Audio.ResumeSnapshot(Level.DialogSnapshot);
        }

        private IEnumerator Routine()
        {
            MiniTextbox miniTextbox1 = this;
            List<Entity> entities = miniTextbox1.Scene.Tracker.GetEntities<MiniTextbox>();
            foreach (MiniTextbox miniTextbox2 in entities)
            {
                if (miniTextbox2 != miniTextbox1)
                    miniTextbox2.Add(new Coroutine(miniTextbox2.Close()));
            }
            if (entities.Count > 0)
                yield return 0.3f;
            while ((miniTextbox1.ease += Engine.DeltaTime * 4f) < 1.0)
                yield return null;
            miniTextbox1.ease = 1f;
            if (miniTextbox1.portrait != null)
            {
                string beginAnim = "begin_" + miniTextbox1.portraitData.Animation;
                if (miniTextbox1.portrait.Has(beginAnim))
                {
                    miniTextbox1.portrait.Play(beginAnim);
                    while (miniTextbox1.portrait.CurrentAnimationID == beginAnim && miniTextbox1.portrait.Animating)
                        yield return null;
                }
                miniTextbox1.portrait.Play("talk_" + miniTextbox1.portraitData.Animation);
                miniTextbox1.talkerSfx = new SoundSource().Play(miniTextbox1.portraitData.SfxEvent);
                miniTextbox1.talkerSfx.Param("dialogue_portrait", miniTextbox1.portraitData.SfxExpression);
                miniTextbox1.talkerSfx.Param("dialogue_end", 0.0f);
                miniTextbox1.Add(miniTextbox1.talkerSfx);
                beginAnim = null;
            }
            float num = 0.0f;
            while (miniTextbox1.index < miniTextbox1.text.Nodes.Count)
            {
                if (miniTextbox1.text.Nodes[miniTextbox1.index] is FancyText.Char)
                    num += (miniTextbox1.text.Nodes[miniTextbox1.index] as FancyText.Char).Delay;
                ++miniTextbox1.index;
                if (num > 0.016000000759959221)
                {
                    yield return num;
                    num = 0.0f;
                }
            }
            if (miniTextbox1.portrait != null)
                miniTextbox1.portrait.Play("idle_" + miniTextbox1.portraitData.Animation);
            if (miniTextbox1.talkerSfx != null)
            {
                miniTextbox1.talkerSfx.Param("dialogue_portrait", 0.0f);
                miniTextbox1.talkerSfx.Param("dialogue_end", 1f);
            }
            Audio.EndSnapshot(Level.DialogSnapshot);
            yield return 3f;
            yield return miniTextbox1.Close();
        }

        private IEnumerator Close()
        {
            MiniTextbox miniTextbox = this;
            if (!miniTextbox.closing)
            {
                miniTextbox.closing = true;
                while ((miniTextbox.ease -= Engine.DeltaTime * 4f) > 0.0)
                    yield return null;
                miniTextbox.ease = 0.0f;
                miniTextbox.RemoveSelf();
            }
        }

        public override void Update()
        {
            if ((Scene as Level).RetryPlayerCorpse != null && !closing)
                routine.Replace(Close());
            base.Update();
        }

        public override void Render()
        {
            if (ease <= 0.0)
                return;
            Level scene = Scene as Level;
            if (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene)
                return;
            Vector2 position = new Vector2(Engine.Width / 2, (float) (72.0 + (Engine.Width - 1688.0) / 4.0));
            Vector2 vector2 = position + new Vector2(-828f, -56f);
            box.DrawCentered(position, Color.White, new Vector2(1f, ease));
            if (portrait != null)
            {
                portrait.Scale = new Vector2(1f, ease) * portraitScale;
                portrait.RenderPosition = vector2 + new Vector2(portraitSize / 2f, portraitSize / 2f);
                portrait.Render();
            }
            text.Draw(new Vector2((float) (vector2.X + (double) portraitSize + 32.0), position.Y), new Vector2(0.0f, 0.5f), new Vector2(1f, ease) * 0.75f, 1f, end: index);
        }

        public override void Removed(Scene scene)
        {
            Audio.EndSnapshot(Level.DialogSnapshot);
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Audio.EndSnapshot(Level.DialogSnapshot);
            base.SceneEnd(scene);
        }
    }
}
