using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace ASTool
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal enum Win32AccessMask : uint
    {
        Delete = 0x00010000,
        ReadControl = 0x00020000,
        WriteDac = 0x00040000,
        WriteOwner = 0x00080000,
        Synchronize = 0x00100000,

        StandardRightsRequired = 0x000F0000,

        StandardRightsRead = 0x00020000,
        StandardRightsWrite = 0x00020000,
        StandardRightsExecute = 0x00020000,

        StandardRightsAll = 0x001F0000,

        SpecificRightsAll = 0x0000FFFF,

        AccessSystemSecurity = 0x01000000,

        MaximumAllowed = 0x02000000,

        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000,

        DesktopReadobjects = 0x00000001,
        DesktopCreatewindow = 0x00000002,
        DesktopCreatemenu = 0x00000004,
        DesktopHookcontrol = 0x00000008,
        DesktopJournalrecord = 0x00000010,
        DesktopJournalplayback = 0x00000020,
        DesktopEnumerate = 0x00000040,
        DesktopWriteobjects = 0x00000080,
        DesktopSwitchdesktop = 0x00000100,

        WinstaEnumdesktops = 0x00000001,
        WinstaReadattributes = 0x00000002,
        WinstaAccessclipboard = 0x00000004,
        WinstaCreatedesktop = 0x00000008,
        WinstaWriteattributes = 0x00000010,
        WinstaAccessglobalatoms = 0x00000020,
        WinstaExitwindows = 0x00000040,
        WinstaEnumerate = 0x00000100,
        WinstaReadscreen = 0x00000200,

        WinstaAllAccess = 0x0000037F
    }
    [Flags]
    internal enum ServiceControlAccessRights : uint
    {
        QueryConfig = 0x00001,
        ChangeConfig = 0x00002,
        QueryStatus = 0x00004,
        EnumerateDependents = 0x00008,
        Start = 0x00010,
        Stop = 0x00020,
        PauseContinue = 0x00040,
        Interrogate = 0x00080,
        UserDefinedControl = 0x00100,

        All = Win32AccessMask.StandardRightsRequired
              | QueryConfig
              | ChangeConfig
              | QueryStatus
              | EnumerateDependents
              | Start
              | Stop
              | PauseContinue
              | Interrogate
              | UserDefinedControl
    }
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal enum ServiceType : uint
    {
        FileSystemDriver = 0x00000002,
        KernelDriver = 0x00000001,
        Win32OwnProcess = 0x00000010,
        Win32ShareProcess = 0x00000020,
        InteractiveProcess = 0x00000100
    }
    /// <summary>
    /// The state a service is in
    /// </summary>
    public enum ServiceState : uint
    {
        /// <summary>
        /// The service is stopped (= not running)
        /// </summary>
        Stopped = 0x00000001,

        /// <summary>
        /// The service is starting
        /// </summary>
        StartPending = 0x00000002,

        /// <summary>
        /// The stopping
        /// </summary>
        StopPending = 0x00000003,

        /// <summary>
        /// The service is running (= started successfully)
        /// </summary>
        Running = 0x00000004,

        /// <summary>
        /// The service is about to resume after being paused
        /// </summary>
        ContinuePending = 0x00000005,

        /// <summary>
        /// The service is about to pause
        /// </summary>
        PausePending = 0x00000006,

        /// <summary>
        /// The service is paused
        /// </summary>
        Paused = 0x00000007,

        /// <summary>
        /// The service is starting.
        /// </summary>
#if NETSTANDARD2_0
        [Browsable(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Misspelled, use '" + nameof(StartPending) + "' instead. This member will be removed in upcoming versions.", true)]
        StartPening = StartPending
    }
    /// <summary>
    /// Control command codes used issue control commands or
    /// notification events to services.
    /// </summary>

    public enum ServiceControlCommand : uint
    {
        /// <summary>
        /// Instructs a service to stop.
        /// </summary>
        Stop = 0x00000001,

        /// <summary>
        /// Instructs a service to pause.
        /// </summary>
        Pause = 0x00000002,

        /// <summary>
        /// Instructs a service to continue after being paused.
        /// </summary>
        Continue = 0x00000003,

        /// <summary>
        /// Instructs a service to report its current status to the service control manager.
        /// </summary>
        Interrogate = 0x00000004,

        /// <summary>
        /// Notifies a service of a shutdown.
        /// </summary>
        Shutdown = 0x00000005,

        /// <summary>
        /// Instructs a service to re-read its startup parameters.
        /// </summary>
        Paramchange = 0x00000006,

        /// <summary>
        /// Notifies a network service that there is a new
        /// component available for binding that it should bind to.
        /// </summary>
        NetBindAdd = 0x00000007,

        /// <summary>
        /// Notifies a network service that a component for binding
        /// has been removed and that it should unbind from it.
        /// </summary>
        NetBindRemoved = 0x00000008,

        /// <summary>
        /// Notifies a network service that a disabled binding
        /// has ben enabled and that it should add the new binding.
        /// </summary>
        NetBindEnable = 0x00000009,

        /// <summary>
        /// Notifies a network service that one of its bindings
        /// has ben disabled and that it should remove the binding.
        /// </summary>
        NetBindDisable = 0x0000000A,

        /// <summary>
        /// Notifies a service of device events.
        /// </summary>
        DeviceEvent = 0x0000000B,

        /// <summary>
        /// Notifies a service that the computer's hardware profile has changed.
        /// </summary>
        HardwareProfileChange = 0x0000000C,

        /// <summary>
        /// Notifies a service of system power events.
        /// </summary>
        PowerEvent = 0x0000000D,

        /// <summary>
        /// Notifies a service of session change events.
        /// </summary>
        SessionChange = 0x0000000E,

        /// <summary>
        /// Notifies a service that the system time has changed.
        /// </summary>
        TimeChange = 0x00000010,

        /// <summary>
        /// Notifies a service registered for a service trigger event that the event has occurred.
        /// </summary>
        TriggerEvent = 0x00000020,

        /// <summary>
        /// Notifies a service that the user has initiated a reboot.
        /// </summary>
        UserModeReboot = 0x00000040
    }
    internal delegate void ServiceControlHandler(ServiceControlCommand control, uint eventType, IntPtr eventData, IntPtr eventContext);

    /// <summary>
    /// Represents credentials for accounts to run windows services with.
    /// </summary>
    /// <seealso cref="System.IEquatable{Win32ServiceCredentials}" />

    public struct Win32ServiceCredentials : IEquatable<Win32ServiceCredentials>
    {
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; }

        /// <summary>
        /// Creaes a new <see cref="Win32ServiceCredentials"/> instance to represent an account under which to run widows services.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public Win32ServiceCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// The local system account - the service will have full access to the system and machine network credentials.
        /// Not recommended to use in producton environments. 
        /// </summary>
        public static Win32ServiceCredentials LocalSystem = new Win32ServiceCredentials(userName: null, password: null);

        /// <summary>
        /// The local service account - the service will have minimum access to the system and anonymous network credentails.
        /// Recommended for use in logic-only applications.
        /// Consider using a custom account instead for granular control over file system permissions.
        /// </summary>
        public static Win32ServiceCredentials LocalService = new Win32ServiceCredentials(@"NT AUTHORITY\LocalService", password: null);

        /// <summary>
        /// The local service account - the service will have minimum access to the system and machine network credentails.
        /// Recommended for use in logic-only applications that need to authenticate to networks using machine credentials.
        /// Consider using a custom account instead for granular control over file system permissions and network authorization control.
        /// </summary>
        public static Win32ServiceCredentials NetworkService = new Win32ServiceCredentials(@"NT AUTHORITY\NetworkService", password: null);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Win32ServiceCredentials other)
        {
            return string.Equals(UserName, other.UserName) && string.Equals(Password, other.Password);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(objA: null, objB: obj))
            {
                return false;
            }
            return obj is Win32ServiceCredentials && Equals((Win32ServiceCredentials)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((UserName?.GetHashCode() ?? 0) * 397) ^ (Password?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left handside operand.</param>
        /// <param name="right">The right handside operand.</param>
        /// <returns>
        /// [true] if the operands are equal.
        /// </returns>
        public static bool operator ==(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left handside operand.</param>
        /// <param name="right">The right handside operand.</param>
        /// <returns>
        /// [true] if the operands are not equal.
        /// </returns>
        public static bool operator !=(Win32ServiceCredentials left, Win32ServiceCredentials right)
        {
            return !left.Equals(right);
        }
    }
    /// <summary>
    /// Action types used for failure actions.
    /// Used in the <see cref="ScAction"/> type.
    /// </summary>

    public enum ScActionType
    {
        /// <summary>
        /// No action
        /// </summary>
        ScActionNone = 0,

        /// <summary>
        /// Restart service
        /// </summary>
        ScActionRestart = 1,

        /// <summary>
        /// Reboot the computer (meant to be used for drivers and not in managed services)
        /// </summary>
        ScActionReboot = 2,

        /// <summary>
        /// Run a command
        /// </summary>
        ScActionRunCommand = 3,
    }
    /// <summary>
    /// Service control actions used to specify what to do in case of service failures.
    /// </summary>
    /// <seealso cref="System.IEquatable{ScAction}" />
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]

    public struct ScAction : IEquatable<ScAction>
    {
        private ScActionType _Type;
        private uint _Delay;

        /// <summary>
        /// Gets or sets the type of service control action.
        /// </summary>
        /// <value>
        /// The type of service control action.
        /// </value>
        public ScActionType Type
        {
            get => _Type;
            set => _Type = value;
        }

        /// <summary>
        /// Gets or sets the amount of time the action is to be delayed when a failure occurs.
        /// </summary>
        /// <value>
        /// The amount of time the action is to be delayed when a failure occurs.
        /// </value>
        public TimeSpan Delay
        {
            get => TimeSpan.FromMilliseconds(_Delay);
            set => _Delay = (uint)Math.Round(value.TotalMilliseconds);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ScAction other)
        {
            return _Type == other._Type && _Delay == other._Delay;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ScAction && Equals((ScAction)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode
                .Of(this.Delay)
                .And(this.Type);
        }
    }
    /// <summary>
    /// Values used to indicate which service control commands are accepted by a service.
    /// </summary>
    [Flags]
    public enum ServiceAcceptedControlCommandsFlags : uint
    {
        /// <summary>
        /// No command is accepted. Usually used during StartPending/StopPending/PausePending states.
        /// </summary>
        None = 0,

        /// <summary>
        /// The service can be stopped
        /// </summary>
        Stop = 0x00000001,

        /// <summary>
        /// The service can be paused when running or continued when currently paused.
        /// </summary>
        PauseContinue = 0x00000002,

        /// <summary>
        /// The service can be stopped or paused when running or continued when currently paused.
        /// </summary>
        PauseContinueStop = PauseContinue | Stop,

        /// <summary>
        /// The shutdown command is accepted which notfies the service about a system shutdown.
        /// This event can only be sent by the system.
        /// </summary>
        Shutdown = 0x00000004,

        /// <summary>
        /// Indicates that the service is expected to re-read its startup parameters without needing to be restarted.
        /// </summary>
        ParamChange = 0x00000008,

        /// <summary>
        /// Tndicates that the service is a network service that can re-read its binding parameters without needing to be restarted.
        /// </summary>
        NetBindChange = 0x00000010,

        /// <summary>
        /// Indicates that the service can perform pre-sutdown tasks.
        /// This event can only be sent by the system.
        /// </summary>
        PreShutdown = 0x00000100,

        /// <summary>
        /// The service can react to system hardware changes.
        /// This event can only be sent by the system.
        /// </summary>
        HardwareProfileChange = 0x00000020,

        /// <summary>
        /// The service can react to power status cahnges.
        /// This event can only be sent by the system.
        /// </summary>
        PowerEvent = 0x00000040,

        /// <summary>
        /// The service can react to system session status changes.
        /// This event can only be sent by the system.
        /// </summary>
        SessionChange = 0x00000080,

        /// <summary>
        /// The service can react to system time changes.
        /// Only supported on Windows 7 / Windows Server 2008 R2 and higher
        /// This event can only be sent by the system.
        /// </summary>
        TimeChange = 0x00000200,

        /// <summary>
        /// The service can be notified when an event for which the service has explicitly registered occurs.
        /// This event can only be sent by the system.
        /// </summary>
        TriggerEvent = 0x00000400,

        /// <summary>
        /// The service can react when a user initiates a reboot.
        /// This event can only be sent by the system.
        /// </summary>
        UserModeReboot = 0x00000800
    }
    public enum StopReasonFlags : uint
    {
        SERVICE_STOP_REASON_FLAG_MIN                            = 0x00000000,
        SERVICE_STOP_REASON_FLAG_UNPLANNED                      = 0x10000000,
        SERVICE_STOP_REASON_FLAG_CUSTOM                         = 0x20000000,
        SERVICE_STOP_REASON_FLAG_PLANNED                        = 0x40000000,
        SERVICE_STOP_REASON_FLAG_MAX                            = 0x80000000
    }
    public enum StopReasonMajorReasonFlags : uint
    {
        SERVICE_STOP_REASON_MAJOR_MIN                           =0x00000000,
        SERVICE_STOP_REASON_MAJOR_OTHER                         =0x00010000,
        SERVICE_STOP_REASON_MAJOR_HARDWARE                      =0x00020000,
        SERVICE_STOP_REASON_MAJOR_OPERATINGSYSTEM               =0x00030000,
        SERVICE_STOP_REASON_MAJOR_SOFTWARE                      =0x00040000,
        SERVICE_STOP_REASON_MAJOR_APPLICATION                   =0x00050000,
        SERVICE_STOP_REASON_MAJOR_NONE                          =0x00060000,
        SERVICE_STOP_REASON_MAJOR_MAX                           =0x00070000,
        SERVICE_STOP_REASON_MAJOR_MIN_CUSTOM                    =0x00400000,
        SERVICE_STOP_REASON_MAJOR_MAX_CUSTOM                    =0x00ff0000
    }
    public enum StopReasonMinorReasonFlags : uint
    {
        SERVICE_STOP_REASON_MINOR_MIN                           =0x00000000,
        SERVICE_STOP_REASON_MINOR_OTHER                         =0x00000001,
        SERVICE_STOP_REASON_MINOR_MAINTENANCE                   =0x00000002,
        SERVICE_STOP_REASON_MINOR_INSTALLATION                  =0x00000003,
        SERVICE_STOP_REASON_MINOR_UPGRADE                       =0x00000004,
        SERVICE_STOP_REASON_MINOR_RECONFIG                      =0x00000005,
        SERVICE_STOP_REASON_MINOR_HUNG                          =0x00000006,
        SERVICE_STOP_REASON_MINOR_UNSTABLE                      =0x00000007,
        SERVICE_STOP_REASON_MINOR_DISK                          =0x00000008,
        SERVICE_STOP_REASON_MINOR_NETWORKCARD                   =0x00000009,
        SERVICE_STOP_REASON_MINOR_ENVIRONMENT                   =0x0000000a,
        SERVICE_STOP_REASON_MINOR_HARDWARE_DRIVER               =0x0000000b,
        SERVICE_STOP_REASON_MINOR_OTHERDRIVER                   =0x0000000c,
        SERVICE_STOP_REASON_MINOR_SERVICEPACK                   =0x0000000d,
        SERVICE_STOP_REASON_MINOR_SOFTWARE_UPDATE               =0x0000000e,
        SERVICE_STOP_REASON_MINOR_SECURITYFIX                   =0x0000000f,
        SERVICE_STOP_REASON_MINOR_SECURITY                      =0x00000010,
        SERVICE_STOP_REASON_MINOR_NETWORK_CONNECTIVITY          =0x00000011,
        SERVICE_STOP_REASON_MINOR_WMI                           =0x00000012,
        SERVICE_STOP_REASON_MINOR_SERVICEPACK_UNINSTALL         =0x00000013,
        SERVICE_STOP_REASON_MINOR_SOFTWARE_UPDATE_UNINSTALL     =0x00000014,
        SERVICE_STOP_REASON_MINOR_SECURITYFIX_UNINSTALL         =0x00000015,
        SERVICE_STOP_REASON_MINOR_MMC                           =0x00000016,
        SERVICE_STOP_REASON_MINOR_NONE                          =0x00000017,
        SERVICE_STOP_REASON_MINOR_MAX                           =0x00000018,
        SERVICE_STOP_REASON_MINOR_MIN_CUSTOM                    =0x00000100,
        SERVICE_STOP_REASON_MINOR_MAX_CUSTOM                    =0x0000FFFF
    }
    public enum ServiceControlCommandReasonFlags : uint
    {
        SERVICE_CONTROL_STATUS_REASON_INFO = 1
    }
    public enum ServiceControlCommandFlags : uint
    {
        SERVICE_CONTROL_STOP                  = 0x00000001,
        SERVICE_CONTROL_PAUSE                 = 0x00000002,
        SERVICE_CONTROL_CONTINUE              = 0x00000003,
        SERVICE_CONTROL_INTERROGATE           = 0x00000004,
        SERVICE_CONTROL_SHUTDOWN              = 0x00000005,
        SERVICE_CONTROL_PARAMCHANGE           = 0x00000006,
        SERVICE_CONTROL_NETBINDADD            = 0x00000007,
        SERVICE_CONTROL_NETBINDREMOVE         = 0x00000008,
        SERVICE_CONTROL_NETBINDENABLE         = 0x00000009,
        SERVICE_CONTROL_NETBINDDISABLE        = 0x0000000A,
        SERVICE_CONTROL_DEVICEEVENT           = 0x0000000B,
        SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0x0000000C,
        SERVICE_CONTROL_POWEREVENT            = 0x0000000D,
        SERVICE_CONTROL_SESSIONCHANGE         = 0x0000000E,
        SERVICE_CONTROL_PRESHUTDOWN           = 0x0000000F,
        SERVICE_CONTROL_TIMECHANGE            = 0x00000010,
        SERVICE_CONTROL_TRIGGEREVENT          = 0x00000020
    }
    [StructLayout(LayoutKind.Sequential,Size = 20, CharSet = CharSet.Unicode)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal struct ServiceStatusParam
    {
       [MarshalAs(UnmanagedType.U4)]
        private uint dwReaon;
        private IntPtr lpComment;
        private IntPtr lpStatus;
        public ServiceStatusParam(uint Reason,ServiceStatusProcess status)
        {
            this.dwReaon = Reason;
            this.lpComment = IntPtr.Zero;
            var lpStatus = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceStatusProcess>());
            Marshal.StructureToPtr(status, lpStatus, fDeleteOld: false);
            this.lpStatus = lpStatus;

        }
        public ServiceStatusParam(uint Reason, IntPtr pStatus)
        {
            this.dwReaon = Reason;
            this.lpComment = IntPtr.Zero;
            this.lpStatus = pStatus;

        }
    }
    [StructLayout(LayoutKind.Sequential, Size = 36)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal struct ServiceStatusProcess
    {
        [MarshalAs(UnmanagedType.U4)]
        private ServiceType serviceType;
        [MarshalAs(UnmanagedType.U4)]
        private ServiceState state;
        [MarshalAs(UnmanagedType.U4)]
        private ServiceAcceptedControlCommandsFlags acceptedControlCommands;
        [MarshalAs(UnmanagedType.U4)]
        private int win32ExitCode;
        [MarshalAs(UnmanagedType.U4)]
        private uint serviceSpecificExitCode;
        [MarshalAs(UnmanagedType.U4)]
        private uint checkPoint;
        [MarshalAs(UnmanagedType.U4)]
        private uint waitHint;
        [MarshalAs(UnmanagedType.U4)]
        private uint processid;
        [MarshalAs(UnmanagedType.U4)]
        private uint serviceFlags;

        public ServiceType ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }

        public ServiceState State
        {
            get { return state; }
            set { state = value; }
        }

        public ServiceAcceptedControlCommandsFlags AcceptedControlCommands
        {
            get { return acceptedControlCommands; }
            set { acceptedControlCommands = value; }
        }

        public int Win32ExitCode
        {
            get { return win32ExitCode; }
            set { win32ExitCode = value; }
        }

        public uint ServiceSpecificExitCode
        {
            get { return serviceSpecificExitCode; }
            set { serviceSpecificExitCode = value; }
        }

        public uint CheckPoint
        {
            get { return checkPoint; }
            set { checkPoint = value; }
        }

        public uint WaitHint
        {
            get { return waitHint; }
            set { waitHint = value; }
        }

        public ServiceStatusProcess(ServiceType serviceType, ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint serviceSpecificExitCode, uint checkPoint, uint waitHint)
        {
            this.serviceType = serviceType;
            this.state = state;
            this.acceptedControlCommands = acceptedControlCommands;
            this.win32ExitCode = win32ExitCode;
            this.serviceSpecificExitCode = serviceSpecificExitCode;
            this.checkPoint = checkPoint;
            this.waitHint = waitHint;
            this.processid = 0;
            this.serviceFlags = 0;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal struct ServiceStatus
    {
        private ServiceType serviceType;
        private ServiceState state;
        private ServiceAcceptedControlCommandsFlags acceptedControlCommands;
        private int win32ExitCode;
        private uint serviceSpecificExitCode;
        private uint checkPoint;
        private uint waitHint;


        public ServiceType ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }

        public ServiceState State
        {
            get { return state; }
            set { state = value; }
        }

        public ServiceAcceptedControlCommandsFlags AcceptedControlCommands
        {
            get { return acceptedControlCommands; }
            set { acceptedControlCommands = value; }
        }

        public int Win32ExitCode
        {
            get { return win32ExitCode; }
            set { win32ExitCode = value; }
        }

        public uint ServiceSpecificExitCode
        {
            get { return serviceSpecificExitCode; }
            set { serviceSpecificExitCode = value; }
        }

        public uint CheckPoint
        {
            get { return checkPoint; }
            set { checkPoint = value; }
        }

        public uint WaitHint
        {
            get { return waitHint; }
            set { waitHint = value; }
        }

        public ServiceStatus(ServiceType serviceType, ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands, int win32ExitCode, uint serviceSpecificExitCode, uint checkPoint, uint waitHint)
        {
            this.serviceType = serviceType;
            this.state = state;
            this.acceptedControlCommands = acceptedControlCommands;
            this.win32ExitCode = win32ExitCode;
            this.serviceSpecificExitCode = serviceSpecificExitCode;
            this.checkPoint = checkPoint;
            this.waitHint = waitHint;
        }
    }
    public class ServiceStatusHandle : SafeHandle
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Exposed for testing via InternalsVisibleTo.")]
        internal Win32ServiceInterface NativeInterop { get; set; } = Win32ServiceInterop.Wrapper;

        internal ServiceStatusHandle() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeInterop.CloseServiceHandle(handle);
        }

        public override bool IsInvalid
        {
            [System.Security.SecurityCritical]
            get
            {
                return handle == IntPtr.Zero;
            }
        }
    }
    internal class ServiceControlManager : SafeHandle
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Exposed for testing via InternalsVisibleTo.")]
        internal Win32ServiceInterface NativeInterop { get; set; } = Win32ServiceInterop.Wrapper;

        internal ServiceControlManager() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeInterop.CloseServiceHandle(handle);
        }

        public override bool IsInvalid
        {
            [System.Security.SecurityCritical]
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        internal static ServiceControlManager Connect(Win32ServiceInterface nativeInterop, string machineName, string databaseName, ServiceControlManagerAccessRights desiredAccessRights)
        {
            var mgr = nativeInterop.OpenSCManagerW(machineName, databaseName, desiredAccessRights);

            mgr.NativeInterop = nativeInterop;

            if (mgr.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return mgr;
        }

        public ServiceHandle CreateService(string serviceName, string displayName, string binaryPath, ServiceType serviceType, ServiceStartType startupType, ErrorSeverity errorSeverity, Win32ServiceCredentials credentials)
        {
            var service = NativeInterop.CreateServiceW(this, serviceName, displayName, ServiceControlAccessRights.All, serviceType, startupType, errorSeverity,
                binaryPath, null,
                IntPtr.Zero, null, credentials.UserName, credentials.Password);

            service.NativeInterop = NativeInterop;

            if (service.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return service;
        }

        public ServiceHandle OpenService(string serviceName, ServiceControlAccessRights desiredControlAccess)
        {
            ServiceHandle service;
            Win32Exception errorException;

            if (!TryOpenService(serviceName, desiredControlAccess, out service, out errorException))
            {
                throw errorException;
            }

            return service;
        }

        public virtual bool TryOpenService(string serviceName, ServiceControlAccessRights desiredControlAccess, out ServiceHandle serviceHandle, out Win32Exception errorException)
        {
            var service = NativeInterop.OpenServiceW(this, serviceName, desiredControlAccess);

            service.NativeInterop = NativeInterop;

            if (service.IsInvalid)
            {
                errorException = new Win32Exception(Marshal.GetLastWin32Error());
                serviceHandle = null;
                return false;
            }

            serviceHandle = service;
            errorException = null;
            return true;
        }
    }
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal enum ServiceControlManagerAccessRights : uint
    {
        Connect = 0x00001,
        CreateService = 0x00002,
        EnumerateServices = 0x00004,
        LockServiceDatabase = 0x00008,
        QueryLockStatus = 0x00010,
        ModifyBootConfig = 0x00020,

        All = Win32AccessMask.StandardRightsRequired |
              Connect |
              CreateService |
              EnumerateServices |
              LockServiceDatabase |
              QueryLockStatus |
              ModifyBootConfig,

        GenericRead = Win32AccessMask.StandardRightsRequired |
                      EnumerateServices |
                      QueryLockStatus,

        GenericWrite = Win32AccessMask.StandardRightsRequired |
                       CreateService |
                       ModifyBootConfig,

        GenericExecute = Win32AccessMask.StandardRightsRequired |
                         Connect |
                         LockServiceDatabase,

        GenericAll = All
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceFailureActionsFlag
    {
        private bool _fFailureActionsOnNonCrashFailures;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFailureActionsFlag"/> struct.
        /// </summary>
        public ServiceFailureActionsFlag(bool enabled)
        {
            _fFailureActionsOnNonCrashFailures = enabled;
        }

        public bool Flag
        {
            get => _fFailureActionsOnNonCrashFailures;
            set => _fFailureActionsOnNonCrashFailures = value;
        }
    }
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class KnownWin32ErrorCodes
    {
        internal const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        internal const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;
    }
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal struct ServiceDescriptionInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string serviceDescription;

        public ServiceDescriptionInfo(string serviceDescription)
        {
            this.serviceDescription = serviceDescription;
        }

        public string ServiceDescription
        {
            get { return serviceDescription; }
            set { serviceDescription = value; }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Keep fields to preserve explicit struct layout for marshalling.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal struct ServiceFailureActionsInfo
    {
        [MarshalAs(UnmanagedType.U4)] private uint dwResetPeriod;
        [MarshalAs(UnmanagedType.LPStr)] private string lpRebootMsg;
        [MarshalAs(UnmanagedType.LPStr)] private string lpCommand;
        [MarshalAs(UnmanagedType.U4)] private int cActions;
        private IntPtr lpsaActions;

        public TimeSpan ResetPeriod => TimeSpan.FromSeconds(dwResetPeriod);

        public string RebootMsg => lpRebootMsg;

        public string Command => lpCommand;

        public int CountActions => cActions;

        //flecoqui
        //public ScAction[] Actions => lpsaActions.MarshalUnmananagedArrayToStruct<ScAction>(cActions);

        /// <summary>
        /// This is the default, as reported by Windows.
        /// </summary>
        internal static ServiceFailureActionsInfo Default =
            new ServiceFailureActionsInfo { dwResetPeriod = 0, lpRebootMsg = null, lpCommand = null, cActions = 0, lpsaActions = IntPtr.Zero };

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFailureActionsInfo"/> class.
        /// </summary>
        internal ServiceFailureActionsInfo(TimeSpan resetPeriod, string rebootMessage, string restartCommand, IReadOnlyCollection<ScAction> actions)
        {
            dwResetPeriod = resetPeriod == TimeSpan.MaxValue ? uint.MaxValue : (uint)Math.Round(resetPeriod.TotalSeconds);
            lpRebootMsg = rebootMessage;
            lpCommand = restartCommand;
            cActions = actions?.Count ?? 0;

            if (null != actions)
            {
                lpsaActions = Marshal.AllocHGlobal(Marshal.SizeOf<ScAction>() * cActions);

                if (lpsaActions == IntPtr.Zero)
                {
                    throw new Exception(string.Format("Unable to allocate memory for service action, error was: 0x{0:X}", Marshal.GetLastWin32Error()));
                }

                var nextAction = lpsaActions;

                foreach (var action in actions)
                {
                    Marshal.StructureToPtr(action, nextAction, fDeleteOld: false);
                    nextAction = (IntPtr)(nextAction.ToInt64() + Marshal.SizeOf<ScAction>());
                }
            }
            else
            {
                lpsaActions = IntPtr.Zero;
            }
        }
    }
    /// <summary>
    /// Simplifies the work of hashing.
    /// Taken from https://rehansaeed.com/gethashcode-made-easy/", and modified with Reshaper
    /// </summary>
    internal struct HashCode
    {
        private readonly int value;
        private HashCode(int value)
        {
            this.value = value;
        }
        public static implicit operator int(HashCode hashCode)
        {
            return hashCode.value;
        }
        public static HashCode Of<T>(T item)
        {
            return new HashCode(GetHashCode(item));
        }
        public HashCode And<T>(T item)
        {
            return new HashCode(CombineHashCodes(this.value, GetHashCode(item)));
        }
        public HashCode AndEach<T>(IEnumerable<T> items)
        {
            //flecoqui
            int hashCode = 0;
            //int hashCode = items.Select(GetHashCode).Aggregate(CombineHashCodes);
            return new HashCode(CombineHashCodes(this.value, hashCode));
        }
        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                // Code copied from System.Tuple so it must be the best way to combine hash codes or at least a good one.
                return ((h1 << 5) + h1) ^ h2;
            }
        }
        private static int GetHashCode<T>(T item)
        {
            return item == null ? 0 : item.GetHashCode();
        }
    }
    /// <inheritdoc />
    /// <summary>
    /// Represents a set of configurations that specify which actions to take if a service fails.
    /// 
    /// A managed class that holds data referring to a <see cref="T:DasMulli.Win32.ServiceUtils.ServiceFailureActionsInfo" /> class which has unmanaged resources
    /// </summary>
    public class ServiceFailureActions : IEquatable<ServiceFailureActions>
    {
        /// <summary>
        /// Gets the reset period in seconds after which previous failures are cleared.
        /// For example: When a service fails two times and then doesn't fail for this amount of time, then an
        /// additional failure is considered a first failure and not a third.
        /// </summary>
        /// <value>
        /// The reset period in seconds after which previous failures are cleared.
        /// For example: When a service fails two times and then doesn't fail for this amount of time, then an
        /// additional failure is considered a first failure and not a third.
        /// </value>
        public TimeSpan ResetPeriod { get; }

        /// <summary>
        /// Gets the reboot message used in case a reboot failure action is configured.
        /// </summary>
        /// <value>
        /// The reboot message used in case a reboot failure action is configured.
        /// </value>
        public string RebootMessage { get; }

        /// <summary>
        /// Gets the command run in case a "run command" failure action is configured.
        /// </summary>
        /// <value>
        /// The command run in case a "run command" failure action is configured.
        /// </value>
        public string RestartCommand { get; }

        /// <summary>
        /// Gets the collections of configured failure actions for each successive time the service failes.
        /// </summary>
        /// <value>
        /// The collections of configured failure actions for each successive time the service failes.
        /// </value>
        public IReadOnlyCollection<ScAction> Actions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFailureActions" /> class.
        /// </summary>
        /// <param name="resetPeriod">The reset period in seconds after which previous failures are cleared.</param>
        /// <param name="rebootMessage">The reboot message used in case a reboot failure action is contaiend in <paramref name="actions"/>.</param>
        /// <param name="restartCommand">The command run in case a "run command" failure action is contained in <paramref name="actions"/>.</param>
        /// <param name="actions">The failure actions.</param>
        public ServiceFailureActions(TimeSpan resetPeriod, string rebootMessage, string restartCommand, IReadOnlyCollection<ScAction> actions)
        {
            ResetPeriod = resetPeriod;
            RebootMessage = rebootMessage;
            RestartCommand = restartCommand;
            Actions = actions;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ServiceFailureActions && Equals((ServiceFailureActions)obj);
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode
                .Of(this.ResetPeriod)
                .And(this.RebootMessage)
                .And(this.RestartCommand)
                .AndEach(this.Actions);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ServiceFailureActions other)
        {
            if (other == null)
            {
                return false;
            }
            return this.GetHashCode() == other.GetHashCode();
        }
    }
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Subclassed by test proxy")]
    internal class ServiceHandle : SafeHandle
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Exposed for testing via InternalsVisibleTo.")]
        internal Win32ServiceInterface NativeInterop { get; set; } = Win32ServiceInterop.Wrapper;

        internal ServiceHandle() : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeInterop.CloseServiceHandle(handle);
        }

        public override bool IsInvalid
        {
            [System.Security.SecurityCritical]
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        public virtual void Start(bool throwIfAlreadyRunning = true)
        {
            if (!NativeInterop.StartServiceW(this, 0, IntPtr.Zero))
            {
                var win32Error = Marshal.GetLastWin32Error();
                if (win32Error != KnownWin32ErrorCodes.ERROR_SERVICE_ALREADY_RUNNING || throwIfAlreadyRunning)
                {
                    throw new Win32Exception(win32Error);
                }
            }
        }

        public virtual void Delete()
        {
            if (!NativeInterop.DeleteService(this))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public virtual void SetDescription(string description)
        {
            var descriptionInfo = new ServiceDescriptionInfo(description ?? string.Empty);
            var lpDescriptionInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceDescriptionInfo>());
            try
            {
                Marshal.StructureToPtr(descriptionInfo, lpDescriptionInfo, fDeleteOld: false);
                try
                {
                    if (!NativeInterop.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.ServiceDescription, lpDescriptionInfo))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    Marshal.DestroyStructure<ServiceDescriptionInfo>(lpDescriptionInfo);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpDescriptionInfo);
            }
        }

        public virtual void SetFailureActions(ServiceFailureActions serviceFailureActions)
        {
            var failureActions = serviceFailureActions == null ? ServiceFailureActionsInfo.Default : new ServiceFailureActionsInfo(serviceFailureActions.ResetPeriod, serviceFailureActions.RebootMessage, serviceFailureActions.RestartCommand, serviceFailureActions.Actions);
            var lpFailureActions = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceFailureActionsInfo>());
            try
            {
                Marshal.StructureToPtr(failureActions, lpFailureActions, fDeleteOld: false);
                try
                {
                    if (!NativeInterop.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.FailureActions, lpFailureActions))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    Marshal.DestroyStructure<ServiceFailureActionsInfo>(lpFailureActions);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpFailureActions);
            }
        }

        public virtual void SetFailureActionFlag(bool enabled)
        {
            var failureActionsFlag = new ServiceFailureActionsFlag(enabled);
            var lpFailureActionsFlag = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceFailureActionsFlag>());
            try
            {
                Marshal.StructureToPtr(failureActionsFlag, lpFailureActionsFlag, fDeleteOld: false);
                try
                {
                    bool result = NativeInterop.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.FailureActionsFlag, lpFailureActionsFlag);
                    if (!result)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    Marshal.DestroyStructure<ServiceFailureActionsFlag>(lpFailureActionsFlag);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lpFailureActionsFlag);
            }
        }

        public virtual void ChangeConfig(string displayName, string binaryPath, ServiceType serviceType, ServiceStartType startupType, ErrorSeverity errorSeverity, Win32ServiceCredentials credentials)
        {
            var success = NativeInterop.ChangeServiceConfigW(this, serviceType, startupType, errorSeverity, binaryPath, null, IntPtr.Zero, null, credentials.UserName, credentials.Password, displayName);
            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        //flecoqui
        //public virtual unsafe void SetDelayedAutoStartFlag(bool delayedAutoStart)

        //{
        //    int value = delayedAutoStart ? 1 : 0;
        //    var success = NativeInterop.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.DelayedAutoStartInfo, new IntPtr(&value));
        //    if (!success)
        //    {
        //        throw new Win32Exception(Marshal.GetLastWin32Error());
        //    }
        //}
    }
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal enum ServiceStartType : uint
    {
        StartOnBoot = 0,
        StartOnSystemStart = 1,
        AutoStart = 2,
        StartOnDemand = 3,
        Disabled = 4
    }
    /// <summary>
    /// Specifies the severity of the error if the service fails to start during boot 
    /// </summary>

    public enum ErrorSeverity : uint
    {
        /// <summary>
        /// SC.exe help:
        /// The error is logged and startup continues. No notification is given to the user beyond recording the error in the Event Log.
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// SC.exe help:
        /// The error is logged and a message box is displayed informing the user that a service has failed to start. Startup will continue. This is the default setting.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// SC.exe help:
        /// The error is logged (if possible). The computer attempts to restart with the last-known-good configuration. This could result in the computer being able to restart, but the service may still be unable to run.
        /// </summary>
        Severe = 2,
        /// <summary>
        /// SC.exe help:
        /// The error is logged (if possible). The computer attempts to restart with the last-known-good configuration. If the last-known-good configuration fails, startup also fails, and the boot process halts with a Stop error.
        /// </summary>
        Crititcal = 3
    }
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "External API")]
    internal enum ServiceConfigInfoTypeLevel : uint
    {
        ServiceDescription = 1,
        FailureActions = 2,
        DelayedAutoStartInfo = 3,
        FailureActionsFlag = 4,
        ServiceSidInfo = 5,
        RequiredPrivilegesInfo = 6,
        PreShutdownInfo = 7,
        TriggerInfo = 8,
        PreferredNode = 9,
        LaunchProtected = 12
    }
    internal interface Win32ServiceInterface
    {
            bool CloseServiceHandle(IntPtr handle);

            bool StartServiceCtrlDispatcherW(ServiceTableEntry[] serviceTable);

            ServiceStatusHandle RegisterServiceCtrlHandlerExW(string serviceName, ServiceControlHandler serviceControlHandler, IntPtr context);

            [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Matches native signature.")]
            bool SetServiceStatus(ServiceStatusHandle statusHandle, ref ServiceStatus pServiceStatus);

            ServiceControlManager OpenSCManagerW(string machineName, string databaseName, ServiceControlManagerAccessRights dwAccess);

            ServiceHandle CreateServiceW(
                ServiceControlManager serviceControlManager,
                string serviceName,
                string displayName,
                ServiceControlAccessRights desiredControlAccess,
                ServiceType serviceType,
                ServiceStartType startType,
                ErrorSeverity errorSeverity,
                string binaryPath,
                string loadOrderGroup,
                IntPtr outUIntTagId,
                string dependencies,
                string serviceUserName,
                string servicePassword);

        bool ChangeServiceConfigW(
            ServiceHandle service,
            ServiceType serviceType,
            ServiceStartType startType,
            ErrorSeverity errorSeverity,
            string binaryPath,
            string loadOrderGroup,
            IntPtr outUIntTagId,
            string dependencies,
            string serviceUserName,
            string servicePassword,
            string displayName);

        ServiceHandle OpenServiceW(ServiceControlManager serviceControlManager, string serviceName, ServiceControlAccessRights desiredControlAccess);

        bool StartServiceW(ServiceHandle service, uint argc, IntPtr wargv);
        bool ControlServiceExW(ServiceHandle service, uint control, uint level, IntPtr status);
        bool DeleteService(ServiceHandle service);

        bool ChangeServiceConfig2W(ServiceHandle service, ServiceConfigInfoTypeLevel infoTypeLevel, IntPtr info);
        }

}
