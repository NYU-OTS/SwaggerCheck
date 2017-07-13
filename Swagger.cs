using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;


namespace APICheck
{
    class Swagger
    {
        public IEnumerable<Action> Actions { get; set; }
        public Swagger()
        {
            /*
            //handle $ref
            var swaggerPath = @"C:\Users\bt1124\Documents\Visual Studio 2017\Projects\APICheck\eswagger.json";
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };
           
            var result = JsonConvert.DeserializeObject<JObject>(json, settings);
             var email = result.SelectToken(@"paths./email.post.parameters[0].schema.items");
            */

            //Processing JSON
            var swaggerPath = @"C:\Users\bt1124\Documents\Visual Studio 2017\Projects\APICheck\swagger.json";
            var json = File.ReadAllText(swaggerPath);
            var result = JsonConvert.DeserializeObject<JObject>(json);

            var pathObj = result[@"paths"];//Converts JToken to JObject
            var paths = pathObj.Value<JObject>().Properties().Select(p => p.Name);

            //need a property to access
            Actions = paths.Select(m => new Action(pathObj[m], m)).ToList();

        }
    }
}
