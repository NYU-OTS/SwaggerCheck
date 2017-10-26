using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NJsonSchema;

namespace APICheck
{
    class ActionComparer : IEqualityComparer<Action>
    {
        public bool Equals(Action x, Action y)
        {
            var kEqual = x.simpleParams.Keys.OrderBy(i => i).SequenceEqual(
                y.simpleParams.Keys.OrderBy(i => i));

            if (!kEqual || x.Route != y.Route || !x.Httpmethods.SetEquals(y.Httpmethods))
                return false;

            foreach (var varName in x.simpleParams.Keys)
            {
                String type;
                y.simpleParams.TryGetValue(varName, out type);
                if (x.simpleParams[varName] != y.simpleParams[varName])
                    Console.Error.WriteLine("No matching parameter " + varName);
                    return false;  
            }

            foreach (var schema in x.complexParams.Keys)
            {
                JsonSchema4 type;
                if (y.complexParams.TryGetValue(schema, out type))
                {
                    if (!x.complexParams[schema].CheckEqual(type))
                    {
                        Console.Error.WriteLine("No matching parameter " + schema);
                        return false;
                    }
                    //else do nothing
                }
                else
                {
                    Console.Error.WriteLine("No matching parameter " + schema + " in " + y.Route);
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(Action obj)
        {
            return 0;
        }
    }
}
