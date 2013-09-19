using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PointGaming.ClientWebService
{

    public static class DesktopSessionHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }
        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }
        private static uint MAXIMUM_ALLOWED = 0x02000000;

        [Flags]
        enum CreationFlags
        {
            NORMAL_PRIORITY_CLASS = 0x20,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        }
        [Flags]
        enum LogonFlags
        {
            /// <summary>
            /// Log on, then load the user's profile in the HKEY_USERS registry key. The function
            /// returns after the profile has been loaded. Loading the profile can be time-consuming,
            /// so it is best to use this value only if you must access the information in the
            /// HKEY_CURRENT_USER registry key.
            /// NOTE: Windows Server 2003: The profile is unloaded after the new process has been
            /// terminated, regardless of whether it has created child processes.
            /// </summary>
            /// <remarks>See LOGON_WITH_PROFILE</remarks>
            WithProfile = 1,
            /// <summary>
            /// Log on, but use the specified credentials on the network only. The new process uses the
            /// same token as the caller, but the system creates a new logon session within LSA, and
            /// the process uses the specified credentials as the default credentials.
            /// This value can be used to create a process that uses a different set of credentials
            /// locally than it does remotely. This is useful in inter-domain scenarios where there is
            /// no trust relationship.
            /// The system does not validate the specified credentials. Therefore, the process can start,
            /// but it may not have access to network resources.
            /// </summary>
            /// <remarks>See LOGON_NETCREDENTIALS_ONLY</remarks>
            NetCredentialsOnly

        }

        /// <summary>
        /// The WTSGetActiveConsoleSessionId function retrieves the Remote Desktop Services session that
        /// is currently attached to the physical console. The physical console is the monitor, keyboard, and mouse.
        /// Note that it is not necessary that Remote Desktop Services be running for this function to succeed.
        /// </summary>
        /// <returns>The session identifier of the session that is attached to the physical console. If there is no
        /// session attached to the physical console, (for example, if the physical console session is in the process
        /// of being attached or detached), this function returns 0xFFFFFFFF.</returns>
        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();
        /// <summary>
        /// Retrieves the Remote Desktop Services session associated with a specified process.
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern bool ProcessIdToSessionId(int dwProcessId, ref uint pSessionId);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            CreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessAsUserW(
            IntPtr hToken,
            string lpApplicationName,
            char[] lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            CreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
        
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);


        public static uint GetActiveSessionId()
        {
            return WTSGetActiveConsoleSessionId();
        }

        public static uint GetCurrentProcessSessionId()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var processId = process.Id;
            uint sessionId = 0;
            ProcessIdToSessionId(processId, ref sessionId);
            return sessionId;
        }

        public static void LaunchInActiveSession(string consoleCommand)
        {
            IntPtr hToken;
            IntPtr hTokenDup;
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();

            uint dwSessionId = WTSGetActiveConsoleSessionId();
            WTSQueryUserToken(dwSessionId, out hToken);
            DuplicateTokenEx(hToken, MAXIMUM_ALLOWED, ref sa, SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, TOKEN_TYPE.TokenPrimary, out hTokenDup);

            CreationFlags dwCreationFlag = CreationFlags.NORMAL_PRIORITY_CLASS | CreationFlags.CREATE_NEW_CONSOLE;
            IntPtr pEnv;
            if (CreateEnvironmentBlock(out pEnv, hTokenDup, false))
                dwCreationFlag |= CreationFlags.CREATE_UNICODE_ENVIRONMENT;
            else
                pEnv = IntPtr.Zero;

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            SECURITY_ATTRIBUTES saProcessAttributes = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES saThreadAttributes = new SECURITY_ATTRIBUTES();
            PROCESS_INFORMATION pi;

            var charCommand = new char[1024];
            for (int i = 0; i < consoleCommand.Length; i++)
                charCommand[i] = consoleCommand[i];

            CreateProcessAsUserW(
                hTokenDup,
                null,
                charCommand,
                ref saProcessAttributes,
                ref saThreadAttributes,
                false,
                dwCreationFlag,
                pEnv,
                null,
                ref si,
                out pi
            );

        }
    }
}
