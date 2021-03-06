﻿using Microsoft.AspNetCore.Mvc.Routing;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SwaggerCheck
{
    /// <summary>
    /// An abstraction of an API action/endpoint
    /// </summary>
    class Action
    {
        public string Method { get; set; }
        public string Route { get; set; }
        //simple parameters are: string, int, float, bool
        public IDictionary<string, string> simpleParams { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        //everything else
        public IDictionary<string, JsonSchema4> complexParams { get; set; } = new Dictionary<string, JsonSchema4>(StringComparer.OrdinalIgnoreCase);

        //for testing
        public Action(string route, params string[] httpMethods)
        {
            Route = route;
        }

        #region C# type string <-> OpenAPI keyword

        private static readonly Dictionary<string, string> Mapping = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { typeof(string).ToString(), "string" },
            { typeof(float).ToString(), "number" },
            { typeof(int).ToString(), "integer" },
            { typeof(bool).ToString(), "bool" },
        };
        //missing array and file

        #endregion

        /// <summary>
        /// Create a Action based on information from the binary
        /// </summary>
        /// <param name="action">The MethoInfo object that represents the action</param>
        /// <param name="baseurl">The baseurl from the controller</param>
        /// <param name="method">The Http method of the action</param>
        public Action(MethodInfo action, string baseurl, string method)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            //TODO add support for [action]
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.Trim('/').ToLower();
            Method = method;
            var parameters = action.GetParameters();

            //process parameters
            String alias;
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                if (Nullable.GetUnderlyingType(parameter.ParameterType) != null) //If parameter is a nullable
                {
                    parameterType = Nullable.GetUnderlyingType(parameter.ParameterType);
                }

                if (Mapping.TryGetValue(parameterType.ToString(), out alias)) //Handles nullables
                {
                    simpleParams.Add(parameter.Name.ToLower(), alias);
                }
                else
                {
                    var schema = JsonSchema4.FromTypeAsync(parameterType).Result;
                    //TODO: make this handle general inconsistencies between naming
                    complexParams.Add(parameter.Name.ToLower().Replace("model",""), schema); //Souce code use param names that differntiate between model and entity
                }
            }
        }

        /// <summary>
        /// Create a Action based on information from the Swagger file
        /// </summary>
        /// <param name="action">A description of the endpoint generated by the Swagger class</param>
        public Action(SwaggerOperationDescription action)
        {
            Route = action.Path.Trim('/').ToLower();
            Method = action.Method.ToString().ToUpper(); //HttpAttributes HttpMethods are all Uppercase
            
            var parameters = action.Operation.ActualParameters;
            //ICollection<SwaggerParameter> complex = new List<SwaggerParameter>();
            //simple params
            foreach (var parameter in parameters)
            {
                if (parameter.Type.ToString() != "None" ) //If it's not a complex type
                {
                    simpleParams.Add(parameter.Name.ToLower(), parameter.Type.ToString().ToLower()); //typeof for primitives are all lower
                }
                else
                {
                    complexParams.Add(parameter.Name.ToLower(), parameter.ActualSchema);
                }
            }
        }
    }
}