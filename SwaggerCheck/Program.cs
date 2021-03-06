﻿using System;
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
        static Program()
        {
        }

        static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication();

            var matchRoute = false;
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
                //TODO: change options to arguments https://msdn.microsoft.com/en-us/magazine/mt763239.aspx
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


            if(!File.Exists(assemblyPath))
            {
                Console.Error.WriteLine($"{assemblyPath} is not a valid path");
                Environment.Exit(1);
            }
            if (!File.Exists(swaggerPath))
            {
                Console.Error.WriteLine($"{assemblyPath} is not a valid path");
                Environment.Exit(1);
            }


            Console.WriteLine($"Program fail on mismatch: {matchRoute}");

            var assembly = new Assembly(assemblyPath);
            var swagger = new Swagger(swaggerPath);

            Console.WriteLine("Checking Swagger.....");
            var notInAssembly = InSwagger(assembly, swagger);
            Console.WriteLine("Checking Assembly.....");
            var notInSwagger = InAssembly(assembly, swagger);
            Console.WriteLine($"Found {swagger.Endpoints} endpoints in Swagger file");
            Console.WriteLine($"Found {assembly.Endpoints} endpoints in API");
            Console.WriteLine($"{notInAssembly.Count} endpoints have not been implemented");
            foreach (var action in notInAssembly)
            {
                Console.Error.WriteLine($"Warning: No matching endpoint {action.Route} with Http method {action.Method} in API");
            }
            Console.WriteLine($"{notInSwagger.Count} endpoints are implemented but are not in the Swagger file");
            foreach (var action in notInSwagger)
            {
                Console.Error.WriteLine($"Warning: Endpoint {action.Route} with Http method {action.Method} implemented but does not match Swagger file");
            }
            if (matchRoute && (notInAssembly.Any() || notInAssembly.Any()))
            {
                Console.Error.WriteLine("Tests failing");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// In Swagger but not in Assembly
        /// </summary>
        /// <param name="assembly">The generated Assembly object</param>
        /// <param name="swagger">The generated Swagger object</param>
        /// <returns></returns>
        static List<Action> InSwagger(Assembly assembly, Swagger swagger)
        {
            return Compare(swagger.Routes, assembly.Routes);
        }

        /// <summary>
        /// In Assembly but not in Swagger
        /// </summary>
        /// <param name="assembly">The generated Assembly object</param>
        /// <param name="swagger">The generated Swagger object</param>
        /// <returns></returns>
        static List<Action> InAssembly(Assembly assembly, Swagger swagger)
        {
            return Compare(assembly.Routes, swagger.Routes);
        }

        /// <summary>
        /// Compares the routes and returns the differences
        /// </summary>
        /// <param name="ARoutes">A dictionary with keys being the route and value being the actions</param>
        /// <param name="BRoutes">A dictionary with keys being the route and value being the actions</param>
        /// <returns>A list of Action in A but not in B</returns>
        static List<Action> Compare(Dictionary<string, List<Action>> ARoutes, Dictionary<string, List<Action>> BRoutes)
        {
            List<Action> notExist = new List<Action>();
            foreach (var route in ARoutes.Keys)
            {
                List<Action> matchingRoute;
                if (BRoutes.TryGetValue(route, out matchingRoute))
                {
                    var except = ARoutes[route].Except(matchingRoute, new ActionComparer());
                    notExist.AddRange(except);
                }
                else
                {
                    notExist.AddRange(ARoutes[route]);
                }
            }
            return notExist;
        }
    }
}