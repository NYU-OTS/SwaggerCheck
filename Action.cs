using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Linq;
using NSwag;

namespace APICheck
{
    class Action
    {
        //public Type Controller;
        public HashSet<string> Httpmethods { get; set; }
        public string Route { get; set; }
        public IEnumerable<string> param { get; set; }

        //for testing
        public Action(string route, params string[] httpMethods)
        {
            Route = route;
            Httpmethods = new HashSet<string>(httpMethods);
        }

        //From binary
        public Action(MethodInfo action, string baseurl)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.Trim('/');
            Httpmethods = new HashSet<string>(attribute.HttpMethods);
            var paramTypes = action.GetParameters().Select(p => p.ParameterType);
        }

        //From Swagger
        public Action(string path, KeyValuePair<SwaggerOperationMethod, SwaggerOperation> action)
        {
            Route = path;
            Httpmethods = new HashSet<string>();
            Httpmethods.Add(action.Key.ToString());
            var paramList =  action.Value.ActualParameters.Select(ps => ps.ActualSchema.ActualProperties);
            var p1 = paramList.Select(p => p.Values.Select(m => m.Type));
        }
        
      
        public override bool Equals(object obj)
        {
            Action a  = obj as Action;
            if (a == null) return false;
            return Route == a.Route && Httpmethods.SetEquals(a.Httpmethods);
        }
        
    }
}
