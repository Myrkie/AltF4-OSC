using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WPF_OSC_Keyboard
{
    public static class ConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern int AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static void ShowConsole()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        public static void ConsoleInitalize()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.Clear();
            ShowConsole();
        }
    }
}