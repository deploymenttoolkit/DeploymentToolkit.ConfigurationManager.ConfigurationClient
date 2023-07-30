using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using Microsoft.UI.Xaml.Data;
using System;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Converters
{
    public class SoftwareUpdateStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is not SoftwareUpdate softwareUpdate)
            {
                throw new NotSupportedException();
            }

            if(softwareUpdate.EvaluationState == SoftwareUpdateEvaluationState.ciJobStateDownloading || softwareUpdate.EvaluationState == SoftwareUpdateEvaluationState.ciJobStateInstalling)
            {
                return $"{softwareUpdate.EvaluationState} ({softwareUpdate.PercentComplete})";
            }
            return softwareUpdate.EvaluationState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
