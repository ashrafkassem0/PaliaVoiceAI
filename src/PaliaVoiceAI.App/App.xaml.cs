using System.Windows;
using System.Windows.Threading;
using PaliaVoiceAI.Core;

namespace PaliaVoiceAI.App;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            var typeName = args.Exception?.GetBaseException().GetType().Name ?? "Exception";
            SessionLog.WriteError(AppPaths.Logs, "task", typeName);
            args.SetObserved();
        };
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var typeName = e.Exception.GetType().Name;
        SessionLog.WriteError(AppPaths.Logs, "app", typeName);
        try
        {
            if (MainWindow is MainWindow window)
                window.ReportFailure(typeName);
        }
        catch (Exception ex) when (ex is InvalidOperationException or NullReferenceException)
        {
        }

        e.Handled = true;
    }
}
