﻿using System;
using System.IO;
using BinaryFormatter.Types;
using BinaryFormatter.Utils;

namespace BinaryFormatter.TypeConverter
{
    internal class ByteArrayConverter : BaseTypeConverter<byte[]>
    {
        private int Size { get; set; }

        protected override void WriteObjectToStream(byte[] obj, Stream stream)
        {
            Size = obj.Length;

            stream.WriteWithLengthPrefix(obj);
        }

        protected override byte[] ProcessDeserialize(byte[] stream, ref int offset)
        {
            int size = BitConverter.ToInt32(stream, offset);
            offset += sizeof(int);

            byte[] deserialized = new byte[size];
            Array.Copy(stream, offset, deserialized, 0, size);
            return deserialized;
        }

        protected override int GetTypeSize()
        {
            return Size;
        }

        public override SerializedType Type => SerializedType.ByteArray;
    }
}
