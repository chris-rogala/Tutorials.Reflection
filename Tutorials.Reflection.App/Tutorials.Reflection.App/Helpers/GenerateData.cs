using System;
using System.Collections.Generic;
using Tutorials.Reflection.App.Models;

namespace Tutorials.Reflection.App.Helpers
{
    public static class GenerateData
    {
        public static IEnumerable<ExampleClass> GetList(int count)
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
    }
}
