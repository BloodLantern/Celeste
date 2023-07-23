using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monocle
{
    public class EntityList : IEnumerable<Entity>, IEnumerable
    {
        private readonly List<Entity> entities;
        private readonly List<Entity> toAdd;
        private readonly List<Entity> toAwake;
        private readonly List<Entity> toRemove;
        private readonly HashSet<Entity> current;
        private readonly HashSet<Entity> adding;
        private readonly HashSet<Entity> removing;
        private bool unsorted;
        public static Comparison<Entity> CompareDepth = (a, b) => Math.Sign(b.actualDepth - a.actualDepth);

        public Scene Scene { get; private set; }

        internal EntityList(Scene scene)
        {
            Scene = scene;
            entities = new List<Entity>();
            toAdd = new List<Entity>();
            toAwake = new List<Entity>();
            toRemove = new List<Entity>();
            current = new HashSet<Entity>();
            adding = new HashSet<Entity>();
            removing = new HashSet<Entity>();
        }

        internal void MarkUnsorted() => unsorted = true;

        public void UpdateLists()
        {
            if (toAdd.Count > 0)
            {
                for (int index = 0; index < toAdd.Count; ++index)
                {
                    Entity entity = toAdd[index];
                    if (!current.Contains(entity))
                    {
                        current.Add(entity);
                        entities.Add(entity);
                        if (Scene != null)
                        {
                            Scene.TagLists.EntityAdded(entity);
                            Scene.Tracker.EntityAdded(entity);
                            entity.Added(Scene);
                        }
                    }
                }
                unsorted = true;
            }
            if (toRemove.Count > 0)
            {
                for (int index = 0; index < toRemove.Count; ++index)
                {
                    Entity entity = toRemove[index];
                    if (entities.Contains(entity))
                    {
                        current.Remove(entity);
                        entities.Remove(entity);
                        if (Scene != null)
                        {
                            entity.Removed(Scene);
                            Scene.TagLists.EntityRemoved(entity);
                            Scene.Tracker.EntityRemoved(entity);
                            Engine.Pooler.EntityRemoved(entity);
                        }
                    }
                }
                toRemove.Clear();
                removing.Clear();
            }
            if (unsorted)
            {
                unsorted = false;
                entities.Sort(EntityList.CompareDepth);
            }
            if (toAdd.Count <= 0)
                return;
            toAwake.AddRange(toAdd);
            toAdd.Clear();
            adding.Clear();
            foreach (Entity entity in toAwake)
            {
                if (entity.Scene == Scene)
                    entity.Awake(Scene);
            }
            toAwake.Clear();
        }

        public void Add(Entity entity)
        {
            if (adding.Contains(entity) || current.Contains(entity))
                return;
            adding.Add(entity);
            toAdd.Add(entity);
        }

        public void Remove(Entity entity)
        {
            if (removing.Contains(entity) || !current.Contains(entity))
                return;
            removing.Add(entity);
            toRemove.Add(entity);
        }

        public void Add(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
                Add(entity);
        }

        public void Remove(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
                Remove(entity);
        }

        public void Add(params Entity[] entities)
        {
            for (int index = 0; index < entities.Length; ++index)
                Add(entities[index]);
        }

        public void Remove(params Entity[] entities)
        {
            for (int index = 0; index < entities.Length; ++index)
                Remove(entities[index]);
        }

        public int Count => entities.Count;

        public Entity this[int index]
        {
            get
            {
                if (index < 0 || index >= entities.Count)
                    throw new IndexOutOfRangeException();
                return entities[index];
            }
        }

        public int AmountOf<T>() where T : Entity
        {
            int num = 0;
            foreach (Entity entity in entities)
            {
                if (entity is T)
                    ++num;
            }
            return num;
        }

        public T FindFirst<T>() where T : Entity
        {
            foreach (Entity entity in entities)
            {
                if (entity is T)
                    return entity as T;
            }
            return default;
        }

        public List<T> FindAll<T>() where T : Entity
        {
            List<T> all = new();
            foreach (Entity entity in entities)
            {
                if (entity is T)
                    all.Add(entity as T);
            }
            return all;
        }

        public void With<T>(Action<T> action) where T : Entity
        {
            foreach (Entity entity in entities)
            {
                if (entity is T)
                    action(entity as T);
            }
        }

        public IEnumerator<Entity> GetEnumerator() => entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Entity[] ToArray() => entities.ToArray<Entity>();

        public bool HasVisibleEntities(int matchTags)
        {
            foreach (Entity entity in entities)
            {
                if (entity.Visible && entity.TagCheck(matchTags))
                    return true;
            }
            return false;
        }

        internal void Update()
        {
            foreach (Entity entity in entities)
            {
                if (entity.Active)
                    entity.Update();
            }
        }

        public void Render()
        {
            foreach (Entity entity in entities)
            {
                if (entity.Visible)
                    entity.Render();
            }
        }

        public void RenderOnly(int matchTags)
        {
            foreach (Entity entity in entities)
            {
                if (entity.Visible && entity.TagCheck(matchTags))
                    entity.Render();
            }
        }

        public void RenderOnlyFullMatch(int matchTags)
        {
            foreach (Entity entity in entities)
            {
                if (entity.Visible && entity.TagFullCheck(matchTags))
                    entity.Render();
            }
        }

        public void RenderExcept(int excludeTags)
        {
            foreach (Entity entity in entities)
            {
                if (entity.Visible && !entity.TagCheck(excludeTags))
                    entity.Render();
            }
        }

        public void DebugRender(Camera camera)
        {
            foreach (Entity entity in entities)
                entity.DebugRender(camera);
        }

        internal void HandleGraphicsReset()
        {
            foreach (Entity entity in entities)
                entity.HandleGraphicsReset();
        }

        internal void HandleGraphicsCreate()
        {
            foreach (Entity entity in entities)
                entity.HandleGraphicsCreate();
        }
    }
}
