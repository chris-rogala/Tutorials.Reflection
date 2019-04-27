using System.Linq;
using System.Text;

namespace Tutorials.Reflection.App.Helpers
{
        public static class Basics
        {
            public static void RunGetPropertiesExample()
            {
                OutputHelper.WriteHeader("What is GetProperties?");

                StringBuilder sb = new StringBuilder();
                string stringObject = "hello world";
                foreach (System.Reflection.PropertyInfo pi in stringObject.GetType().GetProperties())
                {
                    sb.AppendLine($"Name: {pi.Name}");
                    sb.AppendLine($"PropertyType: {pi.PropertyType}");
                    sb.AppendLine();
                }

                OutputHelper.OutputResult(sb);
            }

            public static void RunGetValueExample()
            {
                OutputHelper.WriteHeader("What is GetValue?");

                StringBuilder sb = new StringBuilder();
                string stringObject = "hello world";

                System.Reflection.PropertyInfo lengthPropertyInfo = stringObject.GetType().GetProperties().First(x => x.Name == "Length");
                System.Reflection.PropertyInfo charsPropertyInfo = stringObject.GetType().GetProperties().First(x => x.Name == "Chars");

                for (int i = 0; i < (int)(lengthPropertyInfo.GetValue(stringObject)); i++)
                {
                    sb.Append(charsPropertyInfo.GetValue(stringObject, new object[] { i }));
                }

                OutputHelper.OutputResult(sb);
            }
        }
}
