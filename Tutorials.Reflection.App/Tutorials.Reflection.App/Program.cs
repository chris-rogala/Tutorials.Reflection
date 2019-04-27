using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using Tutorials.Reflection.App.Helpers;
using Tutorials.Reflection.App.Models;

namespace Tutorials.Reflection.App
{
    internal class Program
    {
        private static readonly Dictionary<Type, PropertyInfo[]> _propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<PropertyInfo, Func<object, object>> _getterCache = new Dictionary<PropertyInfo, Func<object, object>>();

        private static void Main(string[] args)
        {
            //Section: I’m new to reflection, what the heck is it?
            Basics.RunGetPropertiesExample();
            Basics.RunGetValueExample();

            //Section: Now comes the great responsibility...
            //Front load the cost of reflection and load into some cache
            EnhancePerfomance.LoadGetterCache<ExampleClass>();

            //Run examples
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.Clear();
                int count = 10000000;
                IEnumerable<ExampleClass> @objects = GenerateData.GetList(count);
                EnhancePerfomance.RunExample(objects, count, "Using reflection getter...", EnhancePerfomance.ReflectionGetter);
                EnhancePerfomance.RunExample(objects, count, "Using cached reflection getter...", EnhancePerfomance.CachedReflectionGetter);
                EnhancePerfomance.RunExample(objects, count, "Using class getter...", EnhancePerfomance.FromClass);

                keyInfo = Console.ReadKey();
            } while (keyInfo.Key == ConsoleKey.Enter);
        }
    }
}
