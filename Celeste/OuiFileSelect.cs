// Decompiled with JetBrains decompiler
// Type: Celeste.OuiFileSelect
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class OuiFileSelect : Oui
    {
        public OuiFileSelectSlot[] Slots = new OuiFileSelectSlot[3];
        public int SlotIndex;
        public bool SlotSelected;
        public static bool Loaded;
        private bool loadedSuccess;
        public bool HasSlots;

        public OuiFileSelect()
        {
            OuiFileSelect.Loaded = false;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiFileSelect ouiFileSelect = this;
            ouiFileSelect.SlotSelected = false;
            if (!OuiFileSelect.Loaded)
            {
                for (int index = 0; index < ouiFileSelect.Slots.Length; ++index)
                {
                    if (ouiFileSelect.Slots[index] != null)
                    {
                        ouiFileSelect.Scene.Remove(ouiFileSelect.Slots[index]);
                    }
                }
                RunThread.Start(new Action(ouiFileSelect.LoadThread), "FILE_LOADING");
                float elapsed = 0.0f;
                while (!OuiFileSelect.Loaded || (double)elapsed < 0.5)
                {
                    elapsed += Engine.DeltaTime;
                    yield return null;
                }
                for (int index = 0; index < ouiFileSelect.Slots.Length; ++index)
                {
                    if (ouiFileSelect.Slots[index] != null)
                    {
                        ouiFileSelect.Scene.Add(ouiFileSelect.Slots[index]);
                    }
                }
                if (!ouiFileSelect.loadedSuccess)
                {
                    FileErrorOverlay error = new(FileErrorOverlay.Error.Load);
                    while (error.Open)
                    {
                        yield return null;
                    }

                    if (!error.Ignore)
                    {
                        _ = ouiFileSelect.Overworld.Goto<OuiMainMenu>();
                        yield break;
                    }
                    else
                    {
                        error = null;
                    }
                }
            }
            else if (from is not OuiFileNaming and not OuiAssistMode)
            {
                yield return 0.2f;
            }

            ouiFileSelect.HasSlots = false;
            for (int index = 0; index < ouiFileSelect.Slots.Length; ++index)
            {
                if (ouiFileSelect.Slots[index].Exists)
                {
                    ouiFileSelect.HasSlots = true;
                }
            }
            _ = Audio.Play("event:/ui/main/whoosh_savefile_in");
            if (from is OuiFileNaming or OuiAssistMode)
            {
                if (!ouiFileSelect.SlotSelected)
                {
                    ouiFileSelect.SelectSlot(false);
                }
            }
            else if (!ouiFileSelect.HasSlots)
            {
                ouiFileSelect.SlotIndex = 0;
                ouiFileSelect.Slots[ouiFileSelect.SlotIndex].Position = new Vector2(ouiFileSelect.Slots[ouiFileSelect.SlotIndex].HiddenPosition(1, 0).X, ouiFileSelect.Slots[ouiFileSelect.SlotIndex].SelectedPosition.Y);
                ouiFileSelect.SelectSlot(true);
            }
            else if (!ouiFileSelect.SlotSelected)
            {
                _ = Alarm.Set(ouiFileSelect, 0.4f, () => Audio.Play("event:/ui/main/savefile_rollover_first"));
                for (int i = 0; i < ouiFileSelect.Slots.Length; ++i)
                {
                    ouiFileSelect.Slots[i].Position = new Vector2(ouiFileSelect.Slots[i].HiddenPosition(1, 0).X, ouiFileSelect.Slots[i].IdlePosition.Y);
                    ouiFileSelect.Slots[i].Show();
                    yield return 0.02f;
                }
            }
        }

        private void LoadThread()
        {
            if (UserIO.Open(UserIO.Mode.Read))
            {
                for (int index = 0; index < Slots.Length; ++index)
                {
                    OuiFileSelectSlot ouiFileSelectSlot;
                    if (!UserIO.Exists(SaveData.GetFilename(index)))
                    {
                        ouiFileSelectSlot = new OuiFileSelectSlot(index, this, false);
                    }
                    else
                    {
                        SaveData data = UserIO.Load<SaveData>(SaveData.GetFilename(index));
                        if (data != null)
                        {
                            data.AfterInitialize();
                            ouiFileSelectSlot = new OuiFileSelectSlot(index, this, data);
                        }
                        else
                        {
                            ouiFileSelectSlot = new OuiFileSelectSlot(index, this, true);
                        }
                    }
                    Slots[index] = ouiFileSelectSlot;
                }
                UserIO.Close();
                loadedSuccess = true;
            }
            OuiFileSelect.Loaded = true;
        }

        public override IEnumerator Leave(Oui next)
        {
            _ = Audio.Play("event:/ui/main/whoosh_savefile_out");
            int slideTo = 1;
            if (next is null or OuiChapterSelect or OuiFileNaming or OuiAssistMode)
            {
                slideTo = -1;
            }

            for (int i = 0; i < Slots.Length; ++i)
            {
                switch (next)
                {
                    case OuiFileNaming _ when SlotIndex == i:
                        Slots[i].MoveTo(Slots[i].IdlePosition.X, Slots[0].IdlePosition.Y);
                        break;
                    case OuiAssistMode _ when SlotIndex == i:
                        Slots[i].MoveTo(Slots[i].IdlePosition.X, -400f);
                        break;
                    default:
                        Slots[i].Hide(slideTo, 0);
                        break;
                }
                yield return 0.02f;
            }
        }

        public void UnselectHighlighted()
        {
            SlotSelected = false;
            Slots[SlotIndex].Unselect();
            for (int index = 0; index < Slots.Length; ++index)
            {
                if (SlotIndex != index)
                {
                    Slots[index].Show();
                }
            }
        }

        public void SelectSlot(bool reset)
        {
            if (!Slots[SlotIndex].Exists & reset)
            {
                Slots[SlotIndex].Name = Settings.Instance == null || string.IsNullOrWhiteSpace(Settings.Instance.DefaultFileName) ? Dialog.Clean("FILE_DEFAULT") : Settings.Instance.DefaultFileName;
                Slots[SlotIndex].AssistModeEnabled = false;
                Slots[SlotIndex].VariantModeEnabled = false;
            }
            SlotSelected = true;
            Slots[SlotIndex].Select(reset);
            for (int index = 0; index < Slots.Length; ++index)
            {
                if (SlotIndex != index)
                {
                    Slots[index].Hide(0, index < SlotIndex ? -1 : 1);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Focused)
            {
                return;
            }

            if (!SlotSelected)
            {
                if (Input.MenuUp.Pressed && SlotIndex > 0)
                {
                    _ = Audio.Play("event:/ui/main/savefile_rollover_up");
                    --SlotIndex;
                }
                else if (Input.MenuDown.Pressed && SlotIndex < Slots.Length - 1)
                {
                    _ = Audio.Play("event:/ui/main/savefile_rollover_down");
                    ++SlotIndex;
                }
                else if (Input.MenuConfirm.Pressed)
                {
                    _ = Audio.Play("event:/ui/main/button_select");
                    _ = Audio.Play("event:/ui/main/whoosh_savefile_out");
                    SelectSlot(true);
                }
                else
                {
                    if (!Input.MenuCancel.Pressed)
                    {
                        return;
                    }

                    _ = Audio.Play("event:/ui/main/button_back");
                    _ = Overworld.Goto<OuiMainMenu>();
                }
            }
            else
            {
                if (!Input.MenuCancel.Pressed || HasSlots || Slots[SlotIndex].StartingGame)
                {
                    return;
                }

                _ = Audio.Play("event:/ui/main/button_back");
                _ = Overworld.Goto<OuiMainMenu>();
            }
        }
    }
}
