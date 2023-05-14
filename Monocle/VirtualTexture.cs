// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualTexture
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Monocle
{
    public class VirtualTexture : VirtualAsset
    {
        private const int ByteArraySize = 0x80000;
        private const int ByteArrayCheckSize = ByteArraySize - 0x20;
        internal static readonly byte[] buffer = new byte[0x4000000];
        internal static readonly byte[] bytes = new byte[ByteArraySize];
        public Texture2D Texture;
        private Color color;

        public string Path { get; private set; }

        public bool IsDisposed => Texture == null || Texture.IsDisposed || Texture.GraphicsDevice.IsDisposed;

        internal VirtualTexture(string path)
        {
            Name = Path = path;
            Reload();
        }

        internal VirtualTexture(string name, int width, int height, Color color)
        {
            Name = name;
            Width = width;
            Height = height;
            this.color = color;
            Reload();
        }

        internal override void Unload()
        {
            if (Texture != null && !Texture.IsDisposed)
            {
                Texture.Dispose();
            }

            Texture = null;
        }

        internal override unsafe void Reload()
        {
            Unload();
            if (string.IsNullOrEmpty(Path))
            {
                Texture = new Texture2D(Engine.Instance.GraphicsDevice, Width, Height);
                Color[] data = new Color[Width * Height];
                fixed (Color* colorPtr = data)
                {
                    for (int index = 0; index < data.Length; ++index)
                    {
                        colorPtr[index] = color;
                    }
                }
                Texture.SetData(data);
            }
            else
            {
                string extension = System.IO.Path.GetExtension(Path);
                if (extension == ".data")
                {
                    using FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path));
                    _ = fileStream.Read(bytes, 0, ByteArraySize);
                    int startIndex = 0;
                    int width = BitConverter.ToInt32(bytes, startIndex);
                    int height = BitConverter.ToInt32(bytes, startIndex + 4);
                    bool flag = bytes[startIndex + 8] == 1;
                    int index1 = startIndex + 9;
                    int elementCount = width * height * 4; // * 4 because of RGBA
                    fixed (byte* bytesPtr = bytes)
                    fixed (byte* bufferPtr = buffer)
                    {
                        int num1 = bytesPtr[index1] * 4;
                        for (int elementIndex = 0; elementIndex < elementCount; elementIndex += num1)
                        {
                            if (flag)
                            {
                                byte num2 = bytesPtr[index1 + 1];
                                if (num2 > 0)
                                {
                                    bufferPtr[elementIndex] = bytesPtr[index1 + 4];
                                    bufferPtr[elementIndex + 1] = bytesPtr[index1 + 3];
                                    bufferPtr[elementIndex + 2] = bytesPtr[index1 + 2];
                                    bufferPtr[elementIndex + 3] = num2;
                                    index1 += 5;
                                }
                                else
                                {
                                    bufferPtr[elementIndex] = 0;
                                    bufferPtr[elementIndex + 1] = 0;
                                    bufferPtr[elementIndex + 2] = 0;
                                    bufferPtr[elementIndex + 3] = 0;
                                    index1 += 2;
                                }
                            }
                            else
                            {
                                bufferPtr[elementIndex] = bytesPtr[index1 + 3];
                                bufferPtr[elementIndex + 1] = bytesPtr[index1 + 2];
                                bufferPtr[elementIndex + 2] = bytesPtr[index1 + 1];
                                bufferPtr[elementIndex + 3] = byte.MaxValue;
                                index1 += 4;
                            }
                            if (num1 > 4)
                            {
                                int index3 = elementIndex + 4;
                                for (int index4 = elementIndex + num1; index3 < index4; index3 += 4)
                                {
                                    bufferPtr[index3] = bufferPtr[elementIndex];
                                    bufferPtr[index3 + 1] = bufferPtr[elementIndex + 1];
                                    bufferPtr[index3 + 2] = bufferPtr[elementIndex + 2];
                                    bufferPtr[index3 + 3] = bufferPtr[elementIndex + 3];
                                }
                            }
                            if (index1 > ByteArrayCheckSize)
                            {
                                int offset = ByteArraySize - index1;
                                for (int index5 = 0; index5 < offset; ++index5)
                                {
                                    bytesPtr[index5] = bytesPtr[index1 + index5];
                                }

                                _ = fileStream.Read(bytes, offset, ByteArraySize - offset);
                                index1 = 0;
                            }
                        }
                    }
                    Texture = new Texture2D(Engine.Graphics.GraphicsDevice, width, height);
                    Texture.SetData<byte>(buffer, 0, elementCount);
                }
                else if (extension == ".png")
                {
                    using (FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
                    {
                        Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, fileStream);
                    }

                    int elementCount = Texture.Width * Texture.Height;
                    Color[] data = new Color[elementCount];
                    Texture.GetData<Color>(data, 0, elementCount);
                    fixed (Color* colorPtr = data)
                    {
                        for (int index = 0; index < elementCount; ++index)
                        {
                            colorPtr[index].R = (byte)(colorPtr[index].R * (colorPtr[index].A / (double)byte.MaxValue));
                            colorPtr[index].G = (byte)(colorPtr[index].G * (colorPtr[index].A / (double)byte.MaxValue));
                            colorPtr[index].B = (byte)(colorPtr[index].B * (colorPtr[index].A / (double)byte.MaxValue));
                        }
                    }
                    Texture.SetData<Color>(data, 0, elementCount);
                }
                else if (extension == ".xnb")
                {
                    Texture = Engine.Instance.Content.Load<Texture2D>(Path.Replace(".xnb", ""));
                }
                else
                {
                    using FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path));
                    Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, fileStream);
                }
                Width = Texture.Width;
                Height = Texture.Height;
            }
        }

        public override void Dispose()
        {
            Unload();
            Texture = null;
            VirtualContent.Remove(this);
        }
    }
}
