using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using Tutorials.Reflection.App.Models;

namespace Tutorials.Reflection.App.Helpers
{
    public static class EnhancePerfomance
    {
        private static readonly Dictionary<PropertyInfo, Func<object, object>> _getterCache = new Dictionary<PropertyInfo, Func<object, object>>();
        private static readonly Dictionary<Type, PropertyInfo[]> _propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();

        public static void LoadGetterCache<T>()
        {
            if (!_propertyInfoCache.TryGetValue(typeof(T), out PropertyInfo[] props))
            {
                LoadPropertyInfoCache<T>();
            }

            foreach (PropertyInfo prop in _propertyInfoCache[typeof(T)])
            {
                if (!_getterCache.TryGetValue(prop, out Func<object, object> getter))
                {
                    getter = Getter(prop);
                    _getterCache[prop] = getter;
                }
            }
        }

        public static void LoadPropertyInfoCache<T>()
        {
            if (!_propertyInfoCache.TryGetValue(typeof(T), out PropertyInfo[] props))
            {
                props = typeof(T).GetProperties();
                _propertyInfoCache[typeof(T)] = props;
            }
        }

        public static void RunExample(IEnumerable<ExampleClass> objects,
            int count,
            string name,
            Action<object, int> writePropertyValues)
        {
            OutputHelper.WriteHeader(name);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int i = 0;

            foreach (ExampleClass @object in @objects)
            {
                writePropertyValues(@object, i++);
                if (@object.Id == 1)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Loading remaining {count - 1}...");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            Console.WriteLine();
            Console.WriteLine($"ElapsedMilliseconds: {stopWatch.ElapsedMilliseconds}");
            Console.WriteLine();
        }

        /// <summary>
        /// Access the values of the directly from the object
        /// </summary>
        public static readonly Action<object,int> FromClass = (obj, index) =>
        {
            var exampleClass = obj as ExampleClass;
            object id = exampleClass.Id;
            object firstName = exampleClass.FirstName;
            object lastName = exampleClass.LastName;
            object homeAddress = exampleClass.HomeAddress;
            if (index == 0)
            {
                Console.WriteLine($"{nameof(ExampleClass.Id)}: {id}");
                Console.WriteLine($"{nameof(ExampleClass.FirstName)}: {firstName}");
                Console.WriteLine($"{nameof(ExampleClass.LastName)}: {lastName}");
                Console.WriteLine($"{nameof(ExampleClass.HomeAddress)}: {homeAddress}");
            }
        };

        /// <summary>
        /// Calls the GetProperties and GetValue to get values from the object
        /// </summary>
        public static readonly Action<object,int> ReflectionGetter = (obj, index) =>
        {
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                object value = prop.GetValue(obj);
                if (index == 0)
                {
                    Console.WriteLine($"{prop.Name}: {value}");
                }
            }
        };

        /// <summary>
        /// Uses the cached getters to get values from the object
        /// </summary>
        public static readonly Action<object, int> CachedReflectionGetter = (obj, index) =>
        {
            foreach (PropertyInfo prop in _propertyInfoCache[obj.GetType()])
            {
                object value = _getterCache[prop](obj);
                if (index == 0)
                {
                    Console.WriteLine($"{prop.Name}: {value}");
                }
            }
        };

        private static Func<object, object> Getter(PropertyInfo propertyInfo)
        {
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
            DynamicMethod getMethod = new DynamicMethod("GetValue", typeof(object), new Type[] { typeof(object) }, typeof(PropertyAccessors), true);
            ILGenerator ilGenerator = getMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, getMethodInfo);

            Type returnType = getMethodInfo.ReturnType;

            if (returnType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, returnType);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return (Func<object, object>)getMethod.CreateDelegate(typeof(Func<object, object>));
        }
    }
}
