using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AOS.Common.DataSerialization.Objects;

namespace AOS.Common.DataSerialization
{
    public static class SConverter
    {
        public static byte[] Serialize(object obj)
        {
            using var stream = new MemoryStream();
            using var writer = new SWriter(stream);

            if (obj is byte or int or string or SObject or IEnumerable)
            {
                writer.Write(obj);
            }
            else
            {
                var sObject = ConvertToSObject(obj);
                writer.Write(sObject);
            }

            return stream.ToArray();
        }

        public static SObject ConvertToSObject(object obj)
        {
            var objType = obj.GetType();

            return new SObject
            {
                Fields = GetOrderedSProperties(objType)
                    .Select(x => x.GetValue(obj)!)
                    .Select(ConvertField)
                    .ToList()
            };
        }

        private static object ConvertField(object obj) =>
            obj switch
            {
                byte or int or string or SObject => obj,
                IEnumerable e => e.Cast<object>().Select(ConvertField),
                _ => ConvertToSObject(obj)
            };

        public static T? Deserialize<T>(byte[] bytes)
        {
            var val = Deserialize(typeof(T), bytes);

            return val is null ? default : (T)val;
        }

        public static object? Deserialize(Type type, byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new SReader(stream);

            var obj = reader.ReadUnknown();

            return obj.Match(
                sObject => ConvertFromSObject(type, sObject),
                sByte => sByte,
                sInt => sInt,
                sString => sString,
                sList => sList
            );
        }

        public static T? ConvertFromSObject<T>(SObject sObject)
        {
            return (T?)ConvertFromSObject(typeof(T), sObject);
        }

        private static object ConvertFromSObject(Type objType, SObject sObject)
        {
            var obj = Activator.CreateInstance(objType)!;

            var properties = GetOrderedSProperties(objType).ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                object? val;

                var type = property.PropertyType;
                if (type == typeof(byte) || type == typeof(int) || type == typeof(string) || type == typeof(SObject))
                {
                    val = sObject.Fields[i];
                }
                else if (type.IsAssignableTo(typeof(IList)))
                {
                    var itemType = type.GetGenericArguments().First();
                    val = ConvertList((IEnumerable)sObject.Fields[i], itemType);
                }
                else
                {
                    val = ConvertFromSObject(type, (SObject)sObject.Fields[i]);
                }

                property.SetValue(obj, val ?? throw new SSerializationException("Couldn't convert object"), null);
            }

            return obj;
        }

        private static IEnumerable<PropertyInfo> GetOrderedSProperties(Type type) => type
            .GetProperties()
            .Where(x => x.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(SOrderAttribute)))
            .OrderBy(x => ((SOrderAttribute)Attribute.GetCustomAttribute(x, typeof(SOrderAttribute))!).Order);

        private static object? ConvertList(IEnumerable list, Type itemType)
        {
            var enumerableType = typeof(Enumerable);
            var castMethod = enumerableType.GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(itemType);
            var toListMethod = enumerableType.GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(itemType);

            IEnumerable<object> objects = list
                .Cast<object>()
                .Select(item => item switch
                {
                    byte or int or string => item,
                    SObject x => ConvertFromSObject(itemType, x),
                    _ => throw new SSerializationException("Unknown array type: " + itemType)
                })
                .Select(x => Convert.ChangeType(x, itemType));

            var castedItems = castMethod.Invoke(null, new object?[] { objects });
            return toListMethod.Invoke(null, new[] { castedItems });
        }
    }
}
