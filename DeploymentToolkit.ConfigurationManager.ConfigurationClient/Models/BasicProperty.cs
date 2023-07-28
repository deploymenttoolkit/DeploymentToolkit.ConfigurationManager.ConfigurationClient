using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Linq;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class BasicProperty : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _value;

        public BasicProperty(string name, object value)
        {
            Name = name;

            if (value == null)
            {
                return;
            }

            if (value.GetType().IsArray && (value as Array).Length > 0)
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
