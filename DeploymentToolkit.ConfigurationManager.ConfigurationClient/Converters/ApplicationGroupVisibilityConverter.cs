using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.CCM.ClientSDK;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Converters
{
    public class ApplicationGroupVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is not ApplicationType applicationType)
            {
                throw new ArgumentException("Invalid argument provided");
            }

            if(applicationType == ApplicationType.Application)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
