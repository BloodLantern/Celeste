// Decompiled with JetBrains decompiler
// Type: Celeste.TestWipes
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class TestWipes : Scene
    {
        private readonly Coroutine coroutine;
        private Color lastColor = Color.White;

        public TestWipes()
        {
            coroutine = new Coroutine(routine());
        }

        private IEnumerator routine()
        {
            TestWipes testWipes = this;
            float dur = 1f;
            yield return 1f;
            while (true)
            {
                ScreenWipe.WipeColor = Color.Black;
                new CurtainWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ff0034");
                new AngledWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("0b0960");
                new DreamWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("39bf00");
                new KeyDoorWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("4376b3");
                new WindWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ffae00");
                new DropWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("cc54ff");
                new FallWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ff007a");
                new MountainWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Color.White;
                new HeartWipe(testWipes, false).Duration = dur;
                yield return dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
            }
        }

        public override void Update()
        {
            base.Update();
            coroutine.Update();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Begin();
            Draw.Rect(-1f, -1f, 1920f, 1080f, lastColor);
            Draw.SpriteBatch.End();
            base.Render();
        }
    }
}
