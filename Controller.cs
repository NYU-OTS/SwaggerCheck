using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;

namespace APICheck
{
    class Controller
    {
        public Dictionary<string, List<Action>> Routes = new Dictionary<string, List<Action>>();
        public string BaseUrl { get; set; }

        public Controller(Type controller)
        {
            BaseUrl = controller.GetTypeInfo()
                .CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value?.ToString();

            var actions = controller
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public) //Instance: is instance method, DeclaredOnly: No inherited methods, Public: is public method
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),true).Any()) //Filter ones with compiler-generated elements
                .ToList();

            actions.ToList().ForEach(a =>
            {
                var attribute = a.GetCustomAttribute<HttpMethodAttribute>();
                var url = BaseUrl + (String.IsNullOrEmpty(attribute.Template) ? attribute.Template : "/" + attribute.Template); //checks if action has associated route

                var path = url.Trim('/').ToLower();
                List<Action> methods;
                if (Routes.TryGetValue(path, out methods))
                {
                    methods.Add(new Action(a, BaseUrl));
                }
                else
                {
                    Routes.Add(path, new List<Action>() { new Action(a, BaseUrl)});
                }
            });
        }
    }
}
