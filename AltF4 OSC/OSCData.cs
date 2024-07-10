using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using OscQueryLibrary;
using VRChatOSCLib;

namespace AltF4_OSC;

public static class OscData
{
    private static readonly VRChatOSC Osc = new();

    private static readonly OscQueryServer OscQueryServer = new(GenerateRandomPrefixedString(), "127.0.0.1");
    internal static void Dispose()
    {
        Osc.Dispose();
        OscQueryServer.Dispose();
    }

    private static void InvokeMessageOnMainThread(string message)
    {
        Application.Current.Dispatcher.Invoke(() => { MainWindow.Message(message); });
    }


    internal static void Start()
    {
        Osc.Connect(OscQueryServer.OscSendPort);
        Osc.Listen(OscQueryServer.OscReceivePort);
        Osc.OnMessage += OnMessageReceived;
    }
    
    
    private static void OnMessageReceived(object? source, VRCMessage message)
    {
        VRCMessage.MessageType messageType = message.Type;

        switch (messageType)
        {
            case VRCMessage.MessageType.AvatarParameter:
                switch (message.AvatarParameter)
                {
                    case "Misc/Disconnect":
                        if (message.GetValue() is bool disconnectBoolean)
                        {
                            if (disconnectBoolean)
                            {
                                InvokeMessageOnMainThread($"{message.AvatarParameter} Changed state to | {disconnectBoolean}");
                    
                                if (MainWindow.Instance.IsCheckBoxChecked)
                                { 
                                    InvokeMessageOnMainThread("Conditions met, closing VRChat.");
                                    YeetVrc("VRChat");
                                }
                            }
                            else
                            {
                                InvokeMessageOnMainThread($"{message.AvatarParameter} Changed state to | {disconnectBoolean}");
                            }
                        }
                        break;
                    
                    default: 
                        message.Print(); 
                        break;
                }
                break;
            
            case VRCMessage.MessageType.AvatarChange:
                if (message.GetValue() is string s) InvokeMessageOnMainThread($"Avatar Changed to {s}");
                break;
        }
    }
    private static string GenerateRandomPrefixedString()
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
