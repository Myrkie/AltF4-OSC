using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AltF4_OSC.Misc;
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
            Utils.InvokeMessageOnMainThread($"{AppDomain.CurrentDomain.FriendlyName}: Starting up, building connections.");
            Utils.WaitForListening(ref _oscInstance);
        }
        else
        {
            Logger.Debug("Debug mode has been activated, your ports will be defined by config.");
            Utils.InvokeMessageOnMainThread("Debug mode has been activated, your ports will be defined by config.");
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

        _oscInstance.Connect(ipEndPoint!.Address, ipEndPoint.Port);
        _oscInstance.Listen(ipEndPoint.Address, oscQueryServer!.OscReceivePort);
        
        Utils.InvokeMessageOnMainThread($"Listening on {ipEndPoint.Address}|{oscQueryServer.OscReceivePort} ");
        Logger.Information("Listening on {ip}|{port} ", ipEndPoint.Address, oscQueryServer.OscReceivePort);
        
        _oscInstance.TryAddMethod(Config.Instance.Parameter, DisconnectReceived);
        _oscInstance.TryAddMethod("Misc/Ping", ReceivedPing);
        
        _currentOscQueryServer = oscQueryServer;
    }
    
    
    
    private static async Task PortOverride()
    {
        await _loopCancellationToken.CancelAsync();
        _loopCancellationToken = new CancellationTokenSource();
        _oscInstance?.Dispose();
        _oscInstance = null;
        
        _oscInstance = new VRChatOSC();
        _oscInstance.Connect(IPAddress.Parse(Config.Instance.Network.Ip), Config.Instance.Network.SendingPort);
        _oscInstance.Listen(IPAddress.Parse(Config.Instance.Network.Ip), Config.Instance.Network.ListeningPort);
        
        Utils.InvokeMessageOnMainThread($"Listening on {Config.Instance.Network.Ip}|{Config.Instance.Network.ListeningPort} ");
        Logger.Information("Listening on {ip}|{port} ", Config.Instance.Network.Ip, Config.Instance.Network.ListeningPort);
        
        _oscInstance.TryAddMethod(Config.Instance.Parameter, DisconnectReceived);
        _oscInstance.TryAddMethod(Config.Instance.PingParameter, ReceivedPing);
    }
    
    
    private static void DisconnectReceived(VRCMessage msg)
    {
        if (msg.GetValue() is not bool disconnectBoolean) return;
        if (disconnectBoolean)
        {
            Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {disconnectBoolean}");

            if (!MainWindow.Instance.IsCheckBoxChecked) return;
            Utils.InvokeMessageOnMainThread("Conditions met, closing VRChat.");
            CloseProcess("VRChat");
        }
        else
        {
            Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {disconnectBoolean}");
            _oscInstance!.SendParameter(Config.Instance.Parameter, false);
        }
    }
    
    private static void ReceivedPing(VRCMessage msg)
    {
        if (msg.GetValue() is not bool ping) return;
        if (ping)
        {
            Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {ping}");
                
        }
        else
        {
            Utils.InvokeMessageOnMainThread($"{msg.AvatarParameter} Changed state to | {ping}");
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

    
    private static void CloseProcess(string processName)
    {
        var processes = Process.GetProcessesByName(processName);

        if (processes.Length != 1) return;
        
        // only close the first process.
        var process = processes[0];
        Utils.InvokeMessageOnMainThread($"Found process: {process.ProcessName}, PID: {process.Id}");
        Thread.Sleep(Config.Instance.WaitTimeout);

        try
        {
            process.CloseMainWindow();
            Utils.InvokeMessageOnMainThread($"Process {process.ProcessName} terminated.");
        }
        catch (Exception ex)
        {
            Utils.InvokeMessageOnMainThread($"Failed to terminate process {process.ProcessName}: {ex.Message}");
        }
    }
}
