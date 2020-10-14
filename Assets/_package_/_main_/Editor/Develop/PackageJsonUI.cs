using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UPMToolDevelop;

namespace UPMTool
{
    public class PackageJsonUI : VisualElement
    {
        public static PackageJsonUI CreateUI()
        {
            return new PackageJsonUI();
        }

        private readonly VisualTreeAsset _dependenciesItemVisualTreeAsset;

        private readonly Queue<VisualElement> _dependenciesItemQueue;

        private readonly VisualElement _dependencyItemsRoot;

        private readonly VisualElement _noneDependenciesTip;
        
//        private List<>

        private PackageJsonUI()
        {
            var uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/package_json_uxml.uxml");
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            var root = asset.CloneTree();
            var ussPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/package_json_uss.uss");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            root.styleSheets.Add(styleSheet);
            Add(root);

            // 依赖项UI模板
            uxmlPath = Path.Combine(PackagePath.MainPath, @"Resources/UIElement/dependencies_item_uxml.uxml");
            _dependenciesItemVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            _dependenciesItemQueue = new Queue<VisualElement>();
            
            _dependencyItemsRoot = this.Q<VisualElement>("dependencies_item_root");
            _noneDependenciesTip = this.Q<VisualElement>("dependencies_none_tip");
        }

        /// <summary>
        /// 初始化共用部分UI
        /// </summary>
        public void InitUIElementCommon(PackageJsonInfo packageJsonInfo)
        {
            var root = this;
            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 插件名 com.xxx.xxx
            var textField = root.Q<TextField>("name_tf");
            textField.value = packageJsonInfo.name;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.name = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // 显示名称
            textField = root.Q<TextField>("displayName_tf");
            textField.value = packageJsonInfo.displayName;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.displayName = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // 版本
            textField = root.Q<TextField>("version_tf");
            textField.value = packageJsonInfo.version;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.version = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // unity版本,例:2019.4
            textField = root.Q<TextField>("unity_tf");
            textField.value = packageJsonInfo.unity;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.unity = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // 类型(内部保留使用)
            textField = root.Q<TextField>("type_tf");
            textField.value = packageJsonInfo.type;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.type = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // 描述
            textField = root.Q<TextField>("description_tf");
            textField.value = packageJsonInfo.description;
            textField.RegisterValueChangedCallback(value =>
            {
                packageJsonInfo.description = value.newValue;
                preview.value = packageJsonInfo.ToJson();
            });

            // 依赖关系(依赖相关UI不在这里添加,在PackageJsonEditor中)

            // 返回消息
            var label = root.Q<Label>("msg_lab");
            label.text = "";
        }

        /// <summary>
        /// 初始化创建界面UI
        /// </summary>
        public void InitUIElementCreate(PackageJsonInfo packageJsonInfo, string path)
        {
            var root = this;

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 创建按钮响应点击
            var button = root.Q<Button>("create_btn");
            button.clicked += () =>
            {
                // 创建插件包的动作
                PackageJsonEditor.CreatePackageAction(packageJsonInfo);
                // 创建或修改package.json
                PackageJsonEditor.SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
                // 刷新,显示插件包框架
                AssetDatabase.Refresh();

                // 创建PackagePath.cs,需要检查插件包路径才能创建
                PackageJsonEditor.AfterCreatePackageAction();
                // 刷新,显示PackagePath.cs
                AssetDatabase.Refresh();
            };

            // 编辑按钮隐藏
            var element = root.Q<VisualElement>("edit_box");
            element.parent.Remove(element);

            // 初始化依赖操作(Create界面不需要依赖操作,所以要隐藏)
            InitDependenciesUIElement(root, packageJsonInfo, path, false);
        }

        /// <summary>
        /// 初始化编辑界面UI
        /// </summary>
        public void InitUIElementEditor(PackageJsonInfo packageJsonInfo, string path)
        {
            var root = this;

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // TODO 编辑按钮-撤销修改响应点击
            var button = root.Q<Button>("revert_btn");
            button.clicked += () => { Debug.Log("revert todo"); };

            // 编辑按钮-应用修改响应点击
            button = root.Q<Button>("apply_btn");
            button.clicked += () =>
            {
                PackageJsonEditor.SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
                AssetDatabase.Refresh();
            };

            // 创建按钮隐藏
            var element = root.Q<VisualElement>("create_box");
            element.parent.Remove(element);

            // 初始化依赖操作
            InitDependenciesUIElement(root, packageJsonInfo, path, true);
        }


        /// <summary>
        /// 插件依赖相关UIElement交互
        /// </summary>
        private void InitDependenciesUIElement(VisualElement root, PackageJsonInfo packageJsonInfo, string path,
            bool isShow)
        {
            if (isShow == false)
            {
                var element = root.Q<VisualElement>("dependencies_box");
                element.parent.Remove(element);

                element = root.Q<VisualElement>("dependencies_lab_box");
                element.parent.Remove(element);
                return;
            }

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 添加依赖按钮响应
            var button = root.Q<Button>("dependencies_add");
            button.clicked += () =>
            {
            };

            // 移除依赖按钮响应
            button = root.Q<Button>("dependencies_remove");
            button.clicked += () =>
            {
            };

            // 绘制依赖项
            DrawDependencyItems(packageJsonInfo);
        }

        /// <summary>
        /// 绘制所有依赖项
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        private void DrawDependencyItems(PackageJsonInfo packageJsonInfo)
        {
            var dependencies = packageJsonInfo.dependencies;
            foreach (var pair in dependencies)
            {
                Debug.Log($"Dependency Item is {pair.Key},{pair.Value}");

                DrawDependencyItem(pair.Key, pair.Value.ToString());
            }
            
            if (_dependencyItemsRoot.childCount <= 0)
            {
                _dependencyItemsRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                _noneDependenciesTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                _dependencyItemsRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _noneDependenciesTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        }

        /// <summary>
        /// 绘制依赖项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void DrawDependencyItem(string key, string value)
        {
            var i = GetDependenciesItem();
            _dependencyItemsRoot.Add(i);

            var textField = i.Q<TextField>("dependencies_name_tf");
            textField.value = key;
            textField = i.Q<TextField>("dependencies_version_tf");
            textField.value = value;
        }

        public VisualElement GetDependenciesItem()
        {
            if (_dependenciesItemQueue.Count > 0)
            {
                return _dependenciesItemQueue.Dequeue();
            }

            return _dependenciesItemVisualTreeAsset != null ? _dependenciesItemVisualTreeAsset.CloneTree() : null;
        }

        public void ReturnDependenciesItem(VisualElement item)
        {
            _dependenciesItemQueue.Enqueue(item);
        }
    }
}