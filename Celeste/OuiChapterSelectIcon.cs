using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class OuiChapterSelectIcon : Entity
    {
        public const float IdleSize = 100f;
        public const float HoverSize = 144f;
        public const float HoverSpacing = 80f;
        public const float IdleY = 130f;
        public const float HoverY = 140f;
        public const float Spacing = 32f;
        public int Area;
        public bool New;
        public Vector2 Scale = Vector2.One;
        public float Rotation;
        public float sizeEase = 1f;
        public bool AssistModeUnlockable;
        public bool HideIcon;
        private Wiggler newWiggle;
        private bool hidden = true;
        private bool selected;
        private Tween tween;
        private Wiggler wiggler;
        private bool wiggleLeft;
        private int rotateDir = -1;
        private Vector2 shake;
        private float spotlightAlpha;
        private float spotlightRadius;
        private MTexture front;
        private MTexture back;

        public Vector2 IdlePosition
        {
            get
            {
                float x = (float) (960.0 + (Area - SaveData.Instance.LastArea.ID) * 132.0);
                if (Area < SaveData.Instance.LastArea.ID)
                    x -= 80f;
                else if (Area > SaveData.Instance.LastArea.ID)
                    x += 80f;
                float y = 130f;
                if (Area == SaveData.Instance.LastArea.ID)
                    y = 140f;
                return new Vector2(x, y);
            }
        }

        public Vector2 HiddenPosition => new Vector2(IdlePosition.X, -100f);

        public OuiChapterSelectIcon(int area, MTexture front, MTexture back)
        {
            Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
            Position = new Vector2(0.0f, -100f);
            Area = area;
            this.front = front;
            this.back = back;
            Add(wiggler = Wiggler.Create(0.35f, 2f, f =>
            {
                Rotation = (float) ((wiggleLeft ? -(double) f : f) * 0.40000000596046448);
                Scale = Vector2.One * (float) (1.0 + f * 0.5);
            }));
            Add(newWiggle = Wiggler.Create(0.8f, 2f));
            newWiggle.StartZero = true;
        }

        public void Hovered(int dir)
        {
            wiggleLeft = dir < 0;
            wiggler.Start();
        }

        public void Select()
        {
            Audio.Play("event:/ui/world_map/icon/flip_right");
            selected = true;
            hidden = false;
            Vector2 from = Position;
            StartTween(0.6f, t => SetSelectedPercent(from, t.Percent));
        }

        public void SnapToSelected()
        {
            selected = true;
            hidden = false;
            StopTween();
        }

        public void Unselect()
        {
            Audio.Play("event:/ui/world_map/icon/flip_left");
            hidden = false;
            selected = false;
            Vector2 to = IdlePosition;
            StartTween(0.6f, t => SetSelectedPercent(to, 1f - t.Percent));
        }

        public void Hide()
        {
            Scale = Vector2.One;
            hidden = true;
            selected = false;
            Vector2 from = Position;
            StartTween(0.25f, t => Position = Vector2.Lerp(from, HiddenPosition, tween.Eased));
        }

        public void Show()
        {
            if (SaveData.Instance != null)
                New = SaveData.Instance.Areas[Area].Modes[0].TimePlayed <= 0L;
            Scale = Vector2.One;
            hidden = false;
            selected = false;
            Vector2 from = Position;
            StartTween(0.25f, t => Position = Vector2.Lerp(from, IdlePosition, tween.Eased));
        }

        public void AssistModeUnlock(Action onComplete) => Add(new Coroutine(AssistModeUnlockRoutine(onComplete)));

        private IEnumerator AssistModeUnlockRoutine(Action onComplete)
        {
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 4f)
            {
                spotlightRadius = Ease.CubeOut(p) * 128f;
                spotlightAlpha = Ease.CubeOut(p) * 0.8f;
                yield return null;
            }
            shake.X = 6f;
            for (int i = 0; i < 10; ++i)
            {
                shake.X = -shake.X;
                yield return 0.01f;
            }
            shake = Vector2.Zero;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 4f)
            {
                shake = new Vector2(0.0f, -160f * Ease.CubeIn(p));
                Scale = new Vector2(1f - p, (float) (1.0 + p * 0.25));
                yield return null;
            }
            shake = Vector2.Zero;
            Scale = Vector2.One;
            AssistModeUnlockable = false;
            ++SaveData.Instance.UnlockedAreas;
            wiggler.Start();
            yield return 1f;
            for (p = 1f; p > 0.0; p -= Engine.DeltaTime * 4f)
            {
                spotlightRadius = (float) (128.0 + (1.0 - Ease.CubeOut(p)) * 128.0);
                spotlightAlpha = Ease.CubeOut(p) * 0.8f;
                yield return null;
            }
            spotlightAlpha = 0.0f;
            if (onComplete != null)
                onComplete();
        }

        public void HighlightUnlock(Action onComplete)
        {
            HideIcon = true;
            Add(new Coroutine(HighlightUnlockRoutine(onComplete)));
        }

        private IEnumerator HighlightUnlockRoutine(Action onComplete)
        {
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 2f)
            {
                spotlightRadius = (float) (128.0 + (1.0 - Ease.CubeOut(p)) * 128.0);
                spotlightAlpha = Ease.CubeOut(p) * 0.8f;
                yield return null;
            }
            Audio.Play("event:/ui/postgame/unlock_newchapter_icon");
            HideIcon = false;
            wiggler.Start();
            yield return 2f;
            for (p = 1f; p > 0.0; p -= Engine.DeltaTime * 2f)
            {
                spotlightRadius = (float) (128.0 + (1.0 - Ease.CubeOut(p)) * 128.0);
                spotlightAlpha = Ease.CubeOut(p) * 0.8f;
                yield return null;
            }
            spotlightAlpha = 0.0f;
            if (onComplete != null)
                onComplete();
        }

        private void StartTween(float duration, Action<Tween> callback)
        {
            StopTween();
            Add(tween = Tween.Create(Tween.TweenMode.Oneshot, duration: duration, start: true));
            tween.OnUpdate = callback;
            tween.OnComplete = t => tween = null;
        }

        private void StopTween()
        {
            if (tween != null)
                Remove(tween);
            tween = null;
        }

        private void SetSelectedPercent(Vector2 from, float p)
        {
            OuiChapterPanel ui = (Scene as Overworld).GetUI<OuiChapterPanel>();
            Vector2 end = ui.OpenPosition + ui.IconOffset;
            SimpleCurve simpleCurve = new SimpleCurve(from, end, (from + end) / 2f + new Vector2(0.0f, 30f));
            float num = (float) (1.0 + (p < 0.5 ? p * 2.0 : (1.0 - p) * 2.0));
            Scale.X = (float) Math.Cos(Ease.SineInOut(p) * 6.2831854820251465) * num;
            Scale.Y = num;
            Position = simpleCurve.GetPoint(Ease.Invert(Ease.CubeInOut)(p));
            Rotation = (float) (Ease.UpDown(Ease.SineInOut(p)) * (Math.PI / 180.0) * 15.0) * rotateDir;
            if (p <= 0.0)
            {
                rotateDir = -1;
            }
            else
            {
                if (p < 1.0)
                    return;
                rotateDir = 1;
            }
        }

        public override void Update()
        {
            if (SaveData.Instance == null)
                return;
            sizeEase = Calc.Approach(sizeEase, SaveData.Instance.LastArea.ID == Area ? 1f : 0.0f, Engine.DeltaTime * 4f);
            if (SaveData.Instance.LastArea.ID == Area)
                Depth = -50;
            else
                Depth = -45;
            if (tween == null)
            {
                if (selected)
                {
                    OuiChapterPanel ui = (Scene as Overworld).GetUI<OuiChapterPanel>();
                    Position = (!ui.EnteringChapter ? ui.OpenPosition : ui.Position) + ui.IconOffset;
                }
                else if (!hidden)
                    Position = Calc.Approach(Position, IdlePosition, 2400f * Engine.DeltaTime);
            }
            if (New && Scene.OnInterval(1.5f))
                newWiggle.Start();
            base.Update();
        }

        public override void Render()
        {
            MTexture mtexture = front;
            Vector2 scale1 = Scale;
            int width = mtexture.Width;
            if (scale1.X < 0.0)
                mtexture = back;
            if (AssistModeUnlockable)
            {
                mtexture = GFX.Gui["areas/lock"];
                width -= 32;
            }
            if (!HideIcon)
            {
                Vector2 scale2 = scale1 * ((float) (100.0 + 44.0 * Ease.CubeInOut(sizeEase)) / width);
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    scale2.X = -scale2.X;
                mtexture.DrawCentered(Position + shake, Color.White, scale2, Rotation);
                if (New && SaveData.Instance != null && !SaveData.Instance.CheatMode && Area == SaveData.Instance.UnlockedAreas && !selected && tween == null && !AssistModeUnlockable && Celeste.PlayMode != Celeste.PlayModes.Event)
                {
                    Vector2 position = Position + new Vector2(width * 0.25f, -mtexture.Height * 0.25f) + Vector2.UnitY * -Math.Abs(newWiggle.Value * 30f);
                    GFX.Gui["areas/new"].DrawCentered(position);
                }
            }
            if (spotlightAlpha > 0.0)
            {
                HiresRenderer.EndRender();
                SpotlightWipe.DrawSpotlight(new Vector2(Position.X, IdlePosition.Y), spotlightRadius, Color.Black * spotlightAlpha);
                HiresRenderer.BeginRender();
            }
            else
            {
                if (!AssistModeUnlockable || SaveData.Instance.LastArea.ID != Area || hidden)
                    return;
                ActiveFont.DrawOutline(Dialog.Clean("ASSIST_SKIP"), Position + new Vector2(0.0f, 100f), new Vector2(0.5f, 0.0f), Vector2.One * 0.7f, Color.White, 2f, Color.Black);
            }
        }
    }
}
