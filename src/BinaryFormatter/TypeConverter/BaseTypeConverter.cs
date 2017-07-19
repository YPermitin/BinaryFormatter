﻿using System;
using System.IO;
using BinaryFormatter.Types;
using BinaryFormatter.Utils;
using System.Text;

namespace BinaryFormatter.TypeConverter
{
    internal abstract class BaseTypeConverter<T> : BaseTypeConverter
    {
        public void Serialize(T obj, Stream stream)
        {                       
            byte[] objectType = BitConverter.GetBytes((ushort)Type);
            stream.Write(objectType);

            if (obj != null)
            {
                byte[] typeInfo = Encoding.UTF8.GetBytes(obj.GetType().AssemblyQualifiedName);
                stream.WriteWithLengthPrefix(typeInfo);

                WriteObjectToStream(obj, stream);
            }
        }

        public override void Serialize(object obj, Stream stream)
        {
            Serialize((T)obj, stream);
        }

        public T Deserialize(byte[] stream)
        {
            int offset = sizeof (short);

            int typeInfoSize = BitConverter.ToInt32(stream, offset);
            offset += sizeof(int);
            byte[] typeInfo = new byte[typeInfoSize];
            Array.Copy(stream, offset, typeInfo, 0, typeInfo.Length);
            string typeFullName = Encoding.UTF8.GetString(typeInfo, 0, typeInfo.Length);
            Type sourceType = System.Type.GetType(typeFullName);
            offset += typeInfoSize;

            return ProcessDeserialize(stream, sourceType, ref offset);
        }

        public T Deserialize(byte[] stream, ref int offset)
        {
            T obj = ProcessDeserialize(stream, typeof(T), ref offset);
            offset += GetTypeSize();
            return obj;
        }

        public override object DeserializeToObject(byte[] stream)
        {
            return Deserialize(stream);
        }

        public override object DeserializeToObject(byte[] stream, ref int offset)
        {
            return Deserialize(stream, ref offset);
        }

        protected abstract int GetTypeSize();
        protected abstract void WriteObjectToStream(T obj, Stream stream);
        protected abstract T ProcessDeserialize(byte[] stream, Type sourceType, ref int offset);

        protected virtual SerializedType GetPackageType(byte[] stream, ref int offset)
        {
            short type = BitConverter.ToInt16(stream, offset);
            offset += sizeof (short);
            return (SerializedType)type;
        }
    }

    internal abstract class BaseTypeConverter
    {
        public abstract void Serialize(object obj, Stream stream);
        public abstract object DeserializeToObject(byte[] stream);
        public abstract object DeserializeToObject(byte[] stream, ref int offset);
        public abstract SerializedType Type { get; }
    }
}