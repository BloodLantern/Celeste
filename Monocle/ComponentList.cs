// Decompiled with JetBrains decompiler
// Type: Monocle.ComponentList
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monocle
{
    public class ComponentList : IEnumerable<Component>, IEnumerable
    {
        private readonly List<Component> components;
        private readonly List<Component> toAdd;
        private readonly List<Component> toRemove;
        private readonly HashSet<Component> current;
        private readonly HashSet<Component> adding;
        private readonly HashSet<Component> removing;
        private ComponentList.LockModes lockMode;

        public Entity Entity { get; internal set; }

        internal ComponentList(Entity entity)
        {
            Entity = entity;
            components = new List<Component>();
            toAdd = new List<Component>();
            toRemove = new List<Component>();
            current = new HashSet<Component>();
            adding = new HashSet<Component>();
            removing = new HashSet<Component>();
        }

        internal ComponentList.LockModes LockMode
        {
            get => lockMode;
            set
            {
                lockMode = value;
                if (toAdd.Count > 0)
                {
                    foreach (Component component in toAdd)
                    {
                        if (!current.Contains(component))
                        {
                            _ = current.Add(component);
                            components.Add(component);
                            component.Added(Entity);
                        }
                    }
                    adding.Clear();
                    toAdd.Clear();
                }
                if (toRemove.Count <= 0)
                {
                    return;
                }

                foreach (Component component in toRemove)
                {
                    if (current.Contains(component))
                    {
                        _ = current.Remove(component);
                        _ = components.Remove(component);
                        component.Removed(Entity);
                    }
                }
                removing.Clear();
                toRemove.Clear();
            }
        }

        public void Add(Component component)
        {
            switch (lockMode)
            {
                case ComponentList.LockModes.Open:
                    if (current.Contains(component))
                    {
                        break;
                    }

                    _ = current.Add(component);
                    components.Add(component);
                    component.Added(Entity);
                    break;
                case ComponentList.LockModes.Locked:
                    if (current.Contains(component) || adding.Contains(component))
                    {
                        break;
                    }

                    _ = adding.Add(component);
                    toAdd.Add(component);
                    break;
                case ComponentList.LockModes.Error:
                    throw new Exception("Cannot add or remove Entities at this time!");
            }
        }

        public void Remove(Component component)
        {
            switch (lockMode)
            {
                case ComponentList.LockModes.Open:
                    if (!current.Contains(component))
                    {
                        break;
                    }

                    _ = current.Remove(component);
                    _ = components.Remove(component);
                    component.Removed(Entity);
                    break;
                case ComponentList.LockModes.Locked:
                    if (!current.Contains(component) || removing.Contains(component))
                    {
                        break;
                    }

                    _ = removing.Add(component);
                    toRemove.Add(component);
                    break;
                case ComponentList.LockModes.Error:
                    throw new Exception("Cannot add or remove Entities at this time!");
            }
        }

        public void Add(IEnumerable<Component> components)
        {
            foreach (Component component in components)
            {
                Add(component);
            }
        }

        public void Remove(IEnumerable<Component> components)
        {
            foreach (Component component in components)
            {
                Remove(component);
            }
        }

        public void RemoveAll<T>() where T : Component
        {
            Remove(GetAll<T>());
        }

        public void Add(params Component[] components)
        {
            foreach (Component component in components)
            {
                Add(component);
            }
        }

        public void Remove(params Component[] components)
        {
            foreach (Component component in components)
            {
                Remove(component);
            }
        }

        public int Count => components.Count;

        public Component this[int index] => index < 0 || index >= components.Count ? throw new IndexOutOfRangeException() : components[index];

        public IEnumerator<Component> GetEnumerator()
        {
            return components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Component[] ToArray()
        {
            return components.ToArray<Component>();
        }

        internal void Update()
        {
            LockMode = ComponentList.LockModes.Locked;
            foreach (Component component in components)
            {
                if (component.Active)
                {
                    component.Update();
                }
            }
            LockMode = ComponentList.LockModes.Open;
        }

        internal void Render()
        {
            LockMode = ComponentList.LockModes.Error;
            foreach (Component component in components)
            {
                if (component.Visible)
                {
                    component.Render();
                }
            }
            LockMode = ComponentList.LockModes.Open;
        }

        internal void DebugRender(Camera camera)
        {
            LockMode = ComponentList.LockModes.Error;
            foreach (Component component in components)
            {
                component.DebugRender(camera);
            }

            LockMode = ComponentList.LockModes.Open;
        }

        internal void HandleGraphicsReset()
        {
            LockMode = ComponentList.LockModes.Error;
            foreach (Component component in components)
            {
                component.HandleGraphicsReset();
            }

            LockMode = ComponentList.LockModes.Open;
        }

        internal void HandleGraphicsCreate()
        {
            LockMode = ComponentList.LockModes.Error;
            foreach (Component component in components)
            {
                component.HandleGraphicsCreate();
            }

            LockMode = ComponentList.LockModes.Open;
        }

        public T Get<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component is T)
                {
                    return component as T;
                }
            }
            return default;
        }

        public IEnumerable<T> GetAll<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component is T)
                {
                    yield return component as T;
                }
            }
        }

        public enum LockModes
        {
            Open,
            Locked,
            Error,
        }
    }
}
