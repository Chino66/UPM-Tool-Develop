using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UPMToolDevelop
{
    /// <summary>
    /// UPM Tool在PackageManager上的拓展
    /// </summary>
    [InitializeOnLoad]
    public class UPMToolExtension : IPackageManagerExtension
    {
        static UPMToolExtension()
        {
            PackageManagerExtensions.RegisterExtension(new UPMToolExtension());
        }

        private PackageInfo selectPackageInfo;

        private string selectVersion;

        private string gitUrl;

        private UPMToolExtensionUI ui;

        public VisualElement CreateExtensionUI()
        {
            if (ui == null)
            {
                ui = UPMToolExtensionUI.CreateUI();
                InitUI();
            }

            return ui;
        }

        private void InitUI()
        {
            if (ui == null)
            {
                return;
            }

            var choices = ui.TagsList;

            ui.GetGitTagsButton.clicked += () =>
            {
                GitUtils.GetTags(gitUrl, tags =>
                {
                    choices.Clear();

                    for (int i = 0; i < tags.Length; i++)
                    {
                        choices.Add(tags[i]);
                    }

                    choices.Sort((a, b) =>
                    {
                        var sa = a.Split('.');
                        var sb = b.Split('.');
                        if (sa.Length != 3 || sb.Length != 3)
                        {
                            return 0;
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            try
                            {
                                var ia = int.Parse(sa[i]);
                                var ib = int.Parse(sb[i]);
                                if (ia == ib)
                                {
                                    continue;
                                }

                                if (ia > ib)
                                {
                                    return -1;
                                }

                                if (ia < ib)
                                {
                                    return 1;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return 0;
                            }
                        }

                        return 0;
                    });

                    if (choices.Count > 0)
                    {
                        ui.versionTagsPopupField.SetEnabled(true);
                        ui.versionTagsPopupField.value = choices[0];
                        ui.ChangeVersionButton.SetEnabled(true);
                    }
                });
            };

            ui.versionTagsPopupField.RegisterValueChangedCallback<string>((evt) => { selectVersion = evt.newValue; });

            ui.ChangeVersionButton.clicked += () => { PackageUtils.AddOrUpdatePackage(GetPackageIdByNewGitUrl()); };
        }

        private string GetPackageIdByNewGitUrl()
        {
            if (selectPackageInfo == null)
            {
                return "";
            }

            var url = GitUtils.GitPathConvertUnityPackagePath(gitUrl);
//            url = "git@gitee.com/chinochan66/UPM-Tool-Test.git";
            return $"{selectPackageInfo.name}@{url}#{selectVersion}";
        }

        /// <summary>
        /// 在PackageManager视窗
        /// 当选择插件包时,获取这个插件包的信息
        /// </summary>
        /// <param name="packageInfo"></param>
        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (ui == null)
            {
                return;
            }

            selectPackageInfo = packageInfo;

            var packageId = selectPackageInfo.packageId;

            // 判断这个包是否是git途径获取的
            gitUrl = GetGitUrl(packageId);

            // 只有git途径获取的包,才能使用UPM Tool拓展功能
            if (!string.IsNullOrEmpty(gitUrl))
            {
                ui.SetUIVisible(true);
            }
            else
            {
                ui.SetUIVisible(false);
            }
        }

        /// <summary>
        /// 获取git路径,可能是github或gitee
        /// https格式正则表达式:https://(.*).git
        /// ssh格式正则表达式:git@(.*).git
        ///
        /// 例:
        /// 本地插件的packageId:com.chino.upmtool@file:F:\Unity3D\UPM-Tool-Develop\Assets\_package_
        /// git插件的packageId:com.chino.testpackage@ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm
        /// </summary>
        /// <param name="packageId">插件包的来源路径</param>
        /// <returns></returns>
        private string GetGitUrl(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                Debug.LogError("packageId is null");
                return "";
            }
            
            Debug.Log(packageId);

            var pattern = "(https://(.*).git)|(git@(.*).git)";

            Match match = Regex.Match(packageId, pattern);

            if (match.Success == false)
            {
                Debug.LogWarning("packageId is not git url, packageId is :" + packageId);
                return "";
            }

            var url = match.Value;

            url = GitUtils.UnityPackagePathConvertGitPath(url);

            Debug.LogWarning(url);
            return url;
        }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
        }

        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }
    }
}