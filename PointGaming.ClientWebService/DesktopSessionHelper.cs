using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PointGaming.ClientWebService
{

    public static class DesktopSessionHelper
    {

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        } 

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

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
        
        [Flags]
        enum AccessFlags
        {
            MAXIMUM_ALLOWED = 0x02000000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            TOKEN_ASSIGN_PRIMARY = 0x0001,
            TOKEN_DUPLICATE = 0x0002,
            TOKEN_IMPERSONATE = 0x0004,
            TOKEN_QUERY = 0x0008,
            TOKEN_QUERY_SOURCE = 0x0010,
            TOKEN_ADJUST_PRIVILEGES = 0x0020,
            TOKEN_ADJUST_GROUPS = 0x0040,
            TOKEN_ADJUST_DEFAULT = 0x0080,
            TOKEN_ADJUST_SESSIONID = 0x0100,
            TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY),
            TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
                TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
                TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
                TOKEN_ADJUST_SESSIONID),
        }

        [Flags]
        enum CreationFlags
        {
            NORMAL_PRIORITY_CLASS = 0x20,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_NO_WINDOW = 0x08000000,
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

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            LogonFlags dwLogonFlags,
            string lpApplicationName,
            char[] lpCommandLine,
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
            AccessFlags dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSEnumerateSessions(
                        System.IntPtr hServer,
                        int Reserved,
                        int Version,
                        ref System.IntPtr ppSessionInfo,
                        ref int pCount);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenThreadToken(
            IntPtr ThreadHandle,
            AccessFlags DesiredAccess,
            bool OpenAsSelf,
            out IntPtr TokenHandle);

        /// <summary>
        /// This does not return a real handle, but a fixed value which is interpreted as the current, calling thread by any
        /// function which requires a thread handle. To get the real handle, you need to call GetCurrentThreadId, then
        /// OpenThread with the returned value along with the necessary permissions and what-not. Then close the handle
        /// when you're done with it.
        /// </summary>
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool RevertToSelf();


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

        private static uint GetSessionId()
        {
            IntPtr currentToken = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;
            IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

            int dwSessionId = 0;
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr hTokenDup = IntPtr.Zero;

            IntPtr pSessionInfo = IntPtr.Zero;
            int dwCount = 0;

            WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1,
                                 ref pSessionInfo, ref dwCount);

            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

            Int32 current = (int)pSessionInfo;
            for (int i = 0; i < dwCount; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure(
                    (System.IntPtr)current, typeof(WTS_SESSION_INFO));
                if (WTS_CONNECTSTATE_CLASS.WTSActive == si.State)
                {
                    dwSessionId = si.SessionID;
                    break;
                }

                current += dataSize;
            }

            WTSFreeMemory(pSessionInfo);

            return (uint)dwSessionId;
        }

        public static void LaunchInActiveSession(string consoleCommand)
        {
            IntPtr hToken;
            IntPtr hTokenDup;
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();

            AccessFlags acessF = AccessFlags.TOKEN_ASSIGN_PRIMARY | AccessFlags.TOKEN_DUPLICATE | AccessFlags.TOKEN_QUERY | AccessFlags.TOKEN_ADJUST_DEFAULT | AccessFlags.TOKEN_ADJUST_SESSIONID;
            //AccessFlags acessF = AccessFlags.MAXIMUM_ALLOWED;

            //uint dwSessionId = GetSessionId();
            uint dwSessionId = WTSGetActiveConsoleSessionId();
            var gotUser = WTSQueryUserToken(dwSessionId, out hToken);

            if (!gotUser)
            {
                CWService.AppendConsoleLine("failed to get user: " + Marshal.GetLastWin32Error());
                return;
            }

            DuplicateTokenEx(hToken, acessF, ref sa, SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, TOKEN_TYPE.TokenPrimary, out hTokenDup);

            ImpersonateLoggedOnUser(hTokenDup);

            CreationFlags dwCreationFlag = CreationFlags.NORMAL_PRIORITY_CLASS;
            dwCreationFlag |= CreationFlags.CREATE_NO_WINDOW | CreationFlags.CREATE_NEW_PROCESS_GROUP;
            //dwCreationFlag |= CreationFlags.CREATE_NEW_CONSOLE;
            IntPtr pEnv;
            if (CreateEnvironmentBlock(out pEnv, hTokenDup, false))
                dwCreationFlag |= CreationFlags.CREATE_UNICODE_ENVIRONMENT;
            else
                pEnv = IntPtr.Zero;

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = "winsta0\\default";
            SECURITY_ATTRIBUTES saProcessAttributes = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES saThreadAttributes = new SECURITY_ATTRIBUTES();
            PROCESS_INFORMATION pi;

            var charCommand = new char[1024];
            for (int i = 0; i < consoleCommand.Length; i++)
                charCommand[i] = consoleCommand[i];

            var res = CreateProcessAsUserW(
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

            RevertToSelf();

            if (!res)
            {
                CWService.AppendConsoleLine("Failed to create process: " + Marshal.GetLastWin32Error());
            }

            DestroyEnvironmentBlock(pEnv);
            CloseHandle(hTokenDup);
        }
    }
}
