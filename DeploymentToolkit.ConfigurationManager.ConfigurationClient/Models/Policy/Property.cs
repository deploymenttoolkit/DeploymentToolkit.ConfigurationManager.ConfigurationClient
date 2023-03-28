using System.Collections;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public class Property
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Property(string name, object value)
        {
            Name = name;

            if(value == null)
            {
                return;
            }

            if(value.GetType().IsArray)
            {
                Value = (value as IEnumerable).Cast<string>().Aggregate((c, n) => $"{c}\n{n}");
            }
            else
            {
                Value = value.ToString();
            }
        }
    }
}
