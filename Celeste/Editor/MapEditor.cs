// Decompiled with JetBrains decompiler
// Type: Celeste.Editor.MapEditor
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Editor
{
    public class MapEditor : Scene
    {
        private static readonly Color gridColor = new Color(0.1f, 0.1f, 0.1f);
        private static Camera Camera;
        private static AreaKey area = AreaKey.None;
        private static float saveFlash = 0.0f;
        private MapData mapData;
        private List<LevelTemplate> levels = new List<LevelTemplate>();
        private Vector2 mousePosition;
        private MouseModes mouseMode;
        private Vector2 lastMouseScreenPosition;
        private Vector2 mouseDragStart;
        private HashSet<LevelTemplate> selection = new HashSet<LevelTemplate>();
        private HashSet<LevelTemplate> hovered = new HashSet<LevelTemplate>();
        private float fade;
        private List<Vector2[]> undoStack = new List<Vector2[]>();
        private List<Vector2[]> redoStack = new List<Vector2[]>();

        public MapEditor(AreaKey area, bool reloadMapData = true)
        {
            area.ID = Calc.Clamp(area.ID, 0, AreaData.Areas.Count - 1);
            mapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            if (reloadMapData)
                mapData.Reload();
            foreach (LevelData level in mapData.Levels)
                levels.Add(new LevelTemplate(level));
            foreach (Rectangle rectangle in mapData.Filler)
                levels.Add(new LevelTemplate(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
            if (area != area)
            {
                Camera = new Camera();
                Camera.Zoom = 6f;
                Camera.CenterOrigin();
            }
            if (SaveData.Instance != null)
                return;
            SaveData.InitializeDebugMode();
        }

        public override void GainFocus()
        {
            base.GainFocus();
            SaveAndReload();
        }

        private void SelectAll()
        {
            selection.Clear();
            foreach (LevelTemplate level in levels)
                selection.Add(level);
        }

        public void Rename(string oldName, string newName)
        {
            LevelTemplate levelTemplate1 = (LevelTemplate)null;
            LevelTemplate levelTemplate2 = (LevelTemplate)null;
            foreach (LevelTemplate level in levels)
            {
                if (levelTemplate1 == null && level.Name == oldName)
                {
                    levelTemplate1 = level;
                    if (levelTemplate2 != null)
                        break;
                }
                else if (levelTemplate2 == null && level.Name == newName)
                {
                    levelTemplate2 = level;
                    if (levelTemplate1 != null)
                        break;
                }
            }
            string path1 = Path.Combine("..", "..", "..", "Content", "Levels", mapData.Filename);
            if (levelTemplate2 == null)
            {
                File.Move(Path.Combine(path1, levelTemplate1.Name + ".xml"), Path.Combine(path1, newName + ".xml"));
                levelTemplate1.Name = newName;
            }
            else
            {
                string str = Path.Combine(path1, "TEMP.xml");
                File.Move(Path.Combine(path1, levelTemplate1.Name + ".xml"), str);
                File.Move(Path.Combine(path1, levelTemplate2.Name + ".xml"), Path.Combine(path1, oldName + ".xml"));
                File.Move(str, Path.Combine(path1, newName + ".xml"));
                levelTemplate1.Name = newName;
                levelTemplate2.Name = oldName;
            }
            Save();
        }

        private void Save() { }

        private void SaveAndReload() { }

        private void UpdateMouse() => mousePosition = Vector2.Transform(MInput.Mouse.Position, Matrix.Invert(Camera.Matrix));

        public override void Update()
        {
            Vector2 vector2;
            vector2.X = (lastMouseScreenPosition.X - MInput.Mouse.Position.X) / Camera.Zoom;
            vector2.Y = (lastMouseScreenPosition.Y - MInput.Mouse.Position.Y) / Camera.Zoom;
            if (MInput.Keyboard.Pressed(Keys.Space) && MInput.Keyboard.Check(Keys.LeftControl))
            {
                Camera.Zoom = 6f;
                Camera.Position = Vector2.Zero;
            }
            int num = Math.Sign(MInput.Mouse.WheelDelta);
            if (num > 0 && (double)Camera.Zoom >= 1.0 || (double)Camera.Zoom > 1.0)
                Camera.Zoom += (float)num;
            else
                Camera.Zoom += (float)num * 0.25f;
            Camera.Zoom = Math.Max(0.25f, Math.Min(24f, Camera.Zoom));
            Camera.Position += new Vector2((float)Input.MoveX.Value, (float)Input.MoveY.Value) * 300f* Engine.DeltaTime;
            UpdateMouse();
            hovered.Clear();
            if (mouseMode == MouseModes.Hover)
            {
                mouseDragStart = mousePosition;
                if (MInput.Mouse.PressedLeftButton)
                {
                    bool flag1 = LevelCheck(mousePosition);
                    if (MInput.Keyboard.Check(Keys.Space))
                        mouseMode = MouseModes.Pan;
                    else if (MInput.Keyboard.Check(Keys.LeftControl))
                    {
                        if (flag1)
                            ToggleSelection(mousePosition);
                        else
                            mouseMode = MouseModes.Select;
                    }
                    else if (MInput.Keyboard.Check(Keys.F))
                        levels.Add(new LevelTemplate((int)mousePosition.X, (int)mousePosition.Y, 32, 32));
                    else if (flag1)
                    {
                        if (!SelectionCheck(mousePosition))
                            SetSelection(mousePosition);
                        bool flag2 = false;
                        if (selection.Count == 1)
                        {
                            foreach (LevelTemplate levelTemplate in selection)
                            {
                                if (levelTemplate.ResizePosition(mousePosition) && levelTemplate.Type == LevelTemplateType.Filler)
                                    flag2 = true;
                            }
                        }
                        if (flag2)
                        {
                            foreach (LevelTemplate levelTemplate in selection)
                                levelTemplate.StartResizing();
                            mouseMode = MouseModes.Resize;
                        }
                        else
                        {
                            StoreUndo();
                            foreach (LevelTemplate levelTemplate in selection)
                                levelTemplate.StartMoving();
                            mouseMode = MouseModes.Move;
                        }
                    }
                    else
                        mouseMode = MouseModes.Select;
                }
                else if (MInput.Mouse.PressedRightButton)
                {
                    LevelTemplate level = TestCheck(mousePosition);
                    if (level != null)
                    {
                        if (level.Type == LevelTemplateType.Filler)
                        {
                            if (!MInput.Keyboard.Check(Keys.F))
                                return;
                            levels.Remove(level);
                            return;
                        }
                        LoadLevel(level, mousePosition * 8f);
                        return;
                    }
                }
                else if (MInput.Mouse.PressedMiddleButton)
                    mouseMode = MouseModes.Pan;
                else if (!MInput.Keyboard.Check(Keys.Space))
                {
                    foreach (LevelTemplate level in levels)
                    {
                        if (level.Check(mousePosition))
                            hovered.Add(level);
                    }
                    if (MInput.Keyboard.Check(Keys.LeftControl))
                    {
                        if (MInput.Keyboard.Pressed(Keys.Z))
                            Undo();
                        else if (MInput.Keyboard.Pressed(Keys.Y))
                            Redo();
                        else if (MInput.Keyboard.Pressed(Keys.A))
                            SelectAll();
                    }
                }
            }
            else if (mouseMode == MouseModes.Pan)
            {
                Camera.Position += vector2;
                if (!MInput.Mouse.CheckLeftButton && !MInput.Mouse.CheckMiddleButton)
                    mouseMode = MouseModes.Hover;
            }
            else if (mouseMode == MouseModes.Select)
            {
                Rectangle mouseRect = GetMouseRect(mouseDragStart, mousePosition);
                foreach (LevelTemplate level in levels)
                {
                    if (level.Check(mouseRect))
                        hovered.Add(level);
                }
                if (!MInput.Mouse.CheckLeftButton)
                {
                    if (MInput.Keyboard.Check(Keys.LeftControl))
                        ToggleSelection(mouseRect);
                    else
                        SetSelection(mouseRect);
                    mouseMode = MouseModes.Hover;
                }
            }
            else if (mouseMode == MouseModes.Move)
            {
                Vector2 relativeMove = (mousePosition - mouseDragStart).Round();
                bool snap = selection.Count == 1 && !MInput.Keyboard.Check(Keys.LeftAlt);
                foreach (LevelTemplate levelTemplate in selection)
                    levelTemplate.Move(relativeMove, levels, snap);
                if (!MInput.Mouse.CheckLeftButton)
                    mouseMode = MouseModes.Hover;
            }
            else if (mouseMode == MouseModes.Resize)
            {
                Vector2 relativeMove = (mousePosition - mouseDragStart).Round();
                foreach (LevelTemplate levelTemplate in selection)
                    levelTemplate.Resize(relativeMove);
                if (!MInput.Mouse.CheckLeftButton)
                    mouseMode = MouseModes.Hover;
            }
            if (MInput.Keyboard.Pressed(Keys.D1))
                SetEditorColor(0);
            else if (MInput.Keyboard.Pressed(Keys.D2))
                SetEditorColor(1);
            else if (MInput.Keyboard.Pressed(Keys.D3))
                SetEditorColor(2);
            else if (MInput.Keyboard.Pressed(Keys.D4))
                SetEditorColor(3);
            else if (MInput.Keyboard.Pressed(Keys.D5))
                SetEditorColor(4);
            else if (MInput.Keyboard.Pressed(Keys.D6))
                SetEditorColor(5);
            else if (MInput.Keyboard.Pressed(Keys.D7))
                SetEditorColor(6);
            if (MInput.Keyboard.Pressed(Keys.F1) || MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S))
            {
                SaveAndReload();
            }
            else
            {
                if ((double)saveFlash > 0.0)
                    saveFlash -= Engine.DeltaTime * 4f;
                lastMouseScreenPosition = MInput.Mouse.Position;
                base.Update();
            }
        }

        private void SetEditorColor(int index)
        {
            foreach (LevelTemplate levelTemplate in selection)
                levelTemplate.EditorColorIndex = index;
        }

        public override void Render()
        {
            UpdateMouse();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Camera.Matrix * Engine.ScreenMatrix);
            float width = 1920f / Camera.Zoom;
            float height = 1080f / Camera.Zoom;
            int num1 = 5;
            float num2 = (float)Math.Floor((double)Camera.Left / (double)num1 - 1.0) * (float)num1;
            float num3 = (float)Math.Floor((double)Camera.Top / (double)num1 - 1.0) * (float)num1;
            for (float num4 = num2;
                (double)num4 <= (double)num2 + (double)width + 10.0; num4 += 5f)
                Draw.Line(num4, Camera.Top, num4, Camera.Top + height, gridColor);
            for (float num5 = num3;
                (double)num5 <= (double)num3 + (double)height + 10.0; num5 += 5f)
                Draw.Line(Camera.Left, num5, Camera.Left + width, num5, gridColor);
            Draw.Line(0.0f, Camera.Top, 0.0f, Camera.Top + height, Color.DarkSlateBlue, 1f / Camera.Zoom);
            Draw.Line(Camera.Left, 0.0f, Camera.Left + width, 0.0f, Color.DarkSlateBlue, 1f / Camera.Zoom);
            foreach (LevelTemplate level in levels)
                level.RenderContents(Camera, levels);
            foreach (LevelTemplate level in levels)
                level.RenderOutline(Camera);
            foreach (LevelTemplate level in levels)
                level.RenderHighlight(Camera, selection.Contains(level), hovered.Contains(level));
            if (mouseMode == MouseModes.Hover)
            {
                Draw.Line(mousePosition.X - 12f / Camera.Zoom, mousePosition.Y, mousePosition.X + 12f / Camera.Zoom, mousePosition.Y, Color.Yellow, 3f / Camera.Zoom);
                Draw.Line(mousePosition.X, mousePosition.Y - 12f / Camera.Zoom, mousePosition.X, mousePosition.Y + 12f / Camera.Zoom, Color.Yellow, 3f / Camera.Zoom);
            }
            else if (mouseMode == MouseModes.Select)
                Draw.Rect(GetMouseRect(mouseDragStart, mousePosition), Color.Lime * 0.25f);
            if ((double)saveFlash > 0.0)
                Draw.Rect(Camera.Left, Camera.Top, width, height, Color.White * Ease.CubeInOut(saveFlash));
            if ((double)fade > 0.0)
                Draw.Rect(0.0f, 0.0f, 320f, 180f, Color.Black * fade);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect)null, Engine.ScreenMatrix);
            Draw.Rect(0.0f, 0.0f, 1920f, 72f, Color.Black);
            Vector2 position1 = new Vector2(16f, 4f);
            Vector2 position2 = new Vector2(1904f, 4f);
            if (MInput.Keyboard.Check(Keys.Q))
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.25f);
                using (List<LevelTemplate>.Enumerator enumerator = levels.GetEnumerator())
                {
                label_35: while (enumerator.MoveNext())
                    {
                        LevelTemplate current = enumerator.Current;
                        int index = 0;
                        while (true)
                        {
                            if (current.Strawberries != null && index < current.Strawberries.Count)
                            {
                                Vector2 strawberry = current.Strawberries[index];
                                ActiveFont.DrawOutline(current.StrawberryMetadata[index], (new Vector2((float)current.X + strawberry.X, (float)current.Y + strawberry.Y) - Camera.Position) * Camera.Zoom + new Vector2(960f, 532f), new Vector2(0.5f, 1f), Vector2.One * 1f, Color.Red, 2f, Color.Black);
                                ++index;
                            }
                            else
                                goto label_35;
                        }
                    }
                }
            }
            if (hovered.Count == 0)
            {
                if (selection.Count > 0)
                {
                    ActiveFont.Draw(selection.Count.ToString() + " levels selected", position1, Color.Red);
                }
                else
                {
                    ActiveFont.Draw(Dialog.Clean(mapData.Data.Name), position1, Color.Aqua);
                    ActiveFont.Draw(mapData.Area.Mode.ToString() + " MODE", position2, Vector2.UnitX, Vector2.One, Color.Red);
                }
            }
            else if (hovered.Count == 1)
            {
                LevelTemplate levelTemplate = (LevelTemplate)null;
                using (HashSet<LevelTemplate>.Enumerator enumerator = hovered.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        levelTemplate = enumerator.Current;
                }
                string text = levelTemplate.ActualWidth.ToString() + "x" + levelTemplate.ActualHeight.ToString() + "   " + (object)levelTemplate.X + "," + (object)levelTemplate.Y + "   " + (object)(levelTemplate.X * 8) + "," + (object)(levelTemplate.Y * 8);
                ActiveFont.Draw(levelTemplate.Name, position1, Color.Yellow);
                Vector2 position3 = position2;
                Vector2 unitX = Vector2.UnitX;
                Vector2 one = Vector2.One;
                Color green = Color.Green;
                ActiveFont.Draw(text, position3, unitX, one, green);
            }
            else
                ActiveFont.Draw(hovered.Count.ToString() + " levels", position1, Color.Yellow);
            Draw.SpriteBatch.End();
        }

        private void LoadLevel(LevelTemplate level, Vector2 at)
        {
            Save();
            Engine.Scene = (Scene)new LevelLoader(new Session(area)
            {
                FirstLevel = false,
                Level = level.Name,
                StartedFromBeginning = false
            }, new Vector2?(at));
        }

        private void StoreUndo()
        {
            Vector2[] vector2Array = new Vector2[levels.Count];
            for (int index = 0; index < levels.Count; ++index)
                vector2Array[index] = new Vector2((float)levels[index].X, (float)levels[index].Y);
            undoStack.Add(vector2Array);
            while (undoStack.Count > 30)
                undoStack.RemoveAt(0);
            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count <= 0)
                return;
            Vector2[] vector2Array = new Vector2[levels.Count];
            for (int index = 0; index < levels.Count; ++index)
                vector2Array[index] = new Vector2((float)levels[index].X, (float)levels[index].Y);
            redoStack.Add(vector2Array);
            Vector2[] undo = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            for (int index = 0; index < undo.Length; ++index)
            {
                levels[index].X = (int)undo[index].X;
                levels[index].Y = (int)undo[index].Y;
            }
        }

        private void Redo()
        {
            if (redoStack.Count <= 0)
                return;
            Vector2[] vector2Array = new Vector2[levels.Count];
            for (int index = 0; index < levels.Count; ++index)
                vector2Array[index] = new Vector2((float)levels[index].X, (float)levels[index].Y);
            undoStack.Add(vector2Array);
            Vector2[] redo = redoStack[undoStack.Count - 1];
            redoStack.RemoveAt(undoStack.Count - 1);
            for (int index = 0; index < redo.Length; ++index)
            {
                levels[index].X = (int)redo[index].X;
                levels[index].Y = (int)redo[index].Y;
            }
        }

        private Rectangle GetMouseRect(Vector2 a, Vector2 b)
        {
            Vector2 vector2_1 = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            Vector2 vector2_2 = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            return new Rectangle((int)vector2_1.X, (int)vector2_1.Y, (int)((double)vector2_2.X - (double)vector2_1.X), (int)((double)vector2_2.Y - (double)vector2_1.Y));
        }

        private LevelTemplate TestCheck(Vector2 point)
        {
            foreach (LevelTemplate level in levels)
            {
                if (!level.Dummy && level.Check(point))
                    return level;
            }
            return (LevelTemplate)null;
        }

        private bool LevelCheck(Vector2 point)
        {
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(point))
                    return true;
            }
            return false;
        }

        private bool SelectionCheck(Vector2 point)
        {
            foreach (LevelTemplate levelTemplate in selection)
            {
                if (levelTemplate.Check(point))
                    return true;
            }
            return false;
        }

        private bool SetSelection(Vector2 point)
        {
            selection.Clear();
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(point))
                    selection.Add(level);
            }
            return selection.Count > 0;
        }

        private bool ToggleSelection(Vector2 point)
        {
            bool flag = false;
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(point))
                {
                    flag = true;
                    if (selection.Contains(level))
                        selection.Remove(level);
                    else
                        selection.Add(level);
                }
            }
            return flag;
        }

        private void SetSelection(Rectangle rect)
        {
            selection.Clear();
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(rect))
                    selection.Add(level);
            }
        }

        private void ToggleSelection(Rectangle rect)
        {
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(rect))
                {
                    if (selection.Contains(level))
                        selection.Remove(level);
                    else
                        selection.Add(level);
                }
            }
        }

        private enum MouseModes
        {
            Hover,
            Pan,
            Select,
            Move,
            Resize,
        }
    }
}