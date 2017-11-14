using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using NSwag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace APICheck
{
    class Swagger
    {
        public Dictionary<string, List<Action>> Routes = new Dictionary<string, List<Action>>();
        public int Endpoints { get; set; }
        public Swagger(string swaggerPath)
        {
            var document = SwaggerDocument.FromFileAsync(swaggerPath).Result;

            document.Operations.ToList().ForEach(operationDescription =>
            {
                var path = operationDescription.Path.Trim('/').ToLower();
                List<Action> methods;
                if (Routes.TryGetValue(path, out methods))
                {
                    methods.Add(new Action(operationDescription));
                }
                else
                {
                    Routes.Add(path, new List<Action>() { new Action(operationDescription) });
                }
            });

            foreach (var r in Routes.Values)
            {
                Endpoints += r.Count();
            };
        }
        
    }
}