using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace VpServiceAPI.Entities.Tools
{
    public class DBEntityBase
    {
        public object ToParameters()
        {
            var obj = new Dictionary<string, dynamic>();

            foreach (var prop in GetType().GetProperties())
            {
#pragma warning disable CS8604 // Possible null reference argument.
                obj.Add(ToDBName(prop.Name), prop.GetValue(this));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            return obj;
        }
        internal string ToDBName(string propertyName)
        {
            return string.Join("", propertyName.Select(x => char.IsUpper(x) ? $"_{x}" : x.ToString())).ToLower()[1..];
        }
    }
}
