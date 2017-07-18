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
        public IEnumerable<Action> Actions { get; set; }

        public Swagger()
        {
            //Processing JSON
            var swaggerPath = @"C:\Users\bt1124\Documents\Visual Studio 2017\Projects\APICheck\swagger.json";
            var json = File.ReadAllText(swaggerPath);
            var document = SwaggerDocument.FromFileAsync(swaggerPath).Result;

            var Actions = document.Paths.SelectMany(path => path.Value.Select(m => new Action(path.Key, m))).ToList();
        }
        
    }
}