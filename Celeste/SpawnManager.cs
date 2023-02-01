// Decompiled with JetBrains decompiler
// Type: Celeste.SpawnManager
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste
{
  public static class SpawnManager
  {
    public static Dictionary<string, Spawn> SpawnActions = new Dictionary<string, Spawn>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

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
