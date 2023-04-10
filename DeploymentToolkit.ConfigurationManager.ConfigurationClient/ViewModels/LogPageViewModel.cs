using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeploymentToolkit.ConfigurationManager.ConfigurationClient.Models;
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
        private bool _isUpdating = true;

#if DEBUG
        public bool EnableTabs { get; set; } = true;
#else
        public bool EnableTabs { get; set; } = false;
#endif

        private object _logFileLock = new();
        private List<LogFile> _logNames = new();
        public CollectionViewSource LogNames = new();

        private const string _registryLogPath = @"SOFTWARE\Microsoft\CCM\Logging\@Global";
        private const string _registryInstallPath = @"SOFTWARE\Microsoft\SMS\Client\Configuration\Client Properties";

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

        private static Regex _logFileRegex = new Regex(@"(.*)-(\d*)-(\d*)\.log");
        private static Regex _userLogFileRegex = new Regex(@"(?:_)?(.*)_(.{3})@(.*)_\d\.log");

        private List<LogFile> _logFiles;
        private IEnumerable<IGrouping<string, LogFile>> _groupedLogFiles;

        private FileSystemWatcher _watcher;

        public LogPageViewModel()
        {
            Task.Run(() => Setup());
        }

        private void Setup()
        {
            try
            {
                var loggingKey = Registry.LocalMachine.OpenSubKey(_registryLogPath, false);
                var logDirectory = loggingKey.GetValue("LogDirectory") as string;

                _logFiles = new List<LogFile>();
                var files = Directory.GetFiles(logDirectory, "*.log");
                foreach (var file in files)
                {
                    _logFiles.Add(ParseLogFile(file));
                }

                _groupedLogFiles = _logFiles.GroupBy(f => f.Name);

                foreach (var log in _groupedLogFiles)
                {
                    _logNames.Add(log.OrderByDescending(l => l.LastModified).First());
                }

                _watcher = new FileSystemWatcher(logDirectory, "*.log");
                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Changed += OnFileChanged;
                _watcher.EnableRaisingEvents = true;

                App.Current.DispatcherQueue.TryEnqueue(() =>
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
                    IsUpdating = false;

                    App.Current.DispatcherQueue.TryEnqueue(() =>
                    {
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

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                var file = ParseLogFile(e.FullPath);
                lock (_logFileLock)
                {
                    if (!_logNames.Contains(file))
                    {
                        _logNames.Add(file);
                        LogNames.Source = _logNames.OrderBy(f => f.Name);
                    }
                    _logFiles.Add(file);
                    _groupedLogFiles = _logFiles.GroupBy(f => f.Name);
                }
            });
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                var file = ParseLogFile(e.FullPath, false);
                lock (_logFileLock)
                {
                    if (_logNames.Contains(file) && _logFiles.Count(f => f.Name == file.Name) <= 1)
                    {
                        _logNames.Remove(file);
                        LogNames.Source = _logNames.OrderBy(f => f.Name);
                    }
                    if (_logFiles.Contains(file))
                    {
                        _logFiles.Remove(file);
                    }
                    _groupedLogFiles = _logFiles.GroupBy(f => f.Name);
                }
            });
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                lock (_logFileLock)
                {
                    var file = _logFiles.FirstOrDefault(f => f.Path == e.FullPath);
                    if (file != null)
                    {
                        file.LastModified = (new FileInfo(e.FullPath)).LastWriteTime;
                    }
                }
            });
        }

        private LogFile ParseLogFile(string filePath, bool includeProperties = true)
        {
            var fileInfo = includeProperties ? new FileInfo(filePath) : null;
            var fileName = fileInfo?.Name ?? Path.GetFileNameWithoutExtension(filePath);
            var fullName = fileInfo?.FullName ?? Path.GetFullPath(filePath);

            var fileMatch = _logFileRegex.Match(fileName);
            if (fileMatch.Success)
            {
                return new LogFile(this, fileMatch.Groups[1].Value, fullName)
                {
                    Created = fileInfo?.CreationTime ?? null,
                    LastModified = fileInfo?.LastWriteTime ?? null
                };
            }

            var userMatch = _userLogFileRegex.Match(fileName);
            if(userMatch.Success)
            {
                return new LogFile(this, userMatch.Groups[1].Value, fullName)
                {
                    Created = fileInfo?.CreationTime ?? null,
                    LastModified = fileInfo?.LastWriteTime ?? null
                };
            }

            return new LogFile(this, Path.GetFileNameWithoutExtension(fileName), fullName)
            {
                Created = fileInfo?.CreationTime ?? null,
                LastModified = fileInfo?.LastWriteTime ?? null
            };
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        [RelayCommand]
        private void OpenLogFileInTab(string logName)
        {
            var page = new CMTraceFrame(
                Path.Combine(InstallPath, "CMTrace.exe"),
                _groupedLogFiles.First(g => g.Key == logName).OrderByDescending(f => f.LastModified).First().Path
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
                    Arguments = _groupedLogFiles.First(g => g.Key == logName).OrderByDescending(f => f.LastModified).First().Path
                }
            }.Start();
        }

        [RelayCommand]
        private void OpenLogFiles(string logName)
        {
            foreach (var logFile in _groupedLogFiles.First(g => g.Key == logName).Select(f => f.Path))
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
