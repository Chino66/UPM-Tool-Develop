using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace UPMTool
{
    public delegate void ProcessOutput(string result);

    public delegate void CmdOutput(Queue<string> msgs);

    public class ProcessProxy
    {
        /// <summary>
        /// 一条命令结束的标记
        /// </summary>
        public const string CommandReturnFlag = "#command return#";

        /// <summary>
        /// 每执行一条语句,都要附加执行这条语句,这条语句用于输出一句话:"#command return#"
        /// 这句话用于判断前一条命令是否执行完成,已获取完整的输出
        /// 因为cmd不能知道什么时候执行完成,所以通过这句话可以判断上一条命令是否执行完成
        /// 只有上一条命令完成,才能执行这一条命令
        /// </summary>
        private const string CommandReturnCMD = "echo " + CommandReturnFlag;

        private Process _process;

        private ProcessOutput _processOutput;

        private CmdOutput _currentCallback;

        /// <summary>
        /// 返回的消息是否需要包含执行的命令
        /// </summary>
        private bool _fullInfo = true;

        /// <summary>
        /// 命令执行完成的返回消息队列
        /// </summary>
        private Queue<string> _returnMsgs;

        /// <summary>
        /// 调试模式,显示接收到的所有消息
        /// </summary>
        private bool _debugMode = false;

        public ProcessProxy()
        {
            _process = new Process();

            _process.StartInfo.FileName = "cmd.exe";
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;

            _returnMsgs = new Queue<string>();
            RegisterProcessOutput(MessageHandle);
        }

        private void MessageHandle(string msg)
        {
            if (_debugMode)
            {
                Debug.Log($"[debug mode]{msg}");
            }

            if (msg.Equals(ProcessProxy.CommandReturnFlag))
            {
                var msgs = new Queue<string>();
                var command = _returnMsgs.Dequeue();

                if (_fullInfo)
                {
                    msgs.Enqueue(command);
                }

                while (_returnMsgs.Count > 0)
                {
                    var line = _returnMsgs.Dequeue();
                    if (!line.Contains(ProcessProxy.CommandReturnFlag))
                    {
                        msgs.Enqueue(line);
                    }
                }

                // 如果没有如何返回值,则默认添加一个"\n"换行符
                if (msgs.Count == 0)
                {
                    msgs.Enqueue("\n");
                }

                _currentCallback?.Invoke(new Queue<string>(msgs));
            }
            else
            {
                _returnMsgs.Enqueue(msg);
            }
        }

        public void SetDebugMode(bool value)
        {
            _debugMode = value;
        }

        public void RegisterProcessOutput(ProcessOutput func)
        {
            if (func != null)
            {
                _processOutput += func;
            }
        }

        public void UnregisterProcessOutput(ProcessOutput func)
        {
            if (_processOutput != null && func != null)
            {
                _processOutput -= func;
            }
        }

        public void Start()
        {
            _process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
            _process.EnableRaisingEvents = true;
            _process.Start();
            _process.BeginOutputReadLine();
            Input("@ echo off");
        }

        public void Input(string cmd, CmdOutput callback = null, bool fullInfo = true)
        {
            _returnMsgs.Clear();
            
            _currentCallback = callback;
            _fullInfo = fullInfo;
            
            _process.StandardInput.WriteLine(cmd);
            _process.StandardInput.WriteLine(CommandReturnCMD);
        }

        public void Close()
        {
            Input("exit");
            _process.Close();
            _processOutput = null;
            _process = null;
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _processOutput?.Invoke(e.Data);
            }
        }
    }
}