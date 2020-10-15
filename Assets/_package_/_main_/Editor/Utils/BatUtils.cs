//using System.IO;

using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

//using File = UnityEngine.Windows.File;

namespace UPMTool
{
    public static class BatUtils
    {
        /// <summary>
        /// .bat文件路径
        /// </summary>
//        private static string Bat_Path = PackageChecker.PackagePath + @"Resources\Bat\git_get_tags.bat";
        private static string Bat_Path = Path.Combine(PackagePath.MainPath, @"Resources/Bat/git_get_tags.bat");

        /// <summary>
        /// 运行.bat
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public static void RunBat(string url, Action<string> callback)
        {
            GenerateBatFile(url);

            FileInfo info = new FileInfo(Bat_Path);

            var path = info.FullName;

            // 新开一个进程,运行.bat
            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
            callback?.Invoke(result);
            proc.WaitForExit();
            proc.Close();
        }

        /// <summary>
        /// 生成.bat内容
        /// </summary>
        /// <param name="url"></param>
        public static void GenerateBatFile(string url)
        {
//            Debug.LogFormat("BatUtils.GenerateBatFile url is {0}", url);

            FileInfo info = new FileInfo(Bat_Path);

            var path = info.FullName;

            if (File.Exists(path) == false)
            {
                CreateFile(path);
            }

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
            sw.WriteLine("@ echo off");
            sw.WriteLine("git ls-remote --tags {0}", url);
            sw.WriteLine("exit");
            sw.Close();
        }

        /// <summary>
        /// 创建.bat文件,空文件
        /// </summary>
        /// <param name="path"></param>
        private static void CreateFile(string path)
        {
            if (File.Exists(path) == false)
            {
                File.Create(path);
            }
        }


// 直接执行命令行
//        public delegate void DelReadStdOutput(string result);
//        ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
//        private void ReadStdOutputAction(string result)
//        {
//            Debug.Log(result);
//        }

//        Process  proc = new Process();
//        proc.StartInfo.FileName = "cmd.exe";
//        proc.StartInfo.CreateNoWindow = true;
//        proc.StartInfo.UseShellExecute = false;
//        proc.StartInfo.RedirectStandardError = true;
//        proc.StartInfo.RedirectStandardInput = true;
//        proc.StartInfo.RedirectStandardOutput = true;
//        
////        proc.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
////        proc.EnableRaisingEvents = true;   
//        
//        proc.Start();
//        proc.StandardInput.WriteLine("@ echo off");
//        proc.StandardInput.WriteLine("git ls-remote https://github.com/Chino66/Network.git");
//        proc.StandardInput.WriteLine("exit");
////        proc.BeginOutputReadLine();
//        string outStr = proc.StandardOutput.ReadToEnd();
//        Debug.Log(outStr);
//        proc.Close();
//        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            if (e.Data != null)
//            {
//                ReadStdOutput?.Invoke(e.Data);
//            }
//        }
    }
}