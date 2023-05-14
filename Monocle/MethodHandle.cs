// Decompiled with JetBrains decompiler
// Type: Monocle.MethodHandle`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Reflection;

namespace Monocle
{
    public class MethodHandle<T> where T : Entity
    {
        private readonly MethodInfo info;

        public MethodHandle(string methodName)
        {
            info = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);
        }

        public void Call(T instance)
        {
            _ = info.Invoke(instance, null);
        }
    }
}
