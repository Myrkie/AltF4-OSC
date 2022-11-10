using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rug.Osc;

namespace WPF_OSC_Keyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // shit way to get inputbox text from main thread without dispacher
        public static string Inputtextbox = "";
        // only allows the message sender to send if its true
        public static bool TextPopulated;
        // define main window form
        private static MainWindow form;
        // text position for message box
        private int _textpos;
        public MainWindow()
        {
            form = this;
            InitializeComponent();
            InputText.KeyDown += KeydownAction;
            InputText.TextChanged += TextChangedText;
            ChatBox.TextChanged += TextChanged;
            Console.WriteLine("ConsoleInitialized");
            OSCData.start();

        }
        // forces UI to Top after deselecting chatbox
        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var window = (Window)sender;
            window.Topmost = true;
        }
        
        private void TextChangedText(object sender, RoutedEventArgs e)
        {
            Inputtextbox = form.InputText.Text;
        }

        // clear text after its reached a certian length
        private void TextChanged(object sender, RoutedEventArgs e)
        {
            if (ChatBox.Text.Length > 500)
            {
                // this doesnt work and will overload the UI Thread not sure how to fix - dont wanna clear all text only the first 450
                // var remove = ChatBox.Text.Remove(0, 450);
                ChatBox.Clear();
            }
            ChatBox.ScrollToEnd();
            InputText.Clear();
        }
        
        private void KeydownAction(object sender, KeyEventArgs keyargs)
        {
            switch (keyargs.Key)
            {
                case Key.Enter:
                    TextPopulated = true;
                    ChatBoxProvider(InputText.Text);
                    break;
                // only triggers when inputtextbox is focused
                case Key.F12:
                    ConsoleManager.ConsoleInitalize();
                    break;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            TextPopulated = true;
            ChatBoxProvider(InputText.Text); 
        }        
        private void ConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.ConsoleInitalize();
        }

        private void ChatBoxProvider(string message)
        {
            if (_textpos == 0)
            {
                ChatBox.Text += message;
                _textpos = 1;
            }else ChatBox.Text += $"\n{message}";
        }
        
        internal static void Message(string message)
        {
            form.ChatBoxProvider(message);
        }

    }
    class OSCData
    {
        private static List<string> _pathBlacklist = new List<string>()
        {
            "/avatar/parameters/IsLocal",
            "/avatar/parameters/Voice",
            "/avatar/parameters/VelocityX",
            "/avatar/parameters/VelocityY",
            "/avatar/parameters/VelocityZ",
            "/avatar/parameters/AngularX",
            "/avatar/parameters/AngularY",
            "/avatar/parameters/AngularZ",
            "/avatar/parameters/Viseme",
            "/avatar/parameters/Grounded",
            "/avatar/parameters/Upright",
            "/avatar/parameters/GestureLeft",
            "/avatar/parameters/GestureRight",
            "/avatar/parameters/GestureLeftWeight",
            "/avatar/parameters/GestureRightWeight",
            "/avatar/parameters/Seated",
            "/avatar/parameters/MuteSelf",
            "/avatar/parameters/InStation",
            "/avatar/parameters/TrackingType",
            "/avatar/parameters/VRMode"
        };

        static OscReceiver _receiver;
        static OscSender _sender;
        private static bool _isallowed;



        internal static void start()
        {
            static void InvokeMessageOnMainThread(string message)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.Message(message);
                });
            }
            _sender = new OscSender(IPAddress.Parse("127.0.0.1"), 0, 9000);
            _receiver = new OscReceiver(9001);

            // Connect the Sender
            _sender.Connect();
            
            // Connect the receiver
            _receiver.Connect();

            // Create a thread to do the listening
            var threadlisten = new Thread(ListenLoop);

            var chatthread = new Thread(ChatLoop);
            
            
            // start threads
            threadlisten.Start();
            chatthread.Start();

            switch (_sender.State)
            {
                case OscSocketState.Connected:
                    Console.WriteLine("Connected");
                    InvokeMessageOnMainThread("Connected");
                    _isallowed = true;
                    break;
                case OscSocketState.NotConnected:
                    Console.WriteLine("Not Connected");
                    InvokeMessageOnMainThread("Not Connected");
                    _isallowed = false;
                    break;
                case OscSocketState.Closing:
                    Console.WriteLine("Connection Closing");
                    InvokeMessageOnMainThread("Connection Closing");
                    _isallowed = false;
                    break;
                case OscSocketState.Closed:
                    Console.WriteLine("Connection Closed");
                    InvokeMessageOnMainThread("Connection Closed");
                    _isallowed = false;
                    break;
            }


            static void ChatLoop()
            {
                try
                {
                    while (_isallowed = true)
                    {
                        if (MainWindow.TextPopulated)
                        {
                            _sender.Send(new OscMessage("/chatbox/input", MainWindow.Inputtextbox, true));
                            MainWindow.TextPopulated = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            // receiver listen loop
            static void ListenLoop()
            {
                try
                {
                    while (_receiver.State != OscSocketState.Closed)
                    {
                        // if we are in a state to recieve
                        if (_receiver.State == OscSocketState.Connected)
                        {
                            OscPacket packet = _receiver.Receive();
                            var message = (OscMessage) packet;
                            
                            Console.WriteLine(packet.ToString());
                            //InvokeMessageOnMainThread(packet.ToString());
                            // disabled for now do not remove
                            if (!_pathBlacklist.Contains(message.Address)) InvokeMessageOnMainThread(packet.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }

        }
    }
}