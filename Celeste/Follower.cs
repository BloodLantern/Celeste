// Decompiled with JetBrains decompiler
// Type: Celeste.Follower
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    public class Follower : Component
    {
        public EntityID ParentEntityID;
        public Leader Leader;
        public Action OnGainLeader;
        public Action OnLoseLeader;
        public bool PersistentFollow = true;
        public float FollowDelay = 0.5f;
        public float DelayTimer;
        public bool MoveTowardsLeader = true;

        public bool HasLeader => Leader != null;

        public Follower(Action onGainLeader = null, Action onLoseLeader = null)
            : base(true, false)
        {
            OnGainLeader = onGainLeader;
            OnLoseLeader = onLoseLeader;
        }

        public Follower(EntityID entityID, Action onGainLeader = null, Action onLoseLeader = null)
            : base(true, false)
        {
            ParentEntityID = entityID;
            OnGainLeader = onGainLeader;
            OnLoseLeader = onLoseLeader;
        }

        public override void Update()
        {
            base.Update();
            if (DelayTimer <= 0.0)
            {
                return;
            }

            DelayTimer -= Engine.DeltaTime;
        }

        public void OnLoseLeaderUtil()
        {
            if (PersistentFollow)
            {
                Entity.RemoveTag((int)Tags.Persistent);
            }

            Leader = null;
            if (OnLoseLeader == null)
            {
                return;
            }

            OnLoseLeader();
        }

        public void OnGainLeaderUtil(Leader leader)
        {
            if (PersistentFollow)
            {
                Entity.AddTag((int)Tags.Persistent);
            }

            Leader = leader;
            DelayTimer = FollowDelay;
            if (OnGainLeader == null)
            {
                return;
            }

            OnGainLeader();
        }

        public int FollowIndex => Leader == null ? -1 : Leader.Followers.IndexOf(this);
    }
}
