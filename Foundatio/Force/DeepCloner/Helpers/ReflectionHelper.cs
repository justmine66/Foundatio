using System;
using System.Linq;
using System.Reflection;

namespace Foundatio.Force.DeepCloner.Helpers
{
    internal static class ReflectionHelper
    {
        public static bool IsEnum(this Type t)
        {
            return t.GetTypeInfo().IsEnum;
        }

        public static bool IsValueType(this Type t)
        {
            return t.GetTypeInfo().IsValueType;
        }

        public static bool IsClass(this Type t)
        {
            return t.GetTypeInfo().IsClass;
        }

        public static Type BaseType(this Type t)
        {
            return t.GetTypeInfo().BaseType;
        }

        public static FieldInfo[] GetAllFields(this Type t)
        {
            return (from x in t.GetTypeInfo().DeclaredFields
                    where !x.IsStatic
                    select x).ToArray();
        }

        public static FieldInfo[] GetDeclaredFields(this Type t)
        {
            return (from x in t.GetTypeInfo().DeclaredFields
                    where !x.IsStatic
                    select x).ToArray();
        }

        public static ConstructorInfo[] GetPrivateConstructors(this Type t)
        {
            return t.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static MethodInfo GetPrivateMethod(this Type t, string methodName)
        {
            return t.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static MethodInfo GetMethod(this Type t, string methodName)
        {
            return t.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static MethodInfo GetPrivateStaticMethod(this Type t, string methodName)
        {
            return t.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static FieldInfo GetPrivateField(this Type t, string fieldName)
        {
            return t.GetTypeInfo().GetDeclaredField(fieldName);
        }

        public static bool IsSubclassOfTypeByName(this Type t, string typeName)
        {
            while (t != (Type)null)
            {
                if (t.Name == typeName)
                {
                    return true;
                }
                t = BaseType(t);
            }
            return false;
        }

        public static bool IsAssignableFrom(this Type from, Type to)
        {
            return from.GetTypeInfo().IsAssignableFrom(to.GetTypeInfo());
        }

        public static bool IsInstanceOfType(this Type from, object to)
        {
            return from.IsAssignableFrom(to.GetType());
        }
    }

}
