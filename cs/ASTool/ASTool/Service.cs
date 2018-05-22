//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ASTool.ISMHelper;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
namespace ASTool
{
    public partial class Program
    {
        /// <summary>
        /// This is a callback type passed to custom implementaton of windows service state machines.
        /// The callback needs to be called to notify windows about service state changes both when requested to
        /// perform state changes by windows or when they are needed because of other reasons (e.g. unexpected termination).
        /// 
        /// Repeatedly calling this callback also prolonges the default timeout for pending states until the service maanger reports the service as failed.
        /// 
        /// Calling this callback will result in a call to SetServiceStatus - see https://msdn.microsoft.com/en-us/library/windows/desktop/ms686241(v=vs.85).aspx
        /// </summary>
        /// <param name="state">The current state of the service.</param>
        /// <param name="acceptedControlCommands">The currently accepted control commands. E.g. when you set the <paramref name="state"/> to <value>Started</value>, you can indicate that you support pausing and stopping in this state.</param>
        /// <param name="win32ExitCode">The  current win32 exit code. Use this to indicate a failure when setting the state to <value>Stopped</value>.</param>
        /// <param name="waitHint">
        /// The estimeated time in milliseconds until a state changing operation finishes.
        /// For example, you can repeatedly call this callback with <paramref name="state"/> set to <value>StartPending</value> or <value>StopPending</value>
        /// using different values to affect the start/stop progress indicator in service management UI dialogs.
        /// </param>
        public delegate void ServiceStatusReportCallback(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint waitHint);
        internal delegate void ServiceMainFunction(int numArs, IntPtr argPtrPtr);
        private const string ServiceName = "ASTOOL";
        private const string ServiceDescription = "Adaptative Streaming TOOL";
        private static ServiceMainFunction serviceMainFunctionDelegate;
        private static ServiceControlHandler serviceControlHandlerDelegate;
        private static uint checkpointCounter = 1;
        private static ServiceStatus serviceStatus = new ServiceStatus(ServiceType.Win32OwnProcess, ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None,
                        win32ExitCode: 0, serviceSpecificExitCode: 0, checkPoint: 0, waitHint: 0);

        private static ServiceStatusHandle serviceStatusHandle;
        private static ServiceStatusReportCallback StatusReportCallback;

        private static int resultCode;
        private static Exception resultException;
        private static string[] ParseArguments(int numArgs, IntPtr argPtrPtr)
        {
            if (numArgs <= 0)
            {
                return Array.Empty<string>();
            }
            // skip first parameter becuase it is the name of the service
            var args = new string[numArgs - 1];
            for (var i = 0; i < numArgs - 1; i++)
            {
                argPtrPtr = IntPtr.Add(argPtrPtr, IntPtr.Size);
                var argPtr = Marshal.PtrToStructure<IntPtr>(argPtrPtr);
                args[i] = Marshal.PtrToStringUni(argPtr);
            }
            return args;
        }

        public static  void OnStart(string[] startupArguments, ServiceStatusReportCallback statusReportCallback)
        {
            StatusReportCallback = statusReportCallback;

            try
            {

                // Start Services flecoqui
                Options opt = new Options();
                opt.TraceLevel = Options.LogLevel.Verbose;
                opt.TraceFile = "C:\\temp\\ttt.log";
                opt.LogInformation("Starting Service");
                foreach(string s in startupArguments)
                {
                    opt.LogInformation("Starting Service args: " + s);
                }

                bool bContinue;
                List<Options> list = LaunchServices(ServiceOptions, out bContinue);
                if ((list != null) && (list.Count > 0))
                {
                    OptionsList = list;
                    statusReportCallback(ServiceState.Running, ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode: 0, waitHint: 0);
                }
                else
                    statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.Stop, win32ExitCode: 0, waitHint: 0);
            }
            catch
            {
                statusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }
        private static void MainFunction(int numArgs, IntPtr argPtrPtr)
        {
            Options opt = new Options();
            opt.TraceLevel = Options.LogLevel.Verbose;
            opt.TraceFile = "C:\\temp\\ttt.log";
            opt.LogInformation("MainFunction: " + numArgs.ToString());
            var startupArguments = ParseArguments(numArgs, argPtrPtr);

            serviceStatusHandle = Win32ServiceInterop.Wrapper.RegisterServiceCtrlHandlerExW(ServiceName, serviceControlHandlerDelegate, IntPtr.Zero);

            if (serviceStatusHandle.IsInvalid)
            {
                resultException = new Win32Exception(Marshal.GetLastWin32Error());
                return;
            }

            ReportServiceStatus(ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 3000);

            try
            {
                OnStart(startupArguments, ReportServiceStatus);
            }
            catch
            {
                ReportServiceStatus(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }
        private static void ReportServiceStatus(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint waitHint)
        {
            if (serviceStatus.State == ServiceState.Stopped)
            {
                // we refuse to leave or alter the final state
                return;
            }

            serviceStatus.State = state;
            serviceStatus.Win32ExitCode = win32ExitCode;
            serviceStatus.WaitHint = waitHint;

            serviceStatus.AcceptedControlCommands = state == ServiceState.Stopped
                ? ServiceAcceptedControlCommandsFlags.None // since we enforce "Stopped" as final state, no longer accept control messages
                : acceptedControlCommands;

            serviceStatus.CheckPoint = state == ServiceState.Running || state == ServiceState.Stopped || state == ServiceState.Paused
                ? 0 // MSDN: This value is not valid and should be zero when the service does not have a start, stop, pause, or continue operation pending.
                : checkpointCounter++;

            if (state == ServiceState.Stopped)
            {
                resultCode = win32ExitCode;
            }

            Win32ServiceInterop.Wrapper.SetServiceStatus(serviceStatusHandle, ref serviceStatus);
        }
        /// <summary>
        /// Called when a command was received from windows' service system.
        /// </summary>
        /// <param name="command">The received command.</param>
        /// <param name="commandSpecificEventType">Type of the command specific event. See description of dwEventType at https://msdn.microsoft.com/en-us/library/windows/desktop/ms683241(v=vs.85).aspx</param>
        public static void OnCommand(ServiceControlCommand command, uint commandSpecificEventType)
        {
            if (command == ServiceControlCommand.Stop)
            {
                StatusReportCallback(ServiceState.StopPending, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, waitHint: 3000);

                var win32ExitCode = 0;

                try
                {
                    // Stop Services flecoqui
                    Options opt = new Options();
                    opt.TraceLevel = Options.LogLevel.Verbose;
                    opt.TraceFile = "C:\\temp\\ttt.log";
                    TimeSpan t = new TimeSpan(0, 0, 0, 10, 0);
                    opt.LogInformation("Stopping service, timeout ms: " + t.TotalMilliseconds.ToString());
                    bool result = StopServices(t);
                }
                catch
                {
                    win32ExitCode = -1;
                }

                StatusReportCallback(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode, waitHint: 0);
            }
        }
        private static void HandleServiceControlCommand(ServiceControlCommand command, uint eventType, IntPtr eventData, IntPtr eventContext)
        {
            try
            {
                OnCommand(command, eventType);
            }
            catch
            {
                ReportServiceStatus(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None, win32ExitCode: -1, waitHint: 0);
            }
        }
        static public Options ServiceOptions;
        /// <summary>
        /// Runs the windows service that this instance was initialized with.
        /// This method is inteded to be run from the application's main thread and will block until the service has stopped.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Win32Exception">Thrown when an exception ocurrs when communicating with windows' service system.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when run on a non-windows platform.</exception>
        public static  int RunAsService(Options opt)
        {
            ServiceOptions = opt;
            serviceMainFunctionDelegate = MainFunction;
            serviceControlHandlerDelegate = HandleServiceControlCommand;
            var serviceTable = new ServiceTableEntry[2]; // second one is null/null to indicate termination
            serviceTable[0].serviceName = ServiceName;
            serviceTable[0].serviceMainFunction = Marshal.GetFunctionPointerForDelegate(serviceMainFunctionDelegate);

            try
            {
                // StartServiceCtrlDispatcherW call returns when ServiceMainFunction has exited and all services have stopped
                // at least this is what's documented even though linked c++ sample has an additional stop event
                // to let the service main function dispatched to block until the service stops.
                if (!Win32ServiceInterop.Wrapper.StartServiceCtrlDispatcherW(serviceTable))
                {
                    opt.LogError("ASTOOL running as Window Service - Error while calling StartServiceCtrlDispatcherW");
                }
            }
            catch (DllNotFoundException dllException)
            {
                opt.LogError("ASTOOL running as Window Service - this service is not available on the current platform: " + dllException.Message);
            }

            if (resultException != null)
            {
                opt.LogError("ASTOOL running as Window Service - Exception: " + resultException.Message);
            }

            return resultCode;
        }

        static bool InstallService(Options opt)
        {
            bool bResult = false;

            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        string host = Process.GetCurrentProcess().MainModule.FileName;
                        host += " --import --service --configfile " + opt.ConfigFile;
                        ServiceHandle h = mgr.CreateService(ServiceName, ServiceDescription, host, ServiceType.Win32OwnProcess, ServiceStartType.AutoStart, ErrorSeverity.Normal, Win32ServiceCredentials.LocalSystem);
                        if (h != null)
                        {

                            bResult = true;
                        }
                        else
                            opt.LogError("Install feature: can't create Service: " + ServiceName);
                    }
                    else
                        opt.LogError("Install feature: can't open ServiceManager");

                }
                else
                    opt.LogError("Install feature: this service is not available on the current platform");
            }
            catch(Exception ex)
            {
                opt.LogError("Install feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool UninstallService(Options opt)
        {
            bool bResult = false;
            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {

                            if (Win32ServiceInterop.DeleteService(h) == true)
                            {
                                bResult = true;
                            }
                            else
                                opt.LogError("Uninstall feature: can't Uninstall Service: " + ServiceName);
                        }
                        else
                            opt.LogError("Uninstall feature: can't open Service: " + ServiceName);
                    }
                    else
                        opt.LogError("Uninstall feature: can't open ServiceManager");

                }
                else
                    opt.LogError("Uninstall feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                opt.LogError("Uninstall feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool StartService(Options opt)
        {
            bool bResult = false;
            try
            {
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {
                            if(Win32ServiceInterop.StartServiceW(h, 0, IntPtr.Zero)==true)
                            {
                                bResult = true;
                            }
                            else
                                opt.LogError("Start feature: can't start Service: " + ServiceName);
                        }
                        else
                            opt.LogError("Start feature: can't open Service: " + ServiceName);
                    }
                    else
                        opt.LogError("Start feature: can't open ServiceManager");
                }
                else
                    opt.LogError("Start feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                opt.LogError("Start feature: exception: " + ex.Message);
            }
            return bResult;
        }
        static bool StopService(Options opt)
        {
            bool bResult = false;
            try
            { 
                bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                if (IsWindows == true)
                {
                    ServiceControlManager mgr = ServiceControlManager.Connect(Win32ServiceInterop.Wrapper, null, null, ServiceControlManagerAccessRights.All);

                    if ((mgr != null) && (mgr.IsInvalid != true))
                    {
                        ServiceHandle h = mgr.OpenService(ServiceName, ServiceControlAccessRights.All);
                        if (h != null)
                        {
                            ServiceStatusProcess status = new ServiceStatusProcess();
                            uint reason = (uint) StopReasonMinorReasonFlags.SERVICE_STOP_REASON_MINOR_MAINTENANCE |
                                (uint) StopReasonMajorReasonFlags.SERVICE_STOP_REASON_MAJOR_NONE |
                                (uint) StopReasonFlags.SERVICE_STOP_REASON_FLAG_UNPLANNED;
                            ServiceStatusParam param = new ServiceStatusParam(reason, status);

                            int s = Marshal.SizeOf<ServiceStatusParam>();
                            var lpParam = Marshal.AllocHGlobal(s);
                            Marshal.StructureToPtr(param, lpParam, fDeleteOld: false);
                            if (Win32ServiceInterop.ControlServiceExW(h,(uint) ServiceControlCommandFlags.SERVICE_CONTROL_STOP,(uint) ServiceControlCommandReasonFlags.SERVICE_CONTROL_STATUS_REASON_INFO, lpParam) == true)
                            {
                                bResult = true;
                            }
                            else
                            {
                                opt.LogError("Stop feature: can't stop Service: " + ServiceName + " ErrorCode: " + Marshal.GetLastWin32Error().ToString());
                            }
                        }
                        else
                            opt.LogError("Stop feature: can't open Service: " + ServiceName);
                    }
                    else
                        opt.LogError("Stop feature: can't open ServiceManager");

                }
                else
                    opt.LogError("Stop feature: this service is not available on the current platform");

            }
            catch (Exception ex)
            {
                opt.LogError("Stop feature: exception: " + ex.Message);
            }
            return bResult;
        }
    }
}
