using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using NSwag;

namespace APICheck
{
    class Action
    {
        //public Type Controller;
        public HashSet<string> Httpmethods { get; set; }
        public string Route { get; set; }
        public IDictionary<string, string> simpleParams { get; set; } = new Dictionary<string, string>();

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

        #endregion

        //From binary
        public Action(MethodInfo action, string baseurl)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.Trim('/');
            Httpmethods = new HashSet<string>(attribute.HttpMethods);
            var ps = action.GetParameters();

            String alias;
            foreach (var p in ps)
            {
                var t = p.ParameterType;
                if(Mapping.TryGetValue(p.ParameterType.ToString(), out alias))
                    simpleParams.Add(p.Name, alias);
            }
        }

        //From Swagger
        public Action(SwaggerOperationDescription action)
        {
            Route = action.Path.Trim('/');
            Httpmethods = new HashSet<string>();
            Httpmethods.Add(action.Method.ToString().ToUpper());

            //params
            //var paramList =  action.Operation.ActualParameters.Select(ps => ps.ActualSchema.ActualProperties);
            var ps = action.Operation.ActualParameters;

            foreach (var p in ps)
            {
                simpleParams.Add(p.Name, p.Type.ToString());
            }
            //var types = action.Operation.ActualParameters.Select(ps => ps.Type);
        }

        public override bool Equals(object obj)
        {
            Action other  = obj as Action;
            if (other == null) return false;
            foreach (var varName in other.simpleParams.Keys)
            {
                if (other.simpleParams[varName] != simpleParams[varName])
                    return false;
            }
            return Route == other.Route && Httpmethods.SetEquals(other.Httpmethods);
        }

        public override string ToString()
        {
            var route = "Routes: " + Route;
            var httpMethods = "HttpMethods:";
            foreach (var method in Httpmethods)
            {
                httpMethods += " " + method;
            }
            var parameters = simpleParams.ToString();
            return route + httpMethods + parameters;
        }
    }
}
