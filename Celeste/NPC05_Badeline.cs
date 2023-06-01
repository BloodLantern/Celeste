// Decompiled with JetBrains decompiler
// Type: Celeste.NPC05_Badeline
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC05_Badeline : NPC
    {
        public const string FirstLevel = "c-00";
        public const string SecondLevel = "c-01";
        public const string ThirdLevel = "c-01b";
        private BadelineDummy shadow;
        private readonly Vector2[] nodes;
        private Rectangle levelBounds;
        private readonly SoundSource moveSfx;

        public NPC05_Badeline(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            nodes = data.NodesOffset(offset);
            Add(moveSfx = new SoundSource());
            Add(new TransitionListener()
            {
                OnOut = f =>
                {
                    if (shadow == null)
                    {
                        return;
                    }

                    shadow.Hair.Alpha = 1f - Math.Min(1f, f * 2f);
                    shadow.Sprite.Color = Color.White * shadow.Hair.Alpha;
                    shadow.Light.Alpha = shadow.Hair.Alpha;
                }
            });
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Session.Level.Equals("c-00"))
            {
                if (!Session.GetLevelFlag("c-01"))
                {
                    scene.Add(shadow = new BadelineDummy(Position));
                    shadow.Depth = -1000000;
                    Add(new Coroutine(FirstScene()));
                }
                else
                {
                    RemoveSelf();
                }
            }
            else if (Session.Level.Equals("c-01"))
            {
                if (!Session.GetLevelFlag("c-01b"))
                {
                    int num = 0;
                    while (num < 4 && Session.GetFlag(CS05_Badeline.GetFlag(num)))
                    {
                        ++num;
                    }

                    if (num >= 4)
                    {
                        RemoveSelf();
                    }
                    else
                    {
                        Vector2 position = Position;
                        if (num > 0)
                        {
                            position = nodes[num - 1];
                        }

                        scene.Add(shadow = new BadelineDummy(position));
                        shadow.Depth = -1000000;
                        Add(new Coroutine(SecondScene(num)));
                    }
                }
                else
                {
                    RemoveSelf();
                }
            }
            levelBounds = (scene as Level).Bounds;
        }

        private IEnumerator FirstScene()
        {
            NPC05_Badeline npC05Badeline = this;
            npC05Badeline.shadow.Sprite.Scale.X = -1f;
            npC05Badeline.shadow.FloatSpeed = 150f;
            bool playerHasFallen = false;
            bool startedMusic = false;
            Rectangle bounds;
            Player player;
            while (true)
            {
                player = npC05Badeline.Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    double y = (double)player.Y;
                    bounds = npC05Badeline.Level.Bounds;
                    double num = bounds.Top + 180;
                    if (y > num && !player.OnGround() && !playerHasFallen)
                    {
                        player.StateMachine.State = 20;
                        playerHasFallen = true;
                    }
                }
                if (player != null & playerHasFallen && !startedMusic && player.OnGround())
                {
                    npC05Badeline.Level.Session.Audio.Music.Event = "event:/music/lvl5/middle_temple";
                    npC05Badeline.Level.Session.Audio.Apply();
                    startedMusic = true;
                }
                if (player == null || (double)player.X <= (double)npC05Badeline.X - 64.0 || (double)player.Y <= (double)npC05Badeline.Y - 32.0)
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            npC05Badeline.MoveToNode(0, false);
            while (true)
            {
                do
                {
                    double x = (double)npC05Badeline.shadow.X;
                    bounds = npC05Badeline.Level.Bounds;
                    double num = bounds.Right + 8;
                    if (x < num)
                    {
                        yield return null;
                    }
                    else
                    {
                        goto label_12;
                    }
                }
                while ((double)player.X <= (double)npC05Badeline.shadow.X - 24.0);
                npC05Badeline.shadow.X = player.X + 24f;
            }
        label_12:
            npC05Badeline.Scene.Remove(npC05Badeline.shadow);
            npC05Badeline.RemoveSelf();
        }

        private IEnumerator SecondScene(int startIndex)
        {
            NPC05_Badeline npc = this;
            npc.shadow.Sprite.Scale.X = -1f;
            npc.shadow.FloatSpeed = 300f;
            npc.shadow.FloatAccel = 400f;
            yield return 0.1f;
            int index = startIndex;
            while (index < npc.nodes.Length)
            {
                Player player = npc.Scene.Tracker.GetEntity<Player>();
                while (player == null || (double)(player.Position - npc.shadow.Position).Length() > 70.0)
                {
                    yield return null;
                }

                if (index < 4 && !npc.Session.GetFlag(CS05_Badeline.GetFlag(index)))
                {
                    CS05_Badeline cutscene = new(player, npc, npc.shadow, index);
                    npc.Scene.Add(cutscene);
                    yield return null;
                    while (cutscene.Scene != null)
                    {
                        yield return null;
                    }

                    ++index;
                }
            }
            npc.Tag |= (int)Tags.TransitionUpdate;
            npc.shadow.Tag |= (int)Tags.TransitionUpdate;
            npc.Scene.Remove(npc.shadow);
            npc.RemoveSelf();
        }

        public void MoveToNode(int index, bool chatMove = true)
        {
            if (chatMove)
                moveSfx.Play("event:/char/badeline/temple_move_chats");
            else
                SoundEmitter.Play("event:/char/badeline/temple_move_first", this);

            Vector2 start = shadow.Position;
            Vector2 end = nodes[index];
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.5f, true);
            tween.OnUpdate = t =>
            {
                shadow.Position = Vector2.Lerp(start, end, t.Eased);
                if (Scene.OnInterval(0.03f))
                {
                    SceneAs<Level>().ParticlesFG.Emit(BadelineOldsite.P_Vanish, 2, shadow.Position + new Vector2(0.0f, -6f), Vector2.One * 2f);
                }

                if ((double)t.Eased < 0.10000000149011612 || (double)t.Eased > 0.89999997615814209 || !Scene.OnInterval(0.05f))
                {
                    return;
                }

                TrailManager.Add(shadow, Color.Red, 0.5f);
            };
            Add(tween);
        }

        public void SnapToNode(int index)
        {
            shadow.Position = nodes[index];
        }
    }
}
