using Microsoft.AspNetCore.Mvc.Routing;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SwaggerCheck
{
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

        //From binary
        public Action(MethodInfo action, string baseurl, string method)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.Trim('/').ToLower();
            Method = method;
            var parameters = action.GetParameters();

            //simple params
            String alias;
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                if (Nullable.GetUnderlyingType(parameter.ParameterType) != null) //if parameter is a nullable
                {
                    parameterType = Nullable.GetUnderlyingType(parameter.ParameterType);
                }

                if (Mapping.TryGetValue(parameterType.ToString(), out alias)) //fails for nullable ex. RoleAssignments have int? roleFamilyId
                {
                    simpleParams.Add(parameter.Name.ToLower(), alias);
                }
                else
                {
                    var schema = JsonSchema4.FromTypeAsync(parameterType).Result;
                    complexParams.Add(parameter.Name.ToLower().Replace("model",""), schema); // assemblies use param names that differntiate between model and entity
                }
            }
        }

        //From Swagger
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