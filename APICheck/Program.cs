using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.WebEncoders.Testing;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;


namespace APICheck
{
    class Program
    {
        static void Main(string[] args)
        {

            CommandLineApplication app = new CommandLineApplication();

            var matchRoute = false;
            var assemblyPath = ""; // = @"C:\Users\bt1124\TASTIO\TASTIO\bin\Debug\netcoreapp1.1\OTSS.TASTIO.dll";
            var swaggerPath = ""; // = @"C:\Users\bt1124\Documents\Visual Studio 2017\Projects\APICheck\swagger.json";

            app.Name = "apiCheck";
            var matchRouteOption = app.Option("-r|--routes",
                "Find unimplemented Swagger Routes",
                CommandOptionType.NoValue);

            var swaggerRouteOption = app.Option("-s|--swagger",
                "Absolute Path to Swagger file",
                CommandOptionType.SingleValue);

            var assemblyRouteOption = app.Option("-b|--binary",
                "Absolute Path to .dll file",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (matchRouteOption.HasValue())
                {
                    matchRoute = true;
                }
                if (swaggerRouteOption.HasValue())
                {
                    swaggerPath = swaggerRouteOption.Value();
                }
                if (assemblyRouteOption.HasValue())
                {
                    assemblyPath = assemblyRouteOption.Value();
                }
                return 0;
            });

            app.Execute(args);
            
            var assembly = new Assembly(assemblyPath);
            var swagger = new Swagger(swaggerPath);

            var inSwagger = InSwagger(assembly, swagger, matchRoute);
            //var inAssembly = Compare(ARoutes, BRoutes);
            //Console.WriteLine(inSwagger.Any());
        }

        static List<Action> InSwagger(Assembly assembly, Swagger swagger, bool matchRoute)
        {
            return Compare(assembly.Routes, swagger.Routes, matchRoute);
        }
        static List<Action> InAssembly(Assembly assembly, Swagger swagger, bool matchRoute)
        {
            return Compare(swagger.Routes, assembly.Routes, matchRoute);
        }

        static List<Action> Compare(Dictionary<string, List<Action>> ARoutes, Dictionary<string, List<Action>> BRoutes, bool matchRoute)
        {
            List<Action> notExist = new List<Action>();
            foreach (var route in BRoutes.Keys)
            {
                List<Action> matchingRoute;
                if (!ARoutes.TryGetValue(route, out matchingRoute))
                {
                    if (matchRoute)
                    {
                        Console.Error.WriteLine("No matching route " + route);
                    }
                }
                else
                {
                    var except = matchingRoute.Except(BRoutes[route], new ActionComparer());
                    notExist.AddRange(except);
                }
            }
            return notExist;

            //Linear search in low amounts can be faster than dictionary lookup
            //return BRoutes.Except(ARoutes, new ActionComparer()).ToList();
        }
    }
}