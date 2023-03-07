using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Type> _windows = new();
        private Frame _navigationFrame;

        public NavigationService()
        {
            var windows = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(Page));
            foreach (var type in windows)
            {
                _windows.Add(type.Name, type);
            }
        }

        public void SetNavigationFrame(Frame navigationFrame)
        {
            _navigationFrame = navigationFrame;
        }

        public void Navigate(NavigationViewItem item)
        {
            if(item == null)
            {
                return;
            }

            var pageName = item.Tag as string;
            if(_windows.ContainsKey(pageName))
            {
                _navigationFrame.Navigate(_windows[pageName]);
            }
        }

        public bool CanGoBack()
        {
            return _navigationFrame.CanGoBack;
        }
    }
}
