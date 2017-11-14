﻿using System;
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
        public int Endpoints { get; set; }
        public string BaseUrl { get; set; }

        public Controller(Type controller)
        {
            BaseUrl = controller.GetTypeInfo()
                .CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value?.ToString();//Gets the BaseUrl from the Route attribute

            var actions = controller
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public) //Instance: is instance method, DeclaredOnly: No inherited methods, Public: is public method
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),true).Any()) //Filter ones with compiler-generated elements
                .ToList();

            actions.ForEach(a =>
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
            });

            //sums up endpoints
            foreach (var r in Routes.Values)
            {
                Endpoints += r.Count();
            };
        }
    }
}
