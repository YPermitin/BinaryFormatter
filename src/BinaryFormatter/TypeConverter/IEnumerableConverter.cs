﻿using System;
using BinaryFormatter.Types;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Linq;
using BinaryFormatter.Utils;


namespace BinaryFormatter.TypeConverter
{
    internal class IEnumerableConverter : BaseTypeConverter<object>
    {
        private int Size { get; set; }

        protected override void WriteObjectToStream(object obj, Stream stream)
        {
            IEnumerable objectAsCollection = obj as IEnumerable;
            int count = TypeHelper.GetCollectionCount((IEnumerable)obj);
            byte[] collectionSize = BitConverter.GetBytes(count);
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
                    Type listType = typeof(List<>).MakeGenericType(obj.GetType().GenericTypeArguments);
                    var collectionOfObjects = (IList)Activator.CreateInstance(listType);
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

        protected override object ProcessDeserialize(byte[] bytes, Type sourceType, ref int offset)
        {
            var stream = new WorkingStream(bytes);
            stream.ChangeOffset(offset);

            int beforeOffset = stream.Offset;
            Type collectionType = sourceType;
            if (collectionType == typeof(object))
                collectionType = typeof(List<object>);

            var deserializedCollection = Activator.CreateInstance(collectionType);
            bool isDictionary = TypeHelper.IsDictionary(deserializedCollection);
            bool isLinkedList = TypeHelper.IsLinkedList(deserializedCollection);
            bool isHashSet = TypeHelper.IsHashSet(deserializedCollection);

            IList deserializedCollectionAsList = null;            
            IDictionary deserializedCollectionAsDictionary = null;

            if (isDictionary)
                deserializedCollectionAsDictionary = (IDictionary)deserializedCollection;
            else
            {
                if (isLinkedList || isHashSet)
                {
                    Type listType = typeof(List<>).MakeGenericType(sourceType.GenericTypeArguments);
                    deserializedCollectionAsList = (IList)Activator.CreateInstance(listType);
                } else
                {
                    deserializedCollectionAsList = (IList)deserializedCollection;
                }
            }

            if (bytes.Length > 0)
            {
                BinaryConverter converter = new BinaryConverter();
                int sizeCollection = stream.ReadInt();

                for (int i = 0; i < sizeCollection; i++)
                {
                    int sizeData = stream.ReadInt();
                    if (sizeData == 0)
                    {
                        continue;
                    }
                    byte[] dataValue = stream.ReadBytes(sizeData);

                    MethodInfo method = typeof(BinaryConverter).GetRuntimeMethod("Deserialize", new[] { typeof(byte[]) });
                    if (sourceType.GenericTypeArguments.Length > 0)
                    {
                        if(isDictionary)
                        {
                            Type elementType = typeof(KeyValuePair<,>).MakeGenericType(sourceType.GenericTypeArguments);
                            method = method.MakeGenericMethod(new System.Type[] { elementType });
                        }
                        else
                            method = method.MakeGenericMethod(sourceType.GenericTypeArguments);
                    }
                    else
                    {
                        method = method.MakeGenericMethod(typeof(object));
                    }
                    var deserializeItem = method.Invoke(converter, new object[] { dataValue });

                    if (isDictionary)
                    {
                        KeyValuePair<object, object> keyValuePairFormObject = TypeHelper.CastFrom(deserializeItem);
                        deserializedCollectionAsDictionary.Add(keyValuePairFormObject.Key, keyValuePairFormObject.Value);
                    } else
                        deserializedCollectionAsList.Add(deserializeItem);
                }
            }

            Size = stream.Offset - beforeOffset;
            stream.ChangeOffset(beforeOffset);

            if(isLinkedList || isHashSet)
            {
                deserializedCollection = Activator.CreateInstance(collectionType, new object[] { deserializedCollectionAsList });
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
