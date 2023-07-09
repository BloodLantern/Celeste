﻿using System;
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
        private LockModes lockMode;

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

        internal LockModes LockMode
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
                            current.Add(component);
                            components.Add(component);
                            component.Added(Entity);
                        }
                    }
                    adding.Clear();
                    toAdd.Clear();
                }
                if (toRemove.Count <= 0)
                    return;
                foreach (Component component in toRemove)
                {
                    if (current.Contains(component))
                    {
                        current.Remove(component);
                        components.Remove(component);
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
                case LockModes.Open:
                    if (current.Contains(component))
                        break;
                    current.Add(component);
                    components.Add(component);
                    component.Added(Entity);
                    break;
                case LockModes.Locked:
                    if (current.Contains(component) || adding.Contains(component))
                        break;
                    adding.Add(component);
                    toAdd.Add(component);
                    break;
                case LockModes.Error:
                    throw new Exception("Cannot add or remove Entities at this time!");
            }
        }

        public void Remove(Component component)
        {
            switch (lockMode)
            {
                case LockModes.Open:
                    if (!current.Contains(component))
                        break;
                    current.Remove(component);
                    components.Remove(component);
                    component.Removed(Entity);
                    break;
                case LockModes.Locked:
                    if (!current.Contains(component) || removing.Contains(component))
                        break;
                    removing.Add(component);
                    toRemove.Add(component);
                    break;
                case LockModes.Error:
                    throw new Exception("Cannot add or remove Entities at this time!");
            }
        }

        public void Add(IEnumerable<Component> components)
        {
            foreach (Component component in components)
                Add(component);
        }

        public void Remove(IEnumerable<Component> components)
        {
            foreach (Component component in components)
                Remove(component);
        }

        public void RemoveAll<T>() where T : Component => Remove(GetAll<T>());

        public void Add(params Component[] components)
        {
            foreach (Component component in components)
                Add(component);
        }

        public void Remove(params Component[] components)
        {
            foreach (Component component in components)
                Remove(component);
        }

        public int Count => components.Count;

        public Component this[int index]
        {
            get
            {
                if (index < 0 || index >= components.Count)
                    throw new IndexOutOfRangeException();
                return components[index];
            }
        }

        public IEnumerator<Component> GetEnumerator() => components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Component[] ToArray() => components.ToArray<Component>();

        internal void Update()
        {
            LockMode = LockModes.Locked;
            foreach (Component component in components)
            {
                if (component.Active)
                    component.Update();
            }
            LockMode = LockModes.Open;
        }

        internal void Render()
        {
            LockMode = LockModes.Error;
            foreach (Component component in components)
            {
                if (component.Visible)
                    component.Render();
            }
            LockMode = LockModes.Open;
        }

        internal void DebugRender(Camera camera)
        {
            LockMode = LockModes.Error;
            foreach (Component component in components)
                component.DebugRender(camera);
            LockMode = LockModes.Open;
        }

        internal void HandleGraphicsReset()
        {
            LockMode = LockModes.Error;
            foreach (Component component in components)
                component.HandleGraphicsReset();
            LockMode = LockModes.Open;
        }

        internal void HandleGraphicsCreate()
        {
            LockMode = LockModes.Error;
            foreach (Component component in components)
                component.HandleGraphicsCreate();
            LockMode = LockModes.Open;
        }

        public T Get<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component is T)
                    return component as T;
            }
            return default (T);
        }

        public IEnumerable<T> GetAll<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component is T)
                    yield return component as T;
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
