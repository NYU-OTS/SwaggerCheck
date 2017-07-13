using System;
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

            var assemblyPath =
                @"C:\Users\bt1124\Identity\Identity\bin\Debug\netcoreapp1.1\OTSS.Ganesh.Identity.dll";

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

            //Type = controller
            var controllers = assembly.GetTypes()
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.Controller)
                    .IsAssignableFrom(type)) //Can instance of type be assigned to Controller? aka is controller?
                .Where(type => type.GetTypeInfo().CustomAttributes.Any()) //Is it not the BaseController Class
                .Select(type => new Controller(type));

            var actions = controllers.SelectMany(c => c.Actions).ToList();

            //var t = controllers.ElementAt(1);
            //var c = new Controller(t);
            var specs = new Swagger();
            var swaggerActions = specs.Actions.ToList();
            var check = actions[0].Equals(swaggerActions[0]);
        }
    }
}