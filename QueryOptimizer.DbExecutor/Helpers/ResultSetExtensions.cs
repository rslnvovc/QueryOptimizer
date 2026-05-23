using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor.Helpers
{
    internal static class ResultSetExtensions
    {
        public static Type GetListItemType(IList list)
        {
            var listType = list.GetType();

            if (listType.IsGenericType)
                return listType.GetGenericArguments()[0];

            var interfaceType = listType
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IList<>));

            if (interfaceType != null)
                return interfaceType.GetGenericArguments()[0];

            throw new InvalidOperationException(
                $"Cannot detect item type for list type: {listType.FullName}");
        }

        public static object MapReaderRowToObject(DbDataReader reader, Type targetType)
        {
            var instance = Activator.CreateInstance(targetType);

            if (instance == null)
                throw new InvalidOperationException($"Cannot create instance of {targetType.FullName}");

            var properties = targetType
                .GetProperties()
                .Where(x => x.CanWrite)
                .ToDictionary(
                    x => x.Name,
                    x => x,
                    StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);

                if (!properties.TryGetValue(columnName, out var property))
                    continue;

                var value = reader.GetValue(i);

                if (value == DBNull.Value)
                {
                    property.SetValue(instance, null);
                    continue;
                }

                var convertedValue = ConvertValue(value, property.PropertyType);

                property.SetValue(instance, convertedValue);
            }

            return instance;
        }

        public static T MapReaderRowToObject<T>(DbDataReader reader)
        {
            var targetType = typeof(T);

            if (IsSimpleType(targetType))
            {
                var value = reader.GetValue(0);

                if (value == DBNull.Value)
                    return default!;

                return (T)ConvertValue(value, targetType)!;
            }

            var instance = Activator.CreateInstance<T>();

            var properties = targetType
                .GetProperties()
                .Where(x => x.CanWrite)
                .ToDictionary(
                    x => NormalizeName(x.Name),
                    x => x,
                    StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var normalizedColumnName = NormalizeName(columnName);

                if (!properties.TryGetValue(normalizedColumnName, out var property))
                    continue;

                var value = reader.GetValue(i);

                if (value == DBNull.Value)
                {
                    property.SetValue(instance, null);
                    continue;
                }

                var convertedValue = ConvertValue(value, property.PropertyType);

                property.SetValue(instance, convertedValue);
            }

            return instance;
        }

        public static object? ConvertValue(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var nullableType = Nullable.GetUnderlyingType(targetType);

            if (nullableType != null)
                targetType = nullableType;

            if (targetType.IsEnum)
            {
                if (value is string stringValue)
                    return Enum.Parse(targetType, stringValue, ignoreCase: true);

                return Enum.ToObject(targetType, value);
            }

            if (targetType == typeof(Guid))
            {
                if (value is Guid guidValue)
                    return guidValue;

                return Guid.Parse(value.ToString()!);
            }

            if (targetType == typeof(bool))
            {
                if (value is bool boolValue)
                    return boolValue;

                if (value is int intValue)
                    return intValue != 0;

                if (value is byte byteValue)
                    return byteValue != 0;
            }

            if (targetType == typeof(string))
                return value.ToString();

            return Convert.ChangeType(value, targetType);
        }

        private static bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive
                   || underlyingType.IsEnum
                   || underlyingType == typeof(string)
                   || underlyingType == typeof(decimal)
                   || underlyingType == typeof(DateTime)
                   || underlyingType == typeof(Guid)
                   || underlyingType == typeof(double)
                   || underlyingType == typeof(float)
                   || underlyingType == typeof(long)
                   || underlyingType == typeof(int)
                   || underlyingType == typeof(bool);
        }

        private static string NormalizeName(string value)
        {
            return value
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .Trim()
                .ToLowerInvariant();
        }
    }
}
