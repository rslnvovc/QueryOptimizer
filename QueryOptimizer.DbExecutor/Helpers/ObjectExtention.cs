using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace QueryOptimizer.DatabaseExecutor.Helpers
{
    internal static class ObjectExtention
    {
        public static T GetTypedValue<T>(this object value)
        {
            return (T)value.GetTypedValue(typeof(T));
        }

        public static object GetTypedValue(this object value, Type type)
        {
            if (value == DBNull.Value) value = null;
            type = (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? Nullable.GetUnderlyingType(type)
                : type;

            object result;
            if (value == null) result = null;
            else if (type == typeof(decimal)) result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            else if (type == typeof(long)) result = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            else if (type == typeof(int)) result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            else if (type == typeof(double)) result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            else if (type == typeof(DateTime)) result = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            else if (type == typeof(string)) result = Convert.ToString(value, CultureInfo.InvariantCulture);
            else if (type.IsEnum) result = Enum.Parse(type, value.ToString());//TODO: seems wrong
            else result = value;

            return result;
        }
    }
}
