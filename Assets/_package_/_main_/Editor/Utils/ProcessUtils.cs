using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UPMTool
{
    public delegate void DelReadStdOutput(string result);

    public static class ProcessUtils
    {
        private static DelReadStdOutput ReadStdOutput;

        public static void method()
        {
            // todo 异步
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);

            Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
            proc.EnableRaisingEvents = true;

            proc.Start();
            proc.StandardInput.WriteLine("@ echo off");
            proc.StandardInput.WriteLine("git ls-remote https://github.com/Chino66/Network.git");
            proc.StandardInput.WriteLine("exit");
//            proc.BeginOutputReadLine();
            string outStr = proc.StandardOutput.ReadToEnd();
            Debug.LogError(outStr);
            proc.Close();
        }

        private static void ReadStdOutputAction(string result)
        {
            Debug.Log(result);
        }

        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ReadStdOutput?.Invoke(e.Data);
            }
        }
    }
}