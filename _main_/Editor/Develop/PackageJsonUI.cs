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
            textField.bindingPath = "name";
//            textField.value = packageJsonInfo.name;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.name = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // 显示名称
            textField = root.Q<TextField>("displayName_tf");
            textField.bindingPath = "displayName";
//            textField.value = packageJsonInfo.displayName;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.displayName = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // 版本
            textField = root.Q<TextField>("version_tf");
            textField.bindingPath = "version";
//            textField.value = packageJsonInfo.version;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.version = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // unity版本,例:2019.4
            textField = root.Q<TextField>("unity_tf");
            textField.bindingPath = "unity";
//            textField.value = packageJsonInfo.unity;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.unity = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // 类型(内部保留使用)
            textField = root.Q<TextField>("type_tf");
            textField.bindingPath = "type";
//            textField.value = packageJsonInfo.type;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.type = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // 描述
            textField = root.Q<TextField>("description_tf");
            textField.bindingPath = "description";
//            textField.value = packageJsonInfo.description;
//            textField.RegisterValueChangedCallback(value =>
//            {
//                packageJsonInfo.description = value.newValue;
//                preview.value = packageJsonInfo.ToJson();
//            });

            // 依赖关系(依赖相关UI不在这里添加,在PackageJsonEditor中)

            // 返回消息
            var label = root.Q<Label>("msg_lab");
            label.text = "";

            // PackageJsonInfo和UIElement绑定
            var serializedObject = new SerializedObject(packageJsonInfo);
            root.Bind(serializedObject);
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
                // 添加"UPMTool"依赖
                PackageJsonEditor.AddUPMToolDependency(packageJsonInfo);
                // 创建或修改package.json
                var rst = PackageJsonEditor.SavePackageJsonChange(packageJsonInfo, path, out var msg);
                DrawSavePackageJsonInfoRet(rst, msg);

                // 保存失败直接结束
                if (rst == false)
                {
                    return;
                }

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
            InitDependenciesUIElement(root, packageJsonInfo, "dependencies", false);
            InitDependenciesUIElement(root, packageJsonInfo, "dependenciesUt", false);
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
            button.clicked += () => { Debug.Log("抱歉!撤销功能还没做..."); };

            // 编辑按钮-应用修改响应点击
            button = root.Q<Button>("apply_btn");
            button.clicked += () =>
            {
                var rst = PackageJsonEditor.SavePackageJsonChange(packageJsonInfo, path, out var msg);
                DrawSavePackageJsonInfoRet(rst, msg);

                // 保存失败直接结束
                if (rst == false)
                {
                    return;
                }

                preview.value = packageJsonInfo.ToJson();
                AssetDatabase.Refresh();
            };

            // 创建按钮隐藏
            var element = root.Q<VisualElement>("create_box");
            element.parent.Remove(element);

            // 初始化依赖操作
            InitDependenciesUIElement(root, packageJsonInfo, "dependencies", true);
            InitDependenciesUIElement(root, packageJsonInfo, "dependenciesUt", true);
        }

        private void DrawSavePackageJsonInfoRet(bool rst, string msg)
        {
            // todo 提示显示时间,一段时间后消失
            var label = this.Q<Label>("msg_lab");

            label.text = msg;

            if (rst == false)
            {
                label.RemoveFromClassList("color_green");
                label.AddToClassList("color");
                return;
            }
            else
            {
                label.RemoveFromClassList("color");
                label.AddToClassList("color_green");
            }
        }

        /// <summary>
        /// 插件依赖相关UIElement交互
        /// </summary>
        private void InitDependenciesUIElement(VisualElement root, PackageJsonInfo packageJsonInfo,
            string dependType,
            bool isShow)
        {
            if (isShow == false)
            {
                var element = root.Q<VisualElement>($"{dependType}_box");
                element.parent.Remove(element);

                element = root.Q<VisualElement>($"{dependType}_lab_box");
                element.parent.Remove(element);
                return;
            }

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 添加依赖按钮响应
            var button = root.Q<Button>($"{dependType}_add");
            button.clicked += () =>
            {
                AddDependencyItem(packageJsonInfo, packageJsonInfo.GetDependenciesByType(dependType), dependType);
            };

            // 移除依赖按钮响应
            button = root.Q<Button>($"{dependType}_remove");
            button.clicked += () =>
            {
                RemoveDependencyItem(packageJsonInfo, packageJsonInfo.GetDependenciesByType(dependType), dependType);
            };

            // 绘制依赖项
            DrawDependencyItems(packageJsonInfo, dependType);
        }

        /// <summary>
        /// 绘制所有依赖项
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        private void DrawDependencyItems(PackageJsonInfo packageJsonInfo, string dependType)
        {
            var serializedObject = new SerializedObject(packageJsonInfo);
            var dependencies = serializedObject.FindProperty(dependType);

            for (int i = 0; i < dependencies.arraySize; i++)
            {
                var property = dependencies.GetArrayElementAtIndex(i);
                DrawDependencyItem(property, dependType);
            }

            DrawExistDependencyItem(dependType);
        }

        /// <summary>
        /// 绘制依赖项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void DrawDependencyItem(SerializedProperty dependency, string dependType)
        {
            var i = GetDependenciesItem();
            GetDependencyItemsRoot(dependType).Add(i);

            var textField = i.Q<TextField>("dependencies_name_tf");
            textField.bindingPath = "packageName";
            var packageName = dependency.FindPropertyRelative("packageName");
            textField.BindProperty(packageName);

            textField = i.Q<TextField>("dependencies_version_tf");
            textField.bindingPath = "version";
            var version = dependency.FindPropertyRelative("version");
            textField.BindProperty(version);
        }

        /// <summary>
        /// 添加依赖项
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        private void AddDependencyItem(PackageJsonInfo packageJsonInfo, List<PackageDependency> dependList,
            string dependType)
        {
            var dependency = new PackageDependency();
            dependList.Add(dependency);

            var serializedObject = new SerializedObject(packageJsonInfo);
            var dependencies = serializedObject.FindProperty(dependType);

            var property = dependencies.GetArrayElementAtIndex(dependencies.arraySize - 1);
            DrawDependencyItem(property, dependType);

            DrawExistDependencyItem(dependType);
        }

        /// <summary>
        /// 移除依赖项
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        private void RemoveDependencyItem(PackageJsonInfo packageJsonInfo, List<PackageDependency> dependList,
            string dependType)
        {
            var lastIndex = dependList.Count - 1;
            dependList.RemoveAt(lastIndex);

            var root = GetDependencyItemsRoot(dependType);
            var i = root.ElementAt(lastIndex);
            ReturnDependenciesItem(i);
            root.RemoveAt(lastIndex);

            DrawExistDependencyItem(dependType);
        }

        private void DrawExistDependencyItem(string dependType)
        {
            var root = GetDependencyItemsRoot(dependType);
            var noneTip = GetNoneDependenciesTip(dependType);

            if (root == null || noneTip == null)
            {
                return;
            }

            if (root.childCount <= 0)
            {
                root.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                noneTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                root.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                noneTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        }

        private VisualElement GetDependencyItemsRoot(string dependenciesType)
        {
            return this.Q<VisualElement>($"{dependenciesType}_item_root");
        }

        private VisualElement GetNoneDependenciesTip(string dependenciesType)
        {
            return this.Q<VisualElement>($"{dependenciesType}_none_tip");
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