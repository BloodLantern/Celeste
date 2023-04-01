// Decompiled with JetBrains decompiler
// Type: Celeste.Billboard
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(true)]
    public class Billboard : Component
    {
        public MTexture Texture;
        public Vector3 Position;
        public Color Color = Color.White;
        public Vector2 Size = Vector2.One;
        public Vector2 Scale = Vector2.One;
        public Action BeforeRender;

        public Billboard(
            MTexture texture,
            Vector3 position,
            Vector2? size = null,
            Color? color = null,
            Vector2? scale = null)
            : base(true, true)
        {
            Texture = texture;
            Position = position;
            Size = size ?? Vector2.One;
            Color = color ?? Color.White;
            Scale = scale ?? Vector2.One;
        }
    }
}
