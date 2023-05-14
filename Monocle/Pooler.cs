// Decompiled with JetBrains decompiler
// Type: Monocle.Pooler
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Monocle
{
    public class Pooler
    {
        internal Dictionary<Type, Queue<Entity>> Pools { get; private set; }

        public Pooler()
        {
            Pools = new Dictionary<Type, Queue<Entity>>();
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
            {
                if (type.GetCustomAttributes(typeof(Pooled), false).Length != 0)
                {
                    if (!typeof(Entity).IsAssignableFrom(type))
                    {
                        throw new Exception("Type '" + type.Name + "' cannot be Pooled because it doesn't derive from Entity");
                    }

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                    {
                        throw new Exception("Type '" + type.Name + "' cannot be Pooled because it doesn't have a parameterless constructor");
                    }

                    Pools.Add(type, new Queue<Entity>());
                }
            }
        }

        public T Create<T>() where T : Entity, new()
        {
            if (!Pools.ContainsKey(typeof(T)))
            {
                return new T();
            }

            Queue<Entity> pool = Pools[typeof(T)];
            return pool.Count == 0 ? new T() : pool.Dequeue() as T;
        }

        internal void EntityRemoved(Entity entity)
        {
            Type type = entity.GetType();
            if (!Pools.ContainsKey(type))
            {
                return;
            }

            Pools[type].Enqueue(entity);
        }

        public void Log()
        {
            if (Pools.Count == 0)
            {
                Engine.Commands.Log("No Entity types are marked as Pooled!");
            }

            foreach (KeyValuePair<Type, Queue<Entity>> pool in Pools)
            {
                Engine.Commands.Log(pool.Key.Name + " : " + pool.Value.Count);
            }
        }
    }
}
