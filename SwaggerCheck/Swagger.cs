using Newtonsoft.Json;
using NSwag;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SwaggerCheck
{
    /// <summary>
    /// An abstraction of a Swagger document
    /// </summary>
    class Swagger
    {
        public Dictionary<string, List<Action>> Routes { get; set; }
        public int Endpoints { get; set; }

        /// <summary>
        /// A constructor that creates a Swagger object from a file
        /// </summary>
        /// <param name="swaggerPath">Path to Swagger file</param>
        public Swagger(string swaggerPath)
        {
            SwaggerDocument document;
            if (Path.GetExtension(swaggerPath) == ".json")
            {
                document = SwaggerDocument.FromFileAsync(swaggerPath).Result;
            }
            else
            {
                var fs = new FileStream(swaggerPath, FileMode.Open);
                var reader = new StreamReader(fs);
                var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention()); //TODO this is deprecated, update to use deserializer
                var yamlObject = deserializer.Deserialize(reader);

                // now convert the object to JSON. Simple!
                JsonSerializer js = new JsonSerializer();

                var writer = new StringWriter();
                js.Serialize(writer, yamlObject);
                string jsonText = writer.ToString();
                document = SwaggerDocument.FromJsonAsync(jsonText).Result;
            }

            Routes = new Dictionary<string, List<Action>>();

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
                    Routes.Add(path, new List<Action>() {new Action(operationDescription)});
                }
            });

            foreach (var r in Routes.Values)
            {
                Endpoints += r.Count();
            }
            ;
        }
    }
}