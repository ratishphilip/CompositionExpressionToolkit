using System;
using System.Reflection;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Extension methods for System.Type
    /// </summary>
    public static class TypeExtensions
    {
        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static MethodInfo GetMethod(this Type type, string methodName)
        {
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static bool IsSubclassOf(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsSubclassOf(parentType);
        }

        public static bool IsAssignableFrom(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsAssignableFrom(parentType.GetTypeInfo());
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsPublic(this Type type)
        {
            return type.GetTypeInfo().IsPublic;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static object GetPropertyValue(this object instance, string propertyValue)
        {
            return instance.GetType().GetTypeInfo().GetDeclaredProperty(propertyValue).GetValue(instance);
        }

        public static TypeInfo GetTypeInfo(this Type type)
        {
            var reflectableType = type as IReflectableType;
            return reflectableType?.GetTypeInfo();
        }


        internal static bool IsAnonymous(this Type type)
        {
            if (!string.IsNullOrEmpty(type.Namespace) || !type.IsGenericType()) return false;
            return IsAnonymous(type.Name);
        }

        internal static bool IsAnonymous(string typeName)
        {
            // Optimization to improve perf when called from UserCache
            return
                typeName.Length > 5 &&
                    (typeName[0] == '<' && typeName[1] == '>' && (typeName[5] == 'A' && typeName[6] == 'n' || typeName.IndexOf("anon", StringComparison.OrdinalIgnoreCase) > -1) ||
                    typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$' && typeName[3] == 'A' && typeName[4] == 'n');
        }

        internal static string FormattedName(this Type t, bool fullname = false)
        {
            return fullname ? t.FullName : t.Name;
        }
    }
}
