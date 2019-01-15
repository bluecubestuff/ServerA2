using System;
using System.Collections.Generic;
using System.Text;

namespace TestPlugin
{
    public class MyCustomType
    {
        public byte Id { get; set; }

        public static object Deserialize(byte[] data)
        {
            var result = new MyCustomType();
            result.Id = data[0];
            return result;
        }

        // For a SerializeMethod, we need a byte-array as result.
        public static byte[] Serialize(object customType)
        {
            var c = (MyCustomType)customType;
            return new byte[] { c.Id };
        }
    }
}