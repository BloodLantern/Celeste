using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Editor
{
    /// <summary>
    /// A level in the <see cref="MapEditor"/>.
    /// </summary>
    public class LevelTemplate
    {
        /// <summary>
        /// The name of the level.
        /// </summary>
        public string Name;
        /// <summary>
        /// Whether the level is a normal level or a filler.
        /// </summary>
        public LevelTemplateType Type;

        /// <summary>
        /// X position in tiles.
        /// </summary>
        public int X;
        /// <summary>
        /// Y position in tiles.
        /// </summary>
        public int Y;
        /// <summary>
        /// Width in tiles.
        /// </summary>
        public int Width;
        /// <summary>
        /// Height in tiles.
        /// </summary>
        public int Height;
        /// <summary>
        /// Width in pixels.
        /// </summary>
        public int ActualWidth;
        /// <summary>
        /// Height in pixels.
        /// </summary>
        public int ActualHeight;

        /// <summary>
        /// The foreground tiles grid.
        /// </summary>
        public Grid Grid;
        /// <summary>
        /// The background tiles grid.
        /// </summary>
        public Grid Back;

        /// <summary>
        /// List of player spawns.
        /// </summary>
        public List<Vector2> Spawns;
        /// <summary>
        /// List of strawberries.
        /// </summary>
        public List<Vector2> Strawberries;
        /// <summary>
        /// List of strawberry metadatas in format:
        /// "&lt;checkpointIndex&gt;:&lt;indexInCheckpoint&gt;"
        /// </summary>
        public List<string> StrawberryMetadata;
        /// <summary>
        /// List of checkpoints.
        /// </summary>
        public List<Vector2> Checkpoints;
        /// <summary>
        /// List of jumpthrus.
        /// </summary>
        public List<Rectangle> Jumpthrus;

        /// <summary>
        /// A level is considered a dummy if it is not a filler and doesn't have any player spawns.
        /// </summary>
        public bool Dummy;
        /// <summary>
        /// The color index in the <see cref="fgTilesColor"/> array.
        /// </summary>
        public int EditorColorIndex;

        /// <summary>
        /// The start position of the current move.
        /// </summary>
        private Vector2 moveAnchor;
        /// <summary>
        /// The start position of the current resize.
        /// </summary>
        private Vector2 resizeAnchor;

        /// <summary>
        /// List of foreground tiles stored as horizontal rectangles (the height will always be 1).
        /// </summary>
        private readonly List<Rectangle> solids = new();
        /// <summary>
        /// List of background tiles stored as horizontal rectangles (the height will always be 1).
        /// </summary>
        private readonly List<Rectangle> backs = new();

        private static readonly Color bgTilesColor = Color.DarkSlateGray * 0.5f;
        private static readonly Color[] fgTilesColor = new Color[7] {
            Color.White,                // Default white
            Calc.HexToColor("f6735e"),  // Orange
            Calc.HexToColor("85f65e"),  // Green
            Calc.HexToColor("37d7e3"),  // Light blue
            Calc.HexToColor("376be3"),  // Blue
            Calc.HexToColor("c337e3"),  // Purple
            Calc.HexToColor("e33773")   // Pink
        };
        private static readonly Color inactiveBorderColor = Color.DarkSlateGray;
        private static readonly Color selectedBorderColor = Color.Red;
        private static readonly Color hoveredBorderColor = Color.Yellow;
        private static readonly Color dummyBgTilesColor = Color.DarkSlateGray * 0.5f;
        private static readonly Color dummyFgTilesColor = Color.LightGray;
        private static readonly Color dummyInactiveBorderColor = Color.DarkOrange;
        private static readonly Color firstBorderColor = Color.Aqua;

        /// <summary>
        /// The size of the bottom right corner rectangle used to resize fillers.
        /// </summary>
        private Vector2 ResizeHoldSize => new(Math.Min(Width, 4), Math.Min(Height, 4));

        /// <summary>
        /// Default level constructor.
        /// </summary>
        /// <param name="data">The <see cref="LevelData"/> to use for this level template.</param>
        public LevelTemplate(LevelData data)
        {
            Name = data.Name;
            EditorColorIndex = data.EditorColorIndex;

            X = data.Bounds.X / 8;
            Y = data.Bounds.Y / 8;
            ActualWidth = data.Bounds.Width;
            ActualHeight = data.Bounds.Height;
            Width = (int) Math.Ceiling((double) ActualWidth / 8);
            Height = (int) Math.Ceiling((double) ActualHeight / 8);

            Grid = new Grid(8f, 8f, data.Solids);
            Back = new Grid(8f, 8f, data.Bg);

            // Setup the foreground and background tile rectangles arrays 'solids' and 'backs'
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    int width = 0;
                    while (x + width < Width && Back[x + width, y] && !Grid[x + width, y])
                        ++width;

                    if (width > 0)
                    {
                        backs.Add(new Rectangle(x, y, width, 1));
                        x += width - 1;
                    }
                }

                for (int x = 0; x < Width; ++x)
                {
                    int width = 0;
                    while (x + width < Width && Grid[x + width, y])
                        ++width;

                    if (width > 0)
                    {
                        solids.Add(new Rectangle(x, y, width, 1));
                        x += width - 1;
                    }
                }
            }

            Spawns = new List<Vector2>();
            foreach (Vector2 spawn in data.Spawns)
                Spawns.Add(spawn / 8f - new Vector2(X, Y));

            Strawberries = new List<Vector2>();
            StrawberryMetadata = new List<string>();
            Checkpoints = new List<Vector2>();
            Jumpthrus = new List<Rectangle>();

            // Setup strawberries, checkpoints and jumpthrus
            foreach (EntityData entity in data.Entities)
            {
                if (entity.Name.Equals("strawberry") || entity.Name.Equals("snowberry"))
                {
                    Strawberries.Add(entity.Position / 8f);
                    StrawberryMetadata.Add(entity.Int("checkpointID").ToString() + ":" + entity.Int("order"));
                }
                else if (entity.Name.Equals("checkpoint"))
                    Checkpoints.Add(entity.Position / 8f);
                else if (entity.Name.Equals("jumpThru"))
                    Jumpthrus.Add(new Rectangle((int) entity.Position.X / 8, (int) entity.Position.Y / 8, entity.Width / 8, 1));
            }

            Dummy = data.Dummy;

            Type = LevelTemplateType.Level;
        }

        /// <summary>
        /// Filler constructor.
        /// </summary>
        /// <param name="x">The X position of the filler in tiles.</param>
        /// <param name="y">The Y position of the filler in tiles.</param>
        /// <param name="w">The width of the filler in tiles.</param>
        /// <param name="h">The height of the filler in tiles.</param>
        public LevelTemplate(int x, int y, int w, int h)
        {
            Name = "FILLER";

            X = x;
            Y = y;
            Width = w;
            Height = h;
            ActualWidth = w * 8;
            ActualHeight = h * 8;

            Type = LevelTemplateType.Filler;
        }

        /// <summary>
        /// Renders the level. This consists in rendering the tiles of the level,
        /// and a background that can be red if the level is in an invalid position.
        /// </summary>
        /// <param name="camera">The camera used to render.</param>
        /// <param name="allLevels">The other levels.</param>
        /// <exception cref="ArgumentNullException">If camera is null.</exception>
        public void RenderContents(Camera camera, List<LevelTemplate> allLevels)
        {
            if (camera is null)
                throw new ArgumentNullException(nameof(camera));

            if (Type == LevelTemplateType.Level)
            {
                bool invalidPosition = false;
                if (Engine.Scene.BetweenInterval(0.1f))
                {
                    foreach (LevelTemplate level in allLevels)
                    {
                        if (level != this && level.Rect.Intersects(Rect))
                        {
                            invalidPosition = true;
                            break;
                        }
                    }
                }
                // Draw level background
                Draw.Rect(X, Y, Width, Height, (invalidPosition ? Color.Red : Color.Black) * 0.5f);

                // Draw tiles
                foreach (Rectangle back in backs)
                    Draw.Rect(X + back.X, Y + back.Y, back.Width, back.Height, Dummy ? dummyBgTilesColor : bgTilesColor);
                foreach (Rectangle solid in solids)
                    Draw.Rect(X + solid.X, Y + solid.Y, solid.Width, solid.Height, Dummy ? dummyFgTilesColor : fgTilesColor[EditorColorIndex]);
                
                // Draw entities
                foreach (Vector2 spawn in Spawns)
                    Draw.Rect(X + spawn.X, Y + spawn.Y - 1f, 1f, 1f, Color.Red);
                foreach (Vector2 strawberry in Strawberries)
                    Draw.HollowRect(X + strawberry.X - 1f, Y + strawberry.Y - 2f, 3f, 3f, Color.LightPink);
                foreach (Vector2 checkpoint in Checkpoints)
                    Draw.HollowRect(X + checkpoint.X - 1f, Y + checkpoint.Y - 2f, 3f, 3f, Color.Lime);
                foreach (Rectangle jumpthru in Jumpthrus)
                    Draw.Rect(X + jumpthru.X, Y + jumpthru.Y, jumpthru.Width, 1f, Color.Yellow);
            }
            else
            {
                // If filler
                Draw.Rect(X, Y, Width, Height, dummyFgTilesColor);
                Draw.Rect(X + Width - ResizeHoldSize.X, Y + Height - ResizeHoldSize.Y, ResizeHoldSize.X, ResizeHoldSize.Y, Color.Orange);
            }
        }

        /// <summary>
        /// Renders the outline of this level. This is the base outline and, specifically
        /// for the level in (0, 0), an additional aqua outline.
        /// </summary>
        /// <param name="camera">The camera used to render.</param>
        public void RenderOutline(Camera camera)
        {
            // Compute line thickness
            float t = 1f / camera.Zoom * 2f;
            // If the level is at location (0, 0), draw an extra outline
            if (Check(Vector2.Zero))
                Outline(X + 1, Y + 1, Width - 2, Height - 2, t, firstBorderColor);
            // Draw the gray outline for the level bounds
            Outline(X, Y, Width, Height, t, Dummy ? dummyInactiveBorderColor : inactiveBorderColor);
        }

        /// <summary>
        /// Renders the highlighted outline of this level if it is either selected or hovered.
        /// </summary>
        /// <param name="camera">The camera used to render.</param>
        /// <param name="hovered">Whether the level is hovered.</param>
        /// <param name="selected">Whether the level is selected.</param>
        /// <remarks>
        /// Remark: This function is only used in <see cref="MapEditor.Render"/>
        /// and the hovered and selected parameters are used in the wrong order.
        /// </remarks>
        public void RenderHighlight(Camera camera, bool hovered, bool selected)
        {
            if (!(selected | hovered))
                return;
            Outline(X, Y, Width, Height, 1f / camera.Zoom * 2f, hovered ? hoveredBorderColor : selectedBorderColor);
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        /// <param name="t">The line thickness.</param>
        /// <param name="color">The line color.</param>
        private void Outline(float x, float y, float w, float h, float t, Color color)
        {
            Draw.Line(x, y, x + w, y, color, t);
            Draw.Line(x + w, y, x + w, y + h, color, t);
            Draw.Line(x, y + h, x + w, y + h, color, t);
            Draw.Line(x, y, x, y + h, color, t);
        }

        /// <summary>
        /// Checks whether a point is located inside this level.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns><see langword="true"/> if the point is inside the level.</returns>
        public bool Check(Vector2 point) => point.X >= Left && point.Y >= Top && point.X < Right && point.Y < Bottom;

        /// <summary>
        /// Checks whether a rectangle intersects this level.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><see langword="true"/> if the rectangle intersects this level.</returns>
        public bool Check(Rectangle rect) => Rect.Intersects(rect);

        /// <summary>
        /// Sets the move anchor to the current position of the level.
        /// </summary>
        public void StartMoving() => moveAnchor = new Vector2(X, Y);

        /// <summary>
        /// Move the level relative to <see cref="moveAnchor"/>.
        /// </summary>
        /// <param name="relativeMove">The current move change.</param>
        /// <param name="allLevels">The other levels.</param>
        /// <param name="snap">Whether to snap when moving close enough to another level.</param>
        public void Move(Vector2 relativeMove, List<LevelTemplate> allLevels, bool snap)
        {
            X = (int) (moveAnchor.X + relativeMove.X);
            Y = (int) (moveAnchor.Y + relativeMove.Y);

            if (!snap)
                return;

            foreach (LevelTemplate otherLevel in allLevels)
            {
                if (otherLevel != this)
                {
                    // If this level is inside the other level on the Y axis
                    if (Bottom >= otherLevel.Top && Top <= otherLevel.Bottom)
                    {
                        // Maybe change int to bool ?
                        int snapRight = Math.Abs(Left - otherLevel.Right) < 3 ? 1 : 0;
                        bool snapLeft = Math.Abs(Right - otherLevel.Left) < 3;

                        // Try to snap on the X axis
                        if (snapRight != 0)
                            Left = otherLevel.Right;
                        else if (snapLeft)
                            Right = otherLevel.Left;

                        // If snapped
                        if ((snapRight | (snapLeft ? 1 : 0)) != 0)
                        {
                            // Then try to snap on the Y axis
                            if (Math.Abs(Top - otherLevel.Top) < 3)
                                Top = otherLevel.Top;
                            else if (Math.Abs(Bottom - otherLevel.Bottom) < 3)
                                Bottom = otherLevel.Bottom;
                        }
                    }

                    // If this level is inside the other level on the X axis
                    if (Right >= otherLevel.Left && Left <= otherLevel.Right)
                    {
                        // Maybe change int to bool ?
                        int snapDown = Math.Abs(Top - otherLevel.Bottom) < 5 ? 1 : 0;
                        bool snapUp = Math.Abs(Bottom - otherLevel.Top) < 5;

                        // Try to snap on the Y axis
                        if (snapDown != 0)
                            Top = otherLevel.Bottom;
                        else if (snapUp)
                            Bottom = otherLevel.Top;

                        // If snapped
                        if ((snapDown | (snapUp ? 1 : 0)) != 0)
                        {
                            // Then try to snap on the X axis
                            if (Math.Abs(Left - otherLevel.Left) < 3)
                                Left = otherLevel.Left;
                            else if (Math.Abs(Right - otherLevel.Right) < 3)
                                Right = otherLevel.Right;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the resize anchor to the current size of the level.
        /// </summary>
        public void StartResizing() => resizeAnchor = new Vector2(Width, Height);

        /// <summary>
        /// Resizes this level relative to <see cref="resizeAnchor"/>.
        /// </summary>
        /// <param name="relativeMove">The current size change.</param>
        public void Resize(Vector2 relativeMove)
        {
            Width = Math.Max(1, (int) (resizeAnchor.X + relativeMove.X));
            Height = Math.Max(1, (int) (resizeAnchor.Y + relativeMove.Y));
            ActualWidth = Width * 8;
            ActualHeight = Height * 8;
        }

        /// <summary>
        /// Checks whether the mouse is inside the bottom right corner resize rectangle.
        /// </summary>
        /// <param name="mouse">The current mouse position.</param>
        /// <returns><see langword="true"/> if the mouse is in the resize rectangle.</returns>
        public bool ResizePosition(Vector2 mouse) => (mouse.X > X + Width - ResizeHoldSize.X) && (mouse.Y > Y + Height - ResizeHoldSize.Y) && (mouse.X < X + Width) && (mouse.Y < Y + Height);

        /// <summary>
        /// Gets a <see cref="Rectangle"/> with the current position and size of the level.
        /// </summary>
        public Rectangle Rect => new(X, Y, Width, Height);

        /// <summary>
        /// The X position of the level.
        /// </summary>
        public int Left
        {
            get => X;
            set => X = value;
        }

        /// <summary>
        /// The Y position of the level.
        /// </summary>
        public int Top
        {
            get => Y;
            set => Y = value;
        }

        /// <summary>
        /// The X position + width of the level.
        /// </summary>
        public int Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        /// <summary>
        /// The Y position + height of the level.
        /// </summary>
        public int Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }
    }
}