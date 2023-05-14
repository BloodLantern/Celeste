// Decompiled with JetBrains decompiler
// Type: Celeste.MenuButton
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(true)]
    public abstract class MenuButton : Entity
    {
        public Vector2 TargetPosition;
        public Vector2 TweenFrom;
        public MenuButton LeftButton;
        public MenuButton RightButton;
        public MenuButton UpButton;
        public MenuButton DownButton;
        public Action OnConfirm;
        private bool canAcceptInput;
        private readonly Oui oui;
        private bool selected;
        private Tween tween;

        public static MenuButton GetSelection(Scene scene)
        {
            foreach (MenuButton entity in scene.Tracker.GetEntities<MenuButton>())
            {
                if (entity.Selected)
                {
                    return entity;
                }
            }
            return null;
        }

        public static void ClearSelection(Scene scene)
        {
            MenuButton selection = MenuButton.GetSelection(scene);
            if (selection == null)
            {
                return;
            }

            selection.Selected = false;
        }

        public MenuButton(Oui oui, Vector2 targetPosition, Vector2 tweenFrom, Action onConfirm)
            : base(tweenFrom)
        {
            TargetPosition = targetPosition;
            TweenFrom = tweenFrom;
            OnConfirm = onConfirm;
            this.oui = oui;
        }

        public override void Update()
        {
            base.Update();
            if (!canAcceptInput)
            {
                canAcceptInput = true;
            }
            else
            {
                if (!oui.Selected || !oui.Focused || !selected)
                {
                    return;
                }

                if (Input.MenuConfirm.Pressed)
                {
                    Confirm();
                }
                else if (Input.MenuLeft.Pressed && LeftButton != null)
                {
                    _ = Audio.Play("event:/ui/main/rollover_up");
                    LeftButton.Selected = true;
                }
                else if (Input.MenuRight.Pressed && RightButton != null)
                {
                    _ = Audio.Play("event:/ui/main/rollover_down");
                    RightButton.Selected = true;
                }
                else if (Input.MenuUp.Pressed && UpButton != null)
                {
                    _ = Audio.Play("event:/ui/main/rollover_up");
                    UpButton.Selected = true;
                }
                else
                {
                    if (!Input.MenuDown.Pressed || DownButton == null)
                    {
                        return;
                    }

                    _ = Audio.Play("event:/ui/main/rollover_down");
                    DownButton.Selected = true;
                }
            }
        }

        public void TweenIn(float time)
        {
            if (tween != null && tween.Entity == this)
            {
                tween.RemoveSelf();
            }

            Vector2 from = Position;
            Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, time, true));
            tween.OnUpdate = t => Position = Vector2.Lerp(from, TargetPosition, t.Eased);
        }

        public void TweenOut(float time)
        {
            if (tween != null && tween.Entity == this)
            {
                tween.RemoveSelf();
            }

            Vector2 from = Position;
            Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, time, true));
            tween.OnUpdate = t => Position = Vector2.Lerp(from, TweenFrom, t.Eased);
        }

        public Color SelectionColor => !selected
                    ? Color.White
                    : !Settings.Instance.DisableFlashes && !Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;

        public bool Selected
        {
            get => selected;
            set
            {
                if (Scene == null)
                {
                    throw new Exception("Cannot set Selected while MenuButton is not in a Scene.");
                }

                if (!selected & value)
                {
                    MenuButton selection = MenuButton.GetSelection(Scene);
                    if (selection != null)
                    {
                        selection.Selected = false;
                    }

                    selected = true;
                    canAcceptInput = false;
                    OnSelect();
                }
                else
                {
                    if (!selected || value)
                    {
                        return;
                    }

                    selected = false;
                    OnDeselect();
                }
            }
        }

        public virtual void OnSelect()
        {
        }

        public virtual void OnDeselect()
        {
        }

        public virtual void Confirm()
        {
            OnConfirm();
        }

        public virtual void StartSelected()
        {
            selected = true;
        }

        public abstract float ButtonHeight { get; }
    }
}
