using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiJournalPoem : OuiJournalPage
    {
        private const float textScale = 0.5f;
        private const float holdingScaleAdd = 0.1f;
        private const float poemHeight = 44f;
        private const float poemSpacing = 4f;
        private const float poemStanzaSpacing = 16f;
        private List<PoemLine> lines = new List<PoemLine>();
        private int index;
        private float slider;
        private bool dragging;
        private bool swapping;
        private Coroutine swapRoutine = new Coroutine();
        private Wiggler wiggler = Wiggler.Create(0.4f, 4f);
        private Tween tween;

        public OuiJournalPoem(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            swapRoutine.RemoveOnComplete = false;
            float num = 0.0f;
            foreach (string id in SaveData.Instance.Poem)
            {
                string str = Dialog.Clean("poem_" + id).Replace("\n", " - ");
                lines.Add(new PoemLine
                {
                    Text = str,
                    Index = num,
                    Remix = AreaData.IsPoemRemix(id)
                });
                ++num;
            }
        }

        public static float GetY(float index) => (float) (120.0 + 44.0 * (index + 0.5) + 4.0 * index + (int) index / 4 * 16.0);

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            ActiveFont.Draw(Dialog.Clean("journal_poem"), new Vector2(60f, 60f), new Vector2(0.0f, 0.5f), Vector2.One, Color.Black * 0.6f);
            foreach (PoemLine line in lines)
                line.Render();
            if (lines.Count > 0)
                MTN.Journal[dragging ? "poemSlider" : "poemArrow"].DrawCentered(new Vector2(50f, OuiJournalPoem.GetY(slider)), Color.White, (float) (1.0 + 0.25 * wiggler.Value));
            Draw.SpriteBatch.End();
        }

        private IEnumerator Swap(int a, int b)
        {
            string str = SaveData.Instance.Poem[a];
            SaveData.Instance.Poem[a] = SaveData.Instance.Poem[b];
            SaveData.Instance.Poem[b] = str;
            PoemLine poemA = lines[a];
            PoemLine poemB = lines[b];
            PoemLine line = lines[a];
            lines[a] = lines[b];
            lines[b] = line;
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.125f, true);
            tween.OnUpdate = t =>
            {
                poemA.Index = MathHelper.Lerp(a, b, t.Eased);
                poemB.Index = MathHelper.Lerp(b, a, t.Eased);
            };
            tween.OnComplete = t => tween = null;
            yield return tween.Wait();
            swapping = false;
        }

        public override void Update()
        {
            if (lines.Count <= 0)
                return;
            if (tween != null && tween.Active)
                tween.Update();
            if (Input.MenuDown.Pressed && this.index + 1 < lines.Count && !swapping)
            {
                if (dragging)
                {
                    Audio.Play("event:/ui/world_map/journal/heart_shift_down");
                    swapRoutine.Replace(Swap(index, index + 1));
                    swapping = true;
                }
                else
                    Audio.Play("event:/ui/world_map/journal/heart_roll");
                ++index;
            }
            else if (Input.MenuUp.Pressed && this.index > 0 && !swapping)
            {
                if (dragging)
                {
                    Audio.Play("event:/ui/world_map/journal/heart_shift_up");
                    swapRoutine.Replace(Swap(index, index - 1));
                    swapping = true;
                }
                else
                    Audio.Play("event:/ui/world_map/journal/heart_roll");
                --index;
            }
            else if (Input.MenuConfirm.Pressed)
            {
                Journal.PageTurningLocked = true;
                Audio.Play("event:/ui/world_map/journal/heart_grab");
                dragging = true;
                wiggler.Start();
            }
            else if (!Input.MenuConfirm.Check && dragging)
            {
                Journal.PageTurningLocked = false;
                Audio.Play("event:/ui/world_map/journal/heart_release");
                dragging = false;
                wiggler.Start();
            }
            for (int index = 0; index < lines.Count; ++index)
            {
                PoemLine line = lines[index];
                line.HoveringEase = Calc.Approach(line.HoveringEase, this.index == index ? 1f : 0.0f, 8f * Engine.DeltaTime);
                line.HoldingEase = Calc.Approach(line.HoldingEase, this.index != index || !dragging ? 0.0f : 1f, 8f * Engine.DeltaTime);
            }
            slider = Calc.Approach(slider, this.index, 16f * Engine.DeltaTime);
            if (swapRoutine != null && swapRoutine.Active)
                swapRoutine.Update();
            wiggler.Update();
            Redraw(Journal.CurrentPageBuffer);
        }

        private class PoemLine
        {
            public float Index;
            public string Text;
            public float HoveringEase;
            public float HoldingEase;
            public bool Remix;

            public void Render()
            {
                float x = (float) (100.0 + Ease.CubeInOut(HoveringEase) * 20.0);
                float y = OuiJournalPoem.GetY(Index);
                Draw.Rect(x, y - 22f, 810f, 44f, Color.White * 0.25f);
                Vector2 scale1 = Vector2.One * (float) (0.60000002384185791 + HoldingEase * 0.40000000596046448);
                MTN.Journal[Remix ? "heartgem1" : "heartgem0"].DrawCentered(new Vector2(x + 20f, y), Color.White, scale1);
                Color color = Color.Black * (float) (0.699999988079071 + HoveringEase * 0.30000001192092896);
                Vector2 scale2 = Vector2.One * (float) (0.5 + HoldingEase * 0.10000000149011612);
                ActiveFont.Draw(Text, new Vector2(x + 60f, y), new Vector2(0.0f, 0.5f), scale2, color);
            }
        }
    }
}
