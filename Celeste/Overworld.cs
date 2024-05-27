using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste
{
    public class Overworld : Scene, IOverlayHandler
    {
        public List<Oui> UIs = new List<Oui>();
        public Oui Current;
        public Oui Last;
        public Oui Next;
        public bool EnteringPico8;
        public bool ShowInputUI = true;
        public bool ShowConfirmUI = true;
        private float inputEase;
        public MountainRenderer Mountain;
        public HiresSnow Snow;
        private Snow3D Snow3D;
        public Maddy3D Maddy;
        private Entity routineEntity;
        private bool transitioning;
        private int lastArea = -1;

        public Overlay Overlay { get; set; }

        public Overworld(OverworldLoader loader)
        {
            Add(Mountain = new MountainRenderer());
            Add(new HudRenderer());
            Add(routineEntity = new Entity());
            Add(new InputEntity(this));
            Snow = loader.Snow;
            if (Snow == null)
                Snow = new HiresSnow();
            Add(Snow);
            RendererList.UpdateLists();
            Add(Snow3D = new Snow3D(Mountain.Model));
            Add(new MoonParticle3D(Mountain.Model, new Vector3(0.0f, 31f, 0.0f)));
            Add(Maddy = new Maddy3D(Mountain));
            ReloadMenus(loader.StartMode);
            Mountain.OnEaseEnd = () =>
            {
                if (Mountain.Area >= 0 && (!Maddy.Show || lastArea != Mountain.Area))
                {
                    Maddy.Running(Mountain.Area < 7);
                    Maddy.Wiggler.Start();
                }
                lastArea = Mountain.Area;
            };
            lastArea = Mountain.Area;
            if (Mountain.Area < 0)
                Maddy.Hide();
            else
                Maddy.Position = AreaData.Areas[Mountain.Area].MountainCursor;
            Settings.Instance.ApplyVolumes();
        }

        public override void Begin()
        {
            base.Begin();
            SetNormalMusic();
            ScreenWipe.WipeColor = Color.Black;
            FadeWipe fadeWipe = new FadeWipe(this, true);
            RendererList.UpdateLists();
            if (!EnteringPico8)
            {
                RendererList.MoveToFront(Snow);
                RendererList.UpdateLists();
            }
            EnteringPico8 = false;
            ReloadMountainStuff();
        }

        public override void End()
        {
            if (!EnteringPico8)
                Mountain.Dispose();
            base.End();
        }

        public void ReloadMenus(StartMode startMode = StartMode.Titlescreen)
        {
            foreach (Entity ui in UIs)
                Remove(ui);
            UIs.Clear();
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
            {
                if (typeof (Oui).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    Oui instance = (Oui) Activator.CreateInstance(type);
                    instance.Visible = false;
                    Add(instance);
                    UIs.Add(instance);
                    if (instance.IsStart(this, startMode))
                    {
                        instance.Visible = true;
                        Last = Current = instance;
                    }
                }
            }
        }

        public void SetNormalMusic()
        {
            Audio.SetMusic("event:/music/menu/level_select");
            Audio.SetAmbience("event:/env/amb/worldmap");
        }

        public void ReloadMountainStuff()
        {
            MTN.MountainBird.ReassignVertices();
            MTN.MountainMoon.ReassignVertices();
            MTN.MountainTerrain.ReassignVertices();
            MTN.MountainBuildings.ReassignVertices();
            MTN.MountainCoreWall.ReassignVertices();
            Mountain.Model.DisposeBillboardBuffers();
            Mountain.Model.ResetBillboardBuffers();
        }

        public override void HandleGraphicsReset()
        {
            ReloadMountainStuff();
            base.HandleGraphicsReset();
        }

        public override void Update()
        {
            if (Mountain.Area >= 0 && !Mountain.Animating)
            {
                Vector3 mountainCursor = AreaData.Areas[Mountain.Area].MountainCursor;
                if (mountainCursor != Vector3.Zero)
                    Maddy.Position = mountainCursor + new Vector3(0.0f, (float) Math.Sin(TimeActive * 2.0) * 0.02f, 0.0f);
            }
            if (Overlay != null)
            {
                if (Overlay.XboxOverlay)
                {
                    Mountain.Update(this);
                    Snow3D.Update();
                }
                Overlay.Update();
                Entities.UpdateLists();
                if (Snow != null)
                    Snow.Update(this);
            }
            else
            {
                if (!transitioning || !ShowInputUI)
                    inputEase = Calc.Approach(inputEase, !ShowInputUI || Input.GuiInputController() ? 0.0f : 1f, Engine.DeltaTime * 4f);
                base.Update();
            }
            if (SaveData.Instance != null && SaveData.Instance.LastArea.ID == 10 && 10 <= SaveData.Instance.UnlockedAreas && !IsCurrent<OuiMainMenu>())
                Audio.SetMusicParam("moon", 1f);
            else
                Audio.SetMusicParam("moon", 0.0f);
            float num = 1f;
            bool flag1 = false;
            foreach (Renderer renderer in RendererList.Renderers)
            {
                if (renderer is ScreenWipe)
                {
                    flag1 = true;
                    num = (renderer as ScreenWipe).Duration;
                }
            }
            bool flag2 = Current is OuiTitleScreen && Next == null || Next is OuiTitleScreen;
            if (Snow == null)
                return;
            Snow.ParticleAlpha = Calc.Approach(Snow.ParticleAlpha, flag2 | flag1 || Overlay != null && !Overlay.XboxOverlay ? 1f : 0.0f, Engine.DeltaTime / num);
        }

        public T Goto<T>() where T : Oui
        {
            T ui = GetUI<T>();
            if (ui != null)
                routineEntity.Add(new Coroutine(GotoRoutine(ui)));
            return ui;
        }

        public bool IsCurrent<T>() where T : Oui => Current != null ? Current is T : Last is T;

        public T GetUI<T>() where T : Oui
        {
            Oui ui1 = null;
            foreach (Oui ui2 in UIs)
            {
                if (ui2 is T)
                    ui1 = ui2;
            }
            return ui1 as T;
        }

        private IEnumerator GotoRoutine(Oui next)
        {
            while (Current == null)
                yield return null;
            transitioning = true;
            Next = next;
            Last = Current;
            Current = null;
            Last.Focused = false;
            yield return Last.Leave(next);
            if (next.Scene != null)
            {
                yield return next.Enter(Last);
                next.Focused = true;
                Current = next;
                transitioning = false;
            }
            Next = null;
        }

        public enum StartMode
        {
            Titlescreen,
            ReturnFromOptions,
            AreaComplete,
            AreaQuit,
            ReturnFromPico8,
            MainMenu,
        }

        private class InputEntity : Entity
        {
            public Overworld Overworld;
            private Wiggler confirmWiggle;
            private Wiggler cancelWiggle;
            private float confirmWiggleDelay;
            private float cancelWiggleDelay;

            public InputEntity(Overworld overworld)
            {
                Overworld = overworld;
                Tag = (int) Tags.HUD;
                Depth = -100000;
                Add(confirmWiggle = Wiggler.Create(0.4f, 4f));
                Add(cancelWiggle = Wiggler.Create(0.4f, 4f));
            }

            public override void Update()
            {
                if (Input.MenuConfirm.Pressed && confirmWiggleDelay <= 0.0)
                {
                    confirmWiggle.Start();
                    confirmWiggleDelay = 0.5f;
                }
                if (Input.MenuCancel.Pressed && cancelWiggleDelay <= 0.0)
                {
                    cancelWiggle.Start();
                    cancelWiggleDelay = 0.5f;
                }
                confirmWiggleDelay -= Engine.DeltaTime;
                cancelWiggleDelay -= Engine.DeltaTime;
                base.Update();
            }

            public override void Render()
            {
                float inputEase = Overworld.inputEase;
                if (inputEase <= 0.0)
                    return;
                float scale = 0.5f;
                int num1 = 32;
                string label1 = Dialog.Clean("ui_cancel");
                string label2 = Dialog.Clean("ui_confirm");
                float num2 = ButtonUI.Width(label1, Input.MenuCancel);
                float num3 = ButtonUI.Width(label2, Input.MenuConfirm);
                Vector2 position = new Vector2(1880f, 1024f);
                position.X += (float) ((40.0 + (num3 + (double) num2) * scale + num1) * (1.0 - Ease.CubeOut(inputEase)));
                ButtonUI.Render(position, label1, Input.MenuCancel, scale, 1f, cancelWiggle.Value * 0.05f);
                if (!Overworld.ShowConfirmUI)
                    return;
                position.X -= scale * num2 + num1;
                ButtonUI.Render(position, label2, Input.MenuConfirm, scale, 1f, confirmWiggle.Value * 0.05f);
            }
        }
    }
}
