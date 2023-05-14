// Decompiled with JetBrains decompiler
// Type: Celeste.PlaybackData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.IO;

namespace Celeste
{
    public static class PlaybackData
    {
        public static Dictionary<string, List<Player.ChaserState>> Tutorials = new();

        public static void Load()
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Tutorials")))
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(file);
                List<Player.ChaserState> chaserStateList = PlaybackData.Import(File.ReadAllBytes(file));
                PlaybackData.Tutorials[withoutExtension] = chaserStateList;
            }
        }

        public static void Export(List<Player.ChaserState> list, string path)
        {
            float timeStamp = list[0].TimeStamp;
            Vector2 position = list[0].Position;
            using BinaryWriter binaryWriter = new(File.OpenWrite(path));
            binaryWriter.Write("TIMELINE");
            binaryWriter.Write(2);
            binaryWriter.Write(list.Count);
            foreach (Player.ChaserState chaserState in list)
            {
                binaryWriter.Write(chaserState.Position.X - position.X);
                binaryWriter.Write(chaserState.Position.Y - position.Y);
                binaryWriter.Write(chaserState.TimeStamp - timeStamp);
                binaryWriter.Write(chaserState.Animation);
                binaryWriter.Write((int)chaserState.Facing);
                binaryWriter.Write(chaserState.OnGround);
                binaryWriter.Write(chaserState.HairColor.R);
                binaryWriter.Write(chaserState.HairColor.G);
                binaryWriter.Write(chaserState.HairColor.B);
                binaryWriter.Write(chaserState.Depth);
                binaryWriter.Write(chaserState.Scale.X);
                binaryWriter.Write(chaserState.Scale.Y);
                binaryWriter.Write(chaserState.DashDirection.X);
                binaryWriter.Write(chaserState.DashDirection.Y);
            }
        }

        public static List<Player.ChaserState> Import(byte[] buffer)
        {
            List<Player.ChaserState> chaserStateList = new();
            using (BinaryReader binaryReader = new(new MemoryStream(buffer)))
            {
                int num1 = 1;
                if (binaryReader.ReadString() == "TIMELINE")
                {
                    num1 = binaryReader.ReadInt32();
                }
                else
                {
                    _ = binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                }

                int num2 = binaryReader.ReadInt32();
                for (int index = 0; index < num2; ++index)
                {
                    Player.ChaserState chaserState = new();
                    chaserState.Position.X = binaryReader.ReadSingle();
                    chaserState.Position.Y = binaryReader.ReadSingle();
                    chaserState.TimeStamp = binaryReader.ReadSingle();
                    chaserState.Animation = binaryReader.ReadString();
                    chaserState.Facing = (Facings)binaryReader.ReadInt32();
                    chaserState.OnGround = binaryReader.ReadBoolean();
                    chaserState.HairColor = new Color(binaryReader.ReadByte(), binaryReader.ReadByte(), binaryReader.ReadByte(), byte.MaxValue);
                    chaserState.Depth = binaryReader.ReadInt32();
                    chaserState.Sounds = 0;
                    if (num1 == 1)
                    {
                        chaserState.Scale = new Vector2((float)chaserState.Facing, 1f);
                        chaserState.DashDirection = Vector2.Zero;
                    }
                    else
                    {
                        chaserState.Scale.X = binaryReader.ReadSingle();
                        chaserState.Scale.Y = binaryReader.ReadSingle();
                        chaserState.DashDirection.X = binaryReader.ReadSingle();
                        chaserState.DashDirection.Y = binaryReader.ReadSingle();
                    }
                    chaserStateList.Add(chaserState);
                }
            }
            return chaserStateList;
        }
    }
}
