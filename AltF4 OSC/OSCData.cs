using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using VRC.OSCQuery;
using VRChatOSCLib;

namespace WPF_OSC_Keyboard;

public static class OSCData
{
    static VRChatOSC osc = new();

    internal static void Dispose()
    {
        osc.Dispose();
        service.Dispose();
    }

    static void InvokeMessageOnMainThread(string message)
    {
        Application.Current.Dispatcher.Invoke(() => { MainWindow.Message(message); });
    }

    private static OSCQueryService service;

    internal static void start()
    {
        service = new OSCQueryServiceBuilder().WithServiceName("AltF4 OSC").WithUdpPort(Extensions.GetAvailableUdpPort()).WithTcpPort(Extensions.GetAvailableTcpPort()).WithDefaults().Build();
        
        osc.OnConnectionStateChanged += OnConnectionStateChanged;
        osc.Connect();
        osc.Listen();
        osc.OnMessage += OnMessageReceived;
    }
    
    static void OnMessageReceived(object? source, VRCMessage message)
    {
        message.Print();
        
        VRCMessage.MessageType messageType = message.Type;

        if (messageType != VRCMessage.MessageType.AvatarParameter ||
            message.AvatarParameter != "Misc/Disconnect") return;
        object? value = message.GetValue();
        if (value is bool booleanValue)
        {
            if (booleanValue)
            {
                InvokeMessageOnMainThread($"{message.AvatarParameter} | State: true");
                    
                if (MainWindow.Instance.IsCheckBoxChecked)
                { 
                    InvokeMessageOnMainThread("Conditions met, closing vrchat");
                    YeetVrc("vrchat");
                }
            }
            else
            {
                InvokeMessageOnMainThread($"{message.AvatarParameter} | State: false");
            }
        }
    }


    private static void OnConnectionStateChanged(object? sender, ConnectionState state)
    {
        Console.WriteLine($"Connection state changed to: {state}");

        switch (state)
        {
            case ConnectionState.NotConnected:
                InvokeMessageOnMainThread("Connection state changed to: NotConnected");
                break;
            case ConnectionState.Connecting:
                InvokeMessageOnMainThread("Connection state changed to: Connecting");
                break;
            case ConnectionState.Connected:
                InvokeMessageOnMainThread("Connection state changed to: Connected");
                break;
            case ConnectionState.Closing:
                InvokeMessageOnMainThread("Connection state changed to: Closing");
                break;
            case ConnectionState.Closed:
                InvokeMessageOnMainThread("Connection state changed to: Closed");
                break;
        }
    }
    private static void YeetVrc(string processName)
    {
        int delayMilliseconds = 1000;

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
