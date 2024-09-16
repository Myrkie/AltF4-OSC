using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using Serilog;
using VRChatOSCLib;

namespace AltF4_OSC.Misc
{
    public static class Utils
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(Utils));

        public static void WaitForListening(ref VRChatOSC? oscInstance)
        {
            FieldInfo? listeningField = null;

            while (true)
            {
                if (oscInstance == null)
                {
                    Logger.Error("VRChatOSC instance is null. Waiting for it to be initialized, this can also result from VRChat not running or VRChat returning no ports.");
                    InvokeMessageOnMainThread("VRChatOSC instance is null. Waiting for it to be initialized, this can also result from VRChat not running or VRChat returning no ports.");
                    Thread.Sleep(2000);
                    continue;
                }

                if (listeningField == null)
                {
                    try
                    {
                        listeningField =
#pragma warning disable IL2065
                            typeof(VRChatOSC).GetField("m_Listening", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2065

                        if (listeningField == null)
                        {
                            throw new Exception("Field 'm_Listening' not found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to retrieve the 'm_Listening' field.");
                        throw;
                    }
                }

                try
                {
                    var isListening = (bool)listeningField.GetValue(oscInstance)!;

                    if (isListening)
                    {
                        Logger.Information("VRChatOSC instance is ready and listening!");
                        InvokeMessageOnMainThread("VRChatOSC instance is ready and listening!");
                        InvokeMessageOnMainThread("AltF4 OSC is ready for use.");
                        break;
                    }

                    Logger.Error("VRChatOSC is not listening yet. Checking again...");
                    InvokeMessageOnMainThread("VRChatOSC is not listening yet. Checking again...");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to get value of 'm_Listening' field.");
                    throw;
                }

                Thread.Sleep(1000);
            }
        }
        
        
        public static void InvokeMessageOnMainThread(string message)
        {
            Application.Current.Dispatcher.Invoke(() => { MainWindow.Message(message); });
        }
    }
}
