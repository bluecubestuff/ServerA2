using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestPlugin
{
    public class MyCustomType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static byte[] Serialize(object customType)
        {
            var c = (MyCustomType)customType;
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(c.Id);
                    writer.Write(c.Name);
                }
                return m.ToArray();
            }
        }

        public static object Deserialize(byte[] data)
        {
            var result = new MyCustomType();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Id = reader.ReadInt32();
                    result.Name = reader.ReadString();
                }
            }
            return result;
        }

        //public byte Id { get; set; }

        //public static object Deserialize(byte[] data)
        //{
        //    var result = new MyCustomType();
        //    result.Id = data[0];
        //    return result;
        //}

        //// For a SerializeMethod, we need a byte-array as result.
        //public static byte[] Serialize(object customType)
        //{
        //    var c = (MyCustomType)customType;
        //    return new byte[] { c.Id };
        //}
    }
}