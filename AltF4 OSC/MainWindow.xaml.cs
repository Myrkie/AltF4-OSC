using System;
using System.Windows;

namespace WPF_OSC_Keyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _instance;

        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainWindow();
                }
                return _instance;
            }
        }
        public MainWindow()
        {
            _instance = this;
            InitializeComponent();
            OSCData.Start();
        }
        
        
        public bool IsCheckBoxChecked
        {
            get
            {
                bool isChecked = false;
                Dispatcher.Invoke(() =>
                {
                    isChecked = EnableCheckBox.IsChecked ?? false;
                });
                return isChecked;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            OSCData.Dispose();
            Application.Current.Shutdown();
        }

        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.ConsoleInitalize();
        }

        private void ClearBox(object sender, RoutedEventArgs e)
        {
            NotificationBox.Clear();
        }

        private void NotifBoxProvider(string message)
        {
            NotificationBox.AppendText($"{message}\n");
            if (AutoScroll.IsChecked == true)
            {
                NotificationBox.ScrollToEnd();
            }
        }
        
        internal static void Message(string message)
        {
            _instance.NotifBoxProvider(message);
        }
    }
}