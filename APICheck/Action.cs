using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag;

namespace APICheck
{
    class Action
    {
        //public Type Controller;
        public HashSet<string> Httpmethods { get; set; }
        public string Route { get; set; }
        //string, int, float, bool
        public IDictionary<string, string> simpleParams { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        //everything else
        public IDictionary<string, JsonSchema4> complexParams { get; set; } = new Dictionary<string, JsonSchema4>(StringComparer.OrdinalIgnoreCase);

        //for testing
        public Action(string route, params string[] httpMethods)
        {
            Route = route;
            Httpmethods = new HashSet<string>(httpMethods);
        }

        #region User property <-> LDAP attribute mapping

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
        public Action(MethodInfo action, string baseurl)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.Trim('/').ToLower();
            Httpmethods = new HashSet<string>(attribute.HttpMethods);
            var ps = action.GetParameters();


            //simple params
            String alias;
            foreach (var p in ps)
            {
                if (Mapping.TryGetValue(p.ParameterType.ToString(), out alias)) //fails for nullable ex. RoleAssignments have int? roleFamilyId
                {
                    simpleParams.Add(p.Name.ToLower(), alias);
                }
                else if (Nullable.GetUnderlyingType(p.ParameterType) != null) //handles nullables
                {
                    var innerType = Nullable.GetUnderlyingType(p.ParameterType).ToString();
                    if (Mapping.TryGetValue(innerType, out alias)) //checks if inner type is a simpleParameter
                    {
                        simpleParams.Add(p.Name.ToLower(), alias);
                    }
                    else //handles complex parameters
                    {
                        var schema = JsonSchema4.FromTypeAsync(Nullable.GetUnderlyingType(p.ParameterType)).Result;
                        complexParams.Add(p.Name.ToLower().Replace("model", ""), schema);
                    }
                }
                else
                {
                    var schema = JsonSchema4.FromTypeAsync(p.ParameterType).Result;
                    complexParams.Add(p.Name.ToLower().Replace("model",""), schema); // assemblies use param names that differntiate between model and entity
                }
            }
        }

        //From Swagger
        public Action(SwaggerOperationDescription action)
        {
            Route = action.Path.Trim('/').ToLower();
            Httpmethods = new HashSet<string>();
            Httpmethods.Add(action.Method.ToString().ToUpper()); //HttpAttributes HttpMethods are all Uppercase
            
            var ps = action.Operation.ActualParameters;
            //ICollection<SwaggerParameter> complex = new List<SwaggerParameter>();
            //simple params
            foreach (var p in ps)
            {
                if (p.Type.ToString() != "None" ) //If it's not a complex type
                {
                    simpleParams.Add(p.Name.ToLower(), p.Type.ToString().ToLower()); //typeof for primitives are all lower
                }
                else
                {
                    complexParams.Add(p.Name.ToLower(), p.ActualSchema);
                }
            }
        }
    }
}