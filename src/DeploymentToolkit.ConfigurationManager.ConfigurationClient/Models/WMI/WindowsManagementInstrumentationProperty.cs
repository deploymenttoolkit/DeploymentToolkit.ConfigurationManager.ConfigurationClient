using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI
{
    public partial class WindowsManagementInstrumentationProperty : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _value;

        public WindowsManagementInstrumentationProperty(string name, object value)
        {
            Name = name;

            if(value == null)
            {
                return;
            }

            if(value.GetType().IsArray && (value as Array).Length > 0)
            {
                Value = (value as IEnumerable).Cast<string>().Aggregate((c, n) => $"{c}\n{n}");
            }
            else
            {
                Value = value.ToString();
                if(Value.StartsWith("0x") && Value != "0x00000000")
                {
                    try
                    {
                        var errorCode = Convert.ToInt32(Value, 16);
                        var exception = Marshal.GetExceptionForHR(errorCode);
                        if (exception != null)
                        {
                            Value += $"\n{exception.Message}";
                        }
                    }
                    catch(Exception) { }
                }
            }
        }
    }
}
