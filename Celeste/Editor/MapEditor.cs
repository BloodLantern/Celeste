using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Editor
{
    /// <summary>
    /// Debug map editor integrated into the game. Commonly called 'Debug mode' in the community.
    /// </summary>
    public class MapEditor : Scene
    {
        /// <summary>
        /// The color of the background grid.
        /// </summary>
        private static readonly Color gridColor = new(0.1f, 0.1f, 0.1f);
        /// <summary>
        /// The current camera.
        /// </summary>
        private static Camera Camera;
        /// <summary>
        /// The current area.
        /// </summary>
        private static AreaKey area = AreaKey.None;
        /// <summary>
        /// The save flash duration. For some reason this is 0.
        /// </summary>
        private static float saveFlash = 0f;

        /// <summary>
        /// The map data of the current map.
        /// </summary>
        private readonly MapData mapData;
        /// <summary>
        /// The level list.
        /// </summary>
        private readonly List<LevelTemplate> levels = new();

        /// <summary>
        /// The current mouse position.
        /// </summary>
        private Vector2 mousePosition;
        /// <summary>
        /// The current mouse mode. It is one of Hover, Pan, Select, Move or Resize.
        /// </summary>
        private MouseModes mouseMode;
        /// <summary>
        /// Last frame mouse position.
        /// </summary>
        private Vector2 lastMouseScreenPosition;
        /// <summary>
        /// The start position of the current drag action.
        /// </summary>
        private Vector2 mouseDragStart;

        /// <summary>
        /// The currently selected levels.
        /// </summary>
        private readonly HashSet<LevelTemplate> selection = new();
        /// <summary>
        /// The currently hovered levels.
        /// </summary>
        private readonly HashSet<LevelTemplate> hovered = new();

        private readonly float fade;

        /// <summary>
        /// Stack of undo actions.
        /// </summary>
        private readonly List<Vector2[]> undoStack = new();
        /// <summary>
        /// Stack of redo actions.
        /// </summary>
        private readonly List<Vector2[]> redoStack = new();

        /// <summary>
        /// Constructs a map editor for an area.
        /// </summary>
        /// <param name="area">The area to edit.</param>
        /// <param name="reloadMapData">Whether to reload the map data when constructing the editor.</param>
        public MapEditor(AreaKey area, bool reloadMapData = true)
        {
            // Make sure the area exists by clamping it
            area.ID = Calc.Clamp(area.ID, 0, AreaData.Areas.Count - 1);
            mapData = AreaData.Areas[area.ID].Mode[(int) area.Mode].MapData;

            // Why do they reload the map data ? It creates a lag spike for huge maps like Farewell.
            if (reloadMapData)
                mapData.Reload();

            foreach (LevelData level in mapData.Levels)
                levels.Add(new LevelTemplate(level));
            foreach (Rectangle rectangle in mapData.Filler)
                levels.Add(new LevelTemplate(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));

            if (MapEditor.area != area)
            {
                MapEditor.area = area;
                Camera = new Camera
                {
                    Zoom = 6f
                };
                Camera.CenterOrigin();
            }

            if (SaveData.Instance != null)
                return;

            SaveData.InitializeDebugMode();
        }

        /// <summary>
        /// Called when gaining focus.
        /// </summary>
        public override void GainFocus()
        {
            base.GainFocus();
            SaveAndReload();
        }

        /// <summary>
        /// Used to select all the levels of the current map.
        /// </summary>
        private void SelectAll()
        {
            selection.Clear();
            foreach (LevelTemplate level in levels)
                selection.Add(level);
        }

        /// <summary>
        /// Renames the level with the old name with the new one.
        /// </summary>
        /// <param name="oldName">The level to rename.</param>
        /// <param name="newName">The new name for the level.</param>
        public void Rename(string oldName, string newName)
        {
            LevelTemplate levelToRename = null, existingLevelWithNewName = null;

            foreach (LevelTemplate level in levels)
            {
                if (levelToRename == null && level.Name == oldName)
                {
                    levelToRename = level;

                    // Break the loop early if the two levels were already found
                    if (existingLevelWithNewName != null)
                        break;
                }
                else if (existingLevelWithNewName == null && level.Name == newName)
                {
                    existingLevelWithNewName = level;

                    // Break the loop early if the two levels were already found
                    if (levelToRename != null)
                        break;
                }
            }

            string levelsDirectory = Path.Combine("..", "..", "..", "Content", "Levels", mapData.Filename);
            if (existingLevelWithNewName == null)
            {
                // If no level is named with the new name, simply rename the file
                File.Move(Path.Combine(levelsDirectory, levelToRename.Name + ".xml"), Path.Combine(levelsDirectory, newName + ".xml"));
                levelToRename.Name = newName;
            }
            else
            {
                // If a level already exists with the new name, swap the file names
                string tempFile = Path.Combine(levelsDirectory, "TEMP.xml");
                File.Move(Path.Combine(levelsDirectory, levelToRename.Name + ".xml"), tempFile);
                File.Move(Path.Combine(levelsDirectory, existingLevelWithNewName.Name + ".xml"), Path.Combine(levelsDirectory, oldName + ".xml"));
                File.Move(tempFile, Path.Combine(levelsDirectory, newName + ".xml"));
                levelToRename.Name = newName;
                existingLevelWithNewName.Name = oldName;
            }

            Save();
        }

        /// <summary>
        /// Saves the current map state... I suppose. This function is empty.
        /// </summary>
        private void Save() { }

        /// <summary>
        /// Saves and reloads the current map state... I suppose. This function is empty.
        /// </summary>
        private void SaveAndReload() { }

        /// <summary>
        /// Updates mouse position.
        /// </summary>
        private void UpdateMouse() => mousePosition = Vector2.Transform(MInput.Mouse.Position, Matrix.Invert(Camera.Matrix));

        /// <summary>
        /// Updates the map editor.
        /// </summary>
        public override void Update()
        {
            Vector2 mouseDelta;
            mouseDelta.X = (lastMouseScreenPosition.X - MInput.Mouse.Position.X) / Camera.Zoom;
            mouseDelta.Y = (lastMouseScreenPosition.Y - MInput.Mouse.Position.Y) / Camera.Zoom;

            if (MInput.Keyboard.Pressed(Keys.Space) && MInput.Keyboard.Check(Keys.LeftControl))
            {
                Camera.Zoom = 6f;
                Camera.Position = Vector2.Zero;
            }

            int zoomDirection = Math.Sign(MInput.Mouse.WheelDelta);
            if (zoomDirection > 0 && Camera.Zoom >= 1f || Camera.Zoom > 1f)
                Camera.Zoom += zoomDirection;
            else
                Camera.Zoom += zoomDirection * 0.25f;

            Camera.Zoom = Math.Max(0.25f, Math.Min(24f, Camera.Zoom));
            Camera.Position += new Vector2(Input.MoveX.Value, Input.MoveY.Value) * 300f * Engine.DeltaTime;

            UpdateMouse();

            hovered.Clear();
            if (mouseMode == MouseModes.Hover)
            {
                mouseDragStart = mousePosition;
                if (MInput.Mouse.PressedLeftButton)
                {
                    // Whether the mouse's position is inside a level
                    bool inLevel = LevelCheck(mousePosition);

                    // Left click + space key is the same as just middle click: it moves the camera
                    if (MInput.Keyboard.Check(Keys.Space))
                        mouseMode = MouseModes.Pan;
                    else if (MInput.Keyboard.Check(Keys.LeftControl))
                    // Like in many softwares, holding LControl while clicking toggles the selected state
                    {
                        if (inLevel)
                            ToggleSelection(mousePosition);
                        else
                            mouseMode = MouseModes.Select;
                    }
                    // Left click + F adds a filler at the current mouse position
                    else if (MInput.Keyboard.Check(Keys.F))
                        levels.Add(new LevelTemplate((int) mousePosition.X, (int) mousePosition.Y, 32, 32));
                    // If we simply left clicked on a level
                    else if (inLevel)
                    {
                        // Select the level if it wasn't the case
                        if (!SelectionCheck(mousePosition))
                            SetSelection(mousePosition);

                        bool resizing = false;
                        if (selection.Count == 1)
                        {
                            // If we know the selection is already of size 1, why not just use selection.ElementAt(0) instead of a foreach loop ?
                            foreach (LevelTemplate selectedLevel in selection)
                            {
                                if (selectedLevel.ResizePosition(mousePosition) && selectedLevel.Type == LevelTemplateType.Filler)
                                    resizing = true;
                            }
                        }

                        if (resizing)
                        {
                            // Same here, we know the selection has only 1 level so why a foreach loop ?
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

                            // Remove the level if it is a filler, and we right clicked on it and we pressed the F key
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
                Camera.Position += mouseDelta;
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

                foreach (LevelTemplate selectedLevel in selection)
                    selectedLevel.Move(relativeMove, levels, snap);

                if (!MInput.Mouse.CheckLeftButton)
                    mouseMode = MouseModes.Hover;
            }
            else if (mouseMode == MouseModes.Resize)
            {
                Vector2 relativeMove = (mousePosition - mouseDragStart).Round();

                foreach (LevelTemplate selectedLevel in selection)
                    selectedLevel.Resize(relativeMove);

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
                if (saveFlash > 0f)
                    saveFlash -= Engine.DeltaTime * 4f;

                lastMouseScreenPosition = MInput.Mouse.Position;

                base.Update();
            }
        }

        /// <summary>
        /// Sets the editor color for all the selected levels to:
        /// <code><see cref="LevelTemplate.fgTilesColor"/>[index]</code>
        /// </summary>
        /// <param name="index">The editor color index.</param>
        private void SetEditorColor(int index)
        {
            foreach (LevelTemplate selectedLevel in selection)
                selectedLevel.EditorColorIndex = index;
        }

        /// <summary>
        /// Renders the map editor.
        /// </summary>
        public override void Render()
        {
            UpdateMouse();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Camera.Matrix * Engine.ScreenMatrix);
            
            float width = Celeste.TargetWidth / Camera.Zoom;
            float height = Celeste.TargetHeight / Camera.Zoom;

            int gridSquareSize = 5; // Why isn't it const ?
            float x = (float) Math.Floor((double) (Camera.Left / gridSquareSize - 1f)) * gridSquareSize;
            float y = (float) Math.Floor((double) (Camera.Top / gridSquareSize - 1f)) * gridSquareSize;

            // Draw the background grid
            for (float i = x; i <= x + width + gridSquareSize * 2; i += gridSquareSize)
                Draw.Line(i, Camera.Top, i, Camera.Top + height, gridColor);
            for (float i = y; i <= y + height + gridSquareSize * 2; i += gridSquareSize)
                Draw.Line(Camera.Left, i, Camera.Left + width, i, gridColor);
            // Top and left lines
            Draw.Line(0f, Camera.Top, 0f, Camera.Top + height, Color.DarkSlateBlue, 1f / Camera.Zoom);
            Draw.Line(Camera.Left, 0f, Camera.Left + width, 0f, Color.DarkSlateBlue, 1f / Camera.Zoom);

            foreach (LevelTemplate level in levels)
                level.RenderContents(Camera, levels);
            foreach (LevelTemplate level in levels)
                level.RenderOutline(Camera);
            foreach (LevelTemplate level in levels)
                // The parameters are in the wrong order ???
                level.RenderHighlight(Camera, selection.Contains(level), hovered.Contains(level));

            // Draw the crosshair for the mouse if hovering
            if (mouseMode == MouseModes.Hover)
            {
                Draw.Line(mousePosition.X - 12f / Camera.Zoom, mousePosition.Y,
                    mousePosition.X + 12f / Camera.Zoom, mousePosition.Y, Color.Yellow, 3f / Camera.Zoom);
                Draw.Line(mousePosition.X, mousePosition.Y - 12f / Camera.Zoom,
                    mousePosition.X, mousePosition.Y + 12f / Camera.Zoom, Color.Yellow, 3f / Camera.Zoom);
            }
            // Draw the selection rectangle if selecting
            else if (mouseMode == MouseModes.Select)
                Draw.Rect(GetMouseRect(mouseDragStart, mousePosition), Color.Lime * 0.25f);

            if (saveFlash > 0)
                Draw.Rect(Camera.Left, Camera.Top, width, height, Color.White * Ease.CubeInOut(saveFlash));
            
            if (fade > 0)
                Draw.Rect(0f, 0f, Celeste.GameWidth, Celeste.GameHeight, Color.Black * fade);

            Draw.SpriteBatch.End();

            // Top UI
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            
            Draw.Rect(0f, 0f, Celeste.TargetWidth, Celeste.TargetHeight / 15, Color.Black);

            Vector2 topUILeft = new(16f, 4f);
            Vector2 topUIRight = new(Celeste.TargetWidth - 16f, 4f);
            // Holding Q highlights strawberries
            if (MInput.Keyboard.Check(Keys.Q))
            {
                // Draw global dark overlay
                Draw.Rect(-10f, -10f, Celeste.TargetWidth + 20f, Celeste.TargetHeight + 20f, Color.Black * 0.25f);

                using (List<LevelTemplate>.Enumerator enumerator = levels.GetEnumerator())
                {
Celeste_Editor_MapEditor_Render_NextStrawberryLevel:
                    while (enumerator.MoveNext())
                    {
                        LevelTemplate current = enumerator.Current;
                        int strawberryIndex = 0;
                        while (true)
                        {
                            if (current.Strawberries != null && strawberryIndex < current.Strawberries.Count)
                            {
                                Vector2 strawberryPosition = current.Strawberries[strawberryIndex];
                                ActiveFont.DrawOutline(current.StrawberryMetadata[strawberryIndex],
                                    (new Vector2(current.X + strawberryPosition.X, current.Y + strawberryPosition.Y) - Camera.Position) * Camera.Zoom
                                        + new Vector2(Celeste.TargetWidth / 2, (Celeste.TargetHeight - 16f) / 2),
                                    new Vector2(0.5f, 1f), Vector2.One * 1f, Color.Red, 2f, Color.Black);
                                strawberryIndex++;
                            }
                            else
                                // Why not just a 'break' ?
                                goto Celeste_Editor_MapEditor_Render_NextStrawberryLevel;
                        }
                    }
                }
            }

            if (hovered.Count == 0)
            {
                if (selection.Count > 0)
                {
                    ActiveFont.Draw(selection.Count.ToString() + " levels selected", topUILeft, Color.Red);
                }
                else
                {
                    ActiveFont.Draw(Dialog.Clean(mapData.Data.Name), topUILeft, Color.Aqua);
                    ActiveFont.Draw(mapData.Area.Mode.ToString() + " MODE", topUIRight, Vector2.UnitX, Vector2.One, Color.Red);
                }
            }
            else if (hovered.Count == 1)
            {
                LevelTemplate levelTemplate = null;
                // Again, why not just get the last object of the set ?
                using (HashSet<LevelTemplate>.Enumerator enumerator = hovered.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        levelTemplate = enumerator.Current;
                }

                string text = levelTemplate.ActualWidth.ToString() + "x" + levelTemplate.ActualHeight.ToString() + "   "
                    + levelTemplate.X + "," + levelTemplate.Y + "   " + (levelTemplate.X * 8) + "," + (levelTemplate.Y * 8);

                ActiveFont.Draw(levelTemplate.Name, topUILeft, Color.Yellow);
                ActiveFont.Draw(text, topUIRight, Vector2.UnitX, Vector2.One, Color.Green);
            }
            else
                ActiveFont.Draw(hovered.Count.ToString() + " levels", topUILeft, Color.Yellow);

            Draw.SpriteBatch.End();
        }

        /// <summary>
        /// Loads a level at a specific position.
        /// </summary>
        /// <param name="level">The level to load.</param>
        /// <param name="at">The level position.</param>
        private void LoadLevel(LevelTemplate level, Vector2 at)
        {
            Save();

            Engine.Scene = new LevelLoader(
                new Session(area)
                {
                    FirstLevel = false,
                    Level = level.Name,
                    StartedFromBeginning = false
                },
                new Vector2?(at)
            );
        }

        /// <summary>
        /// Stores all the current level positions as a single undo action in <see cref="undoStack"/>.
        /// </summary>
        private void StoreUndo()
        {
            Vector2[] levelPositions = new Vector2[levels.Count];
            for (int i = 0; i < levels.Count; i++)
                levelPositions[i] = new Vector2(levels[i].X, levels[i].Y);
            undoStack.Add(levelPositions);

            while (undoStack.Count > 30)
                undoStack.RemoveAt(0);

            redoStack.Clear();
        }

        /// <summary>
        /// Undo the last modification.
        /// </summary>
        private void Undo()
        {
            if (undoStack.Count <= 0)
                return;

            Vector2[] levelPositions = new Vector2[levels.Count];
            for (int index = 0; index < levels.Count; ++index)
                levelPositions[index] = new Vector2(levels[index].X, levels[index].Y);
            redoStack.Add(levelPositions);

            // Why not just undoStack.Last() ?
            Vector2[] undo = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            for (int i = 0; i < undo.Length; ++i)
            {
                levels[i].X = (int) undo[i].X;
                levels[i].Y = (int) undo[i].Y;
            }
        }

        /// <summary>
        /// Redo the last undo.
        /// </summary>
        private void Redo()
        {
            if (redoStack.Count <= 0)
                return;

            Vector2[] vector2Array = new Vector2[levels.Count];
            for (int index = 0; index < levels.Count; ++index)
                vector2Array[index] = new Vector2(levels[index].X, levels[index].Y);
            undoStack.Add(vector2Array);

            // Why not just redoStack.Last() ?
            Vector2[] redo = redoStack[undoStack.Count - 1];
            redoStack.RemoveAt(undoStack.Count - 1);
            for (int index = 0; index < redo.Length; ++index)
            {
                levels[index].X = (int)redo[index].X;
                levels[index].Y = (int)redo[index].Y;
            }
        }

        /// <summary>
        /// Creates a rectangle from two points. This is used for the mouse selection rectangle.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>A rectangle made from the two points.</returns>
        private Rectangle GetMouseRect(Vector2 a, Vector2 b)
        {
            Vector2 topLeft = new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            Vector2 bottomRight = new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            return new Rectangle((int) topLeft.X, (int) topLeft.Y, (int) (bottomRight.X - topLeft.X), (int) (bottomRight.Y - topLeft.Y));
        }

        /// <summary>
        /// Returns the first non-dummy level with the point inside of it.
        /// </summary>
        /// <param name="point">Where to look for a level.</param>
        /// <returns>The first non-dummy level with the point inside of it, or <see langword="null"/> if no such level exists.</returns>
        private LevelTemplate TestCheck(Vector2 point)
        {
            foreach (LevelTemplate level in levels)
            {
                if (!level.Dummy && level.Check(point))
                    return level;
            }
            return null;
        }

        /// <summary>
        /// Checks whether a point is inside a level.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>true if the point is in at least one level.</returns>
        private bool LevelCheck(Vector2 point)
        {
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(point))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the level at this position is selected.
        /// </summary>
        /// <param name="point">The position to check.</param>
        /// <returns>true if the point is in at least one level and that level is selected.</returns>
        private bool SelectionCheck(Vector2 point)
        {
            foreach (LevelTemplate selectedLevel in selection)
            {
                if (selectedLevel.Check(point))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Selects all the levels at this position.
        /// </summary>
        /// <param name="point">The position to check.</param>
        /// <returns><see langword="true"/> if at least one level was found.</returns>
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

        /// <summary>
        /// Toggle the selected state of all the levels at this position.
        /// </summary>
        /// <param name="point">The position to check.</param>
        /// <returns><see langword="true"/> if at least one level was changed.</returns>
        private bool ToggleSelection(Vector2 point)
        {
            bool foundLevel = false;
            foreach (LevelTemplate level in levels)
            {
                if (level.Check(point))
                {
                    foundLevel = true;
                    if (selection.Contains(level))
                        selection.Remove(level);
                    else
                        selection.Add(level);
                }
            }
            return foundLevel;
        }

        /// <summary>
        /// Selects all the levels in this rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        private void SetSelection(Rectangle rect)
        {
            selection.Clear();

            foreach (LevelTemplate level in levels)
            {
                if (level.Check(rect))
                    selection.Add(level);
            }
        }

        /// <summary>
        /// Toggles the selected state of all the levels in this rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
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

        /// <summary>
        /// Mouse modes for the map editor.
        /// </summary>
        private enum MouseModes
        {
            /// <summary>
            /// Default mode. As its name suggest, it is used when hovering over levels.
            /// </summary>
            Hover,
            /// <summary>
            /// Used when moving the camera by dragging with the middle button or the left button and the space key.
            /// </summary>
            Pan,
            /// <summary>
            /// As its name suggests, it is used when the mouse is selecting levels.
            /// </summary>
            Select,
            /// <summary>
            /// Used when moving levels.
            /// </summary>
            Move,
            /// <summary>
            /// Used when resizing levels.
            /// </summary>
            Resize
        }
    }
}