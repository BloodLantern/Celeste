using System.Reflection;

namespace Monocle
{
    public class MethodHandle<T> where T : Entity
    {
        private MethodInfo info;

        public MethodHandle(string methodName) => this.info = typeof (T).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

        public void Call(T instance) => this.info.Invoke((object) instance, (object[]) null);
    }
}
