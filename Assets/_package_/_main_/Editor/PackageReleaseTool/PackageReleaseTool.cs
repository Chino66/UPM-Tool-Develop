using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMToolDevelop;

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

        /// <summary>
        /// 项目的git信息
        /// </summary>
        private ProjectGitInfo _gitInfo;

        /// <summary>
        /// 当前项目的版本号(package.json中填的版本号)
        /// </summary>
        private Version _version;

        /// <summary>
        /// 输出消息(用于调试)
        /// </summary>
        private Label LabelOutput => rootVisualElement.Q<Label>("lab_output");

        private void OutputLine(string line, bool add = true, bool debug = false)
        {
            Debug.Log($"OutputLine:{line}");

            if (debug)
            {
                return;
            }

            if (LabelOutput == null)
            {
                Debug.LogError("LabelOutput is null !!");
                return;
            }

            if (add)
            {
                LabelOutput.text += line;
            }
            else
            {
                LabelOutput.text = line;
            }
        }

        /// <summary>
        /// 提示消息
        /// </summary>
        private Label LabelTip => rootVisualElement.Q<Label>("lab_tip");

        private void ShowTip(string content)
        {
            ShowTip(content, Color.black);
        }

        private void ShowTip(string content, Color color)
        {
            if (LabelTip == null)
            {
                Debug.LogError("LabelTip is null !!");
                return;
            }

            LabelTip.style.color = color;
            LabelTip.text = content;
        }

        private void OnEnable()
        {
            var label = new Label {name = "lab_output"};
            rootVisualElement.Add(label);

            label = new Label {name = "lab_tip"};
            rootVisualElement.Add(label);

            // 初始化process代理类(必要)
            if (_proxy == null)
            {
                _proxy = new ProcessProxy();
//                _proxy.SetDebugMode(true);
                _proxy.Start();
            }

            // 预处理
            PreProcess();
        }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <returns></returns>
        private async Task PreProcess()
        {
            _gitInfo = new ProjectGitInfo();

            // 这个0.1秒的等待是为了_proxy的初始化完成
            await Task.Delay(TimeSpan.FromSeconds(0.1));

            // 1. 检测当前项目是否有git仓库(远程仓库),没有则需要自行创建并推送远程
            var path = await GitPathCheck();

            if (path == "")
            {
                Debug.LogError("这个项目没有被git控制");
                return;
            }

            // 2. 获取远程git的地址
            var remotePath = await GetGitRemotePath();

            // 3. 获取远程仓库所有tag标签,并过滤,合法格式:refs/tags/x.x.x (x.x.x是版本)
            var tags = await GetGitTags(remotePath);

            // 4. 实例化UI
            InitUI();

            // 5. 刷新信息
            Refresh();

            // 6. 检查版本号是否唯一,且有没有提交
            CheckVersion();
        }

        /// <summary>
        /// 5. 刷新信息
        /// </summary>
        private void Refresh()
        {
            // 1. 刷新当前package.json版本号
            RefreshVersion();

            // 2. 刷新版本tags
            RefreshVersionTags();
        }

        /// <summary>
        /// 2. 刷新当前package.json版本号
        /// </summary>
        private void RefreshVersion()
        {
            var packageJsonVersion = PackageChecker.GetPackageJsonInfo().version;

            _version = new Version(packageJsonVersion);

            TextFieldPackageVersion.value = _version.ToString();
        }

        /// <summary>
        /// 1. 刷新版本tags
        /// </summary>
        private void RefreshVersionTags()
        {
            LabelVersionTags.text = "";

            Version newest = null;

            if (_gitInfo.Versions != null && _gitInfo.Versions.Count > 0)
            {
                newest = _gitInfo.Versions[0];
            }

            foreach (var tag in _gitInfo.Versions)
            {
                if (tag.Equals(newest))
                {
                    LabelVersionTags.text += $"{tag.ToString()} - 最新版本\n";
                }
                else
                {
                    LabelVersionTags.text += $"{tag.ToString()}\n";
                }
            }
        }

        /// <summary>
        /// 6. 检查版本号是否唯一,并更新提示
        /// </summary>
        private void CheckVersion()
        {
            BoxReleasePanel.SetEnabled(false);

            // 检查版本号是否唯一
            var ret = CheckVersionExist(_version, _gitInfo.Versions);
            var content = "";
            var color = Color.blue;
            if (ret)
            {
                content = "当前版本号已经存在,请确定唯一的版本号";
                color = Color.red;
                ShowVersionCheckResult(content, color);
            }
            else
            {
                // 检查package.json是否修改,且没有提交
                CheckPackageJsonModify();
            }
        }


        /// <summary>
        /// package.json是否有修改,有修改且没有提交和推送,则不能进行发布
        /// </summary>
        private async Task<bool> CheckPackageJsonModify()
        {
            BoxReleasePanel.SetEnabled(false);

            var modify = false;
            var condition = new TaskCondition(false);

            getProcessProxy.Input("git diff Assets/_package_/package.json", (msgs) =>
            {
                condition.Value = true;

                var content = msgs.Dequeue();

                // 没有修改可能返回"\n"换行符
                if (content.Equals("\n"))
                {
                    modify = false;
                }
                // 有修改则返回:"diff --git a/Assets/_package_/package.json b/Assets/_package_/package.json"以及修改详情
                else
                {
                    modify = true;
                }
            }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            if (modify)
            {
//                Debug.Log("package.json有修改但没有提交和推送,完成后才能发布");
                ShowVersionCheckResult("package.json有修改且没有提交和推送,完成后才能发布", Color.red);
            }
            else
            {
//                Debug.Log("package.json已经提交了");
                ShowVersionCheckResult("package.json没有修改,可以发布了", Color.blue);
                BoxReleasePanel.SetEnabled(true);
            }

            return !modify;
        }

        /// <summary>
        /// 3. 获取远程仓库所有tag标签
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetGitTags(string remotePath)
        {
            var condition = new TaskCondition();
            List<string> tagList = new List<string>();

            getProcessProxy.Input($"git ls-remote {remotePath}", (msgs) =>
            {
                condition.Value = true;

                var content = "";

                while (msgs.Count > 0)
                {
                    var line = msgs.Dequeue();
                    tagList.Add(line);
                }
            }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            var tags = tagList.ToArray();

            _gitInfo.SetTags(tags);

            OutputLine($"3.版本标签:\n", true, true);

            for (int i = 0; i < _gitInfo.Versions.Count; i++)
            {
                OutputLine($"->tag:{_gitInfo.Versions[i]}\n", true, true);
            }

            return tagList.ToArray();
        }

        private const string Pattern = "(https://gitee.com.*.git)|(git@gitee.com:.*.git)|(https://github.com.*.git)|(git@github.com:.*.git)";
        
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
                
                Match match = Regex.Match(ret, Pattern);
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

            _gitInfo.RemotePath = remotePath;
            OutputLine($"2.远程仓库地址:{remotePath}\n");

            return remotePath;
        }

        /// <summary>
        /// 1. 检测当前项目是否有git仓库(远程仓库),没有则需要自行创建并推送远程
        /// </summary>
        private async Task<string> GitPathCheck()
        {
            var path = "";
            var condition = new TaskCondition(false);

            getProcessProxy.Input("git rev-parse --show-toplevel", (msgs) =>
            {
                condition.Value = true;
                var content = "";
                while (msgs.Count > 0)
                {
                    var line = msgs.Dequeue();
                    if (!line.Contains(ProcessProxy.CommandReturnFlag))
                    {
                        content += line;
                    }
                }

                path = content.Equals(NotGitRepositoryMsg) ? "" : content;
            }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            // 是否有项目路径
            if (string.IsNullOrEmpty(path) == false)
            {
                // 有git仓库
                _gitInfo.Path = path;
                OutputLine($"1.项目路径:{path}\n");
            }
            else
            {
                // 没有git仓库,需要创建
                OutputLine("1.这个项目没有git仓库,请先给这个项目创建git仓库,再推送到远程仓库,然后在使用次工具\n");
            }

            return path;
        }

        private string GetContent(Queue<string> msgs)
        {
            var content = "";
            while (msgs.Count > 0)
            {
                var line = msgs.Dequeue();
                if (!line.Contains(ProcessProxy.CommandReturnFlag))
                {
                    content += line + "\n";
                }
            }

            return content;
        }

        #region 发布流程

        /// <summary>
        /// 发布流程
        /// </summary>
        private async Task ReleaseFlow()
        {
            // 发布过程中灰置面板
            rootVisualElement.SetEnabled(false);

            // 1. 使用package.json中的版本为发布版本

            // 2. 检测版本号是否唯一,发布版本不在版本tag中
            var ret = CheckVersionExist(_version, _gitInfo.Versions);
            if (ret)
            {
                Debug.LogError("当前版本号已经存在,请确定唯一版本号");
                return;
            }

            // 3. git工作流发布版本
            // 执行 git subtree split --prefix=Assets/_package_ --branch upm
            // 执行 git tag x.x.x upm
            // 执行 git push origin upm --tags
            await GitFlow();

            // 4. 发布完成后
            await PostRelease();

            // 发布结束后取消灰置
            rootVisualElement.SetEnabled(true);
        }

        /// <summary>
        /// 4. 发布完成后
        /// </summary>
        private async Task PostRelease()
        {
            // 1. 重新获取package.json的版本号
            RefreshVersion();

            // 2. 重新获取tags信息
            await GetGitTags(_gitInfo.RemotePath);

            // 3. 刷新tags
            RefreshVersionTags();

            // 4. 刷新版本
            CheckVersion();
        }

        /// <summary>
        /// 3. git工作流发布版本
        /// </summary>
        /// <returns></returns>
        private async Task GitFlow()
        {
            await GitSubtreeSplit();
            await GitCreateTagByVersion(_version.ToString());
            await GitPushOrigin();
        }

        /// <summary>
        /// 1. 执行 git subtree split --prefix=Assets/_package_ --branch upm
        /// 分割目录,只对Assets/_package_下的内容加入分支
        /// </summary>
        /// <returns></returns>
        private async Task GitSubtreeSplit()
        {
            var condition = new TaskCondition(false);

            ShowTip("执行:\"git subtree split --prefix=Assets/_package_ --branch upm\"...");

            getProcessProxy.Input("git subtree split --prefix=Assets/_package_ --branch upm",
                (msgs) =>
                {
                    condition.Value = true;

                    // 执行成功则返回:一串哈希码
                    Debug.Log(GetContent(msgs));
                }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            ShowTip("执行:\"git subtree split ... \"完成");
        }

        /// <summary>
        /// 2. 执行 git tag x.x.x upm
        /// 创建tag
        /// </summary>
        /// <returns></returns>
        private async Task GitCreateTagByVersion(string version)
        {
            var condition = new TaskCondition(false);

            ShowTip($"执行:\"git tag {version} upm\"...");

            getProcessProxy.Input($"git tag {version} upm",
                (msgs) =>
                {
                    condition.Value = true;

                    Debug.Log(GetContent(msgs));
                }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            ShowTip($"执行:\"git tag {version} upm\"完成");
        }

        /// <summary>
        /// 3. 执行 git push origin upm --tags
        /// 推送标签
        /// </summary>
        /// <returns></returns>
        private async Task GitPushOrigin()
        {
            var condition = new TaskCondition();

            ShowTip($"执行:\"git push origin upm --tags\"...");

            getProcessProxy.Input("git push origin upm --tags",
                (msgs) =>
                {
                    condition.Value = true;

                    Debug.Log(GetContent(msgs));
                }, false);

            await TimeUtil.WaitUntilConditionSet(condition);

            ShowTip($"执行:\"git push origin upm --tags\"完成");
        }

        /// <summary>
        /// 版本号是否存在在list中
        /// </summary>
        /// <returns></returns>
        private bool CheckVersionExist(Version version, List<Version> versions)
        {
            if (versions == null || versions.Count == 0)
            {
                return false;
            }

            foreach (var v in versions)
            {
                if (version == v)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region UI

        private Box BoxVersionTags => rootVisualElement.Q<Box>("box_version_tags");

        private ScrollView ScrollViewVersionTags => BoxVersionTags.Q<ScrollView>("sv_version_tags");
        private Label LabelVersionTags => ScrollViewVersionTags.Q<Label>("lab_version_tags_content");

        private Box BoxPackageJsonVersion => rootVisualElement.Q<Box>("box_package_json_version");
        private Label LabelVersionCheckResult => BoxPackageJsonVersion.Q<Label>("lab_version_check_result");

        private void ShowVersionCheckResult(string content, Color color)
        {
            if (LabelVersionCheckResult == null)
            {
                Debug.LogError("LabelVersionCheckResult is null");
                return;
            }

            LabelVersionCheckResult.style.color = color;
            LabelVersionCheckResult.text = content;
        }

        private TextField TextFieldPackageVersion => BoxPackageJsonVersion.Q<TextField>("tf_package_version");

        private Box BoxReleasePanel => rootVisualElement.Q<Box>("box_release_panel");

        /// <summary>
        /// 实例化UI
        /// </summary>
        private void InitUI()
        {
            // 1. 绘制所有tag版本(x.x.x)
            DrawVersionTags();

            // 2. 绘制package.json中的版本
            DrawPackageJsonVersion();

            // 3. 绘制发布面板
            DrawReleasePanel();
        }

        /// <summary>
        /// 3. 绘制发布面板
        /// </summary>
        private void DrawReleasePanel()
        {
            Label label = new Label();
            label.text = "发布:";
            rootVisualElement.Add(label);

            Box box = new Box {name = "box_release_panel"};
            box.style.flexDirection = FlexDirection.Row;
            {
                Button button = new Button();
                button.style.width = 100;
                button.text = "发布";
                button.clicked += () => { ReleaseFlow(); };
                box.Add(button);
            }
            rootVisualElement.Add(box);

            label = new Label {name = "lab_release_log"};
            rootVisualElement.Add(label);
        }

        /// <summary>
        /// 2. 绘制package.json中的版本
        /// </summary>
        private void DrawPackageJsonVersion()
        {
            Label label = new Label();
            label.text = "当前版本号:";
            rootVisualElement.Add(label);

            Box box = new Box {name = "box_package_json_version"};
            box.style.flexDirection = FlexDirection.Row;
            {
                TextField textField = new TextField {name = "tf_package_version"};
                textField.style.width = 100;
                box.Add(textField);

                Button button = new Button();
                button.style.width = 60;
                button.text = "设置";
                button.clicked += SetVersion;
                box.Add(button);

                // 添加一个刷新按钮 刷新版本号
                button = new Button();
                button.style.width = 60;
                button.style.backgroundImage = null;
                button.text = "刷新";
                button.clicked += CheckVersion;
                box.Add(button);

                label = new Label {name = "lab_version_check_result", text = ""};
                box.Add(label);
            }
            rootVisualElement.Add(box);
        }

        private void SetVersion()
        {
            var version = TextFieldPackageVersion.value;
            if (version.Equals(_version.ToString()))
            {
                Debug.LogError("无需修改");
                return;
            }

            SetPackageJsonVersion(version);
            AssetDatabase.Refresh();
            CheckVersion();
        }

        /// <summary>
        /// 设置版本号
        /// </summary>
        private void SetPackageJsonVersion(string version)
        {
            string pattern = "(^[0-9]+\\.[0-9]+\\.[0-9]+$)";
            var rst = RegexUtils.RegexMatch(version, pattern);
            if (rst == false)
            {
                ShowVersionCheckResult($"版本检测失败:{version}", Color.red);
                return;
            }

            // 保存版本号
            PackageJsonEditor.SavePackageJsonVersionChange(version);

            _version = new Version(version);
        }

        /// <summary>
        /// 1. 绘制所有版本tag(x.x.x)
        /// </summary>
        private void DrawVersionTags()
        {
            var label = new Label {name = "lab_version_tags_title", text = "版本tag:"};
            rootVisualElement.Add(label);

            var box = new Box {name = "box_version_tags"};
            {
                var scrollView = new ScrollView {name = "sv_version_tags"};
                scrollView.style.height = 200;
                {
                    label = new Label {name = "lab_version_tags_content"};
                    scrollView.Add(label);
                }
                box.Add(scrollView);
            }
            rootVisualElement.Add(box);
        }

        #endregion


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

    public class ProjectGitInfo
    {
        public string[] Tags;
        private string[] _versionTags;
        public List<Version> Versions;

        /// <summary>
        /// 保存所有tags,并将x.x.x格式的tag保存到_versionTags
        /// </summary>
        /// <param name="tags"></param>
        public void SetTags(string[] tags)
        {
            Tags = tags;
            tags = GetVersionTags(tags);
            SetVersionTags(tags);
        }

        private void SetVersionTags(string[] tags)
        {
            Versions = new List<Version>();
            _versionTags = tags;
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }
                Versions.Add(new Version(tag));
                Versions.Sort((a, b) => a < b ? 1 : -1);
            }
        }

        /// <summary>
        /// 过滤tags,获取合法格式的tag:refs/tags/x.x.x (x.x.x是版本)
        /// </summary>
        /// <returns></returns>
        private string[] GetVersionTags(string[] tags)
        {
            string pattern = "[0-9]+\\.[0-9]+\\.[0-9]+";
            List<string> rst = new List<string>();
            for (int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }

                Match match = Regex.Match(tag, pattern);

                if (match.Success)
                {
                    rst.Add(match.Value);
                }
            }

            return rst.ToArray();
        }

        public string Path;
        public string RemotePath;
    }
}