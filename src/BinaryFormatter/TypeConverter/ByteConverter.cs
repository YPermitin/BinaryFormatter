﻿using System;
using System.IO;
using BinaryFormatter.Types;
using BinaryFormatter.Utils;

namespace BinaryFormatter.TypeConverter
{
    internal class ByteConverter : BaseTypeConverter<byte>
    {
        protected override void WriteObjectToStream(byte obj, Stream stream)
        {
            byte[] data = BitConverter.GetBytes(obj);
            stream.Write(data);
        }

        protected override byte ProcessDeserialize(byte[] bytes, Type sourceType, ref int offset)
        {
            return (byte)BitConverter.ToUInt16(bytes, offset);
        }

        protected override int GetTypeSize()
        {
            return sizeof (byte);
        }

        public override SerializedType Type => SerializedType.Byte;
    }
}
