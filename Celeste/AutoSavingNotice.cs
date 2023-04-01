// Decompiled with JetBrains decompiler
// Type: Celeste.AutoSavingNotice
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class AutoSavingNotice : Monocle.Renderer
    {
        private const string title = "autosaving_title_PC";
        private const string desc = "autosaving_desc_PC";
        private const float duration = 3f;
        public static readonly Color TextColor = Color.White;
        public bool Display = true;
        public bool StillVisible;
        public bool ForceClose;
        private float ease;
        private float timer;
        private readonly Sprite icon = GFX.GuiSpriteBank.Create("save");
        private float startTimer = 0.5f;
        private readonly Wiggler wiggler;

        public AutoSavingNotice()
        {
            icon.Visible = false;
            wiggler = Wiggler.Create(0.4f, 4f, f => icon.Rotation = f * 0.1f);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (startTimer > 0.0)
            {
                startTimer -= Engine.DeltaTime;
                if (startTimer <= 0.0)
                {
                    icon.Play("start");
                    icon.Visible = true;
                }
            }
            if (scene.OnInterval(1f))
                wiggler.Start();

            bool flag = ForceClose || (!Display && timer >= 1.0);
            ease = Calc.Approach(ease, !flag ? 1f : 0.0f, Engine.DeltaTime);
            timer += Engine.DeltaTime / duration;
            StillVisible = Display || ease > 0.0;
            wiggler.Update();
            icon.Update();
            if (!flag || string.IsNullOrEmpty(icon.CurrentAnimationID) || !icon.CurrentAnimationID.Equals("idle"))
                return;

            icon.Play("end");
        }

        public override void Render(Scene scene)
        {
            float num = Ease.CubeInOut(ease);
            Color color = TextColor * num;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            ActiveFont.Draw(Dialog.Clean(title), new Vector2(960f, (float)(480.0 - (30.0 * (double)num))), new Vector2(0.5f, 1f), Vector2.One, color);
            if (icon.Visible)
            {
                icon.RenderPosition = new Vector2(1920f, 1080f) / 2f;
                icon.Render();
            }
            ActiveFont.Draw(Dialog.Clean(desc), new Vector2(960f, (float)(600.0 + (30.0 * (double)num))), new Vector2(0.5f, 0.0f), Vector2.One, color);
            Draw.SpriteBatch.End();
        }
    }
}
