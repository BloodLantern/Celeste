using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class Scene : IEnumerable<Entity>, IEnumerable
    {
        public bool Paused;
        public float TimeActive;
        public float RawTimeActive;
        private readonly Dictionary<int, double> actualDepthLookup;

        public bool Focused { get; private set; }

        public EntityList Entities { get; private set; }

        public TagLists TagLists { get; private set; }

        public RendererList RendererList { get; private set; }

        public Entity HelperEntity { get; private set; }

        public Tracker Tracker { get; private set; }

        public event Action OnEndOfFrame;

        public Scene()
        {
            Tracker = new Tracker();
            Entities = new EntityList(this);
            TagLists = new TagLists();
            RendererList = new RendererList(this);
            actualDepthLookup = new Dictionary<int, double>();
            HelperEntity = new Entity();
            Entities.Add(HelperEntity);
        }

        public virtual void Begin()
        {
            Focused = true;
            foreach (Entity entity in Entities)
                entity.SceneBegin(this);
        }

        public virtual void End()
        {
            Focused = false;
            foreach (Entity entity in Entities)
                entity.SceneEnd(this);
        }

        public virtual void BeforeUpdate()
        {
            if (!Paused)
                TimeActive += Engine.DeltaTime;
            RawTimeActive += Engine.RawDeltaTime;
            Entities.UpdateLists();
            TagLists.UpdateLists();
            RendererList.UpdateLists();
        }

        public virtual void Update()
        {
            if (Paused)
                return;
            Entities.Update();
            RendererList.Update();
        }

        public virtual void AfterUpdate()
        {
            if (OnEndOfFrame == null)
                return;
            OnEndOfFrame();
            OnEndOfFrame = null;
        }

        public virtual void BeforeRender() => RendererList.BeforeRender();

        public virtual void Render() => RendererList.Render();

        public virtual void AfterRender() => RendererList.AfterRender();

        public virtual void HandleGraphicsReset() => Entities.HandleGraphicsReset();

        public virtual void HandleGraphicsCreate() => Entities.HandleGraphicsCreate();

        public virtual void GainFocus()
        {
        }

        public virtual void LoseFocus()
        {
        }

        public bool OnInterval(float interval) => (int) ((TimeActive - (double) Engine.DeltaTime) / interval) < (int) (TimeActive / (double) interval);

        public bool OnInterval(float interval, float offset) => Math.Floor((TimeActive - (double) offset - Engine.DeltaTime) / interval) < Math.Floor((TimeActive - (double) offset) / interval);

        public bool BetweenInterval(float interval) => Calc.BetweenInterval(TimeActive, interval);

        public bool OnRawInterval(float interval) => (int) ((RawTimeActive - (double) Engine.RawDeltaTime) / interval) < (int) (RawTimeActive / (double) interval);

        public bool OnRawInterval(float interval, float offset) => Math.Floor((RawTimeActive - (double) offset - Engine.RawDeltaTime) / interval) < Math.Floor((RawTimeActive - (double) offset) / interval);

        public bool BetweenRawInterval(float interval) => Calc.BetweenInterval(RawTimeActive, interval);

        public bool CollideCheck(Vector2 point, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollidePoint(point))
                    return true;
            }
            return false;
        }

        public bool CollideCheck(Vector2 from, Vector2 to, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideLine(from, to))
                    return true;
            }
            return false;
        }

        public bool CollideCheck(Rectangle rect, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideRect(rect))
                    return true;
            }
            return false;
        }

        public bool CollideCheck(Rectangle rect, Entity entity) => entity.Collidable && entity.CollideRect(rect);

        public Entity CollideFirst(Vector2 point, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollidePoint(point))
                    return tagList[index];
            }
            return null;
        }

        public Entity CollideFirst(Vector2 from, Vector2 to, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideLine(from, to))
                    return tagList[index];
            }
            return null;
        }

        public Entity CollideFirst(Rectangle rect, int tag)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideRect(rect))
                    return tagList[index];
            }
            return null;
        }

        public void CollideInto(Vector2 point, int tag, List<Entity> hits)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollidePoint(point))
                    hits.Add(tagList[index]);
            }
        }

        public void CollideInto(Vector2 from, Vector2 to, int tag, List<Entity> hits)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideLine(from, to))
                    hits.Add(tagList[index]);
            }
        }

        public void CollideInto(Rectangle rect, int tag, List<Entity> hits)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideRect(rect))
                    tagList.Add(tagList[index]);
            }
        }

        public List<Entity> CollideAll(Vector2 point, int tag)
        {
            List<Entity> hits = new();
            CollideInto(point, tag, hits);
            return hits;
        }

        public List<Entity> CollideAll(Vector2 from, Vector2 to, int tag)
        {
            List<Entity> hits = new();
            CollideInto(from, to, tag, hits);
            return hits;
        }

        public List<Entity> CollideAll(Rectangle rect, int tag)
        {
            List<Entity> hits = new();
            CollideInto(rect, tag, hits);
            return hits;
        }

        public void CollideDo(Vector2 point, int tag, Action<Entity> action)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollidePoint(point))
                    action(tagList[index]);
            }
        }

        public void CollideDo(Vector2 from, Vector2 to, int tag, Action<Entity> action)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideLine(from, to))
                    action(tagList[index]);
            }
        }

        public void CollideDo(Rectangle rect, int tag, Action<Entity> action)
        {
            List<Entity> tagList = TagLists[tag];
            for (int index = 0; index < tagList.Count; ++index)
            {
                if (tagList[index].Collidable && tagList[index].CollideRect(rect))
                    action(tagList[index]);
            }
        }

        public Vector2 LineWalkCheck(Vector2 from, Vector2 to, int tag, float precision)
        {
            Vector2 vector2_1 = to - from;
            vector2_1.Normalize();
            Vector2 vector2_2 = vector2_1 * precision;
            int num = (int) Math.Floor((from - to).Length() / (double) precision);
            Vector2 vector2_3 = from;
            Vector2 point = from + vector2_2;
            for (int index = 0; index <= num; ++index)
            {
                if (CollideCheck(point, tag))
                    return vector2_3;
                vector2_3 = point;
                point += vector2_2;
            }
            return to;
        }

        public bool CollideCheck<T>(Vector2 point) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollidePoint(point))
                    return true;
            }
            return false;
        }

        public bool CollideCheck<T>(Vector2 from, Vector2 to) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideLine(from, to))
                    return true;
            }
            return false;
        }

        public bool CollideCheck<T>(Rectangle rect) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideRect(rect))
                    return true;
            }
            return false;
        }

        public T CollideFirst<T>(Vector2 point) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollidePoint(point))
                    return entity[index] as T;
            }
            return default (T);
        }

        public T CollideFirst<T>(Vector2 from, Vector2 to) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideLine(from, to))
                    return entity[index] as T;
            }
            return default (T);
        }

        public T CollideFirst<T>(Rectangle rect) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideRect(rect))
                    return entity[index] as T;
            }
            return default (T);
        }

        public void CollideInto<T>(Vector2 point, List<Entity> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollidePoint(point))
                    hits.Add(entity[index]);
            }
        }

        public void CollideInto<T>(Vector2 from, Vector2 to, List<Entity> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideLine(from, to))
                    hits.Add(entity[index]);
            }
        }

        public void CollideInto<T>(Rectangle rect, List<Entity> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideRect(rect))
                    entity.Add(entity[index]);
            }
        }

        public void CollideInto<T>(Vector2 point, List<T> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollidePoint(point))
                    hits.Add(entity[index] as T);
            }
        }

        public void CollideInto<T>(Vector2 from, Vector2 to, List<T> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideLine(from, to))
                    hits.Add(entity[index] as T);
            }
        }

        public void CollideInto<T>(Rectangle rect, List<T> hits) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideRect(rect))
                    hits.Add(entity[index] as T);
            }
        }

        public List<T> CollideAll<T>(Vector2 point) where T : Entity
        {
            List<T> hits = new();
            CollideInto(point, hits);
            return hits;
        }

        public List<T> CollideAll<T>(Vector2 from, Vector2 to) where T : Entity
        {
            List<T> hits = new();
            CollideInto(from, to, hits);
            return hits;
        }

        public List<T> CollideAll<T>(Rectangle rect) where T : Entity
        {
            List<T> hits = new();
            CollideInto(rect, hits);
            return hits;
        }

        public void CollideDo<T>(Vector2 point, Action<T> action) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollidePoint(point))
                    action(entity[index] as T);
            }
        }

        public void CollideDo<T>(Vector2 from, Vector2 to, Action<T> action) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideLine(from, to))
                    action(entity[index] as T);
            }
        }

        public void CollideDo<T>(Rectangle rect, Action<T> action) where T : Entity
        {
            List<Entity> entity = Tracker.Entities[typeof (T)];
            for (int index = 0; index < entity.Count; ++index)
            {
                if (entity[index].Collidable && entity[index].CollideRect(rect))
                    action(entity[index] as T);
            }
        }

        public Vector2 LineWalkCheck<T>(Vector2 from, Vector2 to, float precision) where T : Entity
        {
            Vector2 vector2_1 = to - from;
            vector2_1.Normalize();
            Vector2 vector2_2 = vector2_1 * precision;
            int num = (int) Math.Floor((from - to).Length() / (double) precision);
            Vector2 vector2_3 = from;
            Vector2 point = from + vector2_2;
            for (int index = 0; index <= num; ++index)
            {
                if (CollideCheck<T>(point))
                    return vector2_3;
                vector2_3 = point;
                point += vector2_2;
            }
            return to;
        }

        public bool CollideCheckByComponent<T>(Vector2 point) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollidePoint(point))
                    return true;
            }
            return false;
        }

        public bool CollideCheckByComponent<T>(Vector2 from, Vector2 to) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideLine(from, to))
                    return true;
            }
            return false;
        }

        public bool CollideCheckByComponent<T>(Rectangle rect) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideRect(rect))
                    return true;
            }
            return false;
        }

        public T CollideFirstByComponent<T>(Vector2 point) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollidePoint(point))
                    return component[index] as T;
            }
            return default (T);
        }

        public T CollideFirstByComponent<T>(Vector2 from, Vector2 to) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideLine(from, to))
                    return component[index] as T;
            }
            return default (T);
        }

        public T CollideFirstByComponent<T>(Rectangle rect) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideRect(rect))
                    return component[index] as T;
            }
            return default (T);
        }

        public void CollideIntoByComponent<T>(Vector2 point, List<Component> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollidePoint(point))
                    hits.Add(component[index]);
            }
        }

        public void CollideIntoByComponent<T>(Vector2 from, Vector2 to, List<Component> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideLine(from, to))
                    hits.Add(component[index]);
            }
        }

        public void CollideIntoByComponent<T>(Rectangle rect, List<Component> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideRect(rect))
                    component.Add(component[index]);
            }
        }

        public void CollideIntoByComponent<T>(Vector2 point, List<T> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollidePoint(point))
                    hits.Add(component[index] as T);
            }
        }

        public void CollideIntoByComponent<T>(Vector2 from, Vector2 to, List<T> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideLine(from, to))
                    hits.Add(component[index] as T);
            }
        }

        public void CollideIntoByComponent<T>(Rectangle rect, List<T> hits) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideRect(rect))
                    component.Add(component[index] as T);
            }
        }

        public List<T> CollideAllByComponent<T>(Vector2 point) where T : Component
        {
            List<T> hits = new();
            CollideIntoByComponent(point, hits);
            return hits;
        }

        public List<T> CollideAllByComponent<T>(Vector2 from, Vector2 to) where T : Component
        {
            List<T> hits = new();
            CollideIntoByComponent(from, to, hits);
            return hits;
        }

        public List<T> CollideAllByComponent<T>(Rectangle rect) where T : Component
        {
            List<T> hits = new();
            CollideIntoByComponent(rect, hits);
            return hits;
        }

        public void CollideDoByComponent<T>(Vector2 point, Action<T> action) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollidePoint(point))
                    action(component[index] as T);
            }
        }

        public void CollideDoByComponent<T>(Vector2 from, Vector2 to, Action<T> action) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideLine(from, to))
                    action(component[index] as T);
            }
        }

        public void CollideDoByComponent<T>(Rectangle rect, Action<T> action) where T : Component
        {
            List<Component> component = Tracker.Components[typeof (T)];
            for (int index = 0; index < component.Count; ++index)
            {
                if (component[index].Entity.Collidable && component[index].Entity.CollideRect(rect))
                    action(component[index] as T);
            }
        }

        public Vector2 LineWalkCheckByComponent<T>(Vector2 from, Vector2 to, float precision) where T : Component
        {
            Vector2 vector2_1 = to - from;
            vector2_1.Normalize();
            Vector2 vector2_2 = vector2_1 * precision;
            int num = (int) Math.Floor((from - to).Length() / (double) precision);
            Vector2 vector2_3 = from;
            Vector2 point = from + vector2_2;
            for (int index = 0; index <= num; ++index)
            {
                if (CollideCheckByComponent<T>(point))
                    return vector2_3;
                vector2_3 = point;
                point += vector2_2;
            }
            return to;
        }

        internal void SetActualDepth(Entity entity)
        {
            if (actualDepthLookup.TryGetValue(entity.depth, out double num))
                actualDepthLookup[entity.depth] += 9.9999999747524271E-07;
            else
                actualDepthLookup.Add(entity.depth, 9.9999999747524271E-07);
            entity.actualDepth = entity.depth - num;
            Entities.MarkUnsorted();
            for (int index = 0; index < BitTag.TotalTags; ++index)
            {
                if (entity.TagCheck(1 << index))
                    TagLists.MarkUnsorted(index);
            }
        }

        public T CreateAndAdd<T>() where T : Entity, new()
        {
            T andAdd = Engine.Pooler.Create<T>();
            Add(andAdd);
            return andAdd;
        }

        public List<Entity> this[BitTag tag] => TagLists[tag.ID];

        public void Add(Entity entity) => Entities.Add(entity);

        public void Remove(Entity entity) => Entities.Remove(entity);

        public void Add(IEnumerable<Entity> entities) => Entities.Add(entities);

        public void Remove(IEnumerable<Entity> entities) => Entities.Remove(entities);

        public void Add(params Entity[] entities) => Entities.Add(entities);

        public void Remove(params Entity[] entities) => Entities.Remove(entities);

        public IEnumerator<Entity> GetEnumerator() => Entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<Entity> GetEntitiesByTagMask(int mask)
        {
            List<Entity> entitiesByTagMask = new();
            foreach (Entity entity in Entities)
            {
                if ((entity.Tag & mask) != 0)
                    entitiesByTagMask.Add(entity);
            }
            return entitiesByTagMask;
        }

        public List<Entity> GetEntitiesExcludingTagMask(int mask)
        {
            List<Entity> excludingTagMask = new();
            foreach (Entity entity in Entities)
            {
                if ((entity.Tag & mask) == 0)
                    excludingTagMask.Add(entity);
            }
            return excludingTagMask;
        }

        public void Add(Renderer renderer) => RendererList.Add(renderer);

        public void Remove(Renderer renderer) => RendererList.Remove(renderer);
    }
}
