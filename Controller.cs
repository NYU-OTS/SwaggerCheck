using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace APICheck
{
    class Controller
    {
        public ICollection<Action> Actions { get; set; }
        public string BaseUrl { get; set; }

        public Controller(Type controller)
        {

            BaseUrl = controller.GetTypeInfo()
                .CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value?.ToString();

            var actions = controller
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public) //Instance: is instance method, DeclaredOnly: No inherited methods, Public: is public method
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),true).Any()) //Filter ones with compiler-generated elements
                .ToList();

            Actions = actions.Select(a => new Action(a, BaseUrl)).ToList();
        }
    }
}
