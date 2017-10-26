using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace APICheck
{
    class Assembly
    {
        public IEnumerable<Action> Actions { get; set; }
        public Assembly(string assemblyPath)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var controllers = assembly.GetTypes()
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.Controller)
                    .IsAssignableFrom(type)) //Can instance of type be assigned to Controller? aka is controller?
                .Where(type => type.GetTypeInfo().CustomAttributes.Any()) //Is it not the BaseController Class
                .Select(type => new Controller(type));

            Actions = controllers.SelectMany(c => c.Actions).ToList();
        }
    }
}
