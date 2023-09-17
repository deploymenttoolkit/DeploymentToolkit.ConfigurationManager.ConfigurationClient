using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace DeploymentToolkit.ConfigurationManager.ConfigurationClient;

public sealed partial class SplashScreen : WinUIEx.SplashScreen
{
    public SplashScreen(Window window) : base(window)
    {
        this.InitializeComponent();
    }

    public SplashScreen(Type window) : base(window)
    {
        this.InitializeComponent();

        if(Debugger.IsAttached)
        {
            IsAlwaysOnTop = false;
        }
    }

    protected async override Task OnLoading()
    {
        await base.OnLoading();
        
        await Task.Factory.StartNew(() => App.Current.Build());
    }
}
