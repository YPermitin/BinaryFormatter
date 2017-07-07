﻿using System;
using System.IO;
using BinaryFormatter.Types;
using BinaryFormatter.Utils;

namespace BinaryFormatter.TypeConverter
{
    internal class FloatConverter : BaseTypeConverter<float>
    {
        protected override void WriteObjectToStream(float obj, Stream stream)
        {
            byte[] data = BitConverter.GetBytes(obj);
            stream.Write(data);
        }

        protected override float ProcessDeserialize(byte[] stream, ref int offset)
        {
            return BitConverter.ToSingle(stream, offset);
        }

        protected override int GetTypeSize()
        {
            return sizeof (float);
        }

        public override SerializedType Type => SerializedType.Float;
    }
}
