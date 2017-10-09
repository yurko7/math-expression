using System;

namespace YuKu.MathExpression.Tests
{
    internal static class TypeExtensions
    {
        public static Object GetDefaultValue(this Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
