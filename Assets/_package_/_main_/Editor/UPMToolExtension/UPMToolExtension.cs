﻿using System;
using System.Collections.Generic;
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
    /// UPMTool在PackageManager上的拓展
    /// 拓展插件的更新功能
    /// 显示"dependenciesUt"依赖信息,并支持加载
    /// </summary>
    [InitializeOnLoad]
    public class UPMToolExtension : IPackageManagerExtension
    {
        static UPMToolExtension()
        {
            PackageManagerExtensions.RegisterExtension(new UPMToolExtension());
        }

        private PackageInfo _selectPackageInfo;

        private string _selectVersion;

        private string _gitUrl;

        private UPMToolExtensionUI _ui;

        public VisualElement CreateExtensionUI()
        {
            if (_ui == null)
            {
                _ui = UPMToolExtensionUI.CreateUI();
                _ui.Init(this);
            }

            return _ui;
        }

        /// <summary>
        /// 安装选择的版本
        /// </summary>
        public void ChangeVersion()
        {
            PackageUtils.AddOrUpdatePackage(GetPackageIdByNewGitUrl(), AddOrUpdatePackageCallBack);
            _ui.SetEnabled(false);
        }

        private void AddOrUpdatePackageCallBack()
        {
            _ui.SetEnabled(true);
        }

        /// <summary>
        /// 选择插件版本
        /// </summary>
        /// <param name="evt"></param>
        public void SelectVersion(ChangeEvent<string> evt)
        {
            _selectVersion = evt.newValue;
        }

        /// <summary>
        /// 获取git仓库的tags
        /// </summary>
        public void GetGitTags(List<string> choices, Action<List<string>> action)
        {
            GitUtils.GetTags(_gitUrl, tags =>
            {
                choices.Clear();

                choices.AddRange(tags);

                choices.Sort((a, b) =>
                {
                    var sa = a.Split('.');
                    var sb = b.Split('.');
                    if (sa.Length != 3 || sb.Length != 3)
                    {
                        return 0;
                    }

                    for (var i = 0; i < 3; i++)
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
                    action(choices);
                }
            });
        }

        private string GetPackageIdByNewGitUrl()
        {
            if (_selectPackageInfo == null)
            {
                return "";
            }

            var url = GitUtils.GitPathConvertUnityPackagePath(_gitUrl);
//            url = "git@gitee.com/chinochan66/UPM-Tool-Test.git";
            return $"{_selectPackageInfo.name}@{url}#{_selectVersion}";
        }

        /// <summary>
        /// 在PackageManager视窗
        /// 当选择插件包时,获取这个插件包的信息
        /// </summary>
        /// <param name="packageInfo"></param>
        public void OnPackageSelectionChange(PackageInfo packageInfo)
        {
            if (_ui == null)
            {
                return;
            }

            _selectPackageInfo = packageInfo;

            var packageId = _selectPackageInfo.packageId;

            // 判断这个包是否是git途径获取的
            _gitUrl = GetGitUrl(packageId);

            // 只有git途径获取的包,才能使用UPM Tool拓展功能
            if (string.IsNullOrEmpty(_gitUrl))
            {
                _ui.SetUIVisible(false);
                return;
            }

            _ui.SetUIVisible(true);

            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>($"{packageInfo.assetPath}/package.json");

            var packageJsonInfo = PackageJson.Parse(textAsset.text);

            _ui.DrawDependenciesUt(packageJsonInfo);
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
        private static string GetGitUrl(string packageId)
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

        /// <summary>
        /// 当添加或更新插件包时
        /// </summary>
        /// <param name="packageInfo"></param>
        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
        }

        /// <summary>
        /// 当移除插件包时
        /// </summary>
        /// <param name="packageInfo"></param>
        public void OnPackageRemoved(PackageInfo packageInfo)
        {
        }
    }
}