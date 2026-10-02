using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace IFCInfo
{
    // Only this presentation window runs on a separate STA. Revit work stays on its API thread.
    internal sealed class LoadingWindow : IDisposable
    {
        private Window indicator;
        private readonly Window owner;
        private readonly bool wasEnabled;
        private volatile bool disposed;

        private LoadingWindow(Window owner, string title, string detail)
        {
            this.owner = owner;
            wasEnabled = owner.IsEnabled;
            var handle = new WindowInteropHelper(owner).Handle;
            double left = owner.Left + Math.Max(0, (owner.ActualWidth - 440) / 2);
            double top = owner.Top + Math.Max(0, (owner.ActualHeight - 190) / 2);
            var ready = new System.Threading.Tasks.TaskCompletionSource<bool>();
            var thread = new Thread(() =>
            {
                try
                {
                    var panel = new StackPanel { Margin = new Thickness(28) };
                    panel.Children.Add(IFCInfoWindow.Text(title, 22, "#37322B"));
                    var text = IFCInfoWindow.Text(detail, 13, "#716B63");
                    text.Margin = new Thickness(0, 12, 0, 20);
                    panel.Children.Add(text);
                    panel.Children.Add(new ProgressBar { IsIndeterminate = true, Height = 6,
                        Foreground = IFCInfoWindow.Brush("#90805C"), Background = IFCInfoWindow.Brush("#E8E2DA"), BorderThickness = new Thickness(0) });
                    indicator = new Window { Width = 440, SizeToContent = SizeToContent.Height, Left = left, Top = top,
                        WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false,
                        ShowActivated = false, Background = UiDesign.Background, FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                        Content = new Border { BorderBrush = IFCInfoWindow.Brush("#DDD6CE"), BorderThickness = new Thickness(1), Child = panel } };
                    if (handle != IntPtr.Zero) new WindowInteropHelper(indicator).Owner = handle;
                    indicator.ContentRendered += (s, e) => ready.TrySetResult(true);
                    indicator.Closed += (s, e) => indicator.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    if (disposed) return;
                    indicator.Show();
                    Dispatcher.Run();
                }
                catch (Exception) { /* The loading indicator must never abort Revit work. */ }
                finally { ready.TrySetResult(true); }
            }) { IsBackground = true, Name = "IFC loading indicator" };
            thread.SetApartmentState(ApartmentState.STA);
            owner.IsEnabled = false;
            thread.Start();
            ready.Task.Wait(TimeSpan.FromSeconds(3));
        }

        internal static LoadingWindow ShowWhile(Window owner, string title, string detail) => new LoadingWindow(owner, title, detail);

        public void Dispose()
        {
            disposed = true;
            if (indicator != null && !indicator.Dispatcher.HasShutdownStarted)
                indicator.Dispatcher.BeginInvoke(new Action(() => indicator.Close()));
            owner.IsEnabled = wasEnabled;
        }
    }
}
