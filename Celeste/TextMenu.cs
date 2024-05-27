using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class TextMenu : Entity
    {
        public bool Focused = true;
        public InnerContentMode InnerContent;
        private List<Item> items = new List<Item>();
        public int Selection = -1;
        public Vector2 Justify;
        public float ItemSpacing = 4f;
        public float MinWidth;
        public float Alpha = 1f;
        public Color HighlightColor = Color.White;
        public static readonly Color HighlightColorA = Calc.HexToColor("84FF54");
        public static readonly Color HighlightColorB = Calc.HexToColor("FCFF59");
        public Action OnESC;
        public Action OnCancel;
        public Action OnUpdate;
        public Action OnPause;
        public Action OnClose;
        public bool AutoScroll = true;

        public Item Current
        {
            get => items.Count <= 0 || Selection < 0 ? null : items[Selection];
            set => Selection = items.IndexOf(value);
        }

        public new float Width { get; private set; }

        public new float Height { get; private set; }

        public float LeftColumnWidth { get; private set; }

        public float RightColumnWidth { get; private set; }

        public float ScrollableMinSize => Engine.Height - 300;

        public int FirstPossibleSelection
        {
            get
            {
                for (int index = 0; index < items.Count; ++index)
                {
                    if (items[index] != null && items[index].Hoverable)
                        return index;
                }
                return 0;
            }
        }

        public int LastPossibleSelection
        {
            get
            {
                for (int index = items.Count - 1; index >= 0; --index)
                {
                    if (items[index] != null && items[index].Hoverable)
                        return index;
                }
                return 0;
            }
        }

        public TextMenu()
        {
            Tag = (int) Tags.PauseUpdate | (int) Tags.HUD;
            Position = new Vector2(Engine.Width, Engine.Height) / 2f;
            Justify = new Vector2(0.5f, 0.5f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!AutoScroll)
                return;
            if (Height > (double) ScrollableMinSize)
                Position.Y = ScrollTargetY;
            else
                Position.Y = 540f;
        }

        public TextMenu Add(Item item)
        {
            items.Add(item);
            item.Container = this;
            Add(item.ValueWiggler = Wiggler.Create(0.25f, 3f));
            Add(item.SelectWiggler = Wiggler.Create(0.25f, 3f));
            item.ValueWiggler.UseRawDeltaTime = item.SelectWiggler.UseRawDeltaTime = true;
            if (Selection == -1)
                FirstSelection();
            RecalculateSize();
            item.Added();
            return this;
        }

        public void Clear() => items = new List<Item>();

        public int IndexOf(Item item) => items.IndexOf(item);

        public void FirstSelection()
        {
            Selection = -1;
            MoveSelection(1);
        }

        public void MoveSelection(int direction, bool wiggle = false)
        {
            int selection = Selection;
            direction = Math.Sign(direction);
            int num = 0;
            foreach (Item obj in items)
            {
                if (obj.Hoverable)
                    ++num;
            }
            bool flag = num > 2;
            do
            {
                Selection += direction;
                if (flag)
                {
                    if (Selection < 0)
                        Selection = items.Count - 1;
                    else if (Selection >= items.Count)
                        Selection = 0;
                }
                else if (Selection < 0 || Selection > items.Count - 1)
                {
                    Selection = Calc.Clamp(Selection, 0, items.Count - 1);
                    break;
                }
            }
            while (!Current.Hoverable);
            if (!Current.Hoverable)
                Selection = selection;
            if (Selection == selection || Current == null)
                return;
            if (selection >= 0 && items[selection] != null && items[selection].OnLeave != null)
                items[selection].OnLeave();
            if (Current.OnEnter != null)
                Current.OnEnter();
            if (!wiggle)
                return;
            Audio.Play(direction > 0 ? "event:/ui/main/rollover_down" : "event:/ui/main/rollover_up");
            Current.SelectWiggler.Start();
        }

        public void RecalculateSize()
        {
            LeftColumnWidth = RightColumnWidth = Height = 0.0f;
            foreach (Item obj in items)
            {
                if (obj.IncludeWidthInMeasurement)
                    LeftColumnWidth = Math.Max(LeftColumnWidth, obj.LeftWidth());
            }
            foreach (Item obj in items)
            {
                if (obj.IncludeWidthInMeasurement)
                    RightColumnWidth = Math.Max(RightColumnWidth, obj.RightWidth());
            }
            foreach (Item obj in items)
            {
                if (obj.Visible)
                    Height += obj.Height() + ItemSpacing;
            }
            Height -= ItemSpacing;
            Width = Math.Max(MinWidth, LeftColumnWidth + RightColumnWidth);
        }

        public float GetYOffsetOf(Item item)
        {
            if (item == null)
                return 0.0f;
            float num = 0.0f;
            foreach (Item obj in items)
            {
                if (item.Visible)
                    num += obj.Height() + ItemSpacing;
                if (obj == item)
                    break;
            }
            return num - item.Height() * 0.5f - ItemSpacing;
        }

        public void Close()
        {
            if (Current != null && Current.OnLeave != null)
                Current.OnLeave();
            if (OnClose != null)
                OnClose();
            RemoveSelf();
        }

        public void CloseAndRun(IEnumerator routine, Action onClose)
        {
            Focused = false;
            Visible = false;
            Add(new Coroutine(CloseAndRunRoutine(routine, onClose)));
        }

        private IEnumerator CloseAndRunRoutine(IEnumerator routine, Action onClose)
        {
            yield return routine;
            if (onClose != null)
                onClose();
            Close();
        }

        public override void Update()
        {
            base.Update();
            if (OnUpdate != null)
                OnUpdate();
            if (Focused)
            {
                if (Input.MenuDown.Pressed)
                {
                    if (!Input.MenuDown.Repeating || Selection != LastPossibleSelection)
                        MoveSelection(1, true);
                }
                else if (Input.MenuUp.Pressed && (!Input.MenuUp.Repeating || Selection != FirstPossibleSelection))
                    MoveSelection(-1, true);
                if (Current != null)
                {
                    if (Input.MenuLeft.Pressed)
                        Current.LeftPressed();
                    if (Input.MenuRight.Pressed)
                        Current.RightPressed();
                    if (Input.MenuConfirm.Pressed)
                    {
                        Current.ConfirmPressed();
                        if (Current.OnPressed != null)
                            Current.OnPressed();
                    }
                    if (Input.MenuJournal.Pressed && Current.OnAltPressed != null)
                        Current.OnAltPressed();
                }
                if (!Input.MenuConfirm.Pressed)
                {
                    if (Input.MenuCancel.Pressed && OnCancel != null)
                        OnCancel();
                    else if (Input.ESC.Pressed && OnESC != null)
                    {
                        Input.ESC.ConsumeBuffer();
                        OnESC();
                    }
                    else if (Input.Pause.Pressed && OnPause != null)
                    {
                        Input.Pause.ConsumeBuffer();
                        OnPause();
                    }
                }
            }
            foreach (Item obj in items)
            {
                if (obj.OnUpdate != null)
                    obj.OnUpdate();
                obj.Update();
            }
            if (Settings.Instance.DisableFlashes)
                HighlightColor = TextMenu.HighlightColorA;
            else if (Engine.Scene.OnRawInterval(0.1f))
                HighlightColor = !(HighlightColor == TextMenu.HighlightColorA) ? TextMenu.HighlightColorA : TextMenu.HighlightColorB;
            if (!AutoScroll)
                return;
            if (Height > (double) ScrollableMinSize)
                Position.Y += (float) ((ScrollTargetY - (double) Position.Y) * (1.0 - Math.Pow(0.0099999997764825821, Engine.RawDeltaTime)));
            else
                Position.Y = 540f;
        }

        public float ScrollTargetY
        {
            get
            {
                float min = Engine.Height - 150 - Height * Justify.Y;
                float max = (float) (150.0 + Height * (double) Justify.Y);
                return Calc.Clamp(Engine.Height / 2 + Height * Justify.Y - GetYOffsetOf(Current), min, max);
            }
        }

        public override void Render()
        {
            RecalculateSize();
            Vector2 vector2_1 = Position - Justify * new Vector2(Width, Height);
            Vector2 vector2_2 = vector2_1;
            bool flag = false;
            foreach (Item obj in items)
            {
                if (obj.Visible)
                {
                    float num = obj.Height();
                    if (!obj.AboveAll)
                        obj.Render(vector2_2 + new Vector2(0.0f, (float) (num * 0.5 + obj.SelectWiggler.Value * 8.0)), Focused && Current == obj);
                    else
                        flag = true;
                    vector2_2.Y += num + ItemSpacing;
                }
            }
            if (!flag)
                return;
            Vector2 vector2_3 = vector2_1;
            foreach (Item obj in items)
            {
                if (obj.Visible)
                {
                    float num = obj.Height();
                    if (obj.AboveAll)
                        obj.Render(vector2_3 + new Vector2(0.0f, (float) (num * 0.5 + obj.SelectWiggler.Value * 8.0)), Focused && Current == obj);
                    vector2_3.Y += num + ItemSpacing;
                }
            }
        }

        public enum InnerContentMode
        {
            OneColumn,
            TwoColumn,
        }

        public abstract class Item
        {
            public bool Selectable;
            public bool Visible = true;
            public bool Disabled;
            public bool IncludeWidthInMeasurement = true;
            public bool AboveAll;
            public TextMenu Container;
            public Wiggler SelectWiggler;
            public Wiggler ValueWiggler;
            public Action OnEnter;
            public Action OnLeave;
            public Action OnPressed;
            public Action OnAltPressed;
            public Action OnUpdate;

            public bool Hoverable => Selectable && Visible && !Disabled;

            public Item Enter(Action onEnter)
            {
                OnEnter = onEnter;
                return this;
            }

            public Item Leave(Action onLeave)
            {
                OnLeave = onLeave;
                return this;
            }

            public Item Pressed(Action onPressed)
            {
                OnPressed = onPressed;
                return this;
            }

            public Item AltPressed(Action onPressed)
            {
                OnAltPressed = onPressed;
                return this;
            }

            public float Width => LeftWidth() + RightWidth();

            public virtual void ConfirmPressed()
            {
            }

            public virtual void LeftPressed()
            {
            }

            public virtual void RightPressed()
            {
            }

            public virtual void Added()
            {
            }

            public virtual void Update()
            {
            }

            public virtual float LeftWidth() => 0.0f;

            public virtual float RightWidth() => 0.0f;

            public virtual float Height() => 0.0f;

            public virtual void Render(Vector2 position, bool highlighted)
            {
            }
        }

        public class Header : Item
        {
            public const float Scale = 2f;
            public string Title;

            public Header(string title)
            {
                Title = title;
                Selectable = false;
                IncludeWidthInMeasurement = false;
            }

            public override float LeftWidth() => ActiveFont.Measure(Title).X * 2f;

            public override float Height() => ActiveFont.LineHeight * 2f;

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                ActiveFont.DrawEdgeOutline(Title, position + new Vector2(Container.Width * 0.5f, 0.0f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Gray * alpha, 4f, Color.DarkSlateBlue * alpha, 2f, strokeColor);
            }
        }

        public class SubHeader : Item
        {
            public const float Scale = 0.6f;
            public string Title;
            public bool TopPadding = true;

            public SubHeader(string title, bool topPadding = true)
            {
                Title = title;
                Selectable = false;
                TopPadding = topPadding;
            }

            public override float LeftWidth() => ActiveFont.Measure(Title).X * 0.6f;

            public override float Height() => (float) ((Title.Length > 0 ? ActiveFont.LineHeight * 0.60000002384185791 : 0.0) + (TopPadding ? 48.0 : 0.0));

            public override void Render(Vector2 position, bool highlighted)
            {
                if (Title.Length <= 0)
                    return;
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                int y = TopPadding ? 32 : 0;
                ActiveFont.DrawOutline(Title, position + (Container.InnerContent == InnerContentMode.TwoColumn ? new Vector2(0.0f, y) : new Vector2(Container.Width * 0.5f, y)), new Vector2(Container.InnerContent == InnerContentMode.TwoColumn ? 0.0f : 0.5f, 0.5f), Vector2.One * 0.6f, Color.Gray * alpha, 2f, strokeColor);
            }
        }

        public class Option<T> : Item
        {
            public string Label;
            public int Index;
            public Action<T> OnValueChange;
            public int PreviousIndex;
            public List<Tuple<string, T>> Values = new List<Tuple<string, T>>();
            private float sine;
            private int lastDir;

            public Option(string label)
            {
                Label = label;
                Selectable = true;
            }

            public Option<T> Add(string label, T value, bool selected = false)
            {
                Values.Add(new Tuple<string, T>(label, value));
                if (selected)
                    PreviousIndex = Index = Values.Count - 1;
                return this;
            }

            public Option<T> Change(Action<T> action)
            {
                OnValueChange = action;
                return this;
            }

            public override void Added() => Container.InnerContent = InnerContentMode.TwoColumn;

            public override void LeftPressed()
            {
                if (Index <= 0)
                    return;
                Audio.Play("event:/ui/main/button_toggle_off");
                PreviousIndex = Index;
                --Index;
                lastDir = -1;
                ValueWiggler.Start();
                if (OnValueChange == null)
                    return;
                OnValueChange(Values[Index].Item2);
            }

            public override void RightPressed()
            {
                if (Index >= Values.Count - 1)
                    return;
                Audio.Play("event:/ui/main/button_toggle_on");
                PreviousIndex = Index;
                ++Index;
                lastDir = 1;
                ValueWiggler.Start();
                if (OnValueChange == null)
                    return;
                OnValueChange(Values[Index].Item2);
            }

            public override void ConfirmPressed()
            {
                if (Values.Count != 2)
                    return;
                if (Index == 0)
                    Audio.Play("event:/ui/main/button_toggle_on");
                else
                    Audio.Play("event:/ui/main/button_toggle_off");
                PreviousIndex = Index;
                Index = 1 - Index;
                lastDir = Index == 1 ? 1 : -1;
                ValueWiggler.Start();
                if (OnValueChange == null)
                    return;
                OnValueChange(Values[Index].Item2);
            }

            public override void Update() => sine += Engine.RawDeltaTime;

            public override float LeftWidth() => ActiveFont.Measure(Label).X + 32f;

            public override float RightWidth()
            {
                float val1 = 0.0f;
                foreach (Tuple<string, T> tuple in Values)
                    val1 = Math.Max(val1, ActiveFont.Measure(tuple.Item1).X);
                return val1 + 120f;
            }

            public override float Height() => ActiveFont.LineHeight;

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                Color color1 = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
                ActiveFont.DrawOutline(Label, position, new Vector2(0.0f, 0.5f), Vector2.One, color1, 2f, strokeColor);
                if (Values.Count <= 0)
                    return;
                float num = RightWidth();
                ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2((float) (Container.Width - num * 0.5 + lastDir * (double) ValueWiggler.Value * 8.0), 0.0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color1, 2f, strokeColor);
                Vector2 vector2 = Vector2.UnitX * (highlighted ? (float) (Math.Sin(sine * 4.0) * 4.0) : 0.0f);
                bool flag1 = Index > 0;
                Color color2 = flag1 ? color1 : Color.DarkSlateGray * alpha;
                ActiveFont.DrawOutline("<", position + new Vector2((float) (Container.Width - (double) num + 40.0 + (lastDir < 0 ? -(double) ValueWiggler.Value * 8.0 : 0.0)), 0.0f) - (flag1 ? vector2 : Vector2.Zero), new Vector2(0.5f, 0.5f), Vector2.One, color2, 2f, strokeColor);
                bool flag2 = Index < Values.Count - 1;
                Color color3 = flag2 ? color1 : Color.DarkSlateGray * alpha;
                ActiveFont.DrawOutline(">", position + new Vector2((float) (Container.Width - 40.0 + (lastDir > 0 ? ValueWiggler.Value * 8.0 : 0.0)), 0.0f) + (flag2 ? vector2 : Vector2.Zero), new Vector2(0.5f, 0.5f), Vector2.One, color3, 2f, strokeColor);
            }
        }

        public class Slider : Option<int>
        {
            public Slider(string label, Func<int, string> values, int min, int max, int value = -1)
                : base(label)
            {
                for (int index = min; index <= max; ++index)
                    Add(values(index), index, value == index);
            }
        }

        public class OnOff : Option<bool>
        {
            public OnOff(string label, bool on)
                : base(label)
            {
                Add(Dialog.Clean("options_off"), false, !on);
                Add(Dialog.Clean("options_on"), true, on);
            }
        }

        public class Setting : Item
        {
            public string ConfirmSfx = "event:/ui/main/button_select";
            public string Label;
            public List<object> Values = new List<object>();
            public Binding Binding;
            public bool BindingController;
            private int bindingHash;

            public Setting(string label, string value = "")
            {
                Label = label;
                Values.Add(value);
                Selectable = true;
            }

            public Setting(string label, Binding binding, bool controllerMode)
                : this(label)
            {
                Binding = binding;
                BindingController = controllerMode;
                bindingHash = 0;
            }

            public void Set(List<Keys> keys)
            {
                Values.Clear();
                int index1 = 0;
                for (int index2 = Math.Min(Input.MaxBindings, keys.Count); index1 < index2; ++index1)
                {
                    if (keys[index1] != Keys.None)
                    {
                        MTexture mtexture = Input.GuiKey(keys[index1], null);
                        if (mtexture != null)
                        {
                            Values.Add(mtexture);
                        }
                        else
                        {
                            string str1 = keys[index1].ToString();
                            string str2 = "";
                            for (int index3 = 0; index3 < str1.Length; ++index3)
                            {
                                if (index3 > 0 && char.IsUpper(str1[index3]))
                                    str2 += " ";
                                str2 += str1[index3].ToString();
                            }
                            Values.Add(str2);
                        }
                    }
                }
            }

            public void Set(List<Buttons> buttons)
            {
                Values.Clear();
                int index1 = 0;
                for (int index2 = Math.Min(Input.MaxBindings, buttons.Count); index1 < index2; ++index1)
                {
                    MTexture mtexture = Input.GuiSingleButton(buttons[index1], fallback: null);
                    if (mtexture != null)
                    {
                        Values.Add(mtexture);
                    }
                    else
                    {
                        string str1 = buttons[index1].ToString();
                        string str2 = "";
                        for (int index3 = 0; index3 < str1.Length; ++index3)
                        {
                            if (index3 > 0 && char.IsUpper(str1[index3]))
                                str2 += " ";
                            str2 += str1[index3].ToString();
                        }
                        Values.Add(str2);
                    }
                }
            }

            public override void Added() => Container.InnerContent = InnerContentMode.TwoColumn;

            public override void ConfirmPressed()
            {
                Audio.Play(ConfirmSfx);
                base.ConfirmPressed();
            }

            public override float LeftWidth() => ActiveFont.Measure(Label).X;

            public override float RightWidth()
            {
                float num = 0.0f;
                foreach (object text in Values)
                {
                    if (text is MTexture)
                        num += (text as MTexture).Width;
                    else if (text is string)
                        num += (float) (ActiveFont.Measure(text as string).X * 0.699999988079071 + 16.0);
                }
                return num;
            }

            public override float Height() => ActiveFont.LineHeight * 1.2f;

            public override void Update()
            {
                if (Binding == null)
                    return;
                int num = 17;
                if (BindingController)
                {
                    foreach (Buttons buttons in Binding.Controller)
                        num = num * 31 + buttons.GetHashCode();
                }
                else
                {
                    foreach (Keys keys in Binding.Keyboard)
                        num = num * 31 + keys.GetHashCode();
                }
                if (num == bindingHash)
                    return;
                bindingHash = num;
                if (BindingController)
                    Set(Binding.Controller);
                else
                    Set(Binding.Keyboard);
            }

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color strokeColor1 = Color.Black * (alpha * alpha * alpha);
                Color color1 = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
                ActiveFont.DrawOutline(Label, position, new Vector2(0.0f, 0.5f), Vector2.One, color1, 2f, strokeColor1);
                float num1 = RightWidth();
                foreach (object text1 in Values)
                {
                    if (text1 is MTexture)
                    {
                        MTexture mtexture = text1 as MTexture;
                        mtexture.DrawJustified(position + new Vector2(Container.Width - num1, 0.0f), new Vector2(0.0f, 0.5f), Color.White * alpha);
                        num1 -= mtexture.Width;
                    }
                    else if (text1 is string)
                    {
                        string text2 = text1 as string;
                        float num2 = (float) (ActiveFont.Measure(text1 as string).X * 0.699999988079071 + 16.0);
                        Vector2 position1 = position + new Vector2((float) (Container.Width - (double) num1 + num2 * 0.5), 0.0f);
                        Vector2 justify = new Vector2(0.5f, 0.5f);
                        Vector2 scale = Vector2.One * 0.7f;
                        Color color2 = Color.LightGray * alpha;
                        Color strokeColor2 = strokeColor1;
                        ActiveFont.DrawOutline(text2, position1, justify, scale, color2, 2f, strokeColor2);
                        num1 -= num2;
                    }
                }
            }
        }

        public class Button : Item
        {
            public string ConfirmSfx = "event:/ui/main/button_select";
            public string Label;
            public bool AlwaysCenter;

            public Button(string label)
            {
                Label = label;
                Selectable = true;
            }

            public override void ConfirmPressed()
            {
                if (!string.IsNullOrEmpty(ConfirmSfx))
                    Audio.Play(ConfirmSfx);
                base.ConfirmPressed();
            }

            public override float LeftWidth() => ActiveFont.Measure(Label).X;

            public override float Height() => ActiveFont.LineHeight;

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                bool flag = Container.InnerContent == InnerContentMode.TwoColumn && !AlwaysCenter;
                ActiveFont.DrawOutline(Label, position + (flag ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0.0f)), !flag || AlwaysCenter ? new Vector2(0.5f, 0.5f) : new Vector2(0.0f, 0.5f), Vector2.One, color, 2f, strokeColor);
            }
        }

        public class LanguageButton : Item
        {
            public string ConfirmSfx = "event:/ui/main/button_select";
            public string Label;
            public Language Language;
            public bool AlwaysCenter;

            public LanguageButton(string label, Language language)
            {
                Label = label;
                Language = language;
                Selectable = true;
            }

            public override void ConfirmPressed()
            {
                Audio.Play(ConfirmSfx);
                base.ConfirmPressed();
            }

            public override float LeftWidth() => ActiveFont.Measure(Label).X;

            public override float RightWidth() => Language.Icon.Width;

            public override float Height() => ActiveFont.LineHeight;

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                ActiveFont.DrawOutline(Label, position, new Vector2(0.0f, 0.5f), Vector2.One, color, 2f, strokeColor);
                Language.Icon.DrawJustified(position + new Vector2(Container.Width - RightWidth(), 0.0f), new Vector2(0.0f, 0.5f), Color.White, 1f);
            }
        }
    }
}
