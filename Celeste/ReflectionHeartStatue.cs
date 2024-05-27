using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
    public class ReflectionHeartStatue : Entity
    {
        private static readonly string[] Code = new string[6]
        {
            "U",
            "L",
            "DR",
            "UR",
            "L",
            "UL"
        };
        private const string FlagPrefix = "heartTorch_";
        private List<string> currentInputs = new List<string>();
        private List<Torch> torches = new List<Torch>();
        private Vector2 offset;
        private Vector2[] nodes;
        private DashListener dashListener;
        private bool enabled;

        public ReflectionHeartStatue(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.offset = offset;
            nodes = data.Nodes;
            Depth = 8999;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (Scene as Level).Session;
            Image image1 = new Image(GFX.Game["objects/reflectionHeart/statue"]);
            image1.JustifyOrigin(0.5f, 1f);
            --image1.Origin.Y;
            Add(image1);
            List<string[]> strArrayList = new List<string[]>();
            strArrayList.Add(ReflectionHeartStatue.Code);
            strArrayList.Add(FlipCode(true, false));
            strArrayList.Add(FlipCode(false, true));
            strArrayList.Add(FlipCode(true, true));
            for (int index = 0; index < 4; ++index)
            {
                Torch torch = new Torch(session, offset + nodes[index], index, strArrayList[index]);
                Scene.Add(torch);
                torches.Add(torch);
            }
            int length = ReflectionHeartStatue.Code.Length;
            Vector2 vector2 = nodes[4] + offset - Position;
            for (int index = 0; index < length; ++index)
            {
                Image image2 = new Image(GFX.Game["objects/reflectionHeart/gem"]);
                image2.CenterOrigin();
                image2.Color = ForsakenCitySatellite.Colors[ReflectionHeartStatue.Code[index]];
                image2.Position = vector2 + new Vector2((float) ((index - (length - 1) / 2.0) * 24.0), 0.0f);
                Add(image2);
                Add(new BloomPoint(image2.Position, 0.3f, 12f));
            }
            enabled = !session.HeartGem;
            if (!enabled)
                return;
            Add(dashListener = new DashListener());
            dashListener.OnDash = dir =>
            {
                string str = "";
                if (dir.Y < 0.0)
                    str = "U";
                else if (dir.Y > 0.0)
                    str = "D";
                if (dir.X < 0.0)
                    str += "L";
                else if (dir.X > 0.0)
                    str += "R";
                int num = 0;
                if (dir.X < 0.0 && dir.Y == 0.0)
                    num = 1;
                else if (dir.X < 0.0 && dir.Y < 0.0)
                    num = 2;
                else if (dir.X == 0.0 && dir.Y < 0.0)
                    num = 3;
                else if (dir.X > 0.0 && dir.Y < 0.0)
                    num = 4;
                else if (dir.X > 0.0 && dir.Y == 0.0)
                    num = 5;
                else if (dir.X > 0.0 && dir.Y > 0.0)
                    num = 6;
                else if (dir.X == 0.0 && dir.Y > 0.0)
                    num = 7;
                else if (dir.X < 0.0 && dir.Y > 0.0)
                    num = 8;
                Player entity = Scene.Tracker.GetEntity<Player>();
                Audio.Play("event:/game/06_reflection/supersecret_dashflavour", entity != null ? entity.Position : Vector2.Zero, "dash_direction", num);
                currentInputs.Add(str);
                if (currentInputs.Count > ReflectionHeartStatue.Code.Length)
                    currentInputs.RemoveAt(0);
                foreach (Torch torch in torches)
                {
                    if (!torch.Activated && CheckCode(torch.Code))
                        torch.Activate();
                }
                CheckIfAllActivated();
            };
            CheckIfAllActivated(true);
        }

        private string[] FlipCode(bool h, bool v)
        {
            string[] strArray = new string[ReflectionHeartStatue.Code.Length];
            for (int index = 0; index < ReflectionHeartStatue.Code.Length; ++index)
            {
                string source = ReflectionHeartStatue.Code[index];
                if (h)
                    source = source.Contains('L') ? source.Replace('L', 'R') : source.Replace('R', 'L');
                if (v)
                    source = source.Contains('U') ? source.Replace('U', 'D') : source.Replace('D', 'U');
                strArray[index] = source;
            }
            return strArray;
        }

        private bool CheckCode(string[] code)
        {
            if (currentInputs.Count < code.Length)
                return false;
            for (int index = 0; index < code.Length; ++index)
            {
                if (!currentInputs[index].Equals(code[index]))
                    return false;
            }
            return true;
        }

        private void CheckIfAllActivated(bool skipActivateRoutine = false)
        {
            if (!enabled)
                return;
            bool flag = true;
            foreach (Torch torch in torches)
            {
                if (!torch.Activated)
                    flag = false;
            }
            if (!flag)
                return;
            Activate(skipActivateRoutine);
        }

        public void Activate(bool skipActivateRoutine)
        {
            enabled = false;
            if (skipActivateRoutine)
                Scene.Add(new HeartGem(Position + new Vector2(0.0f, -52f)));
            else
                Add(new Coroutine(ActivateRoutine()));
        }

        private IEnumerator ActivateRoutine()
        {
            ReflectionHeartStatue reflectionHeartStatue = this;
            yield return 0.533f;
            Audio.Play("event:/game/06_reflection/supersecret_heartappear");
            Entity dummy = new Entity(reflectionHeartStatue.Position + new Vector2(0.0f, -52f));
            dummy.Depth = 1;
            reflectionHeartStatue.Scene.Add(dummy);
            Image white = new Image(GFX.Game["collectables/heartgem/white00"]);
            white.CenterOrigin();
            white.Scale = Vector2.Zero;
            dummy.Add(white);
            BloomPoint glow = new BloomPoint(0.0f, 16f);
            dummy.Add(glow);
            List<Entity> absorbs = new List<Entity>();
            for (int i = 0; i < 20; ++i)
            {
                AbsorbOrb absorbOrb = new AbsorbOrb(reflectionHeartStatue.Position + new Vector2(0.0f, -20f), dummy);
                reflectionHeartStatue.Scene.Add(absorbOrb);
                absorbs.Add(absorbOrb);
                yield return null;
            }
            yield return 0.8f;
            float duration = 0.6f;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / duration)
            {
                white.Scale = Vector2.One * p;
                glow.Alpha = p;
                (reflectionHeartStatue.Scene as Level).Shake();
                yield return null;
            }
            foreach (Entity entity in absorbs)
                entity.RemoveSelf();
            (reflectionHeartStatue.Scene as Level).Flash(Color.White);
            reflectionHeartStatue.Scene.Remove(dummy);
            reflectionHeartStatue.Scene.Add(new HeartGem(reflectionHeartStatue.Position + new Vector2(0.0f, -52f)));
        }

        public override void Update()
        {
            if (dashListener != null && !enabled)
            {
                Remove(dashListener);
                dashListener = null;
            }
            base.Update();
        }

        public class Torch : Entity
        {
            public string[] Code;
            private Sprite sprite;
            private Session session;

            public string Flag => "heartTorch_" + Index;

            public bool Activated => session.GetFlag(Flag);

            public int Index { get; private set; }

            public Torch(Session session, Vector2 position, int index, string[] code)
                : base(position)
            {
                Index = index;
                Code = code;
                Depth = 8999;
                this.session = session;
                Image image = new Image(GFX.Game.GetAtlasSubtextures("objects/reflectionHeart/hint")[index]);
                image.CenterOrigin();
                image.Position = new Vector2(0.0f, 28f);
                Add(image);
                Add(sprite = new Sprite(GFX.Game, "objects/reflectionHeart/torch"));
                sprite.AddLoop("idle", "", 0.0f, new int[1]);
                sprite.AddLoop("lit", "", 0.08f, 1, 2, 3, 4, 5, 6);
                sprite.Play("idle");
                sprite.Origin = new Vector2(32f, 64f);
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (!Activated)
                    return;
                PlayLit();
            }

            public void Activate()
            {
                session.SetFlag(Flag);
                Alarm.Set(this, 0.2f, () =>
                {
                    Audio.Play("event:/game/06_reflection/supersecret_torch_" + (Index + 1), Position);
                    PlayLit();
                });
            }

            private void PlayLit()
            {
                sprite.Play("lit");
                sprite.SetAnimationFrame(Calc.Random.Next(sprite.CurrentAnimationTotalFrames));
                Add(new VertexLight(Color.LightSeaGreen, 1f, 24, 48));
                Add(new BloomPoint(0.6f, 16f));
            }
        }
    }
}
