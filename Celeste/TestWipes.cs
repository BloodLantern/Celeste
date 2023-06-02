using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class TestWipes : Scene
    {
        private Coroutine coroutine;
        private Color lastColor = Color.White;

        public TestWipes() => this.coroutine = new Coroutine(this.routine());

        private IEnumerator routine()
        {
            TestWipes testWipes = this;
            float dur = 1f;
            yield return (object) 1f;
            while (true)
            {
                ScreenWipe.WipeColor = Color.Black;
                new CurtainWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ff0034");
                new AngledWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("0b0960");
                new DreamWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("39bf00");
                new KeyDoorWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("4376b3");
                new WindWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ffae00");
                new DropWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("cc54ff");
                new FallWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Calc.HexToColor("ff007a");
                new MountainWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
                ScreenWipe.WipeColor = Color.White;
                new HeartWipe((Scene) testWipes, false).Duration = dur;
                yield return (object) dur;
                testWipes.lastColor = ScreenWipe.WipeColor;
            }
        }

        public override void Update()
        {
            base.Update();
            this.coroutine.Update();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Begin();
            Draw.Rect(-1f, -1f, 1920f, 1080f, this.lastColor);
            Draw.SpriteBatch.End();
            base.Render();
        }
    }
}
