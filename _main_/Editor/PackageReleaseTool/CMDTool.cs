using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;

/// <summary>
/// package发布工具
/// 1. 将package发布到git远程仓库
/// 2. 若没有git远程仓库,则创建git仓库并推送到远程
/// </summary>
public class CMDTool : EditorWindow
{
    [MenuItem("Tool/UPM Tool/cmd Tool")]
    public static void Show()
    {
        CMDTool ct = GetWindow<CMDTool>();
        ct.titleContent = new GUIContent("cmd Tool");
    }

    /// <summary>
    /// 进程代理类
    /// </summary>
    private ProcessProxy proxy;

    /// <summary>
    /// 命令执行完成的返回消息队列
    /// </summary>
    private Queue<string> returnMsgs;

    /// <summary>
    /// 输入的命令队列
    /// </summary>
//    private Queue<string> inputCmds;
//    private Queue<CmdItem> _cmdItems;
    private bool _cmdReturnFlag = false;

    private void OnEnable()
    {
        InitUI();
    }

    private void StartCMD()
    {
        returnMsgs = new Queue<string>();

        Debug.Log("启动cmd");

        proxy = new ProcessProxy();

        proxy.Start();
    }


    private void InitUI()
    {
        var start = new Button();
        start.name = "btn_start_cmd";
        start.text = "启动cmd";
        start.clicked += () =>
        {
            StartCMD();
            StartButton.SetEnabled(false);
            CloseButton.SetEnabled(true);
            OutputBox.SetEnabled(true);
            SetInputEnable(true);
        };
        rootVisualElement.Add(start);

        // 关闭cmd进程
        var close = new Button();
        close.name = "btn_close_cmd";
        close.text = "关闭cmd";
        close.clicked += () =>
        {
            CloseProxy();
            StartButton.SetEnabled(true);
            CloseButton.SetEnabled(false);
            OutputBox.SetEnabled(false);
            SetInputEnable(false);
        };
        close.SetEnabled(false);
        rootVisualElement.Add(close);

        var output = new Box();
        output.name = "box_output";
        {
            // 滚动视图
            var scrollView = new ScrollView();
            scrollView.name = "sv_output";
            {
                // 结果输出
                var label = new Label();
                label.name = "lab_output";
                label.text = "";
                scrollView.Add(label);
            }
            output.Add(scrollView);
        }
        output.SetEnabled(false);
        rootVisualElement.Add(output);

        // cmd命令输入框
        var inputText = new TextField();
        inputText.name = "ipt_text";
        inputText.SetEnabled(false);
        rootVisualElement.Add(inputText);

        // 确定输入命令
        var input = new Button();
        input.name = "btn_input";
        input.text = "确定";
        input.clicked += () =>
        {
            returnMsgs.Clear();

            RunCmd(inputText.value, (msgs) =>
            {
                returnMsgs = msgs;
                _cmdReturnFlag = true;
//                returnMsgs.Enqueue(msg);
//                if (msg.Equals(ProcessProxy.CommandReturnFlag))
//                {
//                    _cmdReturnFlag = true;
//                }
            });
            SetInputEnable(false);
        };
        input.SetEnabled(false);
        rootVisualElement.Add(input);
    }

    private void SetInputEnable(bool enable)
    {
        InputTextField.SetEnabled(enable);
        InputButton.SetEnabled(enable);
    }

    public void RunCmd(string cmd, CmdOutput callback)
    {
        proxy.Input(cmd, callback);
    }

    private Box OutputBox => rootVisualElement.Q<Box>("box_output");

    private ScrollView OutputScrollView => OutputBox.Q<ScrollView>("sv_output");

    private Label OutputLabel => OutputScrollView.Q<Label>("lab_output");

    private Button StartButton => rootVisualElement.Q<Button>("btn_start_cmd");

    private Button CloseButton => rootVisualElement.Q<Button>("btn_close_cmd");


    private TextField InputTextField => rootVisualElement.Q<TextField>("ipt_text");

    private Button InputButton => rootVisualElement.Q<Button>("btn_input");

    private float HighValue => OutputScrollView.verticalScroller.slider.highValue;

    private float SliderValue
    {
        get => OutputScrollView.verticalScroller.value;
        set => OutputScrollView.verticalScroller.value = value;
    }

    private void Update()
    {
        if (_cmdReturnFlag)
        {
            var command = returnMsgs.Dequeue();
            var content = "";
            while (returnMsgs.Count > 0)
            {
                var line = returnMsgs.Dequeue();
                if (!line.Contains(ProcessProxy.CommandReturnFlag))
                {
                    content += line + "\n";
                }
            }

//            Debug.Log(command);
//            Debug.Log(content);
            OutputLabel.text += $"输入命令:[{command}]\n\n";
            OutputLabel.text += $"输出:\n{content}\n";
            OutputLabel.text += $">----------------------------------------------------------<";
            TimeUtil.DoActionWaitAtTime(0.1f, () =>
            {
                // 只有在显示滑动条的时候才拉到最底
                var sv = OutputScrollView;
                if (sv != null && sv.verticalScroller.visible)
                {
                    SliderValue = HighValue;
                }
            });
            _cmdReturnFlag = false;
            SetInputEnable(true);
        }
    }

    private void OnDisable()
    {
        CloseProxy();
    }

    private void OnDestroy()
    {
        CloseProxy();
    }

    private void CloseProxy()
    {
        if (proxy != null)
        {
            proxy.Close();
            proxy = null;
            OutputLabel.text = "";
            Debug.Log("Process Proxy Close");
        }
    }
}

//public class CmdItem
//{
//    public string cmd;
//    public ProcessOutput callback;
//
//    public CmdItem(string cmd, ProcessOutput callback)
//    {
//        this.cmd = cmd;
//        this.callback = callback;
//    }
//}

#region 垃圾堆

//        Debug.LogError(Application.dataPath);
//        ProcessUtils.method();
//        // 代码中使用style的方法 水平放置元素
//        box_cmd.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

#endregion