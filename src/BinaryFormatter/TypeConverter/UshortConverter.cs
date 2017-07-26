﻿using System;
using System.IO;
using BinaryFormatter.Types;
using BinaryFormatter.Utils;

namespace BinaryFormatter.TypeConverter
{
    internal class UShortConverter : BaseTypeConverter<ushort>
    {
        protected override void WriteObjectToStream(ushort obj, Stream stream)
        {
            byte[] data = BitConverter.GetBytes(obj);
            stream.Write(data);
        }

        protected override ushort ProcessDeserialize(byte[] bytes, Type sourceType, ref int offset)
        {
            return BitConverter.ToUInt16(bytes, offset);
        }

        protected override int GetTypeSize()
        {
            return sizeof (ushort);
        }

        public override SerializedType Type => SerializedType.UShort;
    }
}
