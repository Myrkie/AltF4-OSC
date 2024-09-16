﻿using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AltF4_OSC.Misc;
using Microsoft.VisualBasic.CompilerServices;
using OscQueryLibrary;
using Serilog;
using VRChatOSCLib;

namespace AltF4_OSC;

public static class OscData
{
    private static readonly ILogger Logger = Log.ForContext(typeof(OscData));

    internal static void Dispose()
    {
        _oscInstance?.Dispose();
        _currentOscQueryServer?.Dispose();
    }

    internal static async Task Start()
    {    
        var oscQueryServer = new OscQueryServer(GenerateRandomPrefixedString(), IPAddress.Parse(Config.Instance.Network.Ip));
        if (!Config.Instance.Network.UseConfigPorts)
        {
            oscQueryServer.FoundVrcClient += FoundVrcClient;
            oscQueryServer.Start();
            Logger.Information($"{AppDomain.CurrentDomain.FriendlyName}: Starting up, building connections.");
            Misc.Utils.InvokeMessageOnMainThread($"{AppDomain.CurrentDomain.FriendlyName}: Starting up, building connections.");
            Misc.Utils.WaitForListening(ref _oscInstance);
        }
        else
        {
            Logger.Debug("Debug mode has been activated, your ports will be defined by config.");
            await PortOverride();
        }
    }
    
    
    #region OSCConnection
    
    private static CancellationTokenSource _loopCancellationToken = new();
    private static OscQueryServer? _currentOscQueryServer;
    private static VRChatOSC? _oscInstance;

    #endregion

    private static async Task FoundVrcClient(OscQueryServer? oscQueryServer, IPEndPoint? ipEndPoint)
    {
        await _loopCancellationToken.CancelAsync();
        _loopCancellationToken = new CancellationTokenSource();
        _oscInstance?.Dispose();
        _oscInstance = null;

        _oscInstance = new VRChatOSC();

        _oscInstance.Listen(ipEndPoint!.Address, oscQueryServer!.OscReceivePort);
        Misc.Utils.InvokeMessageOnMainThread($"Listening on {ipEndPoint.Address}|{oscQueryServer.OscReceivePort} ");
        Logger.Information("Listening on {ip}|{port} ", ipEndPoint.Address, oscQueryServer.OscReceivePort);
        _oscInstance.TryAddMethod(Config.Instance.Parameter, DisconnectReceived);
        
        _currentOscQueryServer = oscQueryServer;
    }
    
    
    
    private static async Task PortOverride()
    {
        await _loopCancellationToken.CancelAsync();
        _loopCancellationToken = new CancellationTokenSource();
        _oscInstance?.Dispose();
        _oscInstance = null;
        
        _oscInstance = new VRChatOSC();
        _oscInstance.Listen(IPAddress.Parse(Config.Instance.Network.Ip), Config.Instance.Network.ListeningPort);
        _oscInstance.Connect(IPAddress.Parse(Config.Instance.Network.Ip), Config.Instance.Network.SendingPort);
        
        Misc.Utils.InvokeMessageOnMainThread($"Listening on {Config.Instance.Network.Ip}|{Config.Instance.Network.ListeningPort} ");
        Logger.Information("Listening on {ip}|{port} ", Config.Instance.Network.Ip, Config.Instance.Network.ListeningPort);
        
        _oscInstance.TryAddMethod(Config.Instance.Parameter, DisconnectReceived);
    }
    
    
    private static void DisconnectReceived(VRCMessage msg)
    {
        Console.WriteLine(msg.GetValue());
        if (msg.GetValue() is bool disconnectBoolean)
        {
            if (disconnectBoolean)
            {
                Misc.Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {disconnectBoolean}");
                    
                if (MainWindow.Instance.IsCheckBoxChecked)
                { 
                    Misc.Utils.InvokeMessageOnMainThread("Conditions met, closing VRChat.");
                    YeetVrc("VRChat");
                }
            }
            else
            {
                Misc.Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {disconnectBoolean}");
            }
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
            Misc.Utils.InvokeMessageOnMainThread($"Found process: {process.ProcessName}, PID: {process.Id}");
            Thread.Sleep(delayMilliseconds);

            try
            {
                process.CloseMainWindow();
                Misc.Utils.InvokeMessageOnMainThread($"Process {process.ProcessName} terminated.");
            }
            catch (Exception ex)
            {
                Misc.Utils.InvokeMessageOnMainThread($"Failed to terminate process {process.ProcessName}: {ex.Message}");
            }
        }
    }
}
