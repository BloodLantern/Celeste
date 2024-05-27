using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste
{
    public class PreviewRecording : Scene
    {
        public string Filename;
        public List<Player.ChaserState> Timeline;
        public PlayerPlayback entity;
        public float ScreenWidth = 640f;
        public float ScreenHeight = 360f;
        public float Width;
        public float Height;

        private Matrix Matrix => Matrix.CreateScale(1920f / ScreenWidth) * Engine.ScreenMatrix;

        public PreviewRecording(string filename)
        {
            Filename = filename;
            Timeline = PlaybackData.Import(File.ReadAllBytes(filename));
            float val2_1 = float.MaxValue;
            float val2_2 = float.MinValue;
            float val2_3 = float.MinValue;
            float val2_4 = float.MaxValue;
            foreach (Player.ChaserState chaserState in Timeline)
            {
                val2_1 = Math.Min(chaserState.Position.X, val2_1);
                val2_2 = Math.Max(chaserState.Position.X, val2_2);
                val2_4 = Math.Min(chaserState.Position.Y, val2_4);
                val2_3 = Math.Max(chaserState.Position.Y, val2_3);
            }
            Width = (int) (val2_2 - (double) val2_1);
            Height = (int) (val2_3 - (double) val2_4);
            Add(entity = new PlayerPlayback(new Vector2((float) ((ScreenWidth - (double) Width) / 2.0) - val2_1, (float) ((ScreenHeight - (double) Height) / 2.0) - val2_4), PlayerSpriteMode.Madeline, Timeline));
        }

        public override void Update()
        {
            if (MInput.Keyboard.Check(Keys.A))
                entity.TrimStart = Math.Max(0.0f, entity.TrimStart -= Engine.DeltaTime);
            if (MInput.Keyboard.Check(Keys.D))
                entity.TrimStart = Math.Min(entity.Duration, entity.TrimStart += Engine.DeltaTime);
            if (MInput.Keyboard.Check(Keys.Left))
                entity.TrimEnd = Math.Max(0.0f, entity.TrimEnd -= Engine.DeltaTime);
            if (MInput.Keyboard.Check(Keys.Right))
                entity.TrimEnd = Math.Min(entity.Duration, entity.TrimEnd += Engine.DeltaTime);
            if (MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S))
            {
                while (Timeline[0].TimeStamp < (double) entity.TrimStart)
                    Timeline.RemoveAt(0);
                while (Timeline[Timeline.Count - 1].TimeStamp > (double) entity.TrimEnd)
                    Timeline.RemoveAt(Timeline.Count - 1);
                PlaybackData.Export(Timeline, Filename);
                Engine.Scene = new PreviewRecording(Filename);
            }
            base.Update();
            entity.Hair.AfterUpdate();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            ActiveFont.Draw("A/D:        Move Start Trim", new Vector2(8f, 8f), new Vector2(0.0f, 0.0f), Vector2.One * 0.5f, Color.White);
            ActiveFont.Draw("Left/Right: Move End Trim", new Vector2(8f, 32f), new Vector2(0.0f, 0.0f), Vector2.One * 0.5f, Color.White);
            ActiveFont.Draw("CTRL+S: Save New Trim", new Vector2(8f, 56f), new Vector2(0.0f, 0.0f), Vector2.One * 0.5f, Color.White);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Matrix);
            Draw.HollowRect((float) ((ScreenWidth - (double) Width) / 2.0 - 16.0), (float) ((ScreenHeight - (double) Height) / 2.0 - 16.0), Width + 32f, Height + 32f, Color.Red * 0.6f);
            Draw.HollowRect((float) ((ScreenWidth - 320.0) / 2.0), (float) ((ScreenHeight - 180.0) / 2.0), 320f, 180f, Color.White * 0.6f);
            if (entity.Visible)
                entity.Render();
            Draw.Rect(32f, ScreenHeight - 48f, ScreenWidth - 64f, 16f, Color.DarkGray);
            Draw.Rect(32f, ScreenHeight - 48f, (float) ((ScreenWidth - 64.0) * (entity.Time / (double) entity.Duration)), 16f, Color.White);
            Draw.Rect((float) (32.0 + (ScreenWidth - 64.0) * (entity.Time / (double) entity.Duration) - 2.0), ScreenHeight - 48f, 4f, 16f, Color.LimeGreen);
            Draw.Rect((float) (32.0 + (ScreenWidth - 64.0) * (entity.TrimStart / (double) entity.Duration) - 2.0), ScreenHeight - 48f, 4f, 16f, Color.Red);
            Draw.Rect((float) (32.0 + (ScreenWidth - 64.0) * (entity.TrimEnd / (double) entity.Duration) - 2.0), ScreenHeight - 48f, 4f, 16f, Color.Red);
            Draw.SpriteBatch.End();
        }
    }
}
