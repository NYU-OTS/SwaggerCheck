using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;


namespace APICheck
{
    class Program
    {
        static void Main()
        {
            /*
            Console.WriteLine("Enter absolute path to compiled .dll file");
            var assemblyPath = Console.ReadLine();

            Console.WriteLine("Enter absolute path to Swagger file");
            var swaggerPath = Console.ReadLine();
            */

            var assemblyPath = @"C:\Users\bt1124\TASTIO\TASTIO\bin\Debug\netcoreapp1.1\OTSS.TASTIO.dll";
            var swaggerPath = @"C:\Users\bt1124\Documents\Visual Studio 2017\Projects\APICheck\swagger.json";

            var assembly = new Assembly(assemblyPath);
            var existingActions = assembly.Actions.ToList();

            var specs = new Swagger(swaggerPath);
            var swaggerActions = specs.Actions.ToList();

            var inSwagger = Compare(swaggerActions, existingActions);
            var inAssembly = Compare(existingActions, swaggerActions);
            Console.WriteLine(inSwagger.Any());
        }

        static IEnumerable<Action> Compare(IEnumerable<Action> swaggerActions, IEnumerable<Action> existingActions)
        {
            //Linear search in low amounts can be faster than dictionary lookup
            return swaggerActions.Except(existingActions, new ActionComparer()).ToList();
        }
    }
}