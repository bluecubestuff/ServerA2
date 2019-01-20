using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestPlugin
{
    public class GeneralData
    {
        public string Pos { get; set; }
        public string ItemName { get; set; }
        public string PlayerName { get; set; }
        public string FriendName { get; set; }

        public static byte[] Serialize(object customType)
        {
            var c = (GeneralData)customType;
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(c.Pos);
                    writer.Write(c.ItemName);
                    writer.Write(c.PlayerName);
                    writer.Write(c.FriendName);
                }
                return m.ToArray();
            }
        }

        public static object Deserialize(byte[] data)
        {
            var result = new GeneralData();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Pos = reader.ReadString();
                    result.ItemName = reader.ReadString();
                    result.PlayerName = reader.ReadString();
                    result.FriendName = reader.ReadString();
                }
            }
            return result;
        }
    }

    //public class MyCustomType
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }

    //    public static byte[] Serialize(object customType)
    //    {
    //        var c = (MyCustomType)customType;
    //        using (MemoryStream m = new MemoryStream())
    //        {
    //            using (BinaryWriter writer = new BinaryWriter(m))
    //            {
    //                writer.Write(c.Id);
    //                writer.Write(c.Name);
    //            }
    //            return m.ToArray();
    //        }
    //    }

    //    public static object Deserialize(byte[] data)
    //    {
    //        var result = new MyCustomType();
    //        using (MemoryStream m = new MemoryStream(data))
    //        {
    //            using (BinaryReader reader = new BinaryReader(m))
    //            {
    //                result.Id = reader.ReadInt32();
    //                result.Name = reader.ReadString();
    //            }
    //        }
    //        return result;
    //    }
    //}
}