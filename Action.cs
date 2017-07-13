using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Linq;

namespace APICheck
{
    class Action
    {
        //public Type Controller;
        public HashSet<string> Httpmethods { get; set; }
        public string Route { get; set; }

        public Action(MethodInfo action, string baseurl)
        {
            var attribute = action.GetCustomAttribute<HttpMethodAttribute>();
            var url = baseurl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

            Route = url.TrimStart('/').TrimEnd('/');
            Httpmethods = new HashSet<string>( attribute.HttpMethods);
        }

        public Action(JToken action, string url)
        {
            Route = url.TrimStart('/').TrimEnd('/');
            Httpmethods = new HashSet<string>(action.Value<JObject>()
                .Properties()
                .Select(p => p.Name.ToUpper()));
        }
      
        public override bool Equals(object obj)
        {
            Action a  = obj as Action;
            if (a == null) return false;
            return Route == a.Route && Httpmethods.SetEquals(a.Httpmethods);
        }
        
    }
}
