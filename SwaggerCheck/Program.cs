using System;
using System.Collections.Generic;
using System.IO;
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
            //var assemblyPath = "";
            //var swaggerPath = "";
            var assemblyPath = @"C:\Users\bt1124\SwaggerCheck\TestApi\bin\Debug\netcoreapp1.1\TestApi.dll";
            var swaggerPath = @"C:\Users\bt1124\SwaggerCheck\TestApi\swagger.json";

            app.Name = "apiCheck";
            var matchRouteOption = app.Option("-r|--routes",
                "Find unimplemented Swagger Routes",
                CommandOptionType.NoValue);

            var swaggerRouteOption = app.Option("-s|--swagger",
                "Path to Swagger file",
                CommandOptionType.SingleValue);

            var assemblyRouteOption = app.Option("-b|--binary",
                "Path to .dll file",
                CommandOptionType.SingleValue);
            
            app.OnExecute(() =>
            {
                if (matchRouteOption.HasValue())
                {
                    matchRoute = true;
                }
                if (swaggerRouteOption.HasValue())
                {
                    swaggerPath = Path.GetFullPath(swaggerRouteOption.Value());
                }
                if (assemblyRouteOption.HasValue())
                {
                    assemblyPath = Path.GetFullPath(assemblyRouteOption.Value());
                }
                return 0;
            });

            app.Execute(args);

            var assembly = new Assembly(assemblyPath);
            var swagger = new Swagger(swaggerPath);

            Console.WriteLine("Checking Swagger.....");
            var inSwagger = InSwagger(assembly, swagger, matchRoute);
            Console.WriteLine("Checking Assembly.....");
            var inAssembly = InAssembly(assembly, swagger, matchRoute);
            Console.WriteLine($"Found {swagger.Endpoints} endpoints in Swagger file");
            Console.WriteLine($"Found {assembly.Endpoints} endpoints in API");
            Console.WriteLine($"{inSwagger.Count} endpoints have not been implemented");
            Console.WriteLine($"{inAssembly.Count} endpoints are implemented but are not in the Swagger file");
            Console.WriteLine("All tests passing");
        }

        static List<Action> InSwagger(Assembly assembly, Swagger swagger, bool matchRoute)
        {
            return Compare(assembly.Routes, swagger.Routes, matchRoute, "Swagger");
        }
        static List<Action> InAssembly(Assembly assembly, Swagger swagger, bool matchRoute)
        {
            return Compare(swagger.Routes, assembly.Routes, matchRoute, "Assembly");
        }

        static List<Action> Compare(Dictionary<string, List<Action>> ARoutes, Dictionary<string, List<Action>> BRoutes, bool matchRoute, string checking)
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
        }
    }
}