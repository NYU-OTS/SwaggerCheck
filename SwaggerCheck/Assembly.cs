using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace APICheck
{
    class Assembly
    {
        public Dictionary<string, List<Action>> Routes { get; set; }
        public int Endpoints { get; set; }
        public Assembly(string assemblyPath)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var controllers = assembly.GetTypes()
                .Where(type => typeof(Microsoft.AspNetCore.Mvc.Controller)
                    .IsAssignableFrom(type)) //Can instance of type be assigned to Controller? aka is controller?
                .Where(type => type.GetTypeInfo().CustomAttributes.Any()) //Is it not the BaseController Class
                .Select(type => new Controller(type));

            Routes = controllers.SelectMany(c => c.Routes)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var c in controllers)
            {
                Endpoints += c.Endpoints;
            };
        }
    }
}
