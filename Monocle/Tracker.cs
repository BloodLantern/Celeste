using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Monocle
{
    public class Tracker
    {
        public static Dictionary<Type, List<Type>> TrackedEntityTypes { get; private set; }

        public static Dictionary<Type, List<Type>> TrackedComponentTypes { get; private set; }

        public static HashSet<Type> StoredEntityTypes { get; private set; }

        public static HashSet<Type> StoredComponentTypes { get; private set; }

        public static void Initialize()
        {
            TrackedEntityTypes = new Dictionary<Type, List<Type>>();
            TrackedComponentTypes = new Dictionary<Type, List<Type>>();
            StoredEntityTypes = new HashSet<Type>();
            StoredComponentTypes = new HashSet<Type>();
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
            {
                object[] customAttributes = type.GetCustomAttributes(typeof (Tracked), false);
                if (customAttributes.Length != 0)
                {
                    bool inherited = (customAttributes[0] as Tracked).Inherited;
                    if (typeof (Entity).IsAssignableFrom(type))
                    {
                        if (!type.IsAbstract)
                        {
                            if (!TrackedEntityTypes.ContainsKey(type))
                                TrackedEntityTypes.Add(type, new List<Type>());
                            TrackedEntityTypes[type].Add(type);
                        }
                        StoredEntityTypes.Add(type);
                        if (inherited)
                        {
                            foreach (Type subclass in GetSubclasses(type))
                            {
                                if (!subclass.IsAbstract)
                                {
                                    if (!TrackedEntityTypes.ContainsKey(subclass))
                                        TrackedEntityTypes.Add(subclass, new List<Type>());
                                    TrackedEntityTypes[subclass].Add(type);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!typeof (Component).IsAssignableFrom(type))
                            throw new Exception("Type '" + type.Name + "' cannot be Tracked because it does not derive from Entity or Component");
                        if (!type.IsAbstract)
                        {
                            if (!TrackedComponentTypes.ContainsKey(type))
                                TrackedComponentTypes.Add(type, new List<Type>());
                            TrackedComponentTypes[type].Add(type);
                        }
                        StoredComponentTypes.Add(type);
                        if (inherited)
                        {
                            foreach (Type subclass in GetSubclasses(type))
                            {
                                if (!subclass.IsAbstract)
                                {
                                    if (!TrackedComponentTypes.ContainsKey(subclass))
                                        TrackedComponentTypes.Add(subclass, new List<Type>());
                                    TrackedComponentTypes[subclass].Add(type);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static List<Type> GetSubclasses(Type type)
        {
            List<Type> subclasses = new();
            foreach (Type type1 in Assembly.GetEntryAssembly().GetTypes())
            {
                if (type != type1 && type.IsAssignableFrom(type1))
                    subclasses.Add(type1);
            }
            return subclasses;
        }

        public Dictionary<Type, List<Entity>> Entities { get; private set; }

        public Dictionary<Type, List<Component>> Components { get; private set; }

        public Tracker()
        {
            Entities = new Dictionary<Type, List<Entity>>(TrackedEntityTypes.Count);
            foreach (Type storedEntityType in StoredEntityTypes)
                Entities.Add(storedEntityType, new List<Entity>());
            Components = new Dictionary<Type, List<Component>>(TrackedComponentTypes.Count);
            foreach (Type storedComponentType in StoredComponentTypes)
                Components.Add(storedComponentType, new List<Component>());
        }

        public bool IsEntityTracked<T>() where T : Entity => Entities.ContainsKey(typeof (T));

        public bool IsComponentTracked<T>() where T : Component => Components.ContainsKey(typeof (T));

        public T GetEntity<T>() where T : Entity
        {
            List<Entity> entity = Entities[typeof (T)];
            return entity.Count == 0 ? default : entity[0] as T;
        }

        public T GetNearestEntity<T>(Vector2 nearestTo) where T : Entity
        {
            List<Entity> entities = GetEntities<T>();
            T nearestEntity = default (T);
            float num1 = 0.0f;
            foreach (T obj in entities)
            {
                float num2 = Vector2.DistanceSquared(nearestTo, obj.Position);
                if (nearestEntity == null || num2 < (double) num1)
                {
                    nearestEntity = obj;
                    num1 = num2;
                }
            }
            return nearestEntity;
        }

        public List<Entity> GetEntities<T>() where T : Entity => Entities[typeof (T)];

        public List<Entity> GetEntitiesCopy<T>() where T : Entity => new(GetEntities<T>());

        public IEnumerator<T> EnumerateEntities<T>() where T : Entity
        {
            foreach (Entity entity in Entities[typeof (T)])
                yield return entity as T;
        }

        public int CountEntities<T>() where T : Entity => Entities[typeof (T)].Count;

        public T GetComponent<T>() where T : Component
        {
            List<Component> component = Components[typeof (T)];
            return component.Count == 0 ? default (T) : component[0] as T;
        }

        public T GetNearestComponent<T>(Vector2 nearestTo) where T : Component
        {
            List<Component> components = GetComponents<T>();
            T nearestComponent = default (T);
            float num1 = 0.0f;
            foreach (T obj in components)
            {
                float num2 = Vector2.DistanceSquared(nearestTo, obj.Entity.Position);
                if (nearestComponent == null || num2 < (double) num1)
                {
                    nearestComponent = obj;
                    num1 = num2;
                }
            }
            return nearestComponent;
        }

        public List<Component> GetComponents<T>() where T : Component => Components[typeof (T)];

        public List<Component> GetComponentsCopy<T>() where T : Component => new(GetComponents<T>());

        public IEnumerator<T> EnumerateComponents<T>() where T : Component
        {
            foreach (Component component in Components[typeof (T)])
                yield return component as T;
        }

        public int CountComponents<T>() where T : Component => Components[typeof (T)].Count;

        internal void EntityAdded(Entity entity)
        {
            if (!TrackedEntityTypes.TryGetValue(entity.GetType(), out List<Type> typeList))
                return;
            foreach (Type key in typeList)
                Entities[key].Add(entity);
        }

        internal void EntityRemoved(Entity entity)
        {
            if (!TrackedEntityTypes.TryGetValue(entity.GetType(), out List<Type> typeList))
                return;
            foreach (Type key in typeList)
                Entities[key].Remove(entity);
        }

        internal void ComponentAdded(Component component)
        {
            if (!TrackedComponentTypes.TryGetValue(component.GetType(), out List<Type> typeList))
                return;
            foreach (Type key in typeList)
                Components[key].Add(component);
        }

        internal void ComponentRemoved(Component component)
        {
            if (!TrackedComponentTypes.TryGetValue(component.GetType(), out List<Type> typeList))
                return;
            foreach (Type key in typeList)
                Components[key].Remove(component);
        }

        public void LogEntities()
        {
            foreach (KeyValuePair<Type, List<Entity>> entity in Entities)
                Engine.Commands.Log(entity.Key.Name + " : " + entity.Value.Count);
        }

        public void LogComponents()
        {
            foreach (KeyValuePair<Type, List<Component>> component in Components)
                Engine.Commands.Log(component.Key.Name + " : " + component.Value.Count);
        }
    }
}
