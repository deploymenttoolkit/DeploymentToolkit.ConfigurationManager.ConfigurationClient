using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.WMI;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.cimv2;
public partial class Win32_Process : ObservableObject, IWindowsManagementInstrumentationInstance
{
    public string Namespace => @"ROOT\cimv2";
    public string Class => nameof(Win32_Process);
    public string Key => $@"Handle=""{Handle}""";
    public bool QueryByFilter => false;

    [ObservableProperty]
    private string _creationClassName;
    [ObservableProperty]
    private string _caption;
    [ObservableProperty]
    private string _commandLine;
    [ObservableProperty]
    private DateTime _creationDate;
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    private string _executablePath;
    [ObservableProperty]
    private ushort _executionState;
    [ObservableProperty]
    private string _handle;
    [ObservableProperty]
    private uint _handleCount;
    [ObservableProperty]
    private DateTime _installDate;
    [ObservableProperty]
    private ulong _kernelModeTime;
    [ObservableProperty]
    private uint _maximumWorkingSetSize;
    [ObservableProperty]
    private uint _minimumWorkingSetSize;
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private ulong _otherOperationCount;
    [ObservableProperty]
    private ulong _otherTransferCount;
    [ObservableProperty]
    private uint _pageFaults;
    [ObservableProperty]
    private uint _pageFileUsage;
    [ObservableProperty]
    private uint _parentProcessId;
    [ObservableProperty]
    private uint _peakPageFileUsage;
    [ObservableProperty]
    private ulong _peakVirtualSize;
    [ObservableProperty]
    private uint _peakWorkingSetSize;
    [ObservableProperty]
    private uint _priority;
    [ObservableProperty]
    private ulong _privatePageCount;
    [ObservableProperty]
    private uint _processId;
    [ObservableProperty]
    private uint _quotaNonPagedPoolUsage;
    [ObservableProperty]
    private uint _quotaPagedPoolUsage;
    [ObservableProperty]
    private uint _quotaPeakNonPagedPoolUsage;
    [ObservableProperty]
    private uint _quotaPeakPagedPoolUsage;
    [ObservableProperty]
    private ulong _readOperationCount;
    [ObservableProperty]
    private ulong _readTransferCount;
    [ObservableProperty]
    private uint _sessionId;
    [ObservableProperty]
    private string _status;
    [ObservableProperty]
    private DateTime _terminationDate;
    [ObservableProperty]
    private uint _threadCount;
    [ObservableProperty]
    private ulong _userModeTime;
    [ObservableProperty]
    private ulong _virtualSize;
    [ObservableProperty]
    private string _windowsVersion;
    [ObservableProperty]
    private ulong _workingSetSize;
    [ObservableProperty]
    private ulong _writeOperationCount;
    [ObservableProperty]
    private ulong _writeTransferCount;
}
