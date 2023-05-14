// Decompiled with JetBrains decompiler
// Type: Celeste.ObjModel
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Celeste
{
    public class ObjModel : IDisposable
    {
        public List<ObjModel.Mesh> Meshes = new();
        public VertexBuffer Vertices;
        private VertexPositionTexture[] verts;

        private bool ResetVertexBuffer()
        {
            if (Vertices != null && !Vertices.IsDisposed && !Vertices.GraphicsDevice.IsDisposed)
            {
                return false;
            }

            Vertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionTexture), verts.Length, BufferUsage.None);
            Vertices.SetData<VertexPositionTexture>(verts);
            return true;
        }

        public void ReassignVertices()
        {
            if (ResetVertexBuffer())
            {
                return;
            }

            Vertices.SetData<VertexPositionTexture>(verts);
        }

        public void Draw(Effect effect)
        {
            _ = ResetVertexBuffer();
            Engine.Graphics.GraphicsDevice.SetVertexBuffer(Vertices);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.VertexCount / 3);
            }
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Meshes = null;
        }

        public static ObjModel Create(string filename)
        {
            _ = Path.GetDirectoryName(filename);
            ObjModel objModel = new();
            List<VertexPositionTexture> vertexPositionTextureList = new();
            List<Vector3> vector3List = new();
            List<Vector2> vector2List = new();
            ObjModel.Mesh mesh = null;
            if (File.Exists(filename + ".export"))
            {
                using BinaryReader binaryReader = new(File.OpenRead(filename + ".export"));
                int num1 = binaryReader.ReadInt32();
                for (int index1 = 0; index1 < num1; ++index1)
                {
                    if (mesh != null)
                    {
                        mesh.VertexCount = vertexPositionTextureList.Count - mesh.VertexStart;
                    }

                    mesh = new ObjModel.Mesh
                    {
                        Name = binaryReader.ReadString(),
                        VertexStart = vertexPositionTextureList.Count
                    };
                    objModel.Meshes.Add(mesh);
                    int num2 = binaryReader.ReadInt32();
                    for (int index2 = 0; index2 < num2; ++index2)
                    {
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        float z = binaryReader.ReadSingle();
                        vector3List.Add(new Vector3(x, y, z));
                    }
                    int num3 = binaryReader.ReadInt32();
                    for (int index3 = 0; index3 < num3; ++index3)
                    {
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        vector2List.Add(new Vector2(x, y));
                    }
                    int num4 = binaryReader.ReadInt32();
                    for (int index4 = 0; index4 < num4; ++index4)
                    {
                        int index5 = binaryReader.ReadInt32() - 1;
                        int index6 = binaryReader.ReadInt32() - 1;
                        vertexPositionTextureList.Add(new VertexPositionTexture()
                        {
                            Position = vector3List[index5],
                            TextureCoordinate = vector2List[index6]
                        });
                    }
                }
            }
            else
            {
                using StreamReader streamReader = new(filename);
                string str1;
                while ((str1 = streamReader.ReadLine()) != null)
                {
                    string[] strArray1 = str1.Split(' ');
                    if (strArray1.Length != 0)
                    {
                        string str2 = strArray1[0];
                        if (str2 == "o")
                        {
                            if (mesh != null)
                            {
                                mesh.VertexCount = vertexPositionTextureList.Count - mesh.VertexStart;
                            }

                            mesh = new ObjModel.Mesh
                            {
                                Name = strArray1[1],
                                VertexStart = vertexPositionTextureList.Count
                            };
                            objModel.Meshes.Add(mesh);
                        }
                        else if (str2 == "v")
                        {
                            Vector3 vector3 = new(ObjModel.Float(strArray1[1]), ObjModel.Float(strArray1[2]), ObjModel.Float(strArray1[3]));
                            vector3List.Add(vector3);
                        }
                        else if (str2 == "vt")
                        {
                            Vector2 vector2 = new(ObjModel.Float(strArray1[1]), ObjModel.Float(strArray1[2]));
                            vector2List.Add(vector2);
                        }
                        else if (str2 == "f")
                        {
                            for (int index = 1; index < Math.Min(4, strArray1.Length); ++index)
                            {
                                VertexPositionTexture vertexPositionTexture = new();
                                string[] strArray2 = strArray1[index].Split('/');
                                if (strArray2[0].Length > 0)
                                {
                                    vertexPositionTexture.Position = vector3List[int.Parse(strArray2[0]) - 1];
                                }

                                if (strArray2[1].Length > 0)
                                {
                                    vertexPositionTexture.TextureCoordinate = vector2List[int.Parse(strArray2[1]) - 1];
                                }

                                vertexPositionTextureList.Add(vertexPositionTexture);
                            }
                        }
                    }
                }
            }
            if (mesh != null)
            {
                mesh.VertexCount = vertexPositionTextureList.Count - mesh.VertexStart;
            }

            objModel.verts = vertexPositionTextureList.ToArray();
            _ = objModel.ResetVertexBuffer();
            return objModel;
        }

        private static float Float(string data)
        {
            return float.Parse(data, CultureInfo.InvariantCulture);
        }

        public class Mesh
        {
            public string Name = "";
            public ObjModel Model;
            public int VertexStart;
            public int VertexCount;
        }
    }
}
