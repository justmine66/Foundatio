using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Foundatio.Force.DeepCloner.Helpers
{
    /// <summary>
    /// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
    /// </summary>
    internal static class DeepClonerSafeTypes
    {
        internal static readonly ConcurrentDictionary<Type, bool> KnownTypes;

        internal static readonly ConcurrentDictionary<Type, bool> KnownClasses;

        static DeepClonerSafeTypes()
        {
            KnownTypes = new ConcurrentDictionary<Type, bool>();
            KnownClasses = new ConcurrentDictionary<Type, bool>();
            Type[] array = new Type[18]
            {
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(char),
            typeof(string),
            typeof(bool),
            typeof(DateTime),
            typeof(IntPtr),
            typeof(UIntPtr),
            Type.GetType("System.RuntimeType"),
            Type.GetType("System.RuntimeTypeHandle")
            };
            foreach (Type key in array)
            {
                KnownTypes.TryAdd(key, true);
            }
        }

        internal static bool CanNotCopyType(Type type, HashSet<Type> processingTypes)
        {
            if (KnownTypes.TryGetValue(type, out bool value))
            {
                return value;
            }
            if (type.IsEnum() || type.IsPointer)
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }
            if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }
            if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }
            if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }
            if (!type.IsValueType())
            {
                KnownTypes.TryAdd(type, false);
                return false;
            }
            if (processingTypes == null)
            {
                processingTypes = new HashSet<Type>();
            }
            processingTypes.Add(type);
            List<FieldInfo> list = new List<FieldInfo>();
            Type type2 = type;
            do
            {
                list.AddRange(type2.GetAllFields());
                type2 = type2.BaseType();
            }
            while (type2 != (Type)null);
            foreach (FieldInfo item in list)
            {
                Type fieldType = item.FieldType;
                if (!processingTypes.Contains(fieldType) && !CanNotCopyType(fieldType, processingTypes))
                {
                    KnownTypes.TryAdd(type, false);
                    return false;
                }
            }
            KnownTypes.TryAdd(type, true);
            return true;
        }

        /// <summary>
        /// Classes with only safe fields are safe for ShallowClone (if they root objects for copying)
        /// </summary>
        internal static bool CanNotDeepCopyClass(Type type)
        {
            if (KnownClasses.TryGetValue(type, out bool value))
            {
                return value;
            }
            if (!type.IsClass() || type.IsArray)
            {
                KnownClasses.TryAdd(type, false);
                return false;
            }
            List<FieldInfo> list = new List<FieldInfo>();
            Type type2 = type;
            do
            {
                list.AddRange(type2.GetAllFields());
                type2 = type2.BaseType();
            }
            while (type2 != (Type)null);
            if (list.Any((FieldInfo fieldInfo) => !CanNotCopyType(fieldInfo.FieldType, null)))
            {
                KnownClasses.TryAdd(type, false);
                return false;
            }
            KnownClasses.TryAdd(type, true);
            return true;
        }
    }
}
