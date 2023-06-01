// Decompiled with JetBrains decompiler
// Type: Monocle.QuaternionExt
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Monocle
{
    public static class QuaternionExt
    {
        public static Quaternion Conjugated(this Quaternion q)
        {
            Quaternion quaternion = q;
            quaternion.Conjugate();
            return quaternion;
        }

        public static Quaternion LookAt(
            this Quaternion q,
            Vector3 from,
            Vector3 to,
            Vector3 up)
        {
            return Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(from, to, up));
        }

        public static Quaternion LookAt(this Quaternion q, Vector3 direction, Vector3 up) => Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, direction, up));

        public static Vector3 Forward(this Quaternion q) => Vector3.Transform(Vector3.Forward, q.Conjugated());

        public static Vector3 Left(this Quaternion q) => Vector3.Transform(Vector3.Left, q.Conjugated());

        public static Vector3 Up(this Quaternion q) => Vector3.Transform(Vector3.Up, q.Conjugated());
    }
}
