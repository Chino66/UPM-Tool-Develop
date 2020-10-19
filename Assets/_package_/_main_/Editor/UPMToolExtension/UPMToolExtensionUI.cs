using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UPMToolDevelop;

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

        private readonly VisualElement _root;

        private readonly List<string> _tagsList;

        private readonly Button _getGitTagsButton;
        private readonly PopupField<string> _versionTagsPopupField;
        private readonly Button _changeVersionButton;
        private VisualTreeAsset _dependenciesItemVisualTreeAsset;

        private Queue<VisualElement> _dependenciesItemQueue;

        private UPMToolExtensionUI()
        {
            _root = new VisualElement {name = "ui_root"};
            Add(_root);

            // 插件版本UI用代码生成,不用uxml的原因是PopupField组件不能用uxml
            _getGitTagsButton = new Button();
            _getGitTagsButton.name = "get_git_tags";
            _getGitTagsButton.text = "获取版本信息";
            _root.Add(_getGitTagsButton);

            _tagsList = new List<string> {"-select version-"};
            _versionTagsPopupField = new PopupField<string>("Version:", _tagsList, 0) {value = "-select version-"};
            _versionTagsPopupField.SetEnabled(false);
            _root.Add(_versionTagsPopupField);

            _changeVersionButton = new Button {name = "change_version", text = "切换版本"};
            _changeVersionButton.SetEnabled(false);
            _root.Add(_changeVersionButton);

            // Ut依赖用uxml
            InitDependenciesUt();
        }

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

        public void DrawDependenciesUt(PackageJsonInfo packageJsonInfo)
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
                button.text = "import";
                button.clicked += () => { PackageUtils.List(); };
            }
        }

//        private 

        private VisualElement GetNoneDependenciesTip()
        {
            return this.Q<VisualElement>("dependencies_none_tip");
        }

        /// <summary>
        /// 如果enable为false,则UI置灰
        /// </summary>
        /// <param name="enable"></param>
        public void SetUIEnable(bool enable)
        {
            _root.SetEnabled(enable);
        }

        /// <summary>
        /// 设置这个UI界面的显隐
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetUIVisible(bool isVisible)
        {
            _root.style.display = isVisible == false
                ? new StyleEnum<DisplayStyle>(DisplayStyle.None)
                : new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        public void Init(UPMToolExtension upmToolExtension)
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
    }
}