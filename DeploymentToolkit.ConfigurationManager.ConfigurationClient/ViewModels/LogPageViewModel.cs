using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models.SMB;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Services;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Views.Frames;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient.ViewModels
{
    public partial class LogPageViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private bool _isLoading = true;

#if DEBUG
        public bool EnableTabs { get; set; } = true;
#else
        public bool EnableTabs { get; set; } = false;
#endif

        private object _logFileLock = new();
        private List<IFileDirectoryInformation> _logNames = new();
        public CollectionViewSource LogNames = new();

        private const string _ccmLogPath = @"C:\Windows\CCM\Logs";
        private const string _registryInstallPath = @"SOFTWARE\Microsoft\SMS\Client\Configuration\Client Properties";

        // TODO: This is not read from remote host
        private string _installPath;
        internal string InstallPath
        {
            get
            {
                if(string.IsNullOrEmpty(_installPath))
                {
                    var registryKey = Registry.LocalMachine.OpenSubKey(_registryInstallPath, false);
                    _installPath = registryKey.GetValue("Local SMS Path") as string;
                }
                return _installPath;
            }
        }

        public TabView LogFilesTabView;

        public ObservableCollection<TabViewItem> Tabs { get; set; } = new();
        public int SelectedIndex { get; set; }

        //private static readonly Regex _logFileRegex = new(@"(.*)-(\d*)-(\d*)\.log");
        //private static readonly Regex _userLogFileRegex = new(@"(?:_)?(.*)_(.{3})@(.*)_\d\.log");

        private readonly List<IFileDirectoryInformation> _logFiles = new();
        private IEnumerable<IGrouping<string, IFileDirectoryInformation>> _groupedLogFiles;

        //private FileSystemWatcher _watcher;

        private readonly IFileExplorer _fileExplorer;

        public LogPageViewModel(ClientConnectionManager clientConnectionManager)
        {
            _fileExplorer = clientConnectionManager.FileExplorerConnection;

            Task.Run(() => Setup());
        }

        private async void Setup()
        {
            try
            {
                await Task.Delay(5);

                _logFiles.Clear();

                var files = _fileExplorer.GetFilesAndFolderInDirectory(_ccmLogPath).Where(f => f.Name.EndsWith(".log"));
                foreach (var file in files)
                {
                    file.ViewModel = this;
                    _logFiles.Add(file);
                }

                _groupedLogFiles = _logFiles.GroupBy(f => f.Name);

                foreach (var log in _groupedLogFiles)
                {
                    _logNames.Add(log.OrderByDescending(l => l.LastWriteTime).First());
                }

                //_watcher = new FileSystemWatcher(logDirectory, "*.log");
                //_watcher.Created += OnFileCreated;
                //_watcher.Deleted += OnFileDeleted;
                //_watcher.Changed += OnFileChanged;
                //_watcher.EnableRaisingEvents = true;

                App.Current.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    LogNames.Source = _logNames.OrderBy(f => f.Name);
                    var newTab = new TabViewItem()
                    {
                        Header = "Logs",
                        IconSource = new SymbolIconSource() { Symbol = Symbol.Folder },
                        Content = new LogFilesFrame(this),
                        IsClosable = false,
                        CanDrag = false
                    };
                    Tabs.Add(newTab);

                    App.Current.DispatcherQueue.TryEnqueue(() =>
                    {
                        IsLoading = false;
                        SelectedIndex = 0;
                        // Databinding currently does not work in WinUI3
                        // https://github.com/microsoft/microsoft-ui-xaml/issues/3907
                        // So this is the workaround I guess
                        LogFilesTabView.SelectedIndex = 0;
                    });
                });
            }
            catch { }
        }

        //private void OnFileCreated(object sender, FileSystemEventArgs e)
        //{
        //    App.Current.DispatcherQueue.TryEnqueue(() =>
        //    {
        //        var file = ParseLogFile(e.FullPath);
        //        lock (_logFileLock)
        //        {
        //            if (!_logNames.Contains(file))
        //            {
        //                _logNames.Add(file);
        //                LogNames.Source = _logNames.OrderBy(f => f.Name);
        //            }
        //            _logFiles.Add(file);
        //            _groupedLogFiles = _logFiles.GroupBy(f => f.Name);
        //        }
        //    });
        //}

        //private void OnFileDeleted(object sender, FileSystemEventArgs e)
        //{
        //    App.Current.DispatcherQueue.TryEnqueue(() =>
        //    {
        //        var file = ParseLogFile(e.FullPath, false);
        //        lock (_logFileLock)
        //        {
        //            if (_logNames.Contains(file) && _logFiles.Count(f => f.Name == file.Name) <= 1)
        //            {
        //                _logNames.Remove(file);
        //                LogNames.Source = _logNames.OrderBy(f => f.Name);
        //            }
        //            if (_logFiles.Contains(file))
        //            {
        //                _logFiles.Remove(file);
        //            }
        //            _groupedLogFiles = _logFiles.GroupBy(f => f.Name);
        //        }
        //    });
        //}

        //private void OnFileChanged(object sender, FileSystemEventArgs e)
        //{
        //    if(e.ChangeType != WatcherChangeTypes.Changed)
        //    {
        //        return;
        //    }

        //    App.Current.DispatcherQueue.TryEnqueue(() =>
        //    {
        //        lock (_logFileLock)
        //        {
        //            var file = _logFiles.FirstOrDefault(f => f.Path == e.FullPath);
        //            if (file != null)
        //            {
        //                file.LastModified = (new FileInfo(e.FullPath)).LastWriteTime;
        //            }
        //        }
        //    });
        //}

        public void Dispose()
        {
            //_watcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        [RelayCommand]
        private void OpenLogFileInTab(string logName)
        {
            var page = new CMTraceFrame(
                Path.Combine(InstallPath, "CMTrace.exe"),
                _groupedLogFiles.First(g => g.Key == logName).OrderByDescending(f => f.LastWriteTime).First().GetFullPath()
            );

            Tabs.Add(new TabViewItem()
            {
                Header = logName,
                IconSource = new SymbolIconSource() { Symbol = Symbol.Document },
                Content = page
            });
        }

        [RelayCommand]
        private void OpenLogFile(string logName)
        {
            new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(InstallPath, "CMTrace.exe"),
                    Arguments = _groupedLogFiles.First(g => g.Key == logName).OrderByDescending(f => f.LastWriteTime).First().GetFullPath()
                }
            }.Start();
        }

        [RelayCommand]
        private void OpenLogFiles(string logName)
        {
            foreach (var logFile in _groupedLogFiles.First(g => g.Key == logName).Select(f => f.GetFullPath()))
            {
                new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Path.Combine(InstallPath, "CMTrace.exe"),
                        Arguments = logFile
                    }
                }.Start();
            }
        }


        [RelayCommand]
        private void TabCloseRequested(TabViewTabCloseRequestedEventArgs args)
        {
            Tabs.Remove(args.Tab);
        }
    }
}
