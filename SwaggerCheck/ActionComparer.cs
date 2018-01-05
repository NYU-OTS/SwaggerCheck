using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APICheck
{
    class ActionComparer : IEqualityComparer<Action>
    {
        public bool Equals(Action lhs, Action rhs)
        {
            var simpleParamsEqual = lhs.simpleParams.Keys.OrderBy(i => i)
                .SequenceEqual(rhs.simpleParams.Keys.OrderBy(i => i));

            if (!simpleParamsEqual || lhs.Method != rhs.Method)
            {
                return false; //No output if not the same route+method
            }

            foreach (var variable in lhs.simpleParams.Keys)
            {
                String type;
                rhs.simpleParams.TryGetValue(variable, out type);
                if (lhs.simpleParams[variable] != rhs.simpleParams[variable])
                {
                    Console.Error.WriteLine("No matching parameter " + variable);
                    return false;
                }
            }

            foreach (var variable in lhs.complexParams.Keys)
            {
                JsonSchema4 type;
                if (rhs.complexParams.TryGetValue(variable, out type))
                {
                    if (!lhs.complexParams[variable].CheckEqual(type))
                    {
                        Console.Error.WriteLine("No matching parameter " + variable);
                        return false;
                    }
                    //else do nothing
                }
                else
                {
                    Console.Error.WriteLine("No matching parameter " + variable + " in " + rhs.Route);
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
