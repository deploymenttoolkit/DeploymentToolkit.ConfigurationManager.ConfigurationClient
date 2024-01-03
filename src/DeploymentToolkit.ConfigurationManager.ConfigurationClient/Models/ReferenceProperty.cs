using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Extensions;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class ReferenceProperty : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        public string? Value
        {
            get
            {
                return ConvertValue();
            }
        }

        private readonly IWindowsManagementInstrumentationStaticInstance _instance;
        private readonly PropertyInfo _propertyInfo;

        public ReferenceProperty(IWindowsManagementInstrumentationInstance instance, PropertyInfo property)
        {
            _name = property.Name;
            _instance = instance;
            _propertyInfo = property;
        }

        private string? ConvertValue()
        {
            var value = _propertyInfo.GetValue(_instance);
            if(value == null)
            {
                return null;
            }

            if(_propertyInfo.PropertyType.IsArray && (value as Array)!.Length > 0)
            {
                return (value as IEnumerable)!.Cast<string>().Aggregate((c, n) => $"{c}\n{n}");
            }
            else if(_propertyInfo.PropertyType.IsObservableCollection())
            {
                var result = new StringBuilder();
                foreach(var child in value as IList)
                {
                    result.AppendLine(child.ToString());
                }
                return result.ToString();
            }

            return value.ToString();
        }
    }
}
