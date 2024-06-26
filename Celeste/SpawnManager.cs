﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste
{
    public static class SpawnManager
    {
        public static Dictionary<string, Spawn> SpawnActions = new Dictionary<string, Spawn>(StringComparer.InvariantCultureIgnoreCase);

        public static void Init()
        {
            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
            {
                if (type.GetCustomAttribute(typeof (SpawnableAttribute)) != null)
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        SpawnerAttribute customAttribute = method.GetCustomAttribute(typeof (SpawnerAttribute)) as SpawnerAttribute;
                        if (method.IsStatic && customAttribute != null)
                        {
                            string key = customAttribute.Name ?? type.Name;
                            SpawnManager.SpawnActions.Add(key, (Spawn) method.CreateDelegate(typeof (Spawn)));
                        }
                    }
                }
            }
        }
    }
}
