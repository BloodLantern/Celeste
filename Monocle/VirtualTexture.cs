using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Monocle
{
    public class VirtualTexture : VirtualAsset
    {
        private const int ByteArraySize = 0x8_0000;
        private const int ByteArrayCheckSize = ByteArraySize - 0x20;
        internal static readonly byte[] buffer = new byte[0x400_0000];
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
                Texture.Dispose();
            Texture = null;
        }

        internal override void Reload()
        {
            Unload();

            if (string.IsNullOrEmpty(Path))
            {
                // Set the texture to be filled with 'color'
                Texture = new Texture2D(Engine.Instance.GraphicsDevice, Width, Height);
                Color[] data = new Color[Width * Height];
                for (int i = 0; i < data.Length; i++)
                    data[i] = color;
                Texture.SetData(data);
            }
            else
            {
                string extension = System.IO.Path.GetExtension(Path);
                if (extension == ".data")
                {
                    using FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path));
                    fileStream.Read(bytes, 0, ByteArraySize);

                    int width = BitConverter.ToInt32(bytes, 0);
                    int height = BitConverter.ToInt32(bytes, 4);
                    bool flag = bytes[8] == 1;

                    int bytesIndex = 9;
                    int totalSize = width * height * 4;
                    int bufferIndex = 0;
                    while (bufferIndex < totalSize)
                    {
                        int blockSize = bytes[bytesIndex] * 4;
                        if (flag)
                        {
                            byte num2 = bytes[bytesIndex + 1];
                            if (num2 > 0)
                            {
                                buffer[bufferIndex] = bytes[bytesIndex + 4];
                                buffer[bufferIndex + 1] = bytes[bytesIndex + 3];
                                buffer[bufferIndex + 2] = bytes[bytesIndex + 2];
                                buffer[bufferIndex + 3] = num2;
                                bytesIndex += 5;
                            }
                            else
                            {
                                buffer[bufferIndex] = 0;
                                buffer[bufferIndex + 1] = 0;
                                buffer[bufferIndex + 2] = 0;
                                buffer[bufferIndex + 3] = 0;
                                bytesIndex += 2;
                            }
                        }
                        else
                        {
                            buffer[bufferIndex] = bytes[bytesIndex + 3];
                            buffer[bufferIndex + 1] = bytes[bytesIndex + 2];
                            buffer[bufferIndex + 2] = bytes[bytesIndex + 1];
                            buffer[bufferIndex + 3] = byte.MaxValue;
                            bytesIndex += 4;
                        }
                        if (blockSize > 4)
                        {
                            int index3 = bufferIndex + 4;
                            for (int index4 = bufferIndex + blockSize; index3 < index4; index3 += 4)
                            {
                                buffer[index3] = buffer[bufferIndex];
                                buffer[index3 + 1] = buffer[bufferIndex + 1];
                                buffer[index3 + 2] = buffer[bufferIndex + 2];
                                buffer[index3 + 3] = buffer[bufferIndex + 3];
                            }
                        }
                        bufferIndex += blockSize;
                        if (bytesIndex > ByteArrayCheckSize)
                        {
                            int offset = ByteArraySize - bytesIndex;
                            for (int index5 = 0; index5 < offset; ++index5)
                                bytes[index5] = bytes[bytesIndex + index5];
                            fileStream.Read(bytes, offset, ByteArraySize - offset);
                            bytesIndex = 0;
                        }
                    }
                    Texture = new Texture2D(Engine.Graphics.GraphicsDevice, width, height);
                    Texture.SetData(buffer, 0, totalSize);
                }
                else if (extension == ".png")
                {
                    using (FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
                        Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, fileStream);
                    int elementCount = Texture.Width * Texture.Height;
                    Color[] data = new Color[elementCount];
                    Texture.GetData(data, 0, elementCount);
                    for (int index = 0; index < elementCount; ++index)
                    {
                        data[index].R = (byte) (data[index].R * (data[index].A / (float) byte.MaxValue));
                        data[index].G = (byte) (data[index].G * (data[index].A / (float) byte.MaxValue));
                        data[index].B = (byte) (data[index].B * (data[index].A / (float) byte.MaxValue));
                    }
                    Texture.SetData(data, 0, elementCount);
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
