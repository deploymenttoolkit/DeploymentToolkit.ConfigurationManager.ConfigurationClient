using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services
{
    public class NavigationService
    {
        private readonly Stack<NavigationViewItem> _navigationItems = new();
        private readonly Dictionary<string, Type> _windows = new();
        private Frame? _navigationFrame;

        public NavigationService()
        {
            var windows = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(Page));
            foreach (var type in windows)
            {
                if (type.Name == nameof(SettingsPage))
                {
                    _windows.Add($"Settings", type);
                }
                else
                {
                    _windows.Add(type.Name, type);
                }
            }
        }

        public void SetNavigationFrame(Frame navigationFrame)
        {
            _navigationFrame = navigationFrame;
        }

        public NavigationViewItem? GetCurrentNavigationViewItem()
        {
            if(_navigationItems.Count == 0)
            {
                return null;
            }
            return _navigationItems.Peek();
        }

        public void Navigate(NavigationViewItem item)
        {
            if (_navigationFrame == null)
            {
                throw new NullReferenceException(nameof(_navigationFrame));
            }

            if (item == null)
            {
                return;
            }

            var pageName = item.Tag as string;
            if(!string.IsNullOrEmpty(pageName) && _windows.ContainsKey(pageName))
            {
                _navigationFrame.Navigate(_windows[pageName]);
                _navigationItems.Push(item);
            }
        }

        public bool CanGoBack()
        {
            return _navigationFrame?.CanGoBack ?? false;
        }

        public void GoBack()
        {
            if (_navigationFrame == null)
            {
                throw new NullReferenceException(nameof(_navigationFrame));
            }

            _navigationFrame.GoBack();
            _navigationItems.Pop();
        }
    }
}
