using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using OscQueryLibrary;
using VRChatOSCLib;

namespace WPF_OSC_Keyboard;

public static class OSCData
{
    static VRChatOSC osc = new();

    private static OscQueryServer _oscQueryServer;
    internal static void Dispose()
    {
        osc.Dispose();
        _oscQueryServer.Dispose();
    }

    static void InvokeMessageOnMainThread(string message)
    {
        Application.Current.Dispatcher.Invoke(() => { MainWindow.Message(message); });
    }


    internal static void Start()
    {
        _oscQueryServer = new OscQueryServer(
            GenerateRandomPrefixedString(),
            "127.0.0.1"
        );
        
        osc.Connect(OscQueryServer.OscSendPort);
        osc.Listen(OscQueryServer.OscReceivePort);
        osc.OnMessage += OnMessageReceived;
    }
    
    
    public static string GenerateRandomPrefixedString()
    {
        Random random = new Random();
        StringBuilder stringBuilder = new StringBuilder("AltF4-OSC-");

        for (int i = 0; i < 5; i++)
        {
            int randomNumber = random.Next(0, 10);
            stringBuilder.Append(randomNumber);
        }
        
        char randomLetter = (char)random.Next('A', 'Z' + 1);
        stringBuilder.Append(randomLetter);

        return stringBuilder.ToString();
    }
    
    static void OnMessageReceived(object? source, VRCMessage message)
    {
        VRCMessage.MessageType messageType = message.Type;

        if (messageType != VRCMessage.MessageType.AvatarParameter ||
            message.AvatarParameter != "Misc/Disconnect") return;
        object? value = message.GetValue();
        if (value is bool booleanValue)
        {
            if (booleanValue)
            {
                InvokeMessageOnMainThread($"{message.AvatarParameter} Changed state to | {booleanValue}");
                    
                if (MainWindow.Instance.IsCheckBoxChecked)
                { 
                    InvokeMessageOnMainThread("Conditions met, closing vrchat");
                    YeetVrc("vrchat");
                }
            }
            else
            {
                InvokeMessageOnMainThread($"{message.AvatarParameter} Changed state to | {booleanValue}");
            }
        }
    }

    
    private static void YeetVrc(string processName)
    {
        int delayMilliseconds = 800;

        var processes = Process.GetProcessesByName(processName);

        if (processes.Length == 1)
        {
            Process process = processes[0];
            InvokeMessageOnMainThread($"Found process: {process.ProcessName}, PID: {process.Id}");
            Thread.Sleep(delayMilliseconds);

            try
            {
                process.CloseMainWindow();
                InvokeMessageOnMainThread($"Process {process.ProcessName} terminated.");
            }
            catch (Exception ex)
            {
                InvokeMessageOnMainThread($"Failed to terminate process {process.ProcessName}: {ex.Message}");
            }
        }
    }
}
