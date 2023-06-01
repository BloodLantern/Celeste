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

        public bool HasLeader => this.Leader != null;

        public Follower(Action onGainLeader = null, Action onLoseLeader = null)
            : base(true, false)
        {
            this.OnGainLeader = onGainLeader;
            this.OnLoseLeader = onLoseLeader;
        }

        public Follower(EntityID entityID, Action onGainLeader = null, Action onLoseLeader = null)
            : base(true, false)
        {
            this.ParentEntityID = entityID;
            this.OnGainLeader = onGainLeader;
            this.OnLoseLeader = onLoseLeader;
        }

        public override void Update()
        {
            base.Update();
            if ((double) this.DelayTimer <= 0.0)
                return;
            this.DelayTimer -= Engine.DeltaTime;
        }

        public void OnLoseLeaderUtil()
        {
            if (this.PersistentFollow)
                this.Entity.RemoveTag((int) Tags.Persistent);
            this.Leader = (Leader) null;
            if (this.OnLoseLeader == null)
                return;
            this.OnLoseLeader();
        }

        public void OnGainLeaderUtil(Leader leader)
        {
            if (this.PersistentFollow)
                this.Entity.AddTag((int) Tags.Persistent);
            this.Leader = leader;
            this.DelayTimer = this.FollowDelay;
            if (this.OnGainLeader == null)
                return;
            this.OnGainLeader();
        }

        public int FollowIndex => this.Leader == null ? -1 : this.Leader.Followers.IndexOf(this);
    }
}
