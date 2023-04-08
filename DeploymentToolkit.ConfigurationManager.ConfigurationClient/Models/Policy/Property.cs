using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.Policy
{
    public partial class Property : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _value;

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
                if(Value.StartsWith("0x"))
                {
                    try
                    {
                        var errorCode = Convert.ToInt32(Value, 16);
                        var exception = Marshal.GetExceptionForHR(errorCode);
                        Value += $"\n{exception.Message}";
                    }
                    catch(Exception) { }
                }
            }
        }
    }
}
