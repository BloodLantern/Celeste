﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class DetachStrawberryTrigger : Trigger
    {
        public Vector2 Target;
        public bool Global;

        public DetachStrawberryTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Vector2[] vector2Array = data.NodesOffset(offset);
            if (vector2Array.Length != 0)
                Target = vector2Array[0];
            Global = data.Bool("global", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            for (int index = player.Leader.Followers.Count - 1; index >= 0; --index)
            {
                if (player.Leader.Followers[index].Entity is Strawberry)
                    Add(new Coroutine(DetatchFollower(player.Leader.Followers[index])));
            }
        }

        private IEnumerator DetatchFollower(Follower follower)
        {
            Leader leader = follower.Leader;
            Entity entity = follower.Entity;
            float time = (entity.Position - Target).Length() / 200f;
            if (entity is Strawberry strawberry)
                strawberry.ReturnHomeWhenLost = false;
            Follower follower1 = follower;
            leader.LoseFollower(follower1);
            entity.Active = false;
            entity.Collidable = false;
            if (Global)
            {
                entity.AddTag((int) Tags.Global);
                follower.OnGainLeader += () => entity.RemoveTag((int) Tags.Global);
            }
            else
                entity.AddTag((int) Tags.Persistent);
            Audio.Play("event:/new_content/game/10_farewell/strawberry_gold_detach", entity.Position);
            Vector2 position = entity.Position;
            SimpleCurve curve = new SimpleCurve(position, Target, position + (Target - position) * 0.5f + new Vector2(0.0f, -64f));
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / time)
            {
                entity.Position = curve.GetPoint(Ease.CubeInOut(p));
                yield return null;
            }
            entity.Active = true;
            entity.Collidable = true;
        }
    }
}
