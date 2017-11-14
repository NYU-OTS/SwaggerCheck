﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJsonSchema;

namespace APICheck
{
    /// <summary>
    /// Use to check equality of complex parameters
    /// </summary>
    public static class SchemaEquality
    {
        public static bool CheckEqual(this JsonSchema4 x, JsonSchema4 y)
        {
            foreach (var property in x.ActualProperties.Keys)
            {
                if (!y.ActualProperties.Any(i => i.Key.ToLower().Contains(property.ToLower())))
                {
                    return false;
                }
            }

            //If schema represents an array
            if (x.Type.ToString() == "Array")
            {
                if (y.Type.ToString() == "Array")
                {
                    return x.ActualSchema.Item.ActualSchema.CheckEqual(y.ActualSchema.Item.ActualSchema);
                }
                {
                    //type mismatch
                    return false;
                }
            }
            

            return true;
        }
    }
}
