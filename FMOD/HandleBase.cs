using System;

namespace FMOD
{
    public class HandleBase
    {
        protected IntPtr rawPtr;

        public HandleBase(IntPtr newPtr) => this.rawPtr = newPtr;

        public bool isValid() => this.rawPtr != IntPtr.Zero;

        public IntPtr getRaw() => this.rawPtr;

        public override bool Equals(object obj) => this.Equals(obj as HandleBase);

        public bool Equals(HandleBase p) => (object) p != null && this.rawPtr == p.rawPtr;

        public override int GetHashCode() => this.rawPtr.ToInt32();

        public static bool operator ==(HandleBase a, HandleBase b)
        {
            if ((object) a == (object) b)
                return true;
            return (object) a != null && (object) b != null && a.rawPtr == b.rawPtr;
        }

        public static bool operator !=(HandleBase a, HandleBase b) => !(a == b);
    }
}
