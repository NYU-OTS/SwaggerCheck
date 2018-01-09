using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SwaggerCheck
{
    class ActionComparer : IEqualityComparer<Action>
    {
        private readonly ILogger _logger;

        public ActionComparer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ActionComparer>();
        }
        public bool Equals(Action lhs, Action rhs)
        {
            _logger.LogDebug("Comparing");
            if (lhs.Method != rhs.Method)
            {
                return false;
            }

            var equals = true; //So all parameters are compared before exiting

            foreach (var variable in lhs.simpleParams.Keys)
            {
                String type;
                rhs.simpleParams.TryGetValue(variable, out type);
                if (lhs.simpleParams[variable] != rhs.simpleParams[variable])
                {
                    Console.Error.WriteLine($"No matching parameter {variable} in {rhs.Route}");
                    equals = false;
                }
            }

            foreach (var variable in lhs.complexParams.Keys)
            {
                JsonSchema4 type;
                //check if it's in rhs's complex parameters
                if (rhs.complexParams.TryGetValue(variable, out type))
                {
                    //check if the schema is equal
                    if (!lhs.complexParams[variable].CheckEqual(type))
                    {
                        Console.Error.WriteLine($"Parameter {variable} type mismatch");
                        equals = false;
                    }
                    //else do nothing
                }
                else
                {
                    Console.Error.WriteLine($"No matching parameter {variable} in {rhs.Route}");
                    equals = false;
                }
            }

            return equals;
        }

        public int GetHashCode(Action obj)
        {
            return 0;
        }
    }
}
