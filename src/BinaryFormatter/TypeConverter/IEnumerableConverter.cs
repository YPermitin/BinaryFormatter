﻿using System;
using BinaryFormatter.Types;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using BinaryFormatter.Utils;

namespace BinaryFormatter.TypeConverter
{
    internal class IEnumerableConverter : BaseTypeConverter<object>
    {
        private int Size { get; set; }

        protected override void WriteObjectToStream(object obj, Stream stream)
        {
            var objectAsCollection = (IList)obj;

            byte[] collectionSize = BitConverter.GetBytes(objectAsCollection.Count);
            stream.Write(collectionSize);

            BinaryConverter converter = new BinaryConverter();
            foreach (var sourceElementValue in objectAsCollection)
            {
                if (sourceElementValue == null)
                    continue;

                object elementValue = (sourceElementValue as IEnumerable<object>);
                if (elementValue == null)
                {
                    elementValue = sourceElementValue;
                }
                else
                {
                    List<object> collectionOfObjects = new List<object>();
                    foreach (var item in (IList)elementValue)
                    {
                        collectionOfObjects.Add(item);
                    }
                    elementValue = collectionOfObjects;
                }

                byte[] data = converter.Serialize(elementValue);
                stream.WriteWithLengthPrefix(data);

                Size += data.Length;
            }
        }

        protected override object ProcessDeserialize(byte[] stream, Type sourceType, ref int offset)
        {
            List<object> deserializedCollection = null;

            if (stream.Length > 0)
            {
                BinaryConverter converter = new BinaryConverter();
                deserializedCollection = new List<object>();

                int sizeCollection = BitConverter.ToInt32(stream, offset);
                offset += sizeof(int);

                for (int i = 0; i < sizeCollection; i++)
                {
                    //int sizeTypeInfo = BitConverter.ToInt32(stream, offset);
                    //offset += sizeof(int);
                    //if (sizeTypeInfo == 0)
                    //{
                    //    continue;
                    //}

                    //byte[] typeInfo = new byte[sizeTypeInfo];
                    //Array.Copy(stream, offset, typeInfo, 0, sizeTypeInfo);
                    //string typeFullName = Encoding.UTF8.GetString(typeInfo, 0, sizeTypeInfo);
                    //Type valueType = System.Type.GetType(typeFullName);
                    //offset += sizeTypeInfo;

                    int sizeData = BitConverter.ToInt32(stream, offset);
                    offset += sizeof(int);
                    if (sizeData == 0)
                    {
                        continue;
                    }
                    byte[] dataValue = new byte[sizeData];
                    Array.Copy(stream, offset, dataValue, 0, sizeData);

                    //int typeInfoSize = BitConverter.ToInt32(dataValue, offset + sizeof(int));
                    //byte[] typeInfo = new byte[typeInfoSize];
                    //Array.Copy(stream, offset, typeInfo, 0, typeInfo.Length);
                    //string typeFullName = Encoding.UTF8.GetString(typeInfo, 0, typeInfo.Length);
                    //Type sourceType111 = System.Type.GetType(typeFullName);

                    MethodInfo method = typeof(BinaryConverter).GetRuntimeMethod("Deserialize", new System.Type[] { typeof(byte[]) });
                    method = method.MakeGenericMethod(typeof(object));                                        
                    object deserializeItem = method.Invoke(converter, new object[] { dataValue });
                    offset += sizeData;

                    deserializedCollection.Add(deserializeItem);
                }
            }

            return deserializedCollection;
        }

        protected override int GetTypeSize()
        {
            return Size;
        }

        public override SerializedType Type => SerializedType.IEnumerable;
    }
}
