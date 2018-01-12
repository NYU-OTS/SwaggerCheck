using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SwaggerCheck
{
    /// <summary>
    /// An abstraction of a API Controller
    /// </summary>
    class Controller
    {
        public Dictionary<string, List<Action>> Routes = new Dictionary<string, List<Action>>();
        public int Endpoints { get; set; }
        public string BaseUrl { get; set; }

        public Controller(Type controller)
        {
            var routeAttribute = controller.GetTypeInfo()
                .CustomAttributes.FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value?.ToString();//Gets the BaseUrl from the Route attribute

            var name = controller.Name.Replace("Controller", "");

            BaseUrl = routeAttribute.Replace("[controller]", name);

            var actions = controller
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public) //Instance: is instance method, DeclaredOnly: No inherited methods, Public: is public method
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),true).Any()); //Filter ones with compiler-generated elements: "Use the CompilerGeneratedAttribute attribute to determine whether an element is added by a compiler or authored directly in source code"

            foreach (var a in actions)
            {
                var methods = a.GetCustomAttribute<HttpMethodAttribute>().HttpMethods;
                var attribute = a.GetCustomAttribute<HttpMethodAttribute>();
                var url = BaseUrl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

                var path = url.Trim('/').ToLower();
                foreach (var method in methods){
                    List<Action> existingMethods;
                    if (Routes.TryGetValue(path, out existingMethods))
                    {
                        existingMethods.Add(new Action(a, BaseUrl, method));
                    }
                    else
                    {
                        Routes.Add(path, new List<Action>() {new Action(a, BaseUrl, method)});
                    }
                }
            }

            //sums up endpoints for easy access
            foreach (var r in Routes.Values)
            {
                Endpoints += r.Count;
            };
        }
    }
}
