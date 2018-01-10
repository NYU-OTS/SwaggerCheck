using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SwaggerCheck
{
    class Program
    {
        private static ILoggerFactory _loggerFactory;

        static Program()
        {
        }

        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();

            var matchRoute = false;
            var verbose = false;
            string assemblyPath = "";
            string swaggerPath = "";

            app.Name = "SwaggerCheck";

            var swaggerRouteOption = app.Option("-s|--swagger",
                "Path to Swagger file",
                CommandOptionType.SingleValue);

            var assemblyRouteOption = app.Option("-b|--binary",
                "Path to .dll file",
                CommandOptionType.SingleValue);
            var matchRouteOption = app.Option("-r|--routes",
                "Find unimplemented Swagger Routes",
                CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                matchRoute = matchRouteOption.HasValue();
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
            foreach (var action in inSwagger)
            {
                Console.Error.WriteLine($"Warning: No matching endpoint {action.Route} with Http method {action.Method} in API");
            }
            Console.WriteLine($"{inAssembly.Count} endpoints are implemented but are not in the Swagger file");
            foreach (var action in inAssembly)
            {
                Console.Error.WriteLine($"Warning: Endpoint {action.Route} with Http method {action.Method} implemented but does not match Swagger file");
            }
            if (matchRoute && (inSwagger.Any() || inAssembly.Any()))
            {
                Environment.Exit(1);
            }
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
                if (ARoutes.TryGetValue(route, out matchingRoute)) {
                    var except = matchingRoute.Except(BRoutes[route], new ActionComparer());
                    notExist.AddRange(except);
                }
            }
            return notExist;
        }
    }
}