﻿using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;
using System;
using System.Collections.ObjectModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models
{
    public partial class DynamicWMIClass : ObservableObject, IWindowsManagementInstrumentationInstance
    {
        public string Key => throw new InvalidOperationException();

        public string Namespace { get; }

        public string Class { get; }

        public ObservableCollection<WindowsManagementInstrumentationProperty> Properties { get; } = new();

        public DynamicWMIClass(string namespaceName, string className)
        {
            Namespace = namespaceName;
            Class = className;
        }
    }
}
