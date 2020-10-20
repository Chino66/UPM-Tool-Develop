using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UPMToolDevelop;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UPMTool
{
    /// <summary>
    /// UPMTool拓展的UI界面
    /// </summary>
    public class UPMToolExtensionUI : VisualElement
    {
        public static UPMToolExtensionUI CreateUI()
        {
            return new UPMToolExtensionUI();
        }

        private VisualElement _packageVersionRoot;

        private List<string> _tagsList;

        private Button _getGitTagsButton;
        private PopupField<string> _versionTagsPopupField;
        private Button _changeVersionButton;
        private VisualTreeAsset _dependenciesItemVisualTreeAsset;

        private Queue<VisualElement> _dependenciesItemQueue;

        private UPMToolExtensionUI()
        {
            InitPackageVersion();

            InitDependenciesUt();
        }

        #region 插件版本

        private void InitPackageVersion()
        {
            _packageVersionRoot = new VisualElement {name = "package_version_root"};
            Add(_packageVersionRoot);

            // 插件版本UI用代码生成,不用uxml的原因是PopupField组件不能用uxml
            _getGitTagsButton = new Button {name = "get_git_tags", text = "获取版本信息"};
            _packageVersionRoot.Add(_getGitTagsButton);

            _tagsList = new List<string> {"-select version-"};
            _versionTagsPopupField = new PopupField<string>("Version:", _tagsList, 0) {value = "-select version-"};
            _versionTagsPopupField.SetEnabled(false);
            _packageVersionRoot.Add(_versionTagsPopupField);

            _changeVersionButton = new Button {name = "change_version", text = "切换版本"};
            _changeVersionButton.SetEnabled(false);
            _packageVersionRoot.Add(_changeVersionButton);
        }

        public void ResetDrawPackageVersionUI()
        {
            _tagsList.Clear();
            _tagsList.Add("-select version-");
            _versionTagsPopupField.value = _tagsList[0];
            _versionTagsPopupField.SetEnabled(false);
            _changeVersionButton.SetEnabled(false);
        }

        public void InitPackageVersionAction(UPMToolExtension upmToolExtension)
        {
            _getGitTagsButton.clicked += () => { upmToolExtension.GetGitTags(_tagsList, ApplyChoices); };

            _versionTagsPopupField.RegisterValueChangedCallback<string>(upmToolExtension.SelectVersion);

            _changeVersionButton.clicked += upmToolExtension.ChangeVersion;
        }


        private void ApplyChoices(List<string> choices)
        {
            if (choices.Count > 0)
            {
                _versionTagsPopupField.SetEnabled(true);
                _versionTagsPopupField.value = choices[0];
                _changeVersionButton.SetEnabled(true);
            }
        }

        #endregion


        #region Ut依赖

        private void InitDependenciesUt()
        {
            var uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/dependenciesUt_root_uxml.uxml");
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var root = asset.CloneTree();
            Add(root);

            // 依赖项UI模板
            uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/dependenciesUt_item_uxml.uxml");
            _dependenciesItemVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            _dependenciesItemQueue = new Queue<VisualElement>();
        }

        public void DrawDependenciesUt(PackageJsonInfo packageJsonInfo, UPMToolExtension upmToolExtension)
        {
            var dependRoot = this.Q<VisualElement>("dependencies_item_root");

            // 归还所有节点
            var childCount = dependRoot.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var item = dependRoot.ElementAt(0);
                ReturnDependenciesItem(item);
                dependRoot.RemoveAt(0);
            }

            if (packageJsonInfo.dependenciesUt.Count <= 0)
            {
                GetNoneDependenciesTip().style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                return;
            }
            else
            {
                GetNoneDependenciesTip().style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            // 绘制节点
            foreach (var dependency in packageJsonInfo.dependenciesUt)
            {
                var item = GetDependenciesItem();
                dependRoot.Add(item);
                item.Q<Label>("packageName_lab").text = dependency.packageName;
                item.Q<Label>("version_lab").text = dependency.version;
                var button = item.Q<Button>("import_btn");
                button.clickable = null;
                if (upmToolExtension.InstalledPackageInfos.ContainsKey(dependency.packageName))
                {
                    button.text = "imported";
                    button.SetEnabled(false);
                }
                else
                {
                    button.text = "import";
                    button.SetEnabled(true);
                    button.clicked += () =>
                    {
                        Debug.Log($"click {dependency.packageName}");
                        upmToolExtension.InstallDependenciesPackage($"{dependency.packageName}@{dependency.version}");
                    };
                }
            }
        }

        private VisualElement GetNoneDependenciesTip()
        {
            return this.Q<VisualElement>("dependencies_none_tip");
        }

        private VisualElement GetDependenciesItem()
        {
            if (_dependenciesItemQueue.Count > 0)
            {
                return _dependenciesItemQueue.Dequeue();
            }

            return _dependenciesItemVisualTreeAsset != null ? _dependenciesItemVisualTreeAsset.CloneTree() : null;
        }

        private void ReturnDependenciesItem(VisualElement item)
        {
            item.Unbind();
            _dependenciesItemQueue.Enqueue(item);
        }

        #endregion

        /// <summary>
        /// 如果enable为false,则UI置灰
        /// </summary>
        /// <param name="enable"></param>
        public void SetUIEnable(bool enable)
        {
            _packageVersionRoot.SetEnabled(enable);
        }

        /// <summary>
        /// 设置这个UI界面的显隐
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetUIVisible(bool isVisible)
        {
            this.style.display = isVisible == false
                ? new StyleEnum<DisplayStyle>(DisplayStyle.None)
                : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }
    }
}