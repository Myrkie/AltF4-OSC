using System;
using System.Windows;

namespace AltF4_OSC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow? _instance;

        public static MainWindow Instance
        {
            get { return _instance ??= new MainWindow(); }
        }
        public MainWindow()
        {
            _instance = this;
            InitializeComponent();
            OscData.Start();
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
            ConsoleManager.InitializeConsole();
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