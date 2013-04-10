using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace PointGaming.Desktop.Update
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            string processName = args[0];
            string updateFileName = args[1];
            string runAfterStart = args[2];
            var updateFilePath = System.IO.Path.GetTempPath() + "\\" + updateFileName;

            WaitForProgramToExit(processName);

            RunUpdate(updateFilePath);

            System.IO.File.Delete(updateFilePath);

            RestartProgram(runAfterStart);
        }

        private static void RunUpdate(string updateFilePath)
        {
            Process updateInvoker = new Process();
            updateInvoker.StartInfo.FileName = "msiexec";
            updateInvoker.StartInfo.Arguments = "/p " + BuildArguments(updateFilePath);
            updateInvoker.StartInfo.UseShellExecute = false;
            updateInvoker.Start();
            updateInvoker.WaitForExit();
        }

        private static void RestartProgram(string runAfterStart)
        {
            Process updateInvoker = new Process();
            updateInvoker.StartInfo.FileName = runAfterStart;
            updateInvoker.StartInfo.UseShellExecute = false;
            updateInvoker.Start();
        }

        private static void WaitForProgramToExit(string processName)
        {
            var list = Process.GetProcessesByName(processName);
            if (list.Length == 0)
                return;
            list[0].WaitForExit();
        }


        private static string BuildArguments(params string[] arguments)
        {
            var result = "";
            foreach (var arg in arguments)
            {
                if (result.Length > 0)
                    result = result + " ";

                var a = arg.Replace("\"", "\"\"");
                result = result + "\"" + a + "\"";
            }
            return result;
        }
    }
}
