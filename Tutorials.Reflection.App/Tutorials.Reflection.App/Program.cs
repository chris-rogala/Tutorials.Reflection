using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Tutorials.Reflection.App
{
    internal class Program
    {
        private static readonly Dictionary<Type, PropertyInfo[]> _propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<PropertyInfo, Func<object, object>> _getterCache = new Dictionary<PropertyInfo, Func<object, object>>();

        private static void Main(string[] args)
        {
            //Front load the cost of reflection and load into some cache
            if (!_propertyInfoCache.TryGetValue(typeof(ExampleClass), out PropertyInfo[] props))
            {
                props = typeof(ExampleClass).GetProperties();
                _propertyInfoCache[typeof(ExampleClass)] = props;
            }

            foreach (PropertyInfo prop in props)
            {
                if (!_getterCache.TryGetValue(prop, out Func<object, object> getter))
                {
                    getter = Getter(prop);
                    _getterCache[prop] = getter;
                }
            }

            //Run examples
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.Clear();
                int count = 10000000;
                IEnumerable<ExampleClass> @objects = GetList(count);
                RunExample(objects, count, "Using reflection getter...", ReflectionGetter);
                RunExample(objects, count, "Using cached reflection getter...", CachedReflectionGetter);
                RunExample(objects, count, "Using class getter...", FromClass);

                keyInfo = Console.ReadKey();
            } while (keyInfo.Key == ConsoleKey.Enter);
        }

        private static readonly Action<ExampleClass> FromClass = (obj) =>
        {
            var id = obj.Id;
            var firstName = obj.FirstName;
            var lastName = obj.LastName;
            var homeAddress = obj.HomeAddress;
            if (obj.Id == 0)
            {
                Console.WriteLine($"{nameof(ExampleClass.Id)}: {id}");
                Console.WriteLine($"{nameof(ExampleClass.FirstName)}: {firstName}");
                Console.WriteLine($"{nameof(ExampleClass.LastName)}: {lastName}");
                Console.WriteLine($"{nameof(ExampleClass.HomeAddress)}: {homeAddress}");
            }
        };

        private static readonly Action<ExampleClass> ReflectionGetter = (obj) =>
        {
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                object value = prop.GetValue(obj);
                if (obj.Id == 0)
                {
                    Console.WriteLine($"{prop.Name}: {value}");
                }
            }
        };

        private static readonly Action<ExampleClass> CachedReflectionGetter = (obj) =>
        {
            foreach (PropertyInfo prop in _propertyInfoCache[typeof(ExampleClass)])
            {
                object value = _getterCache[prop](obj);
                if (obj.Id == 0)
                {
                    Console.WriteLine($"{prop.Name}: {value}");
                }
            }
        };

        private static void RunExample(IEnumerable<ExampleClass> objects,
            int count,
            string name,
            Action<ExampleClass> writePropertyValues)
        {
            Console.WriteLine(name);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (ExampleClass @object in @objects)
            {
                writePropertyValues(@object);
                if (@object.Id == 1)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Loading remaining {count - 1}...");
                }
            }
            Console.WriteLine();
            Console.WriteLine($"ElapsedMilliseconds: {stopWatch.ElapsedMilliseconds}");
            Console.WriteLine();
        }

        private static IEnumerable<ExampleClass> GetList(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new ExampleClass
                {
                    Id = i,
                    FirstName = "Ernie",
                    LastName = "Banks",
                    DateOfBirth = DateTime.Now.AddYears(-29),
                    HomeAddress = new Address
                    {
                        City = "Chicago",
                        State = "IL",
                        Street = "1060 W Addison St",
                        Zipcode = "60613"
                    }
                };
            }
        }

        protected static Func<object, object> Getter(PropertyInfo propertyInfo)
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

    public class ExampleClass
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address HomeAddress { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
    }
}
