using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace ASTool
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceTableEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string serviceName;

        internal IntPtr serviceMainFunction;
    }
    class Win32ServiceInterop
    {
        internal static readonly Win32ServiceInterface Wrapper = new InteropWrapper();

#if NETSTANDARD2_0
        // in .NET Core 2.0, ms-api-* facades are no longer deployed since
        // Windows Nano Server 2016 shipped with inverse forwarding dlls
        // so advapi32.dll is safe and requires a lesser service pack level on win7 / server2008

        // ReSharper disable once InconsistentNaming
        private const string DllServiceCore_L1_1_0 = "advapi32.dll";
        // ReSharper disable once InconsistentNaming
        private const string DllServiceManagement_L1_1_0 = "advapi32.dll";
        // ReSharper disable once InconsistentNaming
        private const string DllServiceManagement_L2_1_0 = "advapi32.dll";
#else
        // ReSharper disable once InconsistentNaming
        private const string DllServiceCore_L1_1_0 = "api-ms-win-service-core-l1-1-0.dll";
        // ReSharper disable once InconsistentNaming
        private const string DllServiceManagement_L1_1_0 = "api-ms-win-service-management-l1-1-0.dll";
        // ReSharper disable once InconsistentNaming
        private const string DllServiceManagement_L2_1_0 = "api-ms-win-service-management-l2-1-0.dll";
#endif

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport(DllServiceCore_L1_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool StartServiceCtrlDispatcherW([MarshalAs(UnmanagedType.LPArray)] ServiceTableEntry[] serviceTable);

        [DllImport(DllServiceCore_L1_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ServiceStatusHandle RegisterServiceCtrlHandlerExW(string serviceName, ServiceControlHandler serviceControlHandler, IntPtr context);

        [DllImport(DllServiceCore_L1_1_0, ExactSpelling = true, SetLastError = true)]
        public static extern bool SetServiceStatus(ServiceStatusHandle statusHandle, ref ServiceStatus pServiceStatus);

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ServiceControlManager OpenSCManagerW(string machineName, string databaseName, ServiceControlManagerAccessRights dwAccess);

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ServiceHandle CreateServiceW(
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

        [DllImport(DllServiceManagement_L2_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ChangeServiceConfigW(
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

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ServiceHandle OpenServiceW(ServiceControlManager serviceControlManager, string serviceName,
            ServiceControlAccessRights desiredControlAccess);

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true)]
        public static extern bool StartServiceW(ServiceHandle service, uint argc, IntPtr wargv);

        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true,CharSet = CharSet.Unicode)]
        public static extern bool ControlServiceExW(ServiceHandle service, uint control, uint level, IntPtr  status);


        [DllImport(DllServiceManagement_L1_1_0, ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteService(ServiceHandle service);

        [DllImport(DllServiceManagement_L2_1_0, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ChangeServiceConfig2W(ServiceHandle service, ServiceConfigInfoTypeLevel infoTypeLevel, IntPtr info);
        private class InteropWrapper : Win32ServiceInterface
        {
             bool Win32ServiceInterface.CloseServiceHandle(IntPtr handle)
            {
                return CloseServiceHandle(handle);
            }

            bool Win32ServiceInterface.StartServiceCtrlDispatcherW(ServiceTableEntry[] serviceTable)
            {
                return StartServiceCtrlDispatcherW(serviceTable);
            }

            ServiceStatusHandle Win32ServiceInterface.RegisterServiceCtrlHandlerExW(string serviceName, ServiceControlHandler serviceControlHandler, IntPtr context)
            {
                return RegisterServiceCtrlHandlerExW(serviceName, serviceControlHandler, context);
            }

            bool Win32ServiceInterface.SetServiceStatus(ServiceStatusHandle statusHandle, ref ServiceStatus pServiceStatus)
            {
                return SetServiceStatus(statusHandle, ref pServiceStatus);
            }

            ServiceControlManager Win32ServiceInterface.OpenSCManagerW(string machineName, string databaseName, ServiceControlManagerAccessRights dwAccess)
            {
                return OpenSCManagerW(machineName, databaseName, dwAccess);
            }

            ServiceHandle Win32ServiceInterface.CreateServiceW(ServiceControlManager serviceControlManager, string serviceName, string displayName,
                ServiceControlAccessRights desiredControlAccess, ServiceType serviceType, ServiceStartType startType, ErrorSeverity errorSeverity,
                string binaryPath,
                string loadOrderGroup, IntPtr outUIntTagId, string dependencies, string serviceUserName, string servicePassword)
            {
                return CreateServiceW(serviceControlManager, serviceName, displayName, desiredControlAccess, serviceType, startType, errorSeverity,
                    binaryPath, loadOrderGroup, outUIntTagId, dependencies, serviceUserName, servicePassword);
            }

            bool Win32ServiceInterface.ChangeServiceConfigW(ServiceHandle service, ServiceType serviceType, ServiceStartType startType, ErrorSeverity errorSeverity, string binaryPath, string loadOrderGroup, IntPtr outUIntTagId, string dependencies, string serviceUserName, string servicePassword, string displayName)
            {
                return ChangeServiceConfigW(service, serviceType, startType, errorSeverity, binaryPath, loadOrderGroup, outUIntTagId, dependencies, serviceUserName, servicePassword, displayName);
            }

            ServiceHandle Win32ServiceInterface.OpenServiceW(ServiceControlManager serviceControlManager, string serviceName, ServiceControlAccessRights desiredControlAccess)
            {
                return OpenServiceW(serviceControlManager, serviceName, desiredControlAccess);
            }

            bool Win32ServiceInterface.StartServiceW(ServiceHandle service, uint argc, IntPtr wargv)
            {
                return StartServiceW(service, argc, wargv);
            }
            bool Win32ServiceInterface.ControlServiceExW(ServiceHandle service, uint control, uint level ,IntPtr status)
            {
                return ControlServiceExW( service,  control, level, status);
            }

            bool Win32ServiceInterface.DeleteService(ServiceHandle service)
            {
                return DeleteService(service);
            }

            bool Win32ServiceInterface.ChangeServiceConfig2W(ServiceHandle service, ServiceConfigInfoTypeLevel infoTypeLevel, IntPtr info)
            {
                return ChangeServiceConfig2W(service, infoTypeLevel, info);
            }
        }
    }
}
