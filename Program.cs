using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;


namespace APICheck
{
    class Program
    {
        static void Main()
        {
            //Console.WriteLine("Enter relative path to compiled .dll file");
            //var path = Console.ReadLine();
            var specs = new Swagger();

            var assemblyPath =
                @"C:\Users\bt1124\Permissions\Permissions\bin\Debug\netcoreapp1.1\OTSS.Ganesh.Permissions.dll";

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

            //Type = controller
            var controllers = assembly.GetTypes()
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.Controller)
                    .IsAssignableFrom(type)) //Can instance of type be assigned to Controller? aka is controller?
                .Where(type => type.GetTypeInfo().CustomAttributes.Any()) //Is it not the BaseController Class
                .Select(type => new Controller(type));

            var existingActions = controllers.SelectMany(c => c.Actions).ToList();

            //var specs = new Swagger();
            //var swaggerActions = specs.Actions.ToList();
            //swaggerActions.Add(new Action("/route", "GET"));
            //Console.WriteLine(Same(swaggerActions, existingActions));
        }

        static bool Same(IEnumerable<Action> swaggerActions, IEnumerable<Action> existingActions)
        {
            //Linear search in low amounts can be faster than dictionary lookup
            return !swaggerActions.Except(existingActions, new ActionComparer()).Any();
        }
    }
}