using CommunityToolkit.Mvvm.ComponentModel;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.cimv2
{
    public enum StartMode
    {
        Boot,
        System,
        Auto,
        Manual,
        Disabled
    }

    public enum ErrorControl
    {
        Ignore,
        Normal,
        Severe,
        Critical,
        Unknown
    }

    public enum ServiceType
    {
        KernelDriver,
        FileSystemDriver,
        Adapter,
        RecognizerDriver,
        OwnProcess,
        ShareProcess,
        InteractiveProcess
    }

    public enum ServiceState
    {
        Stopped,
        StartPending,
        StopPending,
        Running,
        ContinuePending,
        PausePending,
        Paused,
        Unknown
    }

    public partial class Win32_BaseService : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private StartMode _startMode;
        [ObservableProperty]
        bool _acceptPause;
        [ObservableProperty]
        bool _acceptStop;
        [ObservableProperty]
        bool _desktopInteract;
        [ObservableProperty]
        private string _displayName;
        [ObservableProperty]
        private ErrorControl _errorControl;
        [ObservableProperty]
        private string _pathName;
        [ObservableProperty]
        private ServiceType _serviceType;
        [ObservableProperty]
        private string _startName;
        [ObservableProperty]
        private ServiceState _state;
        [ObservableProperty]
        private uint _tagId;
        [ObservableProperty]
        private uint _exitCode;
        [ObservableProperty]
        private uint _serviceSpecificExitCode;
    }
}
