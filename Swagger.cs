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

        public Swagger(string swaggerPath)
        {
            var document = SwaggerDocument.FromFileAsync(swaggerPath).Result;

            Actions = document.Operations.Select(operationDescription =>  new Action(operationDescription)).ToList();
        }
        
    }
}