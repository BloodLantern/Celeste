// Decompiled with JetBrains decompiler
// Type: Celeste.Leader
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class Leader : Component
    {
        public const int MaxPastPoints = 350;
        public List<Follower> Followers = new();
        public List<Vector2> PastPoints = new();
        public Vector2 Position;
        private static List<Strawberry> storedBerries;
        private static List<Vector2> storedOffsets;

        public Leader()
            : base(true, false)
        {
        }

        public Leader(Vector2 position)
            : base(true, false)
        {
            Position = position;
        }

        public void GainFollower(Follower follower)
        {
            Followers.Add(follower);
            follower.OnGainLeaderUtil(this);
        }

        public void LoseFollower(Follower follower)
        {
            _ = Followers.Remove(follower);
            follower.OnLoseLeaderUtil();
        }

        public void LoseFollowers()
        {
            foreach (Follower follower in Followers)
            {
                follower.OnLoseLeaderUtil();
            }

            Followers.Clear();
        }

        public override void Update()
        {
            Vector2 vector2 = Entity.Position + Position;
            if (Scene.OnInterval(0.02f) && (PastPoints.Count == 0 || (double)(vector2 - PastPoints[0]).Length() >= 3.0))
            {
                PastPoints.Insert(0, vector2);
                if (PastPoints.Count > 350)
                {
                    PastPoints.RemoveAt(PastPoints.Count - 1);
                }
            }
            int index = 5;
            foreach (Follower follower in Followers)
            {
                if (index >= PastPoints.Count)
                {
                    break;
                }

                Vector2 pastPoint = PastPoints[index];
                if (follower.DelayTimer <= 0.0 && follower.MoveTowardsLeader)
                {
                    follower.Entity.Position = follower.Entity.Position + ((pastPoint - follower.Entity.Position) * (1f - (float)Math.Pow(0.0099999997764825821, (double)Engine.DeltaTime)));
                }

                index += 5;
            }
        }

        public bool HasFollower<T>()
        {
            foreach (Component follower in Followers)
            {
                if (follower.Entity is T)
                {
                    return true;
                }
            }
            return false;
        }

        public void TransferFollowers()
        {
            for (int index = 0; index < Followers.Count; ++index)
            {
                Follower follower = Followers[index];
                if (!follower.Entity.TagCheck((int)Tags.Persistent))
                {
                    LoseFollower(follower);
                    --index;
                }
            }
        }

        public static void StoreStrawberries(Leader leader)
        {
            Leader.storedBerries = new List<Strawberry>();
            Leader.storedOffsets = new List<Vector2>();
            foreach (Follower follower in leader.Followers)
            {
                if (follower.Entity is Strawberry)
                {
                    Leader.storedBerries.Add(follower.Entity as Strawberry);
                    Leader.storedOffsets.Add(follower.Entity.Position - leader.Entity.Position);
                }
            }
            foreach (Strawberry storedBerry in Leader.storedBerries)
            {
                _ = leader.Followers.Remove(storedBerry.Follower);
                storedBerry.Follower.Leader = null;
                storedBerry.AddTag((int)Tags.Global);
            }
        }

        public static void RestoreStrawberries(Leader leader)
        {
            leader.PastPoints.Clear();
            for (int index = 0; index < Leader.storedBerries.Count; ++index)
            {
                Strawberry storedBerry = Leader.storedBerries[index];
                leader.GainFollower(storedBerry.Follower);
                storedBerry.Position = leader.Entity.Position + Leader.storedOffsets[index];
                storedBerry.RemoveTag((int)Tags.Global);
            }
        }
    }
}
