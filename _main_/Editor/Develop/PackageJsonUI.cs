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
        }

        /// <summary>
        /// 给UIElement添加交互(共用部分)
        /// </summary>
        public static void InitUIElement(VisualElement root, PackageJsonInfo packageJsonInfo)
        {
            if (root == null)
            {
                return;
            }

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