using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UPMTool
{
    /// <summary>
    /// 插件发布流程:
    /// 1. 检测当前项目是否有git仓库(远程仓库),没有则需要自行创建并推送远程
    /// 2. 获取远程仓库所有tag标签,并过滤,合法格式:refs/tags/x.x.x (x.x.x是版本)
    /// 3. 确定要发布的版本号:
    ///     1)先从package.json中获取目前的版本号(这个版本号要和发布的版本号一致)
    ///     2)版本号校验,格式正确且唯一
    /// 4. 创建版本号标签
    /// 5. 推送到远程
    /// </summary>
    public class PackageReleaseTool : EditorWindow
    {
        [MenuItem("Tool/UPM Tool/Package Release Tool")]
        public static void Show()
        {
            PackageReleaseTool pct = GetWindow<PackageReleaseTool>();
            pct.titleContent = new GUIContent("Package Release Tool");
        }

        private const string NotGitRepositoryMsg =
            "fatal: not a git repository (or any of the parent directories): .git";

        /// <summary>
        /// 进程代理类
        /// </summary>
        private ProcessProxy _proxy;

        private ProcessProxy getProcessProxy
        {
            get
            {
                if (_proxy == null)
                {
                    _proxy = new ProcessProxy();
                    _proxy.Start();
                }

                return _proxy;
            }
        }

        private Label OutputLabel => rootVisualElement.Q<Label>("lab_output");

        private void OnEnable()
        {
            var label = new Label();
            label.name = "lab_output";
            rootVisualElement.Add(label);

            // 初始化process代理类(必要)
            if (_proxy == null)
            {
                _proxy = new ProcessProxy();
                _proxy.Start();
            }

            // 处理流
            ProcessStream();
        }

        /// <summary>
        /// 处理流
        /// </summary>
        /// <returns></returns>
        async Task ProcessStream()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            // 1. 检测当前项目是否有git仓库(远程仓库),没有则需要自行创建并推送远程
            var hasPath = await GitPathCheck();
            Debug.Log($"hasPath:{hasPath}");
            OutputLabel.text += $"hasPath:{hasPath}\n";
            if (hasPath)
            {
                // 有git仓库
            }
            else
            {
                // 没有git仓库,需要创建
            }

            // 2. 获取远程git的地址
            var remotePath = await GetGitRemotePath();
            Debug.Log($"remotePath:{remotePath}");
            OutputLabel.text += $"remotePath:{remotePath}\n";
            // 3. 获取远程仓库所有tag标签,并过滤,合法格式:refs/tags/x.x.x (x.x.x是版本)
            var tags = await GetGitTags(remotePath);
            for (int i = 0; i < tags.Length; i++)
            {
                OutputLabel.text += $"-->tag:{tags[i]}\n";
            }
        }

        /// <summary>
        /// 3. 获取远程仓库所有tag标签,并过滤,合法格式:refs/tags/x.x.x (x.x.x是版本)
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetGitTags(string remotePath)
        {
            var condition = new TaskCondition();
            List<string> tags = new List<string>();

            getProcessProxy.Input($"git ls-remote {remotePath}", (msgs) =>
            {
                condition.Value = true;

                var content = "";

                while (msgs.Count > 0)
                {
                    var line = msgs.Dequeue();
                    tags.Add(line);
                }
            }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            return tags.ToArray();
        }

        /// <summary>
        /// 2. 获取远程git的地址
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetGitRemotePath()
        {
            var condition = new TaskCondition();
            var remotePath = "";

            getProcessProxy.Input("git remote -v", (msgs) =>
            {
//                complete = true;
                condition.Value = true;
                // 格式:"origin	https://github.com/Chino66/UPM-Tool-Develop.git (fetch)"
                // 格式:"origin	git@github.com:Chino66/UPM-Tool-Develop.git  (fetch)"
                var ret = msgs.Dequeue();
                string pattern = "(https://github.com.*.git)|(git@github.com:.*.git)";
                Match match = Regex.Match(ret, pattern);
                if (match.Success)
                {
                    remotePath = match.Value;
                }
                else
                {
                    remotePath = "";
                }
            }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            return remotePath;
        }

        /// <summary>
        /// 1. 检测当前项目是否有git仓库(远程仓库),没有则需要自行创建并推送远程
        /// </summary>
        private async Task<bool> GitPathCheck()
        {
            bool hasPath = false;
            var condition = new TaskCondition(false);
            getProcessProxy.Input("git rev-parse --show-toplevel", (msgs) =>
            {
                condition.Value = true;
                var command = msgs.Dequeue();
                var content = "";
                while (msgs.Count > 0)
                {
                    var line = msgs.Dequeue();
                    if (!line.Contains(ProcessProxy.CommandReturnFlag))
                    {
                        content += line;
                    }
                }

                if (content.Equals(NotGitRepositoryMsg))
                {
                    // "这个项目没有git仓库,请先给这个项目创建git仓库,再推送到远程仓库,然后在使用次工具";
                    hasPath = false;
                }
                else
                {
                    // $"仓库的git地址为:{content}";
                    hasPath = true;
                }
            });

            await TimeUtil.WaitUntilConditionSet(condition);

            return hasPath;
        }

        private void InitUI()
        {
            Label label = new Label();
            label.text = "";
            rootVisualElement.Add(label);

            Button click = new Button();
            click.clicked += () =>
            {
                getProcessProxy.Input("git rev-parse --show-toplevel", (msgs) =>
                {
                    var command = msgs.Dequeue();
                    var content = "";
                    while (msgs.Count > 0)
                    {
                        var line = msgs.Dequeue();
                        if (!line.Contains(ProcessProxy.CommandReturnFlag))
                        {
                            content += line;
                        }
                    }

                    if (content.Equals(NotGitRepositoryMsg))
                    {
                        var text = "这个项目没有git仓库,请先给这个项目创建git仓库,再推送到远程仓库,然后在使用次工具";
                        Debug.Log(text);
                    }
                    else
                    {
                        var text = $"仓库的git地址为:{content}";
                        Debug.Log(text);
                    }
                });
            };

            rootVisualElement.Add(click);
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
            if (_proxy != null)
            {
                _proxy.Close();
                _proxy = null;
                Debug.Log("Process Proxy Close");
            }
        }
    }
}