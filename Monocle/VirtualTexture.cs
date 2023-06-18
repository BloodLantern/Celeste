using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Monocle
{
    public class VirtualTexture : VirtualAsset
    {
        private const int ByteArraySize = 524288;
        private const int ByteArrayCheckSize = 524256;
        internal static readonly byte[] buffer = new byte[67108864];
        internal static readonly byte[] bytes = new byte[524288];
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
                        colorPtr[index] = color;
                }
                Texture.SetData<Color>(data);
            }
            else
            {
                string extension = System.IO.Path.GetExtension(Path);
                if (extension == ".data")
                {
                    using (FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
                    {
                        fileStream.Read(VirtualTexture.bytes, 0, 524288);
                        int startIndex = 0;
                        int int32_1 = BitConverter.ToInt32(VirtualTexture.bytes, startIndex);
                        int int32_2 = BitConverter.ToInt32(VirtualTexture.bytes, startIndex + 4);
                        bool flag = VirtualTexture.bytes[startIndex + 8] == 1;
                        int index1 = startIndex + 9;
                        int elementCount = int32_1 * int32_2 * 4;
                        int index2 = 0;
                        fixed (byte* numPtr1 = VirtualTexture.bytes)
                            fixed (byte* numPtr2 = VirtualTexture.buffer)
                            {
                                while (index2 < elementCount)
                                {
                                    int num1 = numPtr1[index1] * 4;
                                    if (flag)
                                    {
                                        byte num2 = numPtr1[index1 + 1];
                                        if (num2 > 0)
                                        {
                                            numPtr2[index2] = numPtr1[index1 + 4];
                                            numPtr2[index2 + 1] = numPtr1[index1 + 3];
                                            numPtr2[index2 + 2] = numPtr1[index1 + 2];
                                            numPtr2[index2 + 3] = num2;
                                            index1 += 5;
                                        }
                                        else
                                        {
                                            numPtr2[index2] = 0;
                                            numPtr2[index2 + 1] = 0;
                                            numPtr2[index2 + 2] = 0;
                                            numPtr2[index2 + 3] = 0;
                                            index1 += 2;
                                        }
                                    }
                                    else
                                    {
                                        numPtr2[index2] = numPtr1[index1 + 3];
                                        numPtr2[index2 + 1] = numPtr1[index1 + 2];
                                        numPtr2[index2 + 2] = numPtr1[index1 + 1];
                                        numPtr2[index2 + 3] = byte.MaxValue;
                                        index1 += 4;
                                    }
                                    if (num1 > 4)
                                    {
                                        int index3 = index2 + 4;
                                        for (int index4 = index2 + num1; index3 < index4; index3 += 4)
                                        {
                                            numPtr2[index3] = numPtr2[index2];
                                            numPtr2[index3 + 1] = numPtr2[index2 + 1];
                                            numPtr2[index3 + 2] = numPtr2[index2 + 2];
                                            numPtr2[index3 + 3] = numPtr2[index2 + 3];
                                        }
                                    }
                                    index2 += num1;
                                    if (index1 > 524256)
                                    {
                                        int offset = 524288 - index1;
                                        for (int index5 = 0; index5 < offset; ++index5)
                                            numPtr1[index5] = numPtr1[index1 + index5];
                                        fileStream.Read(VirtualTexture.bytes, offset, 524288 - offset);
                                        index1 = 0;
                                    }
                                }
                            }
                        Texture = new Texture2D(Engine.Graphics.GraphicsDevice, int32_1, int32_2);
                        Texture.SetData<byte>(VirtualTexture.buffer, 0, elementCount);
                    }
                }
                else if (extension == ".png")
                {
                    using (FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
                        Texture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, fileStream);
                    int elementCount = Texture.Width * Texture.Height;
                    Color[] data = new Color[elementCount];
                    Texture.GetData(data, 0, elementCount);
                    fixed (Color* colorPtr = data)
                    {
                        for (int index = 0; index < elementCount; ++index)
                        {
                            colorPtr[index].R = (byte) (colorPtr[index].R * (colorPtr[index].A / (double) byte.MaxValue));
                            colorPtr[index].G = (byte) (colorPtr[index].G * (colorPtr[index].A / (double) byte.MaxValue));
                            colorPtr[index].B = (byte) (colorPtr[index].B * (colorPtr[index].A / (double) byte.MaxValue));
                        }
                    }
                    Texture.SetData(data, 0, elementCount);
                }
                else if (extension == ".xnb")
                {
                    Texture = Engine.Instance.Content.Load<Texture2D>(Path.Replace(".xnb", ""));
                }
                else
                {
                    using (FileStream fileStream = File.OpenRead(System.IO.Path.Combine(Engine.ContentDirectory, Path)))
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
