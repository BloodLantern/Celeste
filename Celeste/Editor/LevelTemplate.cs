// Decompiled with JetBrains decompiler
// Type: Celeste.Editor.LevelTemplate
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Editor
{
    public class LevelTemplate
    {
        public string Name;
        public LevelTemplateType Type;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int ActualWidth;
        public int ActualHeight;
        public Grid Grid;
        public Grid Back;
        public List<Vector2> Spawns;
        public List<Vector2> Strawberries;
        public List<string> StrawberryMetadata;
        public List<Vector2> Checkpoints;
        public List<Rectangle> Jumpthrus;
        public bool Dummy;
        public int EditorColorIndex;
        private Vector2 moveAnchor;
        private Vector2 resizeAnchor;
        private readonly List<Rectangle> solids = new();
        private readonly List<Rectangle> backs = new();
        private static readonly Color bgTilesColor = Color.DarkSlateGray * 0.5f;
        private static readonly Color[] fgTilesColor = new Color[7] {
            Color.White,
                Calc.HexToColor("f6735e"),
                Calc.HexToColor("85f65e"),
                Calc.HexToColor("37d7e3"),
                Calc.HexToColor("376be3"),
                Calc.HexToColor("c337e3"),
                Calc.HexToColor("e33773")
        };
        private static readonly Color inactiveBorderColor = Color.DarkSlateGray;
        private static readonly Color selectedBorderColor = Color.Red;
        private static readonly Color hoveredBorderColor = Color.Yellow;
        private static readonly Color dummyBgTilesColor = Color.DarkSlateGray * 0.5f;
        private static readonly Color dummyFgTilesColor = Color.LightGray;
        private static readonly Color dummyInactiveBorderColor = Color.DarkOrange;
        private static readonly Color firstBorderColor = Color.Aqua;

        private Vector2 ResizeHoldSize => new(Math.Min(Width, 4), Math.Min(Height, 4));

        public LevelTemplate(LevelData data)
        {
            Name = data.Name;
            EditorColorIndex = data.EditorColorIndex;
            X = data.Bounds.X / 8;
            Y = data.Bounds.Y / 8;
            ActualWidth = data.Bounds.Width;
            ActualHeight = data.Bounds.Height;
            Width = (int)Math.Ceiling(ActualWidth / 8.0);
            Height = (int)Math.Ceiling(ActualHeight / 8.0);
            Grid = new Grid(8f, 8f, data.Solids);
            Back = new Grid(8f, 8f, data.Bg);
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
                    Jumpthrus.Add(new Rectangle((int)(entity.Position.X / 8.0), (int)(entity.Position.Y / 8.0), entity.Width / 8, 1));
            }
            Dummy = data.Dummy;
            Type = LevelTemplateType.Level;
        }

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

        public void RenderContents(Camera camera, List<LevelTemplate> allLevels)
        {
            if (camera is null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (Type == LevelTemplateType.Level)
            {
                bool flag = false;
                if (Engine.Scene.BetweenInterval(0.1f))
                {
                    foreach (LevelTemplate allLevel in allLevels)
                    {
                        if (allLevel != this && allLevel.Rect.Intersects(Rect))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                Draw.Rect(X, Y, Width, Height, (flag ? Color.Red : Color.Black) * 0.5f);
                foreach (Rectangle back in backs)
                    Draw.Rect((X + back.X), (Y + back.Y), back.Width, back.Height, Dummy ? dummyBgTilesColor : bgTilesColor);
                foreach (Rectangle solid in solids)
                    Draw.Rect((X + solid.X), (Y + solid.Y), solid.Width, solid.Height, Dummy ? dummyFgTilesColor : fgTilesColor[EditorColorIndex]);
                foreach (Vector2 spawn in Spawns)
                    Draw.Rect(X + spawn.X, (float)(Y + spawn.Y - 1.0), 1f, 1f, Color.Red);
                foreach (Vector2 strawberry in Strawberries)
                    Draw.HollowRect((float)(X + strawberry.X - 1.0), (float)(Y + strawberry.Y - 2.0), 3f, 3f, Color.LightPink);
                foreach (Vector2 checkpoint in Checkpoints)
                    Draw.HollowRect((float)(X + checkpoint.X - 1.0), (float)(Y + checkpoint.Y - 2.0), 3f, 3f, Color.Lime);
                foreach (Rectangle jumpthru in Jumpthrus)
                    Draw.Rect((X + jumpthru.X), (Y + jumpthru.Y), jumpthru.Width, 1f, Color.Yellow);
            }
            else
            {
                Draw.Rect(X, Y, Width, Height, dummyFgTilesColor);
                Draw.Rect((X + Width) - ResizeHoldSize.X, (Y + Height) - ResizeHoldSize.Y, ResizeHoldSize.X, ResizeHoldSize.Y, Color.Orange);
            }
        }

        public void RenderOutline(Camera camera)
        {
            float t = (float)(1.0 / (double)camera.Zoom * 2.0);
            if (Check(Vector2.Zero))
                Outline((X + 1), (Y + 1), (Width - 2), (Height - 2), t, firstBorderColor);
            Outline(X, Y, Width, Height, t, Dummy ? dummyInactiveBorderColor : inactiveBorderColor);
        }

        public void RenderHighlight(Camera camera, bool hovered, bool selected)
        {
            if (!(selected | hovered))
                return;
            Outline(X, Y, Width, Height, (float)(1.0 / (double)camera.Zoom * 2.0), hovered ? hoveredBorderColor : selectedBorderColor);
        }

        private void Outline(float x, float y, float w, float h, float t, Color color)
        {
            Draw.Line(x, y, x + w, y, color, t);
            Draw.Line(x + w, y, x + w, y + h, color, t);
            Draw.Line(x, y + h, x + w, y + h, color, t);
            Draw.Line(x, y, x, y + h, color, t);
        }

        public bool Check(Vector2 point) => point.X >= Left && point.Y >= Top && point.X < Right && point.Y < Bottom;

        public bool Check(Rectangle rect) => Rect.Intersects(rect);

        public void StartMoving() => moveAnchor = new Vector2(X, Y);

        public void Move(Vector2 relativeMove, List<LevelTemplate> allLevels, bool snap)
        {
            X = (int)(moveAnchor.X + relativeMove.X);
            Y = (int)(moveAnchor.Y + relativeMove.Y);
            if (!snap)
                return;
            foreach (LevelTemplate allLevel in allLevels)
            {
                if (allLevel != this)
                {
                    if (Bottom >= allLevel.Top && Top <= allLevel.Bottom)
                    {
                        int num = Math.Abs(Left - allLevel.Right) < 3 ? 1 : 0;
                        bool flag = Math.Abs(Right - allLevel.Left) < 3;
                        if (num != 0)
                            Left = allLevel.Right;
                        else if (flag)
                            Right = allLevel.Left;
                        if ((num | (flag ? 1 : 0)) != 0)
                        {
                            if (Math.Abs(Top - allLevel.Top) < 3)
                                Top = allLevel.Top;
                            else if (Math.Abs(Bottom - allLevel.Bottom) < 3)
                                Bottom = allLevel.Bottom;
                        }
                    }
                    if (Right >= allLevel.Left && Left <= allLevel.Right)
                    {
                        int num = Math.Abs(Top - allLevel.Bottom) < 5 ? 1 : 0;
                        bool flag = Math.Abs(Bottom - allLevel.Top) < 5;
                        if (num != 0)
                            Top = allLevel.Bottom;
                        else if (flag)
                            Bottom = allLevel.Top;
                        if ((num | (flag ? 1 : 0)) != 0)
                        {
                            if (Math.Abs(Left - allLevel.Left) < 3)
                                Left = allLevel.Left;
                            else if (Math.Abs(Right - allLevel.Right) < 3)
                                Right = allLevel.Right;
                        }
                    }
                }
            }
        }

        public void StartResizing() => resizeAnchor = new Vector2(Width, Height);

        public void Resize(Vector2 relativeMove)
        {
            Width = Math.Max(1, (int)(resizeAnchor.X + relativeMove.X));
            Height = Math.Max(1, (int)(resizeAnchor.Y + relativeMove.Y));
            ActualWidth = Width * 8;
            ActualHeight = Height * 8;
        }

        public bool ResizePosition(Vector2 mouse) => mouse.X > (X + Width) - ResizeHoldSize.X && mouse.Y > (Y + Height) - ResizeHoldSize.Y && mouse.X < (X + Width) && mouse.Y < (Y + Height);

        public Rectangle Rect => new(X, Y, Width, Height);

        public int Left
        {
            get => X;
            set => X = value;
        }

        public int Top
        {
            get => Y;
            set => Y = value;
        }

        public int Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        public int Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }
    }
}