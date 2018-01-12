using NJsonSchema;
using System.Linq;

namespace SwaggerCheck
{
    /// <summary>
    /// Use to check equality of complex parameters
    /// </summary>
    public static class SchemaEquality
    {
        public static bool CheckEqual(this JsonSchema4 lhs, JsonSchema4 rhs) //TODO make this an extension
        {
            //Types match
            if (lhs.Type.ToString() != rhs.Type.ToString())
            {
                return false;
            }

            //Check properties in schema
            foreach (var property in lhs.ActualProperties.Keys)
            {
                if (!rhs.ActualProperties.Any(i => i.Key.ToLower().Contains(property.ToLower())))
                {
                    return false;
                }
            }

            //If schema represents an array
            if (rhs.Type.ToString() == "Array")
            {
                //Get type of object in array
                return lhs.ActualSchema.Item.ActualSchema.CheckEqual(rhs.ActualSchema.Item.ActualSchema);
            }

            //Type mismatch
            return false;
        }
    }
}
