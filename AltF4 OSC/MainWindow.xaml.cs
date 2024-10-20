using System;
using System.Threading.Tasks;
using System.Windows;
using AltF4_OSC.Misc;
using Serilog;

namespace AltF4_OSC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(MainWindow));

        private static MainWindow? _instance;

        public static MainWindow Instance
        {
            get { return _instance ??= new MainWindow(); }
        }
        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
                .CreateLogger();
            _instance = this;
            InitializeComponent();

            AutoScroll.IsChecked = Config.Instance.AutoScroll;
            
            Task.Run(async () =>
            {
                try
                {
                    await OscData.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error starting OscData.");
                }
            });
        }
        
        public bool IsCheckBoxChecked
        {
            get
            {
                var isChecked = false;
                Dispatcher.Invoke(() =>
                {
                    isChecked = EnableCheckBox.IsChecked ?? false;
                });
                return isChecked;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            OscData.Dispose();
            Application.Current.Shutdown();
        }

        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.ToggleConsole();
        }

        private void ClearBox(object sender, RoutedEventArgs e)
        {
            NotificationBox.Clear();
        }

        private void NotificationBoxProvider(string message)
        {
            NotificationBox.AppendText($"{message}\n");
            if (AutoScroll.IsChecked == true)
            {
                NotificationBox.ScrollToEnd();
            }
        }
        
        internal static void Message(string message)
        {
            _instance?.NotificationBoxProvider(message);
        }
    }
}